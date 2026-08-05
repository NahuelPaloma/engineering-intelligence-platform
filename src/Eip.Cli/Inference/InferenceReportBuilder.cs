using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace Eip.Cli.Inference;

internal static class InferenceReportBuilder
{
    internal const string GenerationVersion = "1";

    public static ReportBuildResult Build(InferenceExecution execution)
    {
        if (string.IsNullOrWhiteSpace(execution.ExecutionId)
            || string.IsNullOrWhiteSpace(execution.RuleSetId))
        {
            return ReportBuildResult.Failed("execution_identity_invalid");
        }

        var evidenceIds = execution.Evidence.Select(item => item.EvidenceId)
            .ToHashSet(StringComparer.Ordinal);
        var claimIds = execution.Claims.Select(item => item.ClaimId)
            .ToHashSet(StringComparer.Ordinal);
        var hypothesisIds = execution.Hypotheses.Select(item => item.HypothesisId)
            .ToHashSet(StringComparer.Ordinal);

        if (execution.Findings.Any(item => item.Status != "valid")
            || execution.Findings.SelectMany(item => item.HypothesisIds).Any(id => !hypothesisIds.Contains(id))
            || execution.Findings.SelectMany(item => item.EvidenceIds).Any(id => !evidenceIds.Contains(id))
            || execution.Hypotheses.SelectMany(item => item.ClaimIds).Any(id => !claimIds.Contains(id))
            || execution.Claims.SelectMany(item => item.EvidenceIds).Any(id => !evidenceIds.Contains(id)))
        {
            return ReportBuildResult.Failed("traceability_not_constructible");
        }

        var traceability = new ReportTraceability(
            execution.Evidence,
            execution.Claims,
            execution.Hypotheses);
        var statistics = new ReportStatistics(
            execution.Evidence.Count,
            execution.Claims.Count,
            execution.Hypotheses.Count,
            execution.Findings.Count,
            execution.Contradictions.Count,
            execution.Abstentions.Count);
        var identity = string.Join(
            '\n',
            execution.ExecutionId,
            execution.RuleSetId,
            GenerationVersion,
            execution.Coverage.CoverageStatus,
            string.Join('|', execution.Findings.Select(item => item.FindingId)),
            string.Join('|', execution.UncertaintySummary.Select(item => item.UncertaintyId)),
            string.Join('|', execution.Contradictions.Select(item => item.ContradictionId)),
            string.Join('|', execution.Abstentions.Select(item => item.AbstentionId)));
        var candidate = new InferenceReportCandidate(
            Hash(identity),
            execution.ExecutionId,
            execution.RuleSetId,
            GenerationVersion,
            "ready_for_validation",
            execution.Coverage,
            execution.Findings,
            execution.ConfidenceSummary,
            execution.UncertaintySummary,
            execution.Contradictions,
            execution.Abstentions,
            traceability,
            statistics);
        return ReportBuildResult.Succeeded(candidate);
    }

    private static string Hash(string value) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}

internal static class InferenceReportValidation
{
    public static ReportValidationResult Decide(ReportBuildResult buildResult)
    {
        if (buildResult.Status == "construction_failed")
        {
            return ReportValidationResult.ConstructionFailed(buildResult.FailureReason!);
        }

        var candidate = buildResult.Candidate!;
        if (candidate.StatusCandidate != "ready_for_validation"
            || candidate.GenerationVersion != InferenceReportBuilder.GenerationVersion
            || !TraceabilityIsComplete(candidate))
        {
            return ReportValidationResult.Invalid("report_contract_violation");
        }

        var status = candidate.Coverage.CoverageStatus == "full"
            ? "complete"
            : candidate.Coverage.UncoveredScope.Count > 0
                ? "incomplete"
                : "invalid";
        if (status == "invalid")
        {
            return ReportValidationResult.Invalid("coverage_state_invalid");
        }

        return ReportValidationResult.Published(new InferenceReport(
            candidate.ReportId,
            candidate.ExecutionId,
            candidate.RuleSetId,
            candidate.GenerationVersion,
            status,
            candidate.Coverage,
            candidate.Findings,
            candidate.ConfidenceSummary,
            candidate.UncertaintySummary,
            candidate.Contradictions,
            candidate.Abstentions,
            candidate.Traceability,
            candidate.Statistics));
    }

