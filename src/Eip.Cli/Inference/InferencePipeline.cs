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

        var execution = new InferenceExecution(
            CreateExecutionId(input.PackId),
            input.PackId,
            AvailableDocumentContextFindingRule.RuleSetId,
            DetermineStatus(claims.Count, hypotheses.Count, findings.Count),
            Stages,
            new InferenceCounts(
                evidence.Count,
                claims.Count,
                hypotheses.Count,
                findings.Count,
                0,
                discardedCandidates.Count,
                discardedHypotheses.Count,
                discardedFindings.Count),
            evidence,
            claims,
            discardedCandidates,
            hypotheses,
            discardedHypotheses,
            findings,
            discardedFindings);

        return await InferenceExecutionWriter.WriteAsync(
            localContextPath,
            execution,
            cancellationToken);
    }

    private static string CreateExecutionId(string packId)
    {
        var identity = $"{packId}\n{InputBoundary.ContractId}\n{AvailableDocumentContextFindingRule.RuleSetId}";
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(identity)));
    }

    private static string DetermineStatus(int claimCount, int hypothesisCount, int findingCount) =>
        findingCount > 0 ? "findings_produced"
        : hypothesisCount > 0 ? "no_findings"
        : claimCount > 0 ? "no_hypotheses"
        : "no_claims";
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
    [property: JsonPropertyName("discarded_findings")] IReadOnlyList<DiscardedFinding> DiscardedFindings);

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
    [property: JsonPropertyName("discarded_findings")] int DiscardedFindings);
