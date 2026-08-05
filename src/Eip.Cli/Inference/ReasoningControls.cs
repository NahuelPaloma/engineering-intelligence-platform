using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace Eip.Cli.Inference;

internal static class ConfidencePropagation
{
    internal static readonly string[] Levels = ["insufficient", "weak", "moderate", "strong"];

    public static ClaimConfidence Derive(
        ClaimConfidence own,
        IEnumerable<ClaimConfidence> supports,
        bool materialUncertainty = false,
        bool contradiction = false)
    {
        var supportList = supports.ToArray();
        var level = supportList.Aggregate(own.Level, (current, support) => Weaker(current, support.Level));
        if (contradiction)
        {
            level = Reduce(level);
        }
        else if (materialUncertainty && level == "strong")
        {
            level = "moderate";
        }

        return own with
        {
            Level = level,
            Basis = Distinct(own.Basis.Concat(supportList.SelectMany(item => item.Basis))),
            Limitations = Distinct(own.Limitations.Concat(supportList.SelectMany(item => item.Limitations))),
            Dimensions = new ConfidenceDimensions(
                WeakestDimension(own.Dimensions.Directness, supportList.Select(item => item.Dimensions.Directness)),
                WeakestDimension(own.Dimensions.Coverage, supportList.Select(item => item.Dimensions.Coverage)),
                WeakestDimension(own.Dimensions.Consistency, supportList.Select(item => item.Dimensions.Consistency)),
                WeakestDimension(own.Dimensions.Traceability, supportList.Select(item => item.Dimensions.Traceability)),
                WeakestDimension(own.Dimensions.RuleConstraints, supportList.Select(item => item.Dimensions.RuleConstraints)))
        };
    }

    public static bool AllowsDerivation(ClaimConfidence confidence) =>
        confidence.Level != "insufficient";

    private static string Weaker(string left, string right) =>
        Rank(left) <= Rank(right) ? left : right;

    private static string Reduce(string level) => level switch
    {
        "strong" => "moderate",
        "moderate" => "weak",
        "weak" => "insufficient",
        _ => "insufficient"
    };

    private static string WeakestDimension(string own, IEnumerable<string> supports) =>
        supports.Aggregate(own, Weaker);

    private static int Rank(string level)
    {
        var rank = Array.IndexOf(Levels, level);
        return rank >= 0 ? rank : 0;
    }

    private static ImmutableArray<string> Distinct(IEnumerable<string> values) =>
        values.Distinct(StringComparer.Ordinal).ToImmutableArray();
}

internal static class UncertaintyPropagation
{
    public static ImmutableArray<string> Merge(
        IEnumerable<string> own,
        params IEnumerable<string>[] inherited) =>
        own.Concat(inherited.SelectMany(item => item))
            .Distinct(StringComparer.Ordinal)
            .ToImmutableArray();

    public static IReadOnlyList<UncertaintySummaryItem> Summarize(
        IReadOnlyList<ClaimUnit> claims,
        IReadOnlyList<HypothesisUnit> hypotheses,
        IReadOnlyList<FindingUnit> findings)
    {
        var items = new Dictionary<string, UncertaintySummaryItem>(StringComparer.Ordinal);
        Add(items, "claim", claims.Select(item => (item.ClaimId, item.Scope, item.Confidence, item.Uncertainty)));
        Add(items, "hypothesis", hypotheses.Select(item => (item.HypothesisId, item.Scope, item.Confidence, item.Uncertainty)));
        Add(items, "finding", findings.Select(item => (item.FindingId, item.Scope, item.Confidence, item.Uncertainty)));
        return items.Values.ToArray();
    }

    private static void Add(
        Dictionary<string, UncertaintySummaryItem> target,
        string origin,
        IEnumerable<(string Id, DocumentScope Scope, ClaimConfidence Confidence, IReadOnlyList<string> Values)> units)
    {
        foreach (var unit in units)
        {
            foreach (var value in unit.Values.Distinct(StringComparer.Ordinal))
            {
                if (target.TryGetValue(value, out var existing))
                {
                    target[value] = existing with
                    {
                        AffectedUnitIds = existing.AffectedUnitIds.Append(unit.Id)
                            .Distinct(StringComparer.Ordinal).ToImmutableArray()
                    };
                    continue;
                }

                target.Add(value, new UncertaintySummaryItem(
                    Hash(value),
                    value,
                    origin,
                    ImmutableArray.Create(unit.Id),
                    "limits_scope_to_referenced_document",
                    $"confidence_remains_{unit.Confidence.Level}",
                    ImmutableArray<string>.Empty,
                    ImmutableArray.Create("What additional governed evidence could reduce this uncertainty?")));
            }
        }
    }

    private static string Hash(string value) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}