    private static bool TraceabilityIsComplete(InferenceReportCandidate candidate)
    {
        var evidenceIds = candidate.Traceability.Evidence.Select(item => item.EvidenceId)
            .ToHashSet(StringComparer.Ordinal);
        var claimIds = candidate.Traceability.Claims.Select(item => item.ClaimId)
            .ToHashSet(StringComparer.Ordinal);
        var hypothesisIds = candidate.Traceability.Hypotheses.Select(item => item.HypothesisId)
            .ToHashSet(StringComparer.Ordinal);
        return candidate.Findings.All(item => item.Status == "valid")
            && candidate.Findings.SelectMany(item => item.HypothesisIds).All(hypothesisIds.Contains)
            && candidate.Findings.SelectMany(item => item.EvidenceIds).All(evidenceIds.Contains)
            && candidate.Traceability.Hypotheses.SelectMany(item => item.ClaimIds).All(claimIds.Contains)
            && candidate.Traceability.Claims.SelectMany(item => item.EvidenceIds).All(evidenceIds.Contains);
    }
}

internal sealed record ReportBuildResult(string Status, string? FailureReason, InferenceReportCandidate? Candidate)
{
    public static ReportBuildResult Failed(string reason) => new("construction_failed", reason, null);
    public static ReportBuildResult Succeeded(InferenceReportCandidate candidate) => new("candidate_built", null, candidate);
}

internal sealed record ReportValidationResult(
    string Status,
    bool PublicationAuthorized,
    string? Reason,
    InferenceReport? Report)
{
    public static ReportValidationResult ConstructionFailed(string reason) =>
        new("construction_failed", false, reason, null);

    public static ReportValidationResult Invalid(string reason) =>
        new("invalid", false, reason, null);

    public static ReportValidationResult Published(InferenceReport report) =>
        new(report.Status, true, null, report);
}

internal sealed record InferenceReportCandidate(
    string ReportId,
    string ExecutionId,
    string RuleSetId,
    string GenerationVersion,
    string StatusCandidate,
    CoverageSummary Coverage,
    IReadOnlyList<FindingUnit> Findings,
    ConfidenceSummary ConfidenceSummary,
    IReadOnlyList<UncertaintySummaryItem> UncertaintySummary,
    IReadOnlyList<ContradictionUnit> Contradictions,
    IReadOnlyList<AbstentionUnit> Abstentions,
    ReportTraceability Traceability,
    ReportStatistics Statistics);

internal sealed record InferenceReport(
    [property: JsonPropertyName("report_id")] string ReportId,
    [property: JsonPropertyName("execution_id")] string ExecutionId,
    [property: JsonPropertyName("rule_set_id")] string RuleSetId,
    [property: JsonPropertyName("generation_version")] string GenerationVersion,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("coverage")] CoverageSummary Coverage,
    [property: JsonPropertyName("findings")] IReadOnlyList<FindingUnit> Findings,
    [property: JsonPropertyName("confidence_summary")] ConfidenceSummary ConfidenceSummary,
    [property: JsonPropertyName("uncertainty_summary")] IReadOnlyList<UncertaintySummaryItem> UncertaintySummary,
    [property: JsonPropertyName("contradictions")] IReadOnlyList<ContradictionUnit> Contradictions,
    [property: JsonPropertyName("abstentions")] IReadOnlyList<AbstentionUnit> Abstentions,
    [property: JsonPropertyName("traceability_summary")] ReportTraceability Traceability,
    [property: JsonPropertyName("statistics")] ReportStatistics Statistics);

internal sealed record ReportTraceability(
    [property: JsonPropertyName("evidence")] IReadOnlyList<EvidenceUnit> Evidence,
    [property: JsonPropertyName("claims")] IReadOnlyList<ClaimUnit> Claims,
    [property: JsonPropertyName("hypotheses")] IReadOnlyList<HypothesisUnit> Hypotheses);

internal sealed record ReportStatistics(
    [property: JsonPropertyName("evidence")] int Evidence,
    [property: JsonPropertyName("claims")] int Claims,
    [property: JsonPropertyName("hypotheses")] int Hypotheses,
    [property: JsonPropertyName("findings")] int Findings,
    [property: JsonPropertyName("contradictions")] int Contradictions,
    [property: JsonPropertyName("abstentions")] int Abstentions);

internal sealed record PublishedReportReference(
    [property: JsonPropertyName("report_id")] string ReportId,
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("status")] string Status);
