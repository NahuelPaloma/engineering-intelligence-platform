using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace Eip.Cli.Inference;

internal static class AvailableDocumentContextFindingRule
{
    internal const string RuleId = "available-document-context-finding";
    internal const int RuleVersion = 1;
    internal const string RuleSetId = "capability-002-document-context-finding-rules-v1";

    public static FindingRuleApplicability Evaluate(
        HypothesisUnit hypothesis,
        IReadOnlyDictionary<string, ClaimUnit> claimsById,
        IReadOnlyDictionary<string, EvidenceUnit> evidenceById)
    {
        if (hypothesis.Status != "valid"
            || hypothesis.RuleId != AvailableDocumentContextRule.RuleId
            || hypothesis.RuleVersion != AvailableDocumentContextRule.RuleVersion)
        {
            return new(false, "hypothesis_not_eligible");
        }

        if (hypothesis.ClaimIds.Count != 1
            || !claimsById.TryGetValue(hypothesis.ClaimIds[0], out var claim)
            || claim.Status != "valid"
            || claim.EvidenceIds.Count != 1
            || !evidenceById.TryGetValue(claim.EvidenceIds[0], out var evidence))
        {
            return new(false, "hypothesis_traceability_invalid");
        }

        if (hypothesis.Scope != claim.Scope || claim.Scope != evidence.Scope)
        {
            return new(false, "hypothesis_scope_inconsistent");
        }

        if (hypothesis.Confidence is null
            || hypothesis.Uncertainty.Count == 0
            || string.IsNullOrWhiteSpace(hypothesis.VerificationCondition)
            || string.IsNullOrWhiteSpace(hypothesis.FalsificationCondition))
        {
            return new(false, "hypothesis_support_incomplete");
        }

        return new(true, null);
    }
}

internal static class FindingProcessing
{
    internal const string Category = "context_availability";
    internal const string OpenQuestion =
        "Does this document provide applicable context for the current execution?";

    internal static readonly ImmutableArray<string> ApplicabilityLimits =
        ImmutableArray.Create(
            "applies_only_to_the_referenced_document",
            "does_not_establish_relevance",
            "does_not_establish_authority",
            "does_not_establish_currency",
            "does_not_establish_usefulness_for_a_consumer");

    public static FindingCandidate CreateCandidate(
        HypothesisUnit hypothesis,
        IReadOnlyDictionary<string, ClaimUnit> claimsById,
        FindingRuleApplicability applicability)
    {
        if (!applicability.Applies)
        {
            return FindingCandidate.NotApplicable(hypothesis.HypothesisId, applicability.Reason!);
        }

        var claim = claimsById[hypothesis.ClaimIds[0]];
        var statement = CreateStatement(hypothesis.Scope.DocumentPath);
        var openQuestions = ImmutableArray.Create(OpenQuestion);
        var evidenceIds = claim.EvidenceIds.ToImmutableArray();
        var identity = string.Join(
            '\n',
            hypothesis.HypothesisId,
            Category,
            statement,
            AvailableDocumentContextFindingRule.RuleId,
            AvailableDocumentContextFindingRule.RuleVersion,
            hypothesis.Scope.Type,
            hypothesis.Scope.DocumentPath,
            SerializeConfidence(FindingSupport.MinimumConfidence),
            string.Join('|', FindingSupport.RequiredUncertainty),
            string.Join('|', openQuestions),
            string.Join('|', ApplicabilityLimits));
        var finding = new FindingUnit(
            Hash(identity),
            Category,
            statement,
            ImmutableArray.Create(hypothesis.HypothesisId),
            evidenceIds,
            AvailableDocumentContextFindingRule.RuleId,
            AvailableDocumentContextFindingRule.RuleVersion,
            hypothesis.Scope,
            FindingSupport.MinimumConfidence,
            FindingSupport.RequiredUncertainty,
            openQuestions,
            ApplicabilityLimits,
            "candidate");

        return FindingCandidate.Applicable(hypothesis.HypothesisId, finding);
    }

    internal static string CreateStatement(string documentPath) =>
        $"A document available at '{documentPath}' may provide context for this execution.";

    private static string SerializeConfidence(ClaimConfidence confidence) =>
        string.Join(
            '|',
            confidence.Level,
            string.Join(',', confidence.Basis),
            string.Join(',', confidence.Limitations),
            confidence.Dimensions.Directness,
            confidence.Dimensions.Coverage,
            confidence.Dimensions.Consistency,
            confidence.Dimensions.Traceability,
            confidence.Dimensions.RuleConstraints);

    private static string Hash(string value) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}

internal static class FindingSupport
{
    internal static readonly ClaimConfidence MinimumConfidence = new(
        "moderate",
        ImmutableArray.Create(
            "valid_traceable_hypothesis",
            "admitted_document_is_available",
            "statement_is_limited_to_possible_context"),
        ImmutableArray.Create(
            "useful_context_not_verified",
            "content_not_evaluated",
            "authority_not_evaluated",
            "freshness_not_evaluated",
            "relevance_not_evaluated",
            "consumer_applicability_not_evaluated"),
        new ConfidenceDimensions("moderate", "weak", "moderate", "strong", "strong"));