internal static class ContradictionProcessing
{
    public static ContradictionCandidate CreateCandidate(
        string leftUnitId,
        string rightUnitId,
        string leftPosition,
        string rightPosition,
        IReadOnlyList<string> evidenceIds,
        DocumentScope leftScope,
        DocumentScope rightScope)
    {
        if (leftScope != rightScope)
        {
            return new(false, "scope_not_comparable", null);
        }

        if (string.Equals(leftPosition, rightPosition, StringComparison.Ordinal))
        {
            return new(false, "positions_not_incompatible", null);
        }

        var unitIds = new[] { leftUnitId, rightUnitId }.Order(StringComparer.Ordinal).ToImmutableArray();
        var orderedEvidence = evidenceIds.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToImmutableArray();
        var positions = new[] { leftPosition, rightPosition }.Order(StringComparer.Ordinal).ToImmutableArray();
        var id = Hash(string.Join('\n', unitIds.Concat(orderedEvidence).Concat(positions)
            .Append(leftScope.Type).Append(leftScope.DocumentPath)));
        var uncertainty = new UncertaintySummaryItem(
            Hash($"contradiction\n{id}"),
            "supported_positions_are_structurally_incompatible",
            "contradiction",
            unitIds,
            "limited_to_shared_scope",
            "confidence_reduced",
            ImmutableArray<string>.Empty,
            ImmutableArray.Create("Which supported position, if either, applies within the shared scope?"));
        var unit = new ContradictionUnit(
            id,
            unitIds,
            orderedEvidence,
            leftScope,
            ImmutableArray<string>.Empty,
            positions,
            "confidence_reduced",
            uncertainty,
            ImmutableArray<string>.Empty,
            "candidate");
        return new(true, null, unit);
    }

    private static string Hash(string value) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}

internal static class ContradictionValidation
{
    public static ContradictionValidationResult Decide(
        ContradictionCandidate candidate,
        IReadOnlySet<string> validUnitIds,
        IReadOnlySet<string> validEvidenceIds)
    {
        if (!candidate.Applies || candidate.Contradiction is null)
        {
            return new("discarded", null, new DiscardedReason("contradiction", candidate.Reason!));
        }

        var item = candidate.Contradiction;
        if (item.UnitIds.Count != 2
            || item.EvidenceIds.Count < 2
            || item.Positions.Count != 2
            || item.UnitIds.Any(id => !validUnitIds.Contains(id))
            || item.EvidenceIds.Any(id => !validEvidenceIds.Contains(id)))
        {
            return new("discarded", null, new DiscardedReason("contradiction", "contradiction_structure_invalid"));
        }

        return new("valid", item with { Status = "valid" }, null);
    }
}

internal static class AbstentionProcessing
{
    private static readonly HashSet<string> Types = new(StringComparer.Ordinal) { "local", "partial", "total" };

    public static AbstentionCandidate CreateCandidate(
        string type,
        string blockedUnitType,
        DocumentScope blockedScope,
        string condition,
        IReadOnlyList<string> availableEvidenceIds,
        string missingEvidenceDescription,
        IReadOnlyList<string> uncertaintyIds,
        IReadOnlyList<string> contradictionIds,
        IReadOnlyList<DocumentScope> remainingValidScope)
    {
        if (!Types.Contains(type))
        {
            return new(false, "abstention_type_invalid", null);
        }

        var evidenceIds = availableEvidenceIds.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToImmutableArray();
        var orderedUncertainty = uncertaintyIds.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToImmutableArray();
        var orderedContradictions = contradictionIds.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToImmutableArray();
        var remaining = remainingValidScope.OrderBy(item => item.DocumentPath, StringComparer.Ordinal).ToImmutableArray();
        var identity = string.Join('\n', new[]
        {
            type, blockedUnitType, blockedScope.Type, blockedScope.DocumentPath, condition,
            string.Join('|', evidenceIds), missingEvidenceDescription,
            string.Join('|', orderedUncertainty), string.Join('|', orderedContradictions),
            string.Join('|', remaining.Select(item => $"{item.Type}:{item.DocumentPath}"))
        });
        return new(true, null, new AbstentionUnit(
            Hash(identity), type, blockedUnitType, blockedScope, condition, evidenceIds,
            missingEvidenceDescription, orderedUncertainty, orderedContradictions, remaining, "candidate"));
    }

    private static string Hash(string value) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}

internal static class AbstentionValidation
{
    public static AbstentionValidationResult Decide(AbstentionCandidate candidate)
    {
        if (!candidate.Applies || candidate.Abstention is null)
        {
            return new("discarded", null, new DiscardedReason("abstention", candidate.Reason!));
        }

        var item = candidate.Abstention;
        if (string.IsNullOrWhiteSpace(item.Condition)
            || string.IsNullOrWhiteSpace(item.MissingEvidenceDescription)
            || (item.Type == "total" && item.RemainingValidScope.Count != 0))
        {
            return new("discarded", null, new DiscardedReason("abstention", "abstention_structure_invalid"));
        }

        return new("abstained", item with { Status = "abstained" }, null);
    }
}

