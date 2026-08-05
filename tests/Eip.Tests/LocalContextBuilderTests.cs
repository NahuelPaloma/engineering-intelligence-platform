using System.Text.Json;

namespace Eip.Tests;

public sealed class LocalContextBuilderTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    [Fact]
    public async Task ConsolidatesExistingValuesInRankingOrder()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var inputs = await WriteInputsAsync(root);
            var outputPath = await WriteLocalContextAsync(inputs);

            using var output = JsonDocument.Parse(await File.ReadAllTextAsync(outputPath));
            var result = output.RootElement;
            Assert.Equal("example/widgets", result.GetProperty("repository").GetString());
            Assert.Equal(123, result.GetProperty("pull_request").GetInt32());
            Assert.Equal(64, result.GetProperty("pack_id").GetString()!.Length);
            var documents = result.GetProperty("documents").EnumerateArray().ToArray();
            Assert.Equal("src/README.md", documents[0].GetProperty("path").GetString());
            Assert.Equal(100, documents[0].GetProperty("score").GetInt32());
            Assert.Equal("same_directory", documents[0].GetProperty("reason").GetString());
            Assert.Equal("Source", documents[0].GetProperty("name").GetString());
            Assert.Equal("Source purpose.", documents[0].GetProperty("purpose").GetString());
            Assert.Equal("# Source\n\nSource purpose.\n", documents[0].GetProperty("content").GetString());
            Assert.Equal("extracted", documents[0].GetProperty("status").GetString());
            Assert.Equal("name", documents[0].GetProperty("evidence")[0].GetProperty("field").GetString());
            Assert.Equal("README.md", documents[1].GetProperty("path").GetString());
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task TransportsModifiedFilesWithNormalizedStatusProvenanceAndStableOrder()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            Cli.ChangedFileEvidence[] changedFiles =
            [
                new("src/added.json", "added", null, 1, 0),
                new("src/modified.yaml", "modified", null, 1, 1),
                new("src/deleted.yml", "removed", null, 0, 1),
                new("src/renamed.json", "renamed", "src/old.json", 0, 0),
                new("src/added.json", "added", null, 1, 0)
            ];
            var inputs = await WriteInputsAsync(root, changedFiles: changedFiles);
            var outputPath = await WriteLocalContextAsync(inputs);

            using var output = JsonDocument.Parse(await File.ReadAllBytesAsync(outputPath));
            var modifiedFiles = output.RootElement.GetProperty("modified_files").EnumerateArray().ToArray();
            Assert.Equal(4, modifiedFiles.Length);
            Assert.Equal(
                ["src/added.json", "src/modified.yaml", "src/deleted.yml", "src/renamed.json"],
                modifiedFiles.Select(item => item.GetProperty("path").GetString()!).ToArray());
            Assert.Equal(
                ["added", "modified", "deleted", "renamed"],
                modifiedFiles.Select(item => item.GetProperty("change_status").GetString()!).ToArray());
            foreach (var item in modifiedFiles)
            {
                var provenance = item.GetProperty("provenance");
                Assert.Equal("github", provenance.GetProperty("provider").GetString());
                Assert.Equal("example/widgets", provenance.GetProperty("repository").GetString());
                Assert.Equal(123, provenance.GetProperty("pull_request").GetInt32());
                Assert.Equal("manifest.changed_files", provenance.GetProperty("source").GetString());
                Assert.Equal(3, item.EnumerateObject().Count());
            }
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task EmitsEmptyModifiedFilesWhenManifestHasNoChangedFiles()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var inputs = await WriteInputsAsync(root);
            var outputPath = await WriteLocalContextAsync(inputs);

            using var output = JsonDocument.Parse(await File.ReadAllBytesAsync(outputPath));
            Assert.Empty(output.RootElement.GetProperty("modified_files").EnumerateArray());
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Theory]
    [InlineData("/absolute/contract.yaml", "modified")]
    [InlineData("C:/absolute/contract.yaml", "modified")]
    [InlineData("../contract.yaml", "modified")]
    [InlineData("contracts/../contract.yaml", "modified")]
    [InlineData("contracts\\contract.yaml", "modified")]
    [InlineData("contracts/contract.yaml", "copied")]
    public async Task RejectsInvalidModifiedFileWithoutWritingOutput(string path, string status)
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var inputs = await WriteInputsAsync(
                root,
                changedFiles: [new(path, status, null, 0, 0)]);

            await Assert.ThrowsAsync<InvalidDataException>(() => WriteLocalContextAsync(inputs));
            Assert.False(File.Exists(Path.Combine(root, "local-context.json")));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task RejectsDuplicatePathWithConflictingStatuses()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var inputs = await WriteInputsAsync(
                root,
                changedFiles:
                [
                    new("contracts/api.yaml", "added", null, 0, 0),
                    new("contracts/api.yaml", "modified", null, 0, 0)
                ]);

            await Assert.ThrowsAsync<InvalidDataException>(() => WriteLocalContextAsync(inputs));
            Assert.False(File.Exists(Path.Combine(root, "local-context.json")));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Theory]
    [InlineData("insufficient")]
    [InlineData("missing_name")]
    [InlineData("missing_purpose")]
    public async Task PreservesIncompleteMetadataStatus(string status)
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var inputs = await WriteInputsAsync(root, status);
            var outputPath = await WriteLocalContextAsync(inputs);

            using var output = JsonDocument.Parse(await File.ReadAllTextAsync(outputPath));
            var document = output.RootElement.GetProperty("documents")[0];
            Assert.Equal(status, document.GetProperty("status").GetString());
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task PreservesUnreadableDocumentWithoutAbortingOthers()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var inputs = await WriteInputsAsync(root);
            var contents = new Cli.ReadmeDocuments(
            [
                Cli.ReadmeDocument.Failed("src/README.md"),
                Cli.ReadmeDocument.Succeeded("README.md", "# Root\n")
            ]);
            await WriteJsonAsync(inputs.ContentsPath, contents);

            var outputPath = await WriteLocalContextAsync(inputs);
            using var output = JsonDocument.Parse(await File.ReadAllTextAsync(outputPath));
            var documents = output.RootElement.GetProperty("documents");
            Assert.Equal(JsonValueKind.Null, documents[0].GetProperty("content").ValueKind);
            Assert.Equal("The document could not be read.", documents[0].GetProperty("error").GetString());
            Assert.Equal("# Root\n", documents[1].GetProperty("content").GetString());
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Theory]
    [InlineData("contents", true)]
    [InlineData("contents", false)]
    [InlineData("metadata", true)]
    [InlineData("ranking", false)]
    [InlineData("readmes", true)]
    public async Task RejectsMissingOrExtraDocuments(string target, bool remove)
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var inputs = await WriteInputsAsync(root);
            var paths = target switch
            {
                "contents" => inputs.ContentsPath,
                "metadata" => inputs.MetadataPath,
                "ranking" => inputs.RankingPath,
                _ => inputs.ReadmesPath
            };
            await ReplaceDocumentsAsync(paths, target, remove);

            await Assert.ThrowsAsync<InvalidDataException>(() => WriteLocalContextAsync(inputs));
            Assert.False(File.Exists(Path.Combine(root, "local-context.json")));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Theory]
    [InlineData("manifest")]
    [InlineData("readmes")]
    [InlineData("contents")]
    [InlineData("metadata")]
    [InlineData("ranking")]
    public async Task RejectsInvalidJsonWithoutWritingOutput(string target)
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var inputs = await WriteInputsAsync(root);
            await File.WriteAllTextAsync(GetPath(inputs, target), "{ invalid");

            await Assert.ThrowsAsync<InvalidDataException>(() => WriteLocalContextAsync(inputs));
            Assert.False(File.Exists(Path.Combine(root, "local-context.json")));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task RepeatedExecutionIsByteDeterministicAndLeavesEveryInputUnchanged()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var inputs = await WriteInputsAsync(root);
            var paths = InputPaths(inputs);
            var before = await Task.WhenAll(paths.Select(path => File.ReadAllBytesAsync(path)));

            var outputPath = await WriteLocalContextAsync(inputs);
            var first = await File.ReadAllBytesAsync(outputPath);
            await WriteLocalContextAsync(inputs);
            var second = await File.ReadAllBytesAsync(outputPath);

            Assert.Equal(first, second);
            var after = await Task.WhenAll(paths.Select(path => File.ReadAllBytesAsync(path)));
            for (var index = 0; index < before.Length; index++)
            {
                Assert.Equal(before[index], after[index]);
            }
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private static async Task<InputFiles> WriteInputsAsync(
        string root,
        string firstStatus = "extracted",
        IReadOnlyList<Cli.ChangedFileEvidence>? changedFiles = null)
    {
        var manifest = new Cli.EvidenceManifest(
            "example/widgets",
            "example",
            123,
            "Change widgets",
            "octocat",
            "base-sha",
            "head-sha",
            [],
            changedFiles ?? [],
            new DateTimeOffset(2026, 8, 4, 12, 0, 0, TimeSpan.Zero));
        var readmes = new Cli.ReadmeCandidates(
        [
            new("README.md", "repository root"),
            new("src/README.md", "changed file directory")
        ]);
        var contents = new Cli.ReadmeDocuments(
        [
            Cli.ReadmeDocument.Succeeded("README.md", "# Root\n"),
            Cli.ReadmeDocument.Succeeded("src/README.md", "# Source\n\nSource purpose.\n")
        ]);
        var metadata = new Cli.ReadmeMetadataDocuments(
        [
            new("README.md", "Root", null, "missing_purpose",
                [new("name", "Root", 1, 1)]),
            new("src/README.md", "Source", "Source purpose.", firstStatus,
                [new("name", "Source", 1, 1), new("purpose", "Source purpose.", 3, 3)])
        ]);
        var ranking = new Cli.ReadmeRankingDocuments(
        [
            new("src/README.md", 100, "same_directory"),
            new("README.md", 10, "repository_root")
        ]);
        var inputs = new InputFiles(
            Path.Combine(root, "manifest.json"),
            Path.Combine(root, "readmes.json"),
            Path.Combine(root, "readme-contents.json"),
            Path.Combine(root, "readme-metadata.json"),
            Path.Combine(root, "readme-ranking.json"));
        await WriteJsonAsync(inputs.ManifestPath, manifest);
        await WriteJsonAsync(inputs.ReadmesPath, readmes);
        await WriteJsonAsync(inputs.ContentsPath, contents);
        await WriteJsonAsync(inputs.MetadataPath, metadata);
        await WriteJsonAsync(inputs.RankingPath, ranking);
        return inputs;
    }

    private static async Task ReplaceDocumentsAsync(string path, string target, bool remove)
    {
        var replacementPath = remove ? "README.md" : "extra/README.md";
        object value = target switch
        {
            "contents" => new Cli.ReadmeDocuments(
                [Cli.ReadmeDocument.Succeeded(replacementPath, "content")]),
            "metadata" => new Cli.ReadmeMetadataDocuments(
                [new(replacementPath, null, null, "insufficient", [])]),
            "ranking" => new Cli.ReadmeRankingDocuments(
                [new(replacementPath, 10, "repository_root")]),
            _ => new Cli.ReadmeCandidates(
                [new(replacementPath, "repository root")])
        };
        await WriteJsonAsync(path, value);
    }

    private static Task<string> WriteLocalContextAsync(InputFiles inputs) =>
        Cli.LocalContextBuilder.WriteAsync(
            inputs.ManifestPath,
            inputs.ReadmesPath,
            inputs.ContentsPath,
            inputs.MetadataPath,
            inputs.RankingPath,
            CancellationToken.None);

    private static async Task WriteJsonAsync<T>(string path, T value)
    {
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, value, JsonOptions);
        await stream.WriteAsync("\n"u8.ToArray());
    }

    private static string GetPath(InputFiles inputs, string target) => target switch
    {
        "manifest" => inputs.ManifestPath,
        "readmes" => inputs.ReadmesPath,
        "contents" => inputs.ContentsPath,
        "metadata" => inputs.MetadataPath,
        _ => inputs.RankingPath
    };

    private static string[] InputPaths(InputFiles inputs) =>
    [
        inputs.ManifestPath,
        inputs.ReadmesPath,
        inputs.ContentsPath,
        inputs.MetadataPath,
        inputs.RankingPath
    ];

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"eip-local-context-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private sealed record InputFiles(
        string ManifestPath,
        string ReadmesPath,
        string ContentsPath,
        string MetadataPath,
        string RankingPath);
}
