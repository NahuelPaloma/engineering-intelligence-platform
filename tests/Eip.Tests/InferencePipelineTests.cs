using System.Text.Json;
using System.Text.Json.Nodes;

namespace Eip.Tests;

public sealed class InferencePipelineTests
{
    private static readonly string[] ExpectedStages =
    [
        "input_boundary:completed",
        "claim_processing:not_implemented",
        "hypothesis_processing:not_implemented",
        "finding_processing:not_implemented",
        "report_builder:not_implemented"
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private static readonly string[] RetrievalArtifactNames =
    [
        "manifest.json",
        "readmes.json",
        "readme-contents.json",
        "readme-metadata.json",
        "readme-ranking.json"
    ];

    [Fact]
    public async Task ExecutesEmptyPipelineWithoutProducingInferences()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var localContextPath = await WriteValidLocalContextAsync(root);
            var outputPath = await Cli.Inference.InferencePipeline.ExecuteAsync(
                localContextPath,
                CancellationToken.None);

            using var document = JsonDocument.Parse(await File.ReadAllBytesAsync(outputPath));
            var execution = document.RootElement;
            Assert.Equal(ExpectedExecutionId, execution.GetProperty("execution_id").GetString());
            Assert.Equal(PackId, execution.GetProperty("input_pack_id").GetString());
            Assert.Equal("capability-002-empty-rules-v1", execution.GetProperty("rule_set_id").GetString());
            Assert.Equal("no_inferences", execution.GetProperty("status").GetString());
            Assert.Equal(
                ExpectedStages,
                execution.GetProperty("stages")
                    .EnumerateArray()
                    .Select(stage => $"{stage.GetProperty("name").GetString()}:{stage.GetProperty("status").GetString()}")
                    .ToArray());

            var counts = execution.GetProperty("counts");
            Assert.Equal(0, counts.GetProperty("evidence").GetInt32());
            Assert.Equal(0, counts.GetProperty("claims").GetInt32());
            Assert.Equal(0, counts.GetProperty("hypotheses").GetInt32());
            Assert.Equal(0, counts.GetProperty("findings").GetInt32());
            Assert.Equal(0, counts.GetProperty("abstentions").GetInt32());
            Assert.False(execution.TryGetProperty("claims", out _));
            Assert.False(execution.TryGetProperty("hypotheses", out _));
            Assert.False(execution.TryGetProperty("findings", out _));
            Assert.False(execution.TryGetProperty("abstentions", out _));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task IsByteDeterministicAndLeavesEveryExistingArtifactUnchanged()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var localContextPath = await WriteValidLocalContextAsync(root);
            var inputPaths = RetrievalArtifactNames
                .Select(name => Path.Combine(root, name))
                .Append(localContextPath)
                .ToArray();
            foreach (var path in inputPaths[..^1])
            {
                await File.WriteAllTextAsync(path, $"unchanged:{Path.GetFileName(path)}\n");
            }

            var before = await Task.WhenAll(inputPaths.Select(path => File.ReadAllBytesAsync(path)));
            var outputPath = await Cli.Inference.InferencePipeline.ExecuteAsync(
                localContextPath,
                CancellationToken.None);
            var first = await File.ReadAllBytesAsync(outputPath);
            await Cli.Inference.InferencePipeline.ExecuteAsync(localContextPath, CancellationToken.None);
            var second = await File.ReadAllBytesAsync(outputPath);
            var after = await Task.WhenAll(inputPaths.Select(path => File.ReadAllBytesAsync(path)));

            Assert.Equal(first, second);
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

    [Fact]
    public async Task DoesNotReadRepositoryFilesOrRequireHttpAccess()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var localContextPath = await WriteValidLocalContextAsync(
                root,
                documentPath: "repository-does-not-exist/README.md");

            var outputPath = await Cli.Inference.InferencePipeline.ExecuteAsync(
                localContextPath,
                CancellationToken.None);

            Assert.True(File.Exists(outputPath));
            Assert.False(Directory.Exists(Path.Combine(root, "repository-does-not-exist")));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Theory]
    [InlineData("{ invalid")]
    [InlineData("{}")]
    [InlineData("{\"repository\":\"example/widgets\",\"pull_request\":123,\"pack_id\":null,\"documents\":[]}")]
    [InlineData("{\"repository\":\"example/widgets\",\"pull_request\":123,\"pack_id\":\"invalid\",\"documents\":[]}")]
    [InlineData("{\"repository\":\"example/widgets\",\"pull_request\":123,\"pack_id\":\"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\",\"documents\":[],\"contract_version\":2}")]
    public async Task RejectsInvalidOrIncompatibleInputWithoutPublishingOutput(string content)
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var localContextPath = Path.Combine(root, "local-context.json");
            await File.WriteAllTextAsync(localContextPath, content);

