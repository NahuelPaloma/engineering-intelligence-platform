using System.Text.Json;
using System.Text.Json.Nodes;

namespace Eip.Tests;

public sealed class ModifiedFileEvidenceAdmissionTests
{
    [Fact]
    public async Task NotProvidedProducesUnknownCoverageAndValidatedTotalAbstention()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var output = await ExecuteCapability003Async(await WriteLocalContextAsync(root, null));
            using var execution = JsonDocument.Parse(await File.ReadAllBytesAsync(output));
            var value = execution.RootElement;
            var context = value.GetProperty("capability_context");
            var coverage = context.GetProperty("contract_detection_coverage");

            Assert.Equal("not_provided", context.GetProperty("modified_files_availability").GetString());
            Assert.Equal(0, context.GetProperty("admitted_modified_files_count").GetInt32());
            Assert.Empty(context.GetProperty("admitted_modified_files").EnumerateArray());
            Assert.Equal("unknown", coverage.GetProperty("coverage_status").GetString());
            Assert.Equal("modified_files_not_provided", coverage.GetProperty("cause").GetString());
            Assert.Empty(coverage.GetProperty("total_scope").EnumerateArray());
            Assert.Empty(coverage.GetProperty("processed_scope").EnumerateArray());
            Assert.Empty(coverage.GetProperty("uncovered_scope").EnumerateArray());

