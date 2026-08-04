using System.Text.Json;
using System.Text.Json.Nodes;

namespace Eip.Tests;

public sealed class InferencePipelineTests
{
    private static readonly string[] ExpectedStages =
    [
        "input_boundary:completed",
        "claim_processing:completed",
        "hypothesis_processing:not_implemented",
        "finding_processing:not_implemented",
        "report_builder:not_implemented"
    ];

    private static readonly string[] RetrievalArtifactNames =
    [
        "manifest.json",
        "readmes.json",
        "readme-contents.json",
        "readme-metadata.json",
        "readme-ranking.json"
    ];

    [Fact]
    public async Task ProducesAtomicTraceableClaimForReadableDocument()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var localContextPath = await WriteLocalContextAsync(
                root,
                Readable("docs/component/README.md", "content that must not be interpreted"));
            var outputPath = await Cli.Inference.InferencePipeline.ExecuteAsync(
                localContextPath,
                CancellationToken.None);

            using var document = JsonDocument.Parse(await File.ReadAllBytesAsync(outputPath));
            var execution = document.RootElement;
            Assert.Equal(NewRuleSetExecutionId, execution.GetProperty("execution_id").GetString());
            Assert.Equal(PackId, execution.GetProperty("input_pack_id").GetString());
            Assert.Equal(
                "capability-002-document-availability-rules-v1",
                execution.GetProperty("rule_set_id").GetString());
            Assert.Equal("claims_produced", execution.GetProperty("status").GetString());
            Assert.Equal(ExpectedStages, ReadStages(execution));

            var counts = execution.GetProperty("counts");
            Assert.Equal(1, counts.GetProperty("evidence").GetInt32());
            Assert.Equal(1, counts.GetProperty("claims").GetInt32());
            Assert.Equal(0, counts.GetProperty("hypotheses").GetInt32());
            Assert.Equal(0, counts.GetProperty("findings").GetInt32());
            Assert.Equal(0, counts.GetProperty("abstentions").GetInt32());
            Assert.Equal(0, counts.GetProperty("discarded_candidates").GetInt32());

            var evidence = Assert.Single(execution.GetProperty("evidence").EnumerateArray());
            var claim = Assert.Single(execution.GetProperty("claims").EnumerateArray());
            var evidenceId = evidence.GetProperty("evidence_id").GetString();
            Assert.Equal(64, evidenceId!.Length);
            Assert.Equal(PackId, evidence.GetProperty("input_pack_id").GetString());
            Assert.Equal("docs/component/README.md", evidence.GetProperty("document_path").GetString());
            Assert.Equal("available", evidence.GetProperty("availability_status").GetString());
            Assert.Equal("document", evidence.GetProperty("scope").GetProperty("type").GetString());
            Assert.Equal(
                "docs/component/README.md",
                evidence.GetProperty("scope").GetProperty("document_path").GetString());
            Assert.Equal(
                "local-context-v1",
                evidence.GetProperty("provenance").GetProperty("contract").GetString());

            Assert.Equal(
                "A context document is available at 'docs/component/README.md'.",
                claim.GetProperty("statement").GetString());
            Assert.Equal(evidenceId, Assert.Single(claim.GetProperty("evidence_ids").EnumerateArray()).GetString());
            Assert.Equal("document-availability-claim", claim.GetProperty("rule_id").GetString());
            Assert.Equal(1, claim.GetProperty("rule_version").GetInt32());
            Assert.Equal(evidence.GetProperty("scope").GetRawText(), claim.GetProperty("scope").GetRawText());
            Assert.Equal("valid", claim.GetProperty("status").GetString());
            Assert.Equal("strong", claim.GetProperty("confidence").GetProperty("level").GetString());
            Assert.Equal(
                [
                    "document_explicitly_present",
                    "admitted_by_input_boundary",
                    "rule_describes_availability_only"
                ],
                ReadStrings(claim.GetProperty("confidence").GetProperty("basis")));
            Assert.Equal(
                [
                    "content_not_evaluated",
                    "authority_not_evaluated",
                    "freshness_not_evaluated",
                    "semantic_relevance_not_evaluated"
                ],
                ReadStrings(claim.GetProperty("confidence").GetProperty("limitations")));
            var dimensions = claim.GetProperty("confidence").GetProperty("dimensions");
            Assert.All(dimensions.EnumerateObject(), dimension => Assert.Equal("strong", dimension.Value.GetString()));
            Assert.Equal(
                ["directness", "coverage", "consistency", "traceability", "rule_constraints"],
                dimensions.EnumerateObject().Select(dimension => dimension.Name).ToArray());
            Assert.Empty(claim.GetProperty("uncertainty").EnumerateArray());
            Assert.Empty(execution.GetProperty("discarded_candidates").EnumerateArray());
            Assert.False(execution.TryGetProperty("hypotheses", out _));
            Assert.False(execution.TryGetProperty("findings", out _));
            Assert.DoesNotContain("content that must not be interpreted", await File.ReadAllTextAsync(outputPath));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task ProducesIndependentClaimsInOrdinalPathOrder()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var localContextPath = await WriteLocalContextAsync(
                root,
                Readable("zeta/README.md", "zeta"),
                Readable("alpha/README.md", "alpha"));

