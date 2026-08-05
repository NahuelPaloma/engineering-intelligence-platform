using System.Security.Cryptography;
using System.Text;
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
        new("report_builder", "not_implemented")
    ];

    public static async Task<string> ExecuteAsync(
        string localContextPath,
        CancellationToken cancellationToken)
    {
        var input = await InputBoundary.ReadAsync(localContextPath, cancellationToken);
        var evidence = new List<EvidenceUnit>();
        var claims = new List<ClaimUnit>();
        var discardedCandidates = new List<DiscardedCandidate>();
        foreach (var document in input.Documents)
        {
            var applicability = DocumentAvailabilityRule.Evaluate(document);
            var candidate = ClaimProcessing.CreateCandidate(input.PackId, document, applicability);
            var decision = ClaimValidation.Decide(candidate);
            if (decision.Status == "valid")
            {
                evidence.Add(decision.Evidence!);
                claims.Add(decision.Claim!);
            }
            else
            {
                discardedCandidates.Add(decision.Discard!);
            }
        }

        var evidenceById = evidence.ToDictionary(item => item.EvidenceId, StringComparer.Ordinal);
        var hypotheses = new List<HypothesisUnit>();
        var discardedHypotheses = new List<DiscardedHypothesis>();
        foreach (var claim in claims)
        {
            var applicability = AvailableDocumentContextRule.Evaluate(claim, evidenceById);
            var candidate = HypothesisProcessing.CreateCandidate(claim, applicability);
            var decision = HypothesisValidation.Decide(candidate, claim, evidenceById);
            if (decision.Status == "valid")
            {
                hypotheses.Add(decision.Hypothesis!);
            }
            else
            {
                discardedHypotheses.Add(decision.Discard!);
            }
        }

        var claimsById = claims.ToDictionary(item => item.ClaimId, StringComparer.Ordinal);
        var findings = new List<FindingUnit>();
        var discardedFindings = new List<DiscardedFinding>();
        foreach (var hypothesis in hypotheses)
        {
            var applicability = AvailableDocumentContextFindingRule.Evaluate(
                hypothesis,
                claimsById,
                evidenceById);
            var candidate = FindingProcessing.CreateCandidate(hypothesis, claimsById, applicability);
            var decision = FindingValidation.Decide(
                candidate,
                hypothesis,
                claimsById,
                evidenceById);
            if (decision.Status == "valid")
            {
                findings.Add(decision.Finding!);
            }
            else
            {
                discardedFindings.Add(decision.Discard!);
            }
        }

        var contradictions = Array.Empty<ContradictionUnit>();
        var abstentions = Array.Empty<AbstentionUnit>();
        var discardedContradictions = Array.Empty<DiscardedReason>();
        var discardedAbstentions = Array.Empty<DiscardedReason>();
        var uncertaintySummary = UncertaintyPropagation.Summarize(claims, hypotheses, findings);
        var confidenceSummary = CreateConfidenceSummary(claims, hypotheses, findings);
        var coverage = CoverageProcessing.Create(input.Documents, findings);

        var execution = new InferenceExecution(
            CreateExecutionId(input.PackId),
            input.PackId,
            ReasoningRuleSetId,
            DetermineStatus(findings.Count, abstentions),
            Stages,
            new InferenceCounts(
                evidence.Count,
                claims.Count,
                hypotheses.Count,
                findings.Count,
                0,
                discardedCandidates.Count,
                discardedHypotheses.Count,
                discardedFindings.Count,
                contradictions.Length,
                discardedContradictions.Length,
                discardedAbstentions.Length),
            evidence,
            claims,
            discardedCandidates,
            hypotheses,
            discardedHypotheses,
            findings,
            discardedFindings,
            contradictions,
            abstentions,
            uncertaintySummary,
            confidenceSummary,
            discardedContradictions,
            discardedAbstentions,
            coverage,
            "reasoning_controls_completed");

        return await InferenceExecutionWriter.WriteAsync(
            localContextPath,
            execution,
            cancellationToken);
    }

    private static string CreateExecutionId(string packId)
    {
        var identity = $"{packId}\n{InputBoundary.ContractId}\n{ReasoningRuleSetId}";
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(identity)));
    }

    private const string ReasoningRuleSetId = "capability-002-reasoning-controls-v1";

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
    [property: JsonPropertyName("execution_completeness_state")] string ExecutionCompletenessState);

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