            await Assert.ThrowsAsync<InvalidDataException>(() =>
                Cli.Inference.InferencePipeline.ExecuteAsync(localContextPath, CancellationToken.None));

            Assert.False(File.Exists(Path.Combine(root, "inference-execution.json")));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task RejectsMissingInputWithoutPublishingOutputOrExposingPath()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var missingPath = Path.Combine(root, "local-context.json");
            var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
                Cli.Inference.InferencePipeline.ExecuteAsync(missingPath, CancellationToken.None));

            Assert.Equal("The local context could not be read.", exception.Message);
            Assert.DoesNotContain(root, exception.ToString(), StringComparison.Ordinal);
            Assert.False(File.Exists(Path.Combine(root, "inference-execution.json")));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task DoesNotPublishPartialOutputWhenAtomicWriteFails()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var localContextPath = await WriteValidLocalContextAsync(root);
            Directory.CreateDirectory(Path.Combine(root, "inference-execution.json.tmp"));

            await Assert.ThrowsAnyAsync<IOException>(() =>
                Cli.Inference.InferencePipeline.ExecuteAsync(localContextPath, CancellationToken.None));

            Assert.False(File.Exists(Path.Combine(root, "inference-execution.json")));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task RejectsMissingRequiredDocumentProperty()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var localContextPath = await WriteValidLocalContextAsync(root);
            var input = JsonNode.Parse(await File.ReadAllTextAsync(localContextPath))!.AsObject();
            input["documents"]![0]!.AsObject().Remove("status");
            await File.WriteAllTextAsync(localContextPath, input.ToJsonString());

            await Assert.ThrowsAsync<InvalidDataException>(() =>
                Cli.Inference.InferencePipeline.ExecuteAsync(localContextPath, CancellationToken.None));

            Assert.False(File.Exists(Path.Combine(root, "inference-execution.json")));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private const string PackId = "f166a136da542904223312b67fbb42ba5d1436fd29a399f7956d02ef50525bdd";
    private const string ExpectedExecutionId = "af2f3c5874d3b95005d707f78e2c35188ea41c3a41ae4275a632f225fce8e149";

    private static async Task<string> WriteValidLocalContextAsync(
        string root,
        string documentPath = "missing/README.md")
    {
        var localContext = new
        {
            repository = "example/widgets",
            pull_request = 123,
            pack_id = PackId,
            documents = new[]
            {
                new
                {
                    path = documentPath,
                    score = 100,
                    reason = "same_directory",
                    name = "Example",
                    purpose = "Provides an example.",
                    content = "# Example\n\nProvides an example.\n",
                    status = "extracted",
                    evidence = new[]
                    {
                        new
                        {
                            field = "name",
                            text = "Example",
                            source_line_start = 1,
                            source_line_end = 1
                        },
                        new
                        {
                            field = "purpose",
                            text = "Provides an example.",
                            source_line_start = 3,
                            source_line_end = 3
                        }
                    }
                }
            }
        };
        var path = Path.Combine(root, "local-context.json");
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, localContext, JsonOptions);
        await stream.WriteAsync("\n"u8.ToArray());
        return path;
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"eip-inference-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