            var outputPath = await Cli.Inference.InferencePipeline.ExecuteAsync(
                localContextPath,
                CancellationToken.None);
            using var document = JsonDocument.Parse(await File.ReadAllBytesAsync(outputPath));
            var execution = document.RootElement;
            var evidence = execution.GetProperty("evidence").EnumerateArray().ToArray();
            var claims = execution.GetProperty("claims").EnumerateArray().ToArray();

            Assert.Equal(["alpha/README.md", "zeta/README.md"],
                evidence.Select(item => item.GetProperty("document_path").GetString()!).ToArray());
            Assert.Equal(2, claims.Length);
            Assert.NotEqual(claims[0].GetProperty("claim_id").GetString(), claims[1].GetProperty("claim_id").GetString());
            Assert.Equal(
                evidence[0].GetProperty("evidence_id").GetString(),
                claims[0].GetProperty("evidence_ids")[0].GetString());
            Assert.Equal(
                evidence[1].GetProperty("evidence_id").GetString(),
                claims[1].GetProperty("evidence_ids")[0].GetString());
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task DiscardsUnreadableDocumentWithoutDegradingValidClaims()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var localContextPath = await WriteLocalContextAsync(
                root,
                Unreadable("broken/README.md"),
                Readable("valid/README.md", "valid"));

            var outputPath = await Cli.Inference.InferencePipeline.ExecuteAsync(
                localContextPath,
                CancellationToken.None);
            using var document = JsonDocument.Parse(await File.ReadAllBytesAsync(outputPath));
            var execution = document.RootElement;

            Assert.Equal("claims_produced", execution.GetProperty("status").GetString());
            Assert.Single(execution.GetProperty("evidence").EnumerateArray());
            Assert.Single(execution.GetProperty("claims").EnumerateArray());
            var discard = Assert.Single(execution.GetProperty("discarded_candidates").EnumerateArray());
            Assert.Equal("broken/README.md", discard.GetProperty("document_path").GetString());
            Assert.Equal("document_not_readable", discard.GetProperty("reason").GetString());
            Assert.Equal("discarded", discard.GetProperty("status").GetString());
            Assert.Equal(1, execution.GetProperty("counts").GetProperty("discarded_candidates").GetInt32());
            Assert.DoesNotContain(
                "broken/README.md",
                execution.GetProperty("claims").GetRawText(),
                StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task ProducesExplicitNoClaimsStateWhenEveryCandidateIsDiscarded()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var localContextPath = await WriteLocalContextAsync(root, Unreadable("README.md"));
            var outputPath = await Cli.Inference.InferencePipeline.ExecuteAsync(
                localContextPath,
                CancellationToken.None);
            using var document = JsonDocument.Parse(await File.ReadAllBytesAsync(outputPath));
            var execution = document.RootElement;

            Assert.Equal("no_claims", execution.GetProperty("status").GetString());
            Assert.Empty(execution.GetProperty("evidence").EnumerateArray());
            Assert.Empty(execution.GetProperty("claims").EnumerateArray());
            Assert.Single(execution.GetProperty("discarded_candidates").EnumerateArray());
            Assert.Equal(0, execution.GetProperty("counts").GetProperty("abstentions").GetInt32());
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task IdentitiesAndBytesAreDeterministicAndRuleSetChangesExecutionIdentity()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var localContextPath = await WriteLocalContextAsync(root, Readable("README.md", "same"));
            var outputPath = await Cli.Inference.InferencePipeline.ExecuteAsync(
                localContextPath,
                CancellationToken.None);
            var first = await File.ReadAllBytesAsync(outputPath);
            using var firstDocument = JsonDocument.Parse(first);
            var firstEvidenceId = firstDocument.RootElement.GetProperty("evidence")[0]
                .GetProperty("evidence_id").GetString();
            var firstClaimId = firstDocument.RootElement.GetProperty("claims")[0]
                .GetProperty("claim_id").GetString();

            await Cli.Inference.InferencePipeline.ExecuteAsync(localContextPath, CancellationToken.None);
            var second = await File.ReadAllBytesAsync(outputPath);
            using var secondDocument = JsonDocument.Parse(second);

            Assert.Equal(first, second);
            Assert.Equal(firstEvidenceId, secondDocument.RootElement.GetProperty("evidence")[0]
                .GetProperty("evidence_id").GetString());
            Assert.Equal(firstClaimId, secondDocument.RootElement.GetProperty("claims")[0]
                .GetProperty("claim_id").GetString());
            Assert.NotEqual(EmptyRuleSetExecutionId, NewRuleSetExecutionId);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task LeavesLocalContextAndEveryRetrievalArtifactByteUnchanged()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var localContextPath = await WriteLocalContextAsync(root, Readable("README.md", "same"));
            var inputPaths = RetrievalArtifactNames
                .Select(name => Path.Combine(root, name))
                .Append(localContextPath)
                .ToArray();
            foreach (var path in inputPaths[..^1])
            {
                await File.WriteAllTextAsync(path, $"unchanged:{Path.GetFileName(path)}\n");
            }

            var before = await Task.WhenAll(inputPaths.Select(path => File.ReadAllBytesAsync(path)));
            await Cli.Inference.InferencePipeline.ExecuteAsync(localContextPath, CancellationToken.None);
            var after = await Task.WhenAll(inputPaths.Select(path => File.ReadAllBytesAsync(path)));

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
            var localContextPath = await WriteLocalContextAsync(
                root,
                Readable("repository-does-not-exist/README.md", "available only in local context"));

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
    public async Task RejectsAmbiguousDocumentIdentityWithoutLeakingContent()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var localContextPath = await WriteLocalContextAsync(
                root,
                Readable("README.md", "sensitive-one"),
                Readable("README.md", "sensitive-two"));

            var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
                Cli.Inference.InferencePipeline.ExecuteAsync(localContextPath, CancellationToken.None));

