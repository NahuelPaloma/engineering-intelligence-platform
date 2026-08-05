using System.Collections.Immutable;
using System.Text.Json;

namespace Eip.Tests;

public sealed class InferenceReportTests
{
    [Fact]
    public void BuilderIsPureDeterministicAndDoesNotModifyInput()
    {
        var execution = CreateExecution(FullCoverage());
        var before = JsonSerializer.SerializeToUtf8Bytes(execution);

        var first = Cli.Inference.InferenceReportBuilder.Build(execution);
        var second = Cli.Inference.InferenceReportBuilder.Build(execution);
        var after = JsonSerializer.SerializeToUtf8Bytes(execution);

        Assert.Equal("candidate_built", first.Status);
        Assert.Equal(before, after);
        Assert.Equal(first.Candidate, second.Candidate);
        Assert.Equal(first.Candidate!.ReportId, second.Candidate!.ReportId);
        Assert.Equal(execution.Coverage, first.Candidate.Coverage);
        Assert.Equal(execution.ConfidenceSummary, first.Candidate.ConfidenceSummary);
        Assert.Equal(execution.UncertaintySummary, first.Candidate.UncertaintySummary);
    }

    [Fact]
    public void ValidationAloneAuthorizesCompleteReport()
    {
        var result = Cli.Inference.InferenceReportValidation.Decide(
            Cli.Inference.InferenceReportBuilder.Build(CreateExecution(FullCoverage())));

        Assert.Equal("complete", result.Status);
        Assert.True(result.PublicationAuthorized);
        Assert.Equal("complete", result.Report!.Status);
    }

    [Fact]
    public void ValidationAloneAuthorizesDocumentedIncompleteReport()
    {
        var scope = Scope("missing/README.md");
        var coverage = new Cli.Inference.CoverageSummary(
            [scope],
            ImmutableArray<Cli.Inference.DocumentScope>.Empty,
            [scope],
            "none");

        var result = Cli.Inference.InferenceReportValidation.Decide(
            Cli.Inference.InferenceReportBuilder.Build(CreateExecution(coverage)));

        Assert.Equal("incomplete", result.Status);
        Assert.True(result.PublicationAuthorized);
    }

    [Fact]
    public void ValidationRejectsContractuallyInvalidCandidate()
    {
        var built = Cli.Inference.InferenceReportBuilder.Build(CreateExecution(FullCoverage()));
        var invalid = built with
        {
            Candidate = built.Candidate! with { StatusCandidate = "invalid_state" }
        };

        var result = Cli.Inference.InferenceReportValidation.Decide(invalid);

        Assert.Equal("invalid", result.Status);
        Assert.False(result.PublicationAuthorized);
        Assert.Null(result.Report);
    }

    [Fact]
    public void ConstructionFailureIsNotInvalidAndCannotBePublished()
    {
        var execution = CreateExecution(FullCoverage()) with { ExecutionId = string.Empty };
        var built = Cli.Inference.InferenceReportBuilder.Build(execution);
        var result = Cli.Inference.InferenceReportValidation.Decide(built);

        Assert.Equal("construction_failed", built.Status);
        Assert.Null(built.Candidate);
        Assert.Equal("construction_failed", result.Status);
        Assert.False(result.PublicationAuthorized);
        Assert.Null(result.Report);
    }

    [Fact]
    public void BuilderPreservesControlledContradictionAndAbstention()
    {
        var scope = Scope("docs/component/README.md");
        var contradictionCandidate = Cli.Inference.ContradictionProcessing.CreateCandidate(
            "claim-a", "claim-b", "position-a", "position-b",
            ["evidence-a", "evidence-b"], scope, scope);
        var contradiction = Cli.Inference.ContradictionValidation.Decide(
            contradictionCandidate,
            new HashSet<string>(["claim-a", "claim-b"], StringComparer.Ordinal),
            new HashSet<string>(["evidence-a", "evidence-b"], StringComparer.Ordinal)).Contradiction!;
        var abstentionCandidate = Cli.Inference.AbstentionProcessing.CreateCandidate(
            "local", "finding", scope, "contradiction_prevents_finding",
            ["evidence-a", "evidence-b"], "Evidence establishing precedence is missing.",
            [contradiction.GeneratedUncertainty.UncertaintyId],
            [contradiction.ContradictionId],
            ImmutableArray<Cli.Inference.DocumentScope>.Empty);
        var abstention = Cli.Inference.AbstentionValidation.Decide(abstentionCandidate).Abstention!;
        var execution = CreateExecution(FullCoverage()) with
        {
            Contradictions = [contradiction],
            Abstentions = [abstention],
            UncertaintySummary = [contradiction.GeneratedUncertainty]
        };

        var candidate = Cli.Inference.InferenceReportBuilder.Build(execution).Candidate!;

        Assert.Equal([contradiction], candidate.Contradictions);
        Assert.Equal([abstention], candidate.Abstentions);
        Assert.Equal([contradiction.GeneratedUncertainty], candidate.UncertaintySummary);
    }

    [Fact]
    public async Task WriterAlwaysPublishesExecutionAndOnlyPublishesReportWhenAuthorized()
    {
        var root = Path.Combine(Path.GetTempPath(), $"eip-report-writer-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var localContextPath = Path.Combine(root, "local-context.json");
            await File.WriteAllTextAsync(localContextPath, "{}");
            var execution = CreateExecution(FullCoverage());

            var outputPath = await Cli.Inference.InferenceExecutionWriter.WriteAllAsync(
                localContextPath,
                execution,
                report: null,
                CancellationToken.None);

            Assert.True(File.Exists(outputPath));
            Assert.False(File.Exists(Path.Combine(root, "inference-report.json")));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private static Cli.Inference.InferenceExecution CreateExecution(
        Cli.Inference.CoverageSummary coverage) =>
        new(
            "execution-id",
            "pack-id",
            "rule-set-id",
            "reasoning_completed",
            ImmutableArray<Cli.Inference.InferenceStage>.Empty,
            new Cli.Inference.InferenceCounts(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0),
            ImmutableArray<Cli.Inference.EvidenceUnit>.Empty,
            ImmutableArray<Cli.Inference.ClaimUnit>.Empty,
            ImmutableArray<Cli.Inference.DiscardedCandidate>.Empty,
            ImmutableArray<Cli.Inference.HypothesisUnit>.Empty,
            ImmutableArray<Cli.Inference.DiscardedHypothesis>.Empty,
            ImmutableArray<Cli.Inference.FindingUnit>.Empty,
            ImmutableArray<Cli.Inference.DiscardedFinding>.Empty,
            ImmutableArray<Cli.Inference.ContradictionUnit>.Empty,
            ImmutableArray<Cli.Inference.AbstentionUnit>.Empty,
            ImmutableArray<Cli.Inference.UncertaintySummaryItem>.Empty,
            new Cli.Inference.ConfidenceSummary(0, 0, 0, 0),
            ImmutableArray<Cli.Inference.DiscardedReason>.Empty,
            ImmutableArray<Cli.Inference.DiscardedReason>.Empty,
            coverage,
            "reasoning_controls_completed",
            null);

    private static Cli.Inference.CoverageSummary FullCoverage() =>
        new(
            ImmutableArray<Cli.Inference.DocumentScope>.Empty,
            ImmutableArray<Cli.Inference.DocumentScope>.Empty,
            ImmutableArray<Cli.Inference.DocumentScope>.Empty,
            "full");

    private static Cli.Inference.DocumentScope Scope(string path) => new("document", path);
}
