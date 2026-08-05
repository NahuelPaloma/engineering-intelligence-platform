using System.Text.Json;
using System.Text.Json.Nodes;

namespace Eip.Tests;

public sealed class LocalContextContractTests
{
    private const string PackId = "f166a136da542904223312b67fbb42ba5d1436fd29a399f7956d02ef50525bdd";

    [Fact]
    public async Task InputBoundaryPreservesValidModifiedFilesWithoutFilesystemAccess()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var path = await WriteLocalContextAsync(
                root,
                ModifiedFile("outside/does-not-exist.yaml", "added"),
                ModifiedFile("contracts/deleted.json", "deleted"));

            var result = await Cli.Inference.InputBoundary.ReadAsync(path, CancellationToken.None);

            Assert.Equal(
                ["outside/does-not-exist.yaml", "contracts/deleted.json"],
                result.ModifiedFiles.Select(item => item.Path).ToArray());
            Assert.Equal(["added", "deleted"], result.ModifiedFiles.Select(item => item.ChangeStatus).ToArray());
            Assert.Equal("available", result.ModifiedFilesAvailability);
            Assert.False(Directory.Exists(Path.Combine(root, "outside")));
            Assert.All(result.ModifiedFiles, item =>
            {
                Assert.Equal("github", item.Provenance.Provider);
                Assert.Equal("example/widgets", item.Provenance.Repository);
                Assert.Equal(123, item.Provenance.PullRequest);
                Assert.Equal("manifest.changed_files", item.Provenance.Source);
            });
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task InputBoundaryAcceptsVersionOneContextWithoutModifiedFiles()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var path = await WriteLocalContextAsync(root, null);

            var result = await Cli.Inference.InputBoundary.ReadAsync(path, CancellationToken.None);

