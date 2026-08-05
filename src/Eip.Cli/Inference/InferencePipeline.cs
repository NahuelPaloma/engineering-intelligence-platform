using System.Text.Json.Serialization;

namespace Eip.Cli.Inference;

internal static class InferencePipeline
{
    private static readonly IReadOnlyList<InferenceStage> Stages =
    [
        new("input_boundary", "completed"),
        new("claim_processing", "completed"),
        new("hypothesis_processing", "completed"),
        new("finding_processing", "completed"),
        new("reasoning_controls", "completed"),
        new("report_builder", "completed")
    ];

    public static async Task<string> ExecuteAsync(
        string localContextPath,
        CancellationToken cancellationToken) =>
        await ExecuteAsync(
            localContextPath,
            ExecutionProfileRegistry.Capability002ProfileId,
            cancellationToken);

    public static async Task<string> ExecuteAsync(
        string localContextPath,
        string profileId,
        CancellationToken cancellationToken)
    {
        var input = await InputBoundary.ReadAsync(localContextPath, cancellationToken);
        var profile = InferenceEngineExtensionBoundary.ResolveProfile(profileId);
        var result = InferenceEngineExtensionBoundary.Execute(profile, input);
        var uncertaintySummary = UncertaintyPropagation.Summarize(
            result.Claims,
            result.Hypotheses,
            result.Findings);
        var confidenceSummary = CreateConfidenceSummary(
            result.Claims,
            result.Hypotheses,
            result.Findings);

        var execution = new InferenceExecution(
            ExecutionIdentity.Create(input.CanonicalContextIdentity, profile),
            input.PackId,
            profile.RuleSetId,
            DetermineStatus(result.Findings.Count, result.Abstentions),
            Stages,
            new InferenceCounts(
                result.Evidence.Count,
                result.Claims.Count,
                result.Hypotheses.Count,
                result.Findings.Count,
                result.Abstentions.Count,
                result.DiscardedCandidates.Count,
                result.DiscardedHypotheses.Count,
                result.DiscardedFindings.Count,
                result.Contradictions.Count,
                result.DiscardedContradictions.Count,
                result.DiscardedAbstentions.Count),
            result.Evidence,
            result.Claims,
            result.DiscardedCandidates,
            result.Hypotheses,
            result.DiscardedHypotheses,
            result.Findings,
            result.DiscardedFindings,
            result.Contradictions,
            result.Abstentions,
            uncertaintySummary,
            confidenceSummary,
            result.DiscardedContradictions,
            result.DiscardedAbstentions,
            result.Coverage,
            "reasoning_controls_completed",
            null);

        var buildResult = InferenceReportBuilder.Build(execution);
        var validation = InferenceReportValidation.Decide(buildResult);
        var reportToPublish = validation.PublicationAuthorized ? validation.Report : null;

        execution = execution with
        {
            ExecutionCompletenessState = reportToPublish is not null
                ? "inference_report_published"
                : "inference_report_not_published",
            PublishedReport = reportToPublish is null
                ? null
                : new PublishedReportReference(
                    reportToPublish.ReportId,
                    "inference-report.json",
                    reportToPublish.Status)
        };

        return await InferenceExecutionWriter.WriteAllAsync(
            localContextPath,
            execution,
            reportToPublish,
            cancellationToken);
    }

    private static string DetermineStatus(int findingCount, IReadOnlyList<AbstentionUnit> abstentions) =>
        abstentions.Any(item => item.Type == "total") ? "reasoning_totally_abstained"
        : abstentions.Any(item => item.Type == "partial") ? "reasoning_partially_abstained"
        : findingCount >= 0 ? "reasoning_completed"
        : throw new InvalidDataException("The reasoning state is invalid.");

    private static ConfidenceSummary CreateConfidenceSummary(
        IReadOnlyList<ClaimUnit> claims,
        IReadOnlyList<HypothesisUnit> hypotheses,
        IReadOnlyList<FindingUnit> findings)
    {
        var levels = claims.Select(item => item.Confidence.Level)
            .Concat(hypotheses.Select(item => item.Confidence.Level))
            .Concat(findings.Select(item => item.Confidence.Level))
            .ToArray();
        return new ConfidenceSummary(
            levels.Count(item => item == "strong"),
            levels.Count(item => item == "moderate"),
            levels.Count(item => item == "weak"),
            levels.Count(item => item == "insufficient"));
    }
}

internal sealed record InferenceExecution(
    [property: JsonPropertyName("execution_id")] string ExecutionId,
    [property: JsonPropertyName("input_pack_id")] string InputPackId,
    [property: JsonPropertyName("rule_set_id")] string RuleSetId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("stages")] IReadOnlyList<InferenceStage> Stages,
    [property: JsonPropertyName("counts")] InferenceCounts Counts,
    [property: JsonPropertyName("evidence")] IReadOnlyList<EvidenceUnit> Evidence,
    [property: JsonPropertyName("claims")] IReadOnlyList<ClaimUnit> Claims,
    [property: JsonPropertyName("discarded_candidates")] IReadOnlyList<DiscardedCandidate> DiscardedCandidates,
    [property: JsonPropertyName("hypotheses")] IReadOnlyList<HypothesisUnit> Hypotheses,
    [property: JsonPropertyName("discarded_hypotheses")] IReadOnlyList<DiscardedHypothesis> DiscardedHypotheses,
    [property: JsonPropertyName("findings")] IReadOnlyList<FindingUnit> Findings,
    [property: JsonPropertyName("discarded_findings")] IReadOnlyList<DiscardedFinding> DiscardedFindings,
    [property: JsonPropertyName("contradictions")] IReadOnlyList<ContradictionUnit> Contradictions,
    [property: JsonPropertyName("abstentions")] IReadOnlyList<AbstentionUnit> Abstentions,
    [property: JsonPropertyName("uncertainty_summary")] IReadOnlyList<UncertaintySummaryItem> UncertaintySummary,
    [property: JsonPropertyName("confidence_summary")] ConfidenceSummary ConfidenceSummary,
    [property: JsonPropertyName("discarded_contradictions")] IReadOnlyList<DiscardedReason> DiscardedContradictions,
    [property: JsonPropertyName("discarded_abstentions")] IReadOnlyList<DiscardedReason> DiscardedAbstentions,
    [property: JsonPropertyName("coverage")] CoverageSummary Coverage,
    [property: JsonPropertyName("execution_completeness_state")] string ExecutionCompletenessState,
    [property: JsonPropertyName("published_report")] PublishedReportReference? PublishedReport);

internal sealed record InferenceStage(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("status")] string Status);

internal sealed record InferenceCounts(
    [property: JsonPropertyName("evidence")] int Evidence,
    [property: JsonPropertyName("claims")] int Claims,
    [property: JsonPropertyName("hypotheses")] int Hypotheses,
    [property: JsonPropertyName("findings")] int Findings,
    [property: JsonPropertyName("abstentions")] int Abstentions,
    [property: JsonPropertyName("discarded_candidates")] int DiscardedCandidates,
    [property: JsonPropertyName("discarded_hypotheses")] int DiscardedHypotheses,
    [property: JsonPropertyName("discarded_findings")] int DiscardedFindings,
    [property: JsonPropertyName("contradictions")] int Contradictions,
    [property: JsonPropertyName("discarded_contradictions")] int DiscardedContradictions,
    [property: JsonPropertyName("discarded_abstentions")] int DiscardedAbstentions);