            var abstention = Assert.Single(value.GetProperty("abstentions").EnumerateArray());
            Assert.Equal("total", abstention.GetProperty("type").GetString());
            Assert.Equal("modified_files_not_provided", abstention.GetProperty("condition").GetString());
            Assert.Contains(
                "not provided",
                abstention.GetProperty("missing_evidence_description").GetString(),
                StringComparison.OrdinalIgnoreCase);
            Assert.Equal(1, value.GetProperty("counts").GetProperty("abstentions").GetInt32());
            AssertNoDomainInferences(value);
            Assert.DoesNotContain("zero files", value.GetRawText(), StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task AvailableEmptyProducesCompleteCoverageWithoutAbstention()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var output = await ExecuteCapability003Async(await WriteLocalContextAsync(root, []));
            using var execution = JsonDocument.Parse(await File.ReadAllBytesAsync(output));
            var value = execution.RootElement;
            var context = value.GetProperty("capability_context");
            var coverage = context.GetProperty("contract_detection_coverage");

            Assert.Equal("available", context.GetProperty("modified_files_availability").GetString());
            Assert.Equal(0, context.GetProperty("admitted_modified_files_count").GetInt32());
            Assert.Equal("complete", coverage.GetProperty("coverage_status").GetString());
            Assert.False(coverage.TryGetProperty("cause", out _));
            Assert.Empty(coverage.GetProperty("total_scope").EnumerateArray());
            Assert.Empty(coverage.GetProperty("processed_scope").EnumerateArray());
            Assert.Empty(coverage.GetProperty("uncovered_scope").EnumerateArray());
            Assert.Empty(value.GetProperty("abstentions").EnumerateArray());
            AssertNoDomainInferences(value);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task AvailableFilesPreserveEvidenceAndProducePartialCoverageInContractualOrder()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var output = await ExecuteCapability003Async(await WriteLocalContextAsync(
                root,
                [
                    ModifiedFile("outside/does-not-exist.yaml", "added"),
                    ModifiedFile("contracts/deleted.json", "deleted")
                ]));
            using var execution = JsonDocument.Parse(await File.ReadAllBytesAsync(output));
            var value = execution.RootElement;
            var context = value.GetProperty("capability_context");
            var admitted = context.GetProperty("admitted_modified_files").EnumerateArray().ToArray();
            var coverage = context.GetProperty("contract_detection_coverage");

            Assert.Equal("available", context.GetProperty("modified_files_availability").GetString());
            Assert.Equal(2, context.GetProperty("admitted_modified_files_count").GetInt32());
            Assert.Equal(["outside/does-not-exist.yaml", "contracts/deleted.json"],
                admitted.Select(item => item.GetProperty("path").GetString()!).ToArray());
            Assert.Equal(["added", "deleted"],
                admitted.Select(item => item.GetProperty("change_status").GetString()!).ToArray());
            Assert.Equal([0, 1], admitted.Select(item => item.GetProperty("position").GetInt32()).ToArray());
            Assert.All(admitted, item =>
            {
                Assert.Equal(64, item.GetProperty("evidence_id").GetString()!.Length);
                Assert.Equal("github", item.GetProperty("provenance").GetProperty("provider").GetString());
                Assert.Equal("example/widgets", item.GetProperty("provenance").GetProperty("repository").GetString());
                Assert.Equal(123, item.GetProperty("provenance").GetProperty("pull_request").GetInt32());
                Assert.Equal("manifest.changed_files", item.GetProperty("provenance").GetProperty("source").GetString());
                Assert.Equal(item.GetProperty("evidence_id").GetString(),
                    item.GetProperty("scope").GetProperty("evidence_id").GetString());
                Assert.Equal(item.GetProperty("position").GetInt32(),
                    item.GetProperty("scope").GetProperty("position").GetInt32());
                Assert.Equal(item.GetProperty("path").GetString(),
                    item.GetProperty("scope").GetProperty("path").GetString());
            });

            Assert.Equal("partial", coverage.GetProperty("coverage_status").GetString());
            Assert.Equal("no_candidate_rules_registered", coverage.GetProperty("cause").GetString());
            Assert.Empty(coverage.GetProperty("processed_scope").EnumerateArray());
            Assert.Equal(
                admitted.Select(item => item.GetProperty("evidence_id").GetString()).ToArray(),
                coverage.GetProperty("total_scope").EnumerateArray()
                    .Select(item => item.GetProperty("evidence_id").GetString()).ToArray());
            Assert.Equal(
                admitted.Select(item => item.GetProperty("evidence_id").GetString()).ToArray(),
                coverage.GetProperty("uncovered_scope").EnumerateArray()
                    .Select(item => item.GetProperty("evidence_id").GetString()).ToArray());
            Assert.Empty(value.GetProperty("abstentions").EnumerateArray());
            AssertNoDomainInferences(value);
            Assert.False(Directory.Exists(Path.Combine(root, "outside")));
            Assert.False(File.Exists(Path.Combine(root, "manifest.json")));
            Assert.False(File.Exists(Path.Combine(root, "contract-change-report.json")));
            Assert.DoesNotContain("contract_type", value.GetRawText(), StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("contract_candidate", value.GetRawText(), StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task SameInputAndProfileProduceSameIdentityCoverageAndBytes()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var context = await WriteLocalContextAsync(
                root,
                [ModifiedFile("contracts/api.yaml", "modified")]);
            var output = await ExecuteCapability003Async(context);
            var first = await File.ReadAllBytesAsync(output);
            using var firstDocument = JsonDocument.Parse(first);

            await ExecuteCapability003Async(context);
            var second = await File.ReadAllBytesAsync(output);
            using var secondDocument = JsonDocument.Parse(second);

            Assert.Equal(first, second);
            Assert.Equal(
                firstDocument.RootElement.GetProperty("execution_id").GetString(),
                secondDocument.RootElement.GetProperty("execution_id").GetString());
            Assert.Equal(
                firstDocument.RootElement.GetProperty("capability_context")
                    .GetProperty("contract_detection_coverage").GetRawText(),
                secondDocument.RootElement.GetProperty("capability_context")
                    .GetProperty("contract_detection_coverage").GetRawText());
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task Capability002IgnoresMissingModifiedFilesAndKeepsAcceptedArtifactShape()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var output = await Cli.Inference.InferencePipeline.ExecuteAsync(
                await WriteLocalContextAsync(root, null),
                CancellationToken.None);
            using var execution = JsonDocument.Parse(await File.ReadAllBytesAsync(output));

            Assert.False(execution.RootElement.TryGetProperty("capability_context", out _));
            Assert.Empty(execution.RootElement.GetProperty("abstentions").EnumerateArray());
            Assert.Equal(0, execution.RootElement.GetProperty("counts").GetProperty("abstentions").GetInt32());
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task InvalidDuplicateEvidenceFailsWithoutPublishingOutput()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var context = await WriteLocalContextAsync(
                root,
                [
                    ModifiedFile("contracts/api.yaml", "modified"),
                    ModifiedFile("contracts/api.yaml", "modified")
                ]);

            await Assert.ThrowsAsync<InvalidDataException>(() => ExecuteCapability003Async(context));
            Assert.False(File.Exists(Path.Combine(root, "inference-execution.json")));
            Assert.False(File.Exists(Path.Combine(root, "inference-report.json")));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private static void AssertNoDomainInferences(JsonElement execution)
    {
        Assert.Empty(execution.GetProperty("claims").EnumerateArray());
        Assert.Empty(execution.GetProperty("hypotheses").EnumerateArray());
        Assert.Empty(execution.GetProperty("findings").EnumerateArray());
        Assert.Equal(0, execution.GetProperty("counts").GetProperty("claims").GetInt32());
        Assert.Equal(0, execution.GetProperty("counts").GetProperty("hypotheses").GetInt32());
        Assert.Equal(0, execution.GetProperty("counts").GetProperty("findings").GetInt32());
    }

    private static Task<string> ExecuteCapability003Async(string path) =>
        Cli.Inference.InferencePipeline.ExecuteAsync(
            path,
            Cli.Inference.ExecutionProfileRegistry.Capability003ProfileId,
            CancellationToken.None);

    private static async Task<string> WriteLocalContextAsync(
        string root,
        JsonObject[]? modifiedFiles)
    {
        var context = new JsonObject
        {
            ["repository"] = "example/widgets",
            ["pull_request"] = 123,
            ["pack_id"] = "f166a136da542904223312b67fbb42ba5d1436fd29a399f7956d02ef50525bdd",
            ["documents"] = new JsonArray()
        };
        if (modifiedFiles is not null)
        {
            context["modified_files"] = new JsonArray(modifiedFiles);
        }

        var path = Path.Combine(root, "local-context.json");
        await File.WriteAllTextAsync(
            path,
            context.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");
        return path;
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

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"eip-modified-files-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