    internal static readonly ImmutableArray<string> RequiredUncertainty =
        ImmutableArray.Create(
            "useful_context_contribution_not_verified",
            "content_and_applicability_not_evaluated",
            "consumer_not_confirmed");
}

internal static class FindingValidation
{
    public static FindingValidationResult Decide(
        FindingCandidate candidate,
        HypothesisUnit sourceHypothesis,
        IReadOnlyDictionary<string, ClaimUnit> claimsById,
        IReadOnlyDictionary<string, EvidenceUnit> evidenceById)
    {
        if (!candidate.Applies)
        {
            return FindingValidationResult.Discarded(
                sourceHypothesis.HypothesisId,
                candidate.DiscardReason!);
        }

        if (candidate.Finding is null
            || candidate.Finding.Status != "candidate"
            || candidate.Finding.HypothesisIds.Count != 1
            || candidate.Finding.HypothesisIds[0] != sourceHypothesis.HypothesisId
            || sourceHypothesis.ClaimIds.Count != 1
            || !claimsById.TryGetValue(sourceHypothesis.ClaimIds[0], out var claim)
            || claim.EvidenceIds.Count != 1
            || !evidenceById.TryGetValue(claim.EvidenceIds[0], out var evidence)
            || candidate.Finding.EvidenceIds.Count != 1
            || candidate.Finding.EvidenceIds[0] != evidence.EvidenceId)
        {
            return FindingValidationResult.Discarded(
                sourceHypothesis.HypothesisId,
                "finding_traceability_invalid");
        }

        if (candidate.Finding.Scope != sourceHypothesis.Scope
            || sourceHypothesis.Scope != claim.Scope
            || claim.Scope != evidence.Scope)
        {
            return FindingValidationResult.Discarded(
                sourceHypothesis.HypothesisId,
                "finding_scope_inconsistent");
        }

        if (candidate.Finding.Category != FindingProcessing.Category
            || candidate.Finding.Statement != FindingProcessing.CreateStatement(sourceHypothesis.Scope.DocumentPath)
            || candidate.Finding.Confidence is null
            || candidate.Finding.Uncertainty.Count == 0
            || !candidate.Finding.OpenQuestions.SequenceEqual(
                [FindingProcessing.OpenQuestion],
                StringComparer.Ordinal)
            || !candidate.Finding.ApplicabilityLimits.SequenceEqual(
                FindingProcessing.ApplicabilityLimits,
                StringComparer.Ordinal))
        {
            return FindingValidationResult.Discarded(
                sourceHypothesis.HypothesisId,
                "finding_structure_invalid");
        }

        return FindingValidationResult.Valid(candidate.Finding with { Status = "valid" });
    }
}

internal sealed record FindingRuleApplicability(bool Applies, string? Reason);

internal sealed record FindingCandidate(
    string SourceHypothesisId,
    bool Applies,
    string? DiscardReason,
    FindingUnit? Finding)
{
    public static FindingCandidate NotApplicable(string hypothesisId, string reason) =>
        new(hypothesisId, false, reason, null);

    public static FindingCandidate Applicable(string hypothesisId, FindingUnit finding) =>
        new(hypothesisId, true, null, finding);
}

internal sealed record FindingValidationResult(
    string Status,
    FindingUnit? Finding,
    DiscardedFinding? Discard)
{
    public static FindingValidationResult Valid(FindingUnit finding) =>
        new("valid", finding, null);

    public static FindingValidationResult Discarded(string hypothesisId, string reason) =>
        new(
            "discarded",
            null,
            new DiscardedFinding(
                ImmutableArray.Create(hypothesisId),
                AvailableDocumentContextFindingRule.RuleId,
                AvailableDocumentContextFindingRule.RuleVersion,
                "discarded",
                reason));
}

internal sealed record FindingUnit(
    [property: JsonPropertyName("finding_id")] string FindingId,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("statement")] string Statement,
    [property: JsonPropertyName("hypothesis_ids")] IReadOnlyList<string> HypothesisIds,
    [property: JsonPropertyName("evidence_ids")] IReadOnlyList<string> EvidenceIds,
    [property: JsonPropertyName("rule_id")] string RuleId,
    [property: JsonPropertyName("rule_version")] int RuleVersion,
    [property: JsonPropertyName("scope")] DocumentScope Scope,
    [property: JsonPropertyName("confidence")] ClaimConfidence Confidence,
    [property: JsonPropertyName("uncertainty")] IReadOnlyList<string> Uncertainty,
    [property: JsonPropertyName("open_questions")] IReadOnlyList<string> OpenQuestions,
    [property: JsonPropertyName("applicability_limits")] IReadOnlyList<string> ApplicabilityLimits,
    [property: JsonPropertyName("status")] string Status);

internal sealed record DiscardedFinding(
    [property: JsonPropertyName("hypothesis_ids")] IReadOnlyList<string> HypothesisIds,
    [property: JsonPropertyName("rule_id")] string RuleId,
    [property: JsonPropertyName("rule_version")] int RuleVersion,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("reason")] string Reason);
