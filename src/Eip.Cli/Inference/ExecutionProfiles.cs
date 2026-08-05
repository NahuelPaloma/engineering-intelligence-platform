using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Eip.Cli.Inference;

internal sealed record RuleDescriptor(string RuleId, int Order);

internal sealed record ExecutionProfile
{
    public ExecutionProfile(
        string profileId,
        string profileVersion,
        string ruleSetId,
        string taxonomyId,
        string taxonomyVersion,
        IEnumerable<RuleDescriptor> rules,
        IEnumerable<IDomainRuleAdapter> domainRuleAdapters,
        IEnumerable<IProfileValidationAdapter> validationAdapters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profileId);
        ArgumentException.ThrowIfNullOrWhiteSpace(profileVersion);
        ArgumentException.ThrowIfNullOrWhiteSpace(ruleSetId);
        ArgumentException.ThrowIfNullOrWhiteSpace(taxonomyId);
        ArgumentException.ThrowIfNullOrWhiteSpace(taxonomyVersion);
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentNullException.ThrowIfNull(domainRuleAdapters);
        ArgumentNullException.ThrowIfNull(validationAdapters);

        var orderedRules = rules.OrderBy(rule => rule.Order).ToImmutableArray();
        var adapters = domainRuleAdapters.ToImmutableArray();
        var validations = validationAdapters.ToImmutableArray();
        if (orderedRules.Select(rule => rule.RuleId).Any(string.IsNullOrWhiteSpace)
            || orderedRules.Select(rule => rule.RuleId).Distinct(StringComparer.Ordinal).Count() != orderedRules.Length
            || orderedRules.Select(rule => rule.Order).Distinct().Count() != orderedRules.Length
            || adapters.Select(adapter => adapter.RuleId).Distinct(StringComparer.Ordinal).Count() != adapters.Length
            || !orderedRules.Select(rule => rule.RuleId).SequenceEqual(
                adapters.Select(adapter => adapter.RuleId),
                StringComparer.Ordinal)
            || validations.Length == 0)
        {
            throw new ArgumentException("The execution profile is invalid.", nameof(rules));
        }

        ProfileId = profileId;
        ProfileVersion = profileVersion;
        RuleSetId = ruleSetId;
        TaxonomyId = taxonomyId;
        TaxonomyVersion = taxonomyVersion;
        Rules = orderedRules;
        DomainRuleAdapters = adapters;
        ValidationAdapters = validations;
    }

    public string ProfileId { get; }
    public string ProfileVersion { get; }
    public string RuleSetId { get; }
    public string TaxonomyId { get; }
    public string TaxonomyVersion { get; }
    public ImmutableArray<RuleDescriptor> Rules { get; }
    public ImmutableArray<IDomainRuleAdapter> DomainRuleAdapters { get; }
    public ImmutableArray<IProfileValidationAdapter> ValidationAdapters { get; }
}

internal interface IDomainRuleAdapter
{
    string RuleId { get; }

    void Execute(
        InputBoundaryResult input,
        ProfileReasoningState state);
}

internal interface IProfileValidationAdapter
{
    ProfileReasoningResult Validate(
        ExecutionProfile profile,
        ProfileReasoningResult result);
}

internal sealed record ProfileReasoningResult(
    IReadOnlyList<EvidenceUnit> Evidence,
    IReadOnlyList<ClaimUnit> Claims,
    IReadOnlyList<DiscardedCandidate> DiscardedCandidates,
    IReadOnlyList<HypothesisUnit> Hypotheses,
    IReadOnlyList<DiscardedHypothesis> DiscardedHypotheses,
    IReadOnlyList<FindingUnit> Findings,
    IReadOnlyList<DiscardedFinding> DiscardedFindings,
    IReadOnlyList<ContradictionUnit> Contradictions,
    IReadOnlyList<AbstentionUnit> Abstentions,
    IReadOnlyList<DiscardedReason> DiscardedContradictions,
    IReadOnlyList<DiscardedReason> DiscardedAbstentions,
    CoverageSummary Coverage);