internal static class CoverageProcessing
{
    public static CoverageSummary Create(
        IReadOnlyList<InputDocument> documents,
        IReadOnlyList<FindingUnit> findings)
    {
        var total = documents.Select(item => new DocumentScope("document", item.Path))
            .OrderBy(item => item.DocumentPath, StringComparer.Ordinal).ToImmutableArray();
        var processedPaths = findings.Select(item => item.Scope.DocumentPath).ToHashSet(StringComparer.Ordinal);
        var processed = total.Where(item => processedPaths.Contains(item.DocumentPath)).ToImmutableArray();
        var uncovered = total.Where(item => !processedPaths.Contains(item.DocumentPath)).ToImmutableArray();
        var status = uncovered.Length == 0 ? "full" : processed.Length == 0 ? "none" : "partial";
        return new(total, processed, uncovered, status);
    }
}

internal sealed record UncertaintySummaryItem(
    [property: JsonPropertyName("uncertainty_id")] string UncertaintyId,
    [property: JsonPropertyName("condition")] string Condition,
    [property: JsonPropertyName("origin")] string Origin,
    [property: JsonPropertyName("affected_unit_ids")] IReadOnlyList<string> AffectedUnitIds,
    [property: JsonPropertyName("scope_effect")] string ScopeEffect,
    [property: JsonPropertyName("confidence_effect")] string ConfidenceEffect,
    [property: JsonPropertyName("reducible_by_evidence_ids")] IReadOnlyList<string> ReducibleByEvidenceIds,
    [property: JsonPropertyName("open_questions")] IReadOnlyList<string> OpenQuestions);

internal sealed record ContradictionCandidate(bool Applies, string? Reason, ContradictionUnit? Contradiction);
internal sealed record ContradictionValidationResult(string Status, ContradictionUnit? Contradiction, DiscardedReason? Discard);
internal sealed record AbstentionCandidate(bool Applies, string? Reason, AbstentionUnit? Abstention);
internal sealed record AbstentionValidationResult(string Status, AbstentionUnit? Abstention, DiscardedReason? Discard);

internal sealed record ContradictionUnit(
    [property: JsonPropertyName("contradiction_id")] string ContradictionId,
    [property: JsonPropertyName("unit_ids")] IReadOnlyList<string> UnitIds,
    [property: JsonPropertyName("evidence_ids")] IReadOnlyList<string> EvidenceIds,
    [property: JsonPropertyName("shared_scope")] DocumentScope SharedScope,
    [property: JsonPropertyName("material_scope_differences")] IReadOnlyList<string> MaterialScopeDifferences,
    [property: JsonPropertyName("positions")] IReadOnlyList<string> Positions,
    [property: JsonPropertyName("confidence_effect")] string ConfidenceEffect,
    [property: JsonPropertyName("generated_uncertainty")] UncertaintySummaryItem GeneratedUncertainty,
    [property: JsonPropertyName("affected_derived_unit_ids")] IReadOnlyList<string> AffectedDerivedUnitIds,
    [property: JsonPropertyName("status")] string Status);

internal sealed record AbstentionUnit(
    [property: JsonPropertyName("abstention_id")] string AbstentionId,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("blocked_unit_type")] string BlockedUnitType,
    [property: JsonPropertyName("blocked_scope")] DocumentScope BlockedScope,
    [property: JsonPropertyName("condition")] string Condition,
    [property: JsonPropertyName("available_evidence_ids")] IReadOnlyList<string> AvailableEvidenceIds,
    [property: JsonPropertyName("missing_evidence_description")] string MissingEvidenceDescription,
    [property: JsonPropertyName("uncertainty_ids")] IReadOnlyList<string> UncertaintyIds,
    [property: JsonPropertyName("contradiction_ids")] IReadOnlyList<string> ContradictionIds,
    [property: JsonPropertyName("remaining_valid_scope")] IReadOnlyList<DocumentScope> RemainingValidScope,
    [property: JsonPropertyName("status")] string Status);

internal sealed record DiscardedReason(
    [property: JsonPropertyName("candidate_type")] string CandidateType,
    [property: JsonPropertyName("reason")] string Reason);

internal sealed record CoverageSummary(
    [property: JsonPropertyName("total_scope")] IReadOnlyList<DocumentScope> TotalScope,
    [property: JsonPropertyName("processed_scope")] IReadOnlyList<DocumentScope> ProcessedScope,
    [property: JsonPropertyName("uncovered_scope")] IReadOnlyList<DocumentScope> UncoveredScope,
    [property: JsonPropertyName("coverage_status")] string CoverageStatus);

internal sealed record ConfidenceSummary(
    [property: JsonPropertyName("strong")] int Strong,
    [property: JsonPropertyName("moderate")] int Moderate,
    [property: JsonPropertyName("weak")] int Weak,
    [property: JsonPropertyName("insufficient")] int Insufficient);