            Assert.Equal("The local context contains ambiguous document identities.", exception.Message);
            Assert.DoesNotContain("sensitive", exception.ToString(), StringComparison.Ordinal);
            Assert.DoesNotContain(root, exception.ToString(), StringComparison.Ordinal);
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
            var localContextPath = await WriteLocalContextAsync(root, Readable("README.md", "content"));
            Directory.CreateDirectory(Path.Combine(root, "inference-execution.json.tmp"));

            var exception = await Assert.ThrowsAnyAsync<IOException>(() =>
                Cli.Inference.InferencePipeline.ExecuteAsync(localContextPath, CancellationToken.None));

            Assert.Equal("The inference execution could not be written.", exception.Message);
            Assert.DoesNotContain(root, exception.ToString(), StringComparison.Ordinal);
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
            var localContextPath = await WriteLocalContextAsync(root, Readable("README.md", "content"));
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
    private const string EmptyRuleSetExecutionId = "af2f3c5874d3b95005d707f78e2c35188ea41c3a41ae4275a632f225fce8e149";
    private const string NewRuleSetExecutionId = "1969c9398663134ffa16150a95895cfdde875fa3a49bb9586241a07c9bf95d72";

    private static string[] ReadStages(JsonElement execution) =>
        execution.GetProperty("stages")
            .EnumerateArray()
            .Select(stage => $"{stage.GetProperty("name").GetString()}:{stage.GetProperty("status").GetString()}")
            .ToArray();

    private static string[] ReadStrings(JsonElement array) =>
        array.EnumerateArray().Select(item => item.GetString()!).ToArray();

    private static async Task<string> WriteLocalContextAsync(
        string root,
        params TestDocument[] documents)
    {
        var documentNodes = documents.Select(document =>
        {
            var node = new JsonObject
            {
                ["path"] = document.Path,
                ["score"] = 100,
                ["reason"] = "same_directory",
                ["name"] = "Example",
                ["purpose"] = "Provides an example.",
                ["content"] = document.Content,
                ["status"] = "extracted",
                ["evidence"] = new JsonArray()
            };
            if (document.Error is not null)
            {
                node["error"] = document.Error;
            }

            return node;
        }).ToArray();
        var localContext = new JsonObject
        {
            ["repository"] = "example/widgets",
            ["pull_request"] = 123,
            ["pack_id"] = PackId,
            ["documents"] = new JsonArray(documentNodes)
        };
        var path = Path.Combine(root, "local-context.json");
        await File.WriteAllTextAsync(path, localContext.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true
        }) + "\n");
        return path;
    }

    private static TestDocument Readable(string path, string content) => new(path, content, null);

    private static TestDocument Unreadable(string path) =>
        new(path, null, "The document could not be read.");

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"eip-inference-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private sealed record TestDocument(string Path, string? Content, string? Error);
}