internal sealed class ProfileReasoningState
{
    public List<EvidenceUnit> Evidence { get; } = [];
    public List<ClaimUnit> Claims { get; } = [];
    public List<DiscardedCandidate> DiscardedCandidates { get; } = [];
    public List<HypothesisUnit> Hypotheses { get; } = [];
    public List<DiscardedHypothesis> DiscardedHypotheses { get; } = [];
    public List<FindingUnit> Findings { get; } = [];
    public List<DiscardedFinding> DiscardedFindings { get; } = [];

    public ProfileReasoningResult Complete(InputBoundaryResult input) =>
        new(
            Evidence,
            Claims,
            DiscardedCandidates,
            Hypotheses,
            DiscardedHypotheses,
            Findings,
            DiscardedFindings,
            [],
            [],
            [],
            [],
            CoverageProcessing.Create(input.Documents, Findings));
}

internal static class ExecutionProfileRegistry
{
    internal const string Capability002ProfileId = "capability-002-default";
    internal const string Capability003ProfileId = "capability-003-empty";

    private static readonly IProfileValidationAdapter Validation = new ProfileIsolationValidationAdapter();

    private static readonly Dictionary<string, ExecutionProfile> Profiles =
        new Dictionary<string, ExecutionProfile>(StringComparer.Ordinal)
        {
            [Capability002ProfileId] = new(
                Capability002ProfileId,
                "1",
                "capability-002-reasoning-controls-v1",
                "capability-002-document-context",
                "1",
                [
                    new(DocumentAvailabilityRule.RuleId, 1),
                    new(AvailableDocumentContextRule.RuleId, 2),
                    new(AvailableDocumentContextFindingRule.RuleId, 3)
                ],
                [
                    new DocumentAvailabilityRuleAdapter(),
                    new DocumentContextHypothesisRuleAdapter(),
                    new DocumentContextFindingRuleAdapter()
                ],
                [Validation]),
            [Capability003ProfileId] = new(
                Capability003ProfileId,
                "1",
                "capability-003-empty-rules-v1",
                "capability-003-empty-taxonomy",
                "1",
                [],
                [],
                [Validation])
        };

    public static ExecutionProfile Resolve(string profileId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profileId);
        return Profiles.TryGetValue(profileId, out var profile)
            ? profile
            : throw new ArgumentException("The execution profile is not registered.", nameof(profileId));
    }
}

internal static class InferenceEngineExtensionBoundary
{
    public static ExecutionProfile ResolveProfile(string profileId) =>
        ExecutionProfileRegistry.Resolve(profileId);

    public static ProfileReasoningResult Execute(
        ExecutionProfile profile,
        InputBoundaryResult input) =>
        RuleRuntime.Execute(profile, input);
}

internal static class RuleRuntime
{
    public static ProfileReasoningResult Execute(
        ExecutionProfile profile,
        InputBoundaryResult input)
    {
        var state = new ProfileReasoningState();
        foreach (var adapter in profile.DomainRuleAdapters)
        {
            adapter.Execute(input, state);
        }

        var result = state.Complete(input);
        foreach (var validation in profile.ValidationAdapters)
        {
            result = validation.Validate(profile, result);
        }

        return result;
    }
}

internal sealed class DocumentAvailabilityRuleAdapter : IDomainRuleAdapter
{
    public string RuleId => DocumentAvailabilityRule.RuleId;

    public void Execute(
        InputBoundaryResult input,
        ProfileReasoningState state)
    {
        foreach (var document in input.Documents)
        {
            var applicability = DocumentAvailabilityRule.Evaluate(document);
            var candidate = ClaimProcessing.CreateCandidate(input.PackId, document, applicability);
            var decision = ClaimValidation.Decide(candidate);
            if (decision.Status == "valid")
            {
                state.Evidence.Add(decision.Evidence!);
                state.Claims.Add(decision.Claim!);
            }
            else
            {
                state.DiscardedCandidates.Add(decision.Discard!);
            }
        }
    }
}