            Assert.Empty(result.ModifiedFiles);
            Assert.Equal("not_provided", result.ModifiedFilesAvailability);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Theory]
    [InlineData("missing_path")]
    [InlineData("missing_status")]
    [InlineData("invalid_status")]
    [InlineData("absolute_path")]
    [InlineData("escape_path")]
    [InlineData("missing_provenance")]
    [InlineData("wrong_repository")]
    [InlineData("wrong_pull_request")]
    [InlineData("wrong_source")]
    public async Task InputBoundaryRejectsInvalidModifiedFileStructure(string mutation)
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var item = ModifiedFile("contracts/api.yaml", "modified");
            switch (mutation)
            {
                case "missing_path": item.Remove("path"); break;
                case "missing_status": item.Remove("change_status"); break;
                case "invalid_status": item["change_status"] = "copied"; break;
                case "absolute_path": item["path"] = "/contracts/api.yaml"; break;
                case "escape_path": item["path"] = "contracts/../api.yaml"; break;
                case "missing_provenance": item.Remove("provenance"); break;
                case "wrong_repository": item["provenance"]!["repository"] = "other/repository"; break;
                case "wrong_pull_request": item["provenance"]!["pull_request"] = 456; break;
                case "wrong_source": item["provenance"]!["source"] = "other"; break;
            }

            var path = await WriteLocalContextAsync(root, item);

            await Assert.ThrowsAsync<InvalidDataException>(() =>
                Cli.Inference.InputBoundary.ReadAsync(path, CancellationToken.None));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task InputBoundaryRejectsDuplicateModifiedFilePaths()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var path = await WriteLocalContextAsync(
                root,
                ModifiedFile("contracts/api.yaml", "added"),
                ModifiedFile("contracts/api.yaml", "added"));

            await Assert.ThrowsAsync<InvalidDataException>(() =>
                Cli.Inference.InputBoundary.ReadAsync(path, CancellationToken.None));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task ModifiedFilesDoNotProduceClaimsOrChangeCurrentReportBehavior()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var path = await WriteLocalContextAsync(
                root,
                ModifiedFile("contracts/api.yaml", "modified"));

            var executionPath = await Cli.Inference.InferencePipeline.ExecuteAsync(path, CancellationToken.None);

            using var execution = JsonDocument.Parse(await File.ReadAllBytesAsync(executionPath));
            Assert.Equal(0, execution.RootElement.GetProperty("counts").GetProperty("evidence").GetInt32());
            Assert.Equal(0, execution.RootElement.GetProperty("counts").GetProperty("claims").GetInt32());
            Assert.Empty(execution.RootElement.GetProperty("findings").EnumerateArray());
            Assert.True(File.Exists(Path.Combine(root, "inference-report.json")));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task AbsentAndPresentEmptyModifiedFilesProduceDifferentExecutionIdentity()
    {
        var firstRoot = CreateTemporaryDirectory();
        var secondRoot = CreateTemporaryDirectory();

        try
        {
            var oldContext = await WriteLocalContextAsync(firstRoot, null);
            var enrichedContext = await WriteLocalContextAsync(secondRoot, []);

            var oldExecution = await Cli.Inference.InferencePipeline.ExecuteAsync(
                oldContext,
                CancellationToken.None);
            var enrichedExecution = await Cli.Inference.InferencePipeline.ExecuteAsync(
                enrichedContext,
                CancellationToken.None);
            var oldInput = await Cli.Inference.InputBoundary.ReadAsync(oldContext, CancellationToken.None);
            var enrichedInput = await Cli.Inference.InputBoundary.ReadAsync(enrichedContext, CancellationToken.None);

            using var oldDocument = JsonDocument.Parse(await File.ReadAllBytesAsync(oldExecution));
            using var enrichedDocument = JsonDocument.Parse(await File.ReadAllBytesAsync(enrichedExecution));
            Assert.Equal("not_provided", oldInput.ModifiedFilesAvailability);
            Assert.Equal("available", enrichedInput.ModifiedFilesAvailability);
            Assert.Empty(enrichedInput.ModifiedFiles);
            Assert.NotEqual(
                oldDocument.RootElement.GetProperty("execution_id").GetString(),
                enrichedDocument.RootElement.GetProperty("execution_id").GetString());
            Assert.Equal(
                oldDocument.RootElement.GetProperty("counts").GetRawText(),
                enrichedDocument.RootElement.GetProperty("counts").GetRawText());
        }
        finally
        {
            Directory.Delete(firstRoot, true);
            Directory.Delete(secondRoot, true);
        }
    }

    [Theory]
    [InlineData("path")]
    [InlineData("change_status")]
    [InlineData("provider")]
    [InlineData("provenance_repository")]
    [InlineData("provenance_pull_request")]
    public async Task MaterialModifiedFileChangesProduceDifferentCanonicalIdentity(string mutation)
    {
        var firstRoot = CreateTemporaryDirectory();
        var secondRoot = CreateTemporaryDirectory();

        try
        {
            var baseline = ModifiedFile("contracts/api.yaml", "modified");
            var changed = ModifiedFile("contracts/api.yaml", "modified");
            switch (mutation)
            {
                case "path": changed["path"] = "contracts/other.yaml"; break;
                case "change_status": changed["change_status"] = "added"; break;
                case "provider": changed["provenance"]!["provider"] = "github-enterprise"; break;
                case "provenance_repository":
                    changed["provenance"]!["repository"] = "other/widgets";
                    break;
                case "provenance_pull_request": changed["provenance"]!["pull_request"] = 456; break;
            }

            var firstContext = await WriteLocalContextAsync(firstRoot, baseline);
            if (mutation == "provenance_repository")
            {
                await SetTopLevelIdentityAsync(secondRoot, changed, "other/widgets", 123);
            }
            else if (mutation == "provenance_pull_request")
            {
                await SetTopLevelIdentityAsync(secondRoot, changed, "example/widgets", 456);
            }
            else
            {
                await WriteLocalContextAsync(secondRoot, changed);
            }

            var first = await Cli.Inference.InputBoundary.ReadAsync(firstContext, CancellationToken.None);
            var second = await Cli.Inference.InputBoundary.ReadAsync(
                Path.Combine(secondRoot, "local-context.json"),
                CancellationToken.None);
            Assert.NotEqual(first.CanonicalContextIdentity, second.CanonicalContextIdentity);
        }
        finally
        {
            Directory.Delete(firstRoot, true);
            Directory.Delete(secondRoot, true);
        }
    }

    [Fact]
    public async Task DifferentModifiedFileCollectionsProduceDifferentExecutionIdentity()
    {
        var firstRoot = CreateTemporaryDirectory();
        var secondRoot = CreateTemporaryDirectory();

        try
        {
            var firstContext = await WriteLocalContextAsync(
                firstRoot,
                ModifiedFile("contracts/first.yaml", "modified"));
            var secondContext = await WriteLocalContextAsync(
                secondRoot,
                ModifiedFile("contracts/second.yaml", "modified"));

            var firstExecution = await Cli.Inference.InferencePipeline.ExecuteAsync(
                firstContext,
                CancellationToken.None);
            var secondExecution = await Cli.Inference.InferencePipeline.ExecuteAsync(
                secondContext,
                CancellationToken.None);

            using var firstDocument = JsonDocument.Parse(await File.ReadAllBytesAsync(firstExecution));
            using var secondDocument = JsonDocument.Parse(await File.ReadAllBytesAsync(secondExecution));
            Assert.NotEqual(
                firstDocument.RootElement.GetProperty("execution_id").GetString(),
                secondDocument.RootElement.GetProperty("execution_id").GetString());
        }
        finally
        {
            Directory.Delete(firstRoot, true);
            Directory.Delete(secondRoot, true);
        }
    }

    [Fact]
    public async Task CanonicalIdentityIgnoresJsonFormattingAndPropertyOrder()
    {
        var firstRoot = CreateTemporaryDirectory();
        var secondRoot = CreateTemporaryDirectory();

        try
        {
            var firstContext = await WriteLocalContextAsync(
                firstRoot,
                ModifiedFile("contracts/api.yaml", "modified"));
            var secondContext = await WriteEquivalentContextWithDifferentPropertyOrderAsync(secondRoot);

            var first = await Cli.Inference.InputBoundary.ReadAsync(firstContext, CancellationToken.None);
            var second = await Cli.Inference.InputBoundary.ReadAsync(secondContext, CancellationToken.None);

            Assert.Equal(first.CanonicalContextIdentity, second.CanonicalContextIdentity);
        }
        finally
        {
            Directory.Delete(firstRoot, true);
            Directory.Delete(secondRoot, true);
        }
    }

    [Fact]
    public async Task RepeatedInputProducesSameExecutionIdentityAndBytes()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var context = await WriteLocalContextAsync(
                root,
                ModifiedFile("contracts/api.yaml", "modified"));

            var output = await Cli.Inference.InferencePipeline.ExecuteAsync(context, CancellationToken.None);
            var first = await File.ReadAllBytesAsync(output);
            await Cli.Inference.InferencePipeline.ExecuteAsync(context, CancellationToken.None);
            var second = await File.ReadAllBytesAsync(output);

            Assert.Equal(first, second);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private static JsonObject ModifiedFile(string path, string status) => new()
    {
        ["path"] = path,
        ["change_status"] = status,
        ["provenance"] = new JsonObject
        {
            ["provider"] = "github",
            ["repository"] = "example/widgets",
            ["pull_request"] = 123,
            ["source"] = "manifest.changed_files"
        }
    };

    private static async Task<string> WriteLocalContextAsync(
        string root,
        params JsonObject?[]? modifiedFiles)
    {
        var context = new JsonObject
        {
            ["repository"] = "example/widgets",
            ["pull_request"] = 123,
            ["pack_id"] = PackId,
            ["documents"] = new JsonArray()
        };
        if (modifiedFiles is not null)
        {
            context["modified_files"] = new JsonArray(modifiedFiles.Where(item => item is not null).ToArray());
        }

        var path = Path.Combine(root, "local-context.json");
        await File.WriteAllTextAsync(
            path,
            context.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");
        return path;
    }

    private static async Task<string> WriteEquivalentContextWithDifferentPropertyOrderAsync(string root)
    {
        var context = new JsonObject
        {
            ["documents"] = new JsonArray(),
            ["modified_files"] = new JsonArray
            {
                new JsonObject
                {
                    ["provenance"] = new JsonObject
                    {
                        ["source"] = "manifest.changed_files",
                        ["pull_request"] = 123,
                        ["repository"] = "example/widgets",
                        ["provider"] = "github"
                    },
                    ["change_status"] = "modified",
                    ["path"] = "contracts/api.yaml"
                }
            },
            ["pack_id"] = PackId,
            ["pull_request"] = 123,
            ["repository"] = "example/widgets"
        };
        var path = Path.Combine(root, "local-context.json");
        await File.WriteAllTextAsync(path, context.ToJsonString());
        return path;
    }

    private static async Task SetTopLevelIdentityAsync(
        string root,
        JsonObject modifiedFile,
        string repository,
        int pullRequest)
    {
        var context = new JsonObject
        {
            ["repository"] = repository,
            ["pull_request"] = pullRequest,
            ["pack_id"] = PackId,
            ["modified_files"] = new JsonArray(modifiedFile),
            ["documents"] = new JsonArray()
        };
        await File.WriteAllTextAsync(
            Path.Combine(root, "local-context.json"),
            context.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"eip-local-context-contract-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
