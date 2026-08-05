using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace Eip.Cli.Inference;

internal static class AvailableDocumentContextRule
{
    internal const string RuleId = "available-document-context-hypothesis";
    internal const int RuleVersion = 1;
    internal const string RuleSetId = "capability-002-document-context-rules-v1";

    public static HypothesisRuleApplicability Evaluate(
        ClaimUnit claim,
        IReadOnlyDictionary<string, EvidenceUnit> evidenceById)
    {
        if (claim.Status != "valid"
            || claim.RuleId != DocumentAvailabilityRule.RuleId
            || claim.RuleVersion != DocumentAvailabilityRule.RuleVersion)
        {
            return new HypothesisRuleApplicability(false, "claim_not_eligible");
        }

        if (claim.EvidenceIds.Count != 1
            || !evidenceById.TryGetValue(claim.EvidenceIds[0], out var evidence)
            || evidence.AvailabilityStatus != "available")
        {
            return new HypothesisRuleApplicability(false, "claim_traceability_invalid");
        }

        if (!ConfidencePropagation.AllowsDerivation(claim.Confidence))
        {
            return new HypothesisRuleApplicability(false, "claim_confidence_insufficient");
        }

        return claim.Scope == evidence.Scope
            && claim.Scope.Type == "document"
            && !string.IsNullOrWhiteSpace(claim.Scope.DocumentPath)
            ? new HypothesisRuleApplicability(true, null)
            : new HypothesisRuleApplicability(false, "claim_scope_inconsistent");
    }
}

internal static class HypothesisProcessing
{
    private const string StatementPrefix = "The available document at '";
    private const string StatementSuffix = "' may provide context for this execution.";

    internal const string VerificationCondition =
        "Additional governed evidence could demonstrate that the document is used as context by a consumer.";

    internal const string FalsificationCondition =
        "Additional governed evidence could demonstrate that the document does not apply to the execution scope or provide usable context.";

    public static HypothesisCandidate CreateCandidate(
        ClaimUnit claim,
        HypothesisRuleApplicability applicability)
    {
        if (!applicability.Applies)
        {
            return HypothesisCandidate.NotApplicable(claim.ClaimId, applicability.Reason!);
        }

        var statement = $"{StatementPrefix}{claim.Scope.DocumentPath}{StatementSuffix}";
        var uncertainty = UncertaintyPropagation.Merge(
            HypothesisSupport.RequiredUncertainty,
            claim.Uncertainty);
        var confidence = ConfidencePropagation.Derive(
            HypothesisSupport.MinimumConfidence,
            [claim.Confidence],
            uncertainty.Length > 0);
        var hypothesisId = Hash(
            $"{claim.ClaimId}\n{AvailableDocumentContextRule.RuleId}\n"
            + $"{AvailableDocumentContextRule.RuleVersion}\n{claim.Scope.Type}\n"
            + $"{claim.Scope.DocumentPath}\n{statement}\n{VerificationCondition}\n{FalsificationCondition}");
        var hypothesis = new HypothesisUnit(
            hypothesisId,
            statement,
            ImmutableArray.Create(claim.ClaimId),
            AvailableDocumentContextRule.RuleId,
            AvailableDocumentContextRule.RuleVersion,
            claim.Scope,
            confidence,
            uncertainty,
            VerificationCondition,
            FalsificationCondition,
            "candidate");

        return HypothesisCandidate.Applicable(claim.ClaimId, hypothesis);
    }

    private static string Hash(string value) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}

internal static class HypothesisSupport
{
    internal static readonly ClaimConfidence MinimumConfidence = new(
        "moderate",
        ImmutableArray.Create(
            "available_document_is_traceable_within_scope",
            "relevance_or_effective_use_not_demonstrated"),
        ImmutableArray.Create(
            "contextual_use_not_verified",
            "content_not_evaluated",
            "authority_not_evaluated",
            "freshness_not_evaluated",
            "semantic_relevance_not_evaluated"),
        new ConfidenceDimensions("moderate", "weak", "moderate", "strong", "strong"));

