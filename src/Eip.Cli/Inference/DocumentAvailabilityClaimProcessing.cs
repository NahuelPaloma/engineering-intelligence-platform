using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace Eip.Cli.Inference;

internal static class DocumentAvailabilityRule
{
    internal const string RuleId = "document-availability-claim";
    internal const int RuleVersion = 1;
    internal const string RuleSetId = "capability-002-document-availability-rules-v1";

    private static readonly HashSet<string> AvailableStatuses = new(StringComparer.Ordinal)
    {
        "extracted",
        "missing_name",
        "missing_purpose",
        "insufficient"
    };

    public static RuleApplicability Evaluate(InputDocument document) =>
        document.Content is not null
            && document.Error is null
            && AvailableStatuses.Contains(document.Status)
            ? new RuleApplicability(true, null)
            : new RuleApplicability(false, "document_not_readable");
}

internal static class ClaimProcessing
{
    private const string StatementPrefix = "A context document is available at '";

    public static ClaimCandidate CreateCandidate(
        string inputPackId,
        InputDocument document,
        RuleApplicability applicability)
    {
        if (!applicability.Applies)
        {
            return ClaimCandidate.NotApplicable(document.Path, applicability.Reason!);
        }

        var scope = new DocumentScope("document", document.Path);
        var contentIdentity = Hash(document.Content!);
        var evidenceId = Hash($"{inputPackId}\n{document.Path}\n{contentIdentity}");
        var evidence = new EvidenceUnit(
            evidenceId,
            inputPackId,
            document.Path,
            scope,
            new EvidenceProvenance(InputBoundary.ContractId, document.Path),
            "available");
        var statement = $"{StatementPrefix}{document.Path}'.";
        var claimId = Hash(
            $"{evidenceId}\n{DocumentAvailabilityRule.RuleId}\n"
            + $"{DocumentAvailabilityRule.RuleVersion}\n{scope.Type}\n{scope.DocumentPath}\n{statement}");
        var claim = new ClaimUnit(
            claimId,
            statement,
            ImmutableArray.Create(evidenceId),
            DocumentAvailabilityRule.RuleId,
            DocumentAvailabilityRule.RuleVersion,
            scope,
            ClaimSupport.MinimumConfidence,
            ImmutableArray<string>.Empty,
            "candidate");

        return ClaimCandidate.Applicable(document.Path, evidence, claim);
    }

    private static string Hash(string value) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}

internal static class ClaimSupport
{
    internal static readonly ClaimConfidence MinimumConfidence = new(
        "strong",
        ImmutableArray.Create(
            "document_explicitly_present",
            "admitted_by_input_boundary",
            "rule_describes_availability_only"),
        ImmutableArray.Create(
            "content_not_evaluated",
            "authority_not_evaluated",
            "freshness_not_evaluated",
            "semantic_relevance_not_evaluated"),
        new ConfidenceDimensions("strong", "strong", "strong", "strong", "strong"));
}

internal static class ClaimValidation
{
    public static ClaimValidationResult Decide(ClaimCandidate candidate)
    {
        if (!candidate.Applies)
        {
            return ClaimValidationResult.Discarded(candidate.DocumentPath, candidate.DiscardReason!);
        }

        if (candidate.Evidence is null
            || candidate.Claim is null
            || candidate.Claim.EvidenceIds.Count != 1
            || candidate.Claim.EvidenceIds[0] != candidate.Evidence.EvidenceId
            || candidate.Claim.Scope != candidate.Evidence.Scope
            || candidate.Claim.RuleId != DocumentAvailabilityRule.RuleId
            || candidate.Claim.RuleVersion != DocumentAvailabilityRule.RuleVersion
            || candidate.Claim.Status != "candidate")
        {
            throw new InvalidDataException("The claim candidate is internally inconsistent.");
        }

        return ClaimValidationResult.Valid(
            candidate.Evidence,
            candidate.Claim with { Status = "valid" });
    }
}

internal sealed record RuleApplicability(bool Applies, string? Reason);

internal sealed record ClaimCandidate(
    string DocumentPath,
    bool Applies,
    string? DiscardReason,
    EvidenceUnit? Evidence,
    ClaimUnit? Claim)
{
    public static ClaimCandidate NotApplicable(string documentPath, string reason) =>
        new(documentPath, false, reason, null, null);

    public static ClaimCandidate Applicable(
        string documentPath,
        EvidenceUnit evidence,
        ClaimUnit claim) =>
        new(documentPath, true, null, evidence, claim);
}

internal sealed record ClaimValidationResult(
    string Status,
    EvidenceUnit? Evidence,
    ClaimUnit? Claim,
    DiscardedCandidate? Discard)
{
    public static ClaimValidationResult Valid(EvidenceUnit evidence, ClaimUnit claim) =>
        new("valid", evidence, claim, null);

    public static ClaimValidationResult Discarded(string documentPath, string reason) =>
        new(
            "discarded",
            null,
            null,
            new DiscardedCandidate(
                "document_availability_claim",
                documentPath,
                DocumentAvailabilityRule.RuleId,
                DocumentAvailabilityRule.RuleVersion,
                "discarded",
                reason));
}

internal sealed record DocumentScope(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("document_path")] string DocumentPath);

internal sealed record EvidenceProvenance(
    [property: JsonPropertyName("contract")] string Contract,
    [property: JsonPropertyName("document_path")] string DocumentPath);

internal sealed record EvidenceUnit(
    [property: JsonPropertyName("evidence_id")] string EvidenceId,
    [property: JsonPropertyName("input_pack_id")] string InputPackId,
    [property: JsonPropertyName("document_path")] string DocumentPath,
    [property: JsonPropertyName("scope")] DocumentScope Scope,
    [property: JsonPropertyName("provenance")] EvidenceProvenance Provenance,
    [property: JsonPropertyName("availability_status")] string AvailabilityStatus);

internal sealed record ClaimUnit(
    [property: JsonPropertyName("claim_id")] string ClaimId,
    [property: JsonPropertyName("statement")] string Statement,
    [property: JsonPropertyName("evidence_ids")] IReadOnlyList<string> EvidenceIds,
    [property: JsonPropertyName("rule_id")] string RuleId,
    [property: JsonPropertyName("rule_version")] int RuleVersion,
    [property: JsonPropertyName("scope")] DocumentScope Scope,
    [property: JsonPropertyName("confidence")] ClaimConfidence Confidence,
    [property: JsonPropertyName("uncertainty")] IReadOnlyList<string> Uncertainty,
    [property: JsonPropertyName("status")] string Status);

internal sealed record ClaimConfidence(
    [property: JsonPropertyName("level")] string Level,
    [property: JsonPropertyName("basis")] IReadOnlyList<string> Basis,
    [property: JsonPropertyName("limitations")] IReadOnlyList<string> Limitations,
    [property: JsonPropertyName("dimensions")] ConfidenceDimensions Dimensions);

internal sealed record ConfidenceDimensions(
    [property: JsonPropertyName("directness")] string Directness,
    [property: JsonPropertyName("coverage")] string Coverage,
    [property: JsonPropertyName("consistency")] string Consistency,
    [property: JsonPropertyName("traceability")] string Traceability,
    [property: JsonPropertyName("rule_constraints")] string RuleConstraints);

internal sealed record DiscardedCandidate(
    [property: JsonPropertyName("candidate_type")] string CandidateType,
    [property: JsonPropertyName("document_path")] string DocumentPath,
    [property: JsonPropertyName("rule_id")] string RuleId,
    [property: JsonPropertyName("rule_version")] int RuleVersion,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("reason")] string Reason);
