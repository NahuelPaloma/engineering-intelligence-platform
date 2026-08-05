using System.Collections.Immutable;

namespace Eip.Tests;

public sealed class ReasoningControlsTests
{
    [Fact]
    public void ConfidenceNeverExceedsSupportOrIncreasesByRepetition()
    {
        var strong = Confidence("strong");
        var moderate = Confidence("moderate");

        Assert.Equal("strong", Cli.Inference.ConfidencePropagation.Derive(strong, []).Level);
        Assert.Equal("moderate", Cli.Inference.ConfidencePropagation.Derive(strong, [moderate]).Level);
        Assert.Equal("moderate", Cli.Inference.ConfidencePropagation.Derive(moderate, [moderate, moderate]).Level);
    }

    [Fact]
    public void MaterialUncertaintyAndContradictionReduceConfidence()
    {
        Assert.Equal(
            "moderate",
            Cli.Inference.ConfidencePropagation.Derive(Confidence("strong"), [], materialUncertainty: true).Level);
        Assert.Equal(
            "weak",
            Cli.Inference.ConfidencePropagation.Derive(Confidence("moderate"), [], contradiction: true).Level);
        var insufficient = Cli.Inference.ConfidencePropagation.Derive(
            Confidence("weak"),
            [],
            contradiction: true);
        Assert.Equal("insufficient", insufficient.Level);
        Assert.False(Cli.Inference.ConfidencePropagation.AllowsDerivation(insufficient));
    }

    [Fact]
    public void UncertaintyIsInheritedWithoutDuplication()
    {
        var merged = Cli.Inference.UncertaintyPropagation.Merge(
            ["own", "shared"],
            ["inherited", "shared"]);

        Assert.Equal(["own", "shared", "inherited"], merged.ToArray());
    }

    [Fact]
    public void ComparableIncompatiblePositionsProduceDeterministicContradiction()
    {
        var scope = Scope("docs/component/README.md");
        var first = Cli.Inference.ContradictionProcessing.CreateCandidate(
            "claim-b", "claim-a", "position-b", "position-a",
            ["evidence-b", "evidence-a"], scope, scope);
        var second = Cli.Inference.ContradictionProcessing.CreateCandidate(
            "claim-b", "claim-a", "position-b", "position-a",
            ["evidence-b", "evidence-a"], scope, scope);
        var decision = Cli.Inference.ContradictionValidation.Decide(
            first,
            new HashSet<string>(["claim-a", "claim-b"], StringComparer.Ordinal),
            new HashSet<string>(["evidence-a", "evidence-b"], StringComparer.Ordinal));

        Assert.Equal("valid", decision.Status);
        Assert.Equal(first.Contradiction!.ContradictionId, second.Contradiction!.ContradictionId);
        Assert.Equal(["claim-a", "claim-b"], decision.Contradiction!.UnitIds);
        Assert.Equal(["evidence-a", "evidence-b"], decision.Contradiction.EvidenceIds);
        Assert.Equal(["position-a", "position-b"], decision.Contradiction.Positions);
        Assert.Equal("contradiction", decision.Contradiction.GeneratedUncertainty.Origin);
        Assert.Equal("confidence_reduced", decision.Contradiction.ConfidenceEffect);
    }

    [Fact]
    public void IncompatibleScopesDiscardContradictionWithoutChoosingPrecedence()
    {
        var candidate = Cli.Inference.ContradictionProcessing.CreateCandidate(
            "claim-a", "claim-b", "position-a", "position-b",
            ["evidence-a", "evidence-b"], Scope("a/README.md"), Scope("b/README.md"));
        var decision = Cli.Inference.ContradictionValidation.Decide(
            candidate,
            new HashSet<string>(["claim-a", "claim-b"], StringComparer.Ordinal),
            new HashSet<string>(["evidence-a", "evidence-b"], StringComparer.Ordinal));

        Assert.Equal("discarded", decision.Status);
        Assert.Equal("scope_not_comparable", decision.Discard!.Reason);
        Assert.Null(decision.Contradiction);
    }

    [Theory]
    [InlineData("local", 1)]
    [InlineData("partial", 1)]
    [InlineData("total", 0)]
    public void ValidationAuthorizesLocalPartialAndTotalAbstention(string type, int remainingScopeCount)
    {
        var remaining = remainingScopeCount == 0
            ? ImmutableArray<Cli.Inference.DocumentScope>.Empty
            : ImmutableArray.Create(Scope("valid/README.md"));
        var first = Cli.Inference.AbstentionProcessing.CreateCandidate(
            type,
            "finding",
            Scope("blocked/README.md"),
            "required_evidence_missing",
            ["evidence-a"],
            "Additional governed evidence is required.",
            ["uncertainty-a"],
            [],
            remaining);
        var second = Cli.Inference.AbstentionProcessing.CreateCandidate(
            type,
            "finding",
            Scope("blocked/README.md"),
            "required_evidence_missing",
            ["evidence-a"],
            "Additional governed evidence is required.",
            ["uncertainty-a"],
            [],
            remaining);
        var decision = Cli.Inference.AbstentionValidation.Decide(first);

        Assert.Equal("abstained", decision.Status);
        Assert.Equal(type, decision.Abstention!.Type);
        Assert.Equal(first.Abstention!.AbstentionId, second.Abstention!.AbstentionId);
        Assert.Equal(remainingScopeCount, decision.Abstention.RemainingValidScope.Count);
        Assert.Equal(["evidence-a"], decision.Abstention.AvailableEvidenceIds);
    }

    [Fact]
    public async Task RoutineDiscardDoesNotBecomeAbstentionAndCoverageRemainsExplicit()
    {
        var root = Path.Combine(Path.GetTempPath(), $"eip-controls-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var localContextPath = Path.Combine(root, "local-context.json");
            await File.WriteAllTextAsync(localContextPath, $$"""
                {
                  "repository": "example/widgets",
                  "pull_request": 123,
                  "pack_id": "f166a136da542904223312b67fbb42ba5d1436fd29a399f7956d02ef50525bdd",
                  "documents": [
                    {
                      "path": "broken/README.md", "score": 100, "reason": "same_directory",
                      "name": null, "purpose": null, "content": null,
                      "error": "The document could not be read.", "status": "insufficient", "evidence": []
                    }
                  ]
                }
                """);

            var output = await Cli.Inference.InferencePipeline.ExecuteAsync(localContextPath, CancellationToken.None);
            var text = await File.ReadAllTextAsync(output);

            Assert.Contains("\"coverage_status\": \"none\"", text, StringComparison.Ordinal);
            Assert.Contains("\"abstentions\": []", text, StringComparison.Ordinal);
            Assert.True(File.Exists(Path.Combine(root, "inference-report.json")));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private static Cli.Inference.ClaimConfidence Confidence(string level) =>
        new(
            level,
            ImmutableArray.Create($"{level}_basis"),
            ImmutableArray.Create($"{level}_limitation"),
            new Cli.Inference.ConfidenceDimensions(level, level, level, level, level));

    private static Cli.Inference.DocumentScope Scope(string path) => new("document", path);
}