internal sealed class DocumentContextHypothesisRuleAdapter : IDomainRuleAdapter
{
    public string RuleId => AvailableDocumentContextRule.RuleId;

    public void Execute(
        InputBoundaryResult input,
        ProfileReasoningState state)
    {
        var evidenceById = state.Evidence.ToDictionary(item => item.EvidenceId, StringComparer.Ordinal);
        foreach (var claim in state.Claims)
        {
            var applicability = AvailableDocumentContextRule.Evaluate(claim, evidenceById);
            var candidate = HypothesisProcessing.CreateCandidate(claim, applicability);
            var decision = HypothesisValidation.Decide(candidate, claim, evidenceById);
            if (decision.Status == "valid")
            {
                state.Hypotheses.Add(decision.Hypothesis!);
            }
            else
            {
                state.DiscardedHypotheses.Add(decision.Discard!);
            }
        }
    }
}

internal sealed class DocumentContextFindingRuleAdapter : IDomainRuleAdapter
{
    public string RuleId => AvailableDocumentContextFindingRule.RuleId;

    public void Execute(
        InputBoundaryResult input,
        ProfileReasoningState state)
    {
        var evidenceById = state.Evidence.ToDictionary(item => item.EvidenceId, StringComparer.Ordinal);
        var claimsById = state.Claims.ToDictionary(item => item.ClaimId, StringComparer.Ordinal);
        foreach (var hypothesis in state.Hypotheses)
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
                state.Findings.Add(decision.Finding!);
            }
            else
            {
                state.DiscardedFindings.Add(decision.Discard!);
            }
        }
    }
}

internal sealed class ProfileIsolationValidationAdapter : IProfileValidationAdapter
{
    public ProfileReasoningResult Validate(
        ExecutionProfile profile,
        ProfileReasoningResult result)
    {
        var allowedRuleIds = profile.Rules.Select(rule => rule.RuleId).ToHashSet(StringComparer.Ordinal);
        if (result.Claims.Any(unit => !allowedRuleIds.Contains(unit.RuleId))
            || result.Hypotheses.Any(unit => !allowedRuleIds.Contains(unit.RuleId))
            || result.Findings.Any(unit => !allowedRuleIds.Contains(unit.RuleId)))
        {
            throw new InvalidDataException("The execution profile produced an invalid result.");
        }

        return result;
    }
}

internal static class ExecutionIdentity
{
    public static string Create(
        string canonicalContextIdentity,
        ExecutionProfile profile)
    {
        if (IsAcceptedCapability002Profile(profile))
        {
            var legacyIdentity = $"{canonicalContextIdentity}\n{InputBoundary.ContractId}\n{profile.RuleSetId}";
            return Hash(legacyIdentity);
        }

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("canonical_context_identity", canonicalContextIdentity);
            writer.WriteString("contract_id", InputBoundary.ContractId);
            writer.WriteString("execution_profile_id", profile.ProfileId);
            writer.WriteString("execution_profile_version", profile.ProfileVersion);
            writer.WriteString("rule_set_id", profile.RuleSetId);
            writer.WriteString("taxonomy_id", profile.TaxonomyId);
            writer.WriteString("taxonomy_version", profile.TaxonomyVersion);
            writer.WriteEndObject();
        }

        return Convert.ToHexStringLower(SHA256.HashData(stream.ToArray()));
    }

    private static bool IsAcceptedCapability002Profile(ExecutionProfile profile) =>
        profile.ProfileId == ExecutionProfileRegistry.Capability002ProfileId
        && profile.ProfileVersion == "1"
        && profile.RuleSetId == "capability-002-reasoning-controls-v1"
        && profile.TaxonomyId == "capability-002-document-context"
        && profile.TaxonomyVersion == "1";

    private static string Hash(string value) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