    internal static readonly ImmutableArray<string> RequiredUncertainty =
        ImmutableArray.Create(
            "useful_context_contribution_not_verified",
            "content_authority_freshness_and_relevance_not_evaluated");
}

internal static class HypothesisValidation
{
    public static HypothesisValidationResult Decide(
        HypothesisCandidate candidate,
        ClaimUnit sourceClaim,
        IReadOnlyDictionary<string, EvidenceUnit> evidenceById)
    {
        if (!candidate.Applies)
        {
            return HypothesisValidationResult.Discarded(
                sourceClaim.ClaimId,
                candidate.DiscardReason!);
        }

        if (candidate.Hypothesis is null
            || candidate.Hypothesis.Status != "candidate"
            || candidate.Hypothesis.ClaimIds.Count != 1
            || candidate.Hypothesis.ClaimIds[0] != sourceClaim.ClaimId
            || sourceClaim.EvidenceIds.Count != 1
            || !evidenceById.TryGetValue(sourceClaim.EvidenceIds[0], out var evidence))
        {
            return HypothesisValidationResult.Discarded(
                sourceClaim.ClaimId,
                "claim_traceability_invalid");
        }

        if (sourceClaim.Scope != evidence.Scope
            || candidate.Hypothesis.Scope != sourceClaim.Scope)
        {
            return HypothesisValidationResult.Discarded(
                sourceClaim.ClaimId,
                "claim_scope_inconsistent");
        }

        return HypothesisValidationResult.Valid(
            candidate.Hypothesis with { Status = "valid" });
    }
}

internal sealed record HypothesisRuleApplicability(bool Applies, string? Reason);

internal sealed record HypothesisCandidate(
    string SourceClaimId,
    bool Applies,
    string? DiscardReason,
    HypothesisUnit? Hypothesis)
{
    public static HypothesisCandidate NotApplicable(string claimId, string reason) =>
        new(claimId, false, reason, null);

    public static HypothesisCandidate Applicable(string claimId, HypothesisUnit hypothesis) =>
        new(claimId, true, null, hypothesis);
}

internal sealed record HypothesisValidationResult(
    string Status,
    HypothesisUnit? Hypothesis,
    DiscardedHypothesis? Discard)
{
    public static HypothesisValidationResult Valid(HypothesisUnit hypothesis) =>
        new("valid", hypothesis, null);

    public static HypothesisValidationResult Discarded(string claimId, string reason) =>
        new(
            "discarded",
            null,
            new DiscardedHypothesis(
                ImmutableArray.Create(claimId),
                AvailableDocumentContextRule.RuleId,
                AvailableDocumentContextRule.RuleVersion,
                "discarded",
                reason));
}

internal sealed record HypothesisUnit(
    [property: JsonPropertyName("hypothesis_id")] string HypothesisId,
    [property: JsonPropertyName("statement")] string Statement,
    [property: JsonPropertyName("claim_ids")] IReadOnlyList<string> ClaimIds,
    [property: JsonPropertyName("rule_id")] string RuleId,
    [property: JsonPropertyName("rule_version")] int RuleVersion,
    [property: JsonPropertyName("scope")] DocumentScope Scope,
    [property: JsonPropertyName("confidence")] ClaimConfidence Confidence,
    [property: JsonPropertyName("uncertainty")] IReadOnlyList<string> Uncertainty,
    [property: JsonPropertyName("verification_condition")] string VerificationCondition,
    [property: JsonPropertyName("falsification_condition")] string FalsificationCondition,
    [property: JsonPropertyName("status")] string Status);

internal sealed record DiscardedHypothesis(
    [property: JsonPropertyName("claim_ids")] IReadOnlyList<string> ClaimIds,
    [property: JsonPropertyName("rule_id")] string RuleId,
    [property: JsonPropertyName("rule_version")] int RuleVersion,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("reason")] string Reason);
