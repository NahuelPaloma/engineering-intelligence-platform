using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Eip.Cli.Inference;

internal sealed class ModifiedFileEvidenceAdmissionAdapter : IProfileInputAdapter
{
    public void Execute(
        ExecutionProfile profile,
        InputBoundaryResult input,
        ProfileReasoningState state)
    {
        if (state.CapabilityContextCandidate is not null)
        {
            throw new InvalidDataException("The execution profile input is invalid.");
        }

        state.CapabilityContextCandidate = ModifiedFileEvidenceAdmissionProcessing.CreateCandidate(
            profile,
            input);
    }
}

internal static class ModifiedFileEvidenceAdmissionProcessing
{
    public static CapabilityExecutionContextCandidate CreateCandidate(
        ExecutionProfile profile,
        InputBoundaryResult input)
    {
        var admitted = input.ModifiedFiles.Select((file, position) =>
            CreateAdmittedFile(input.CanonicalContextIdentity, file, position)).ToImmutableArray();
        var coverage = ContractDetectionCoverageProcessing.CreateCandidate(
            input.ModifiedFilesAvailability,
            admitted.Select(item => item.Scope).ToImmutableArray());
        var abstention = input.ModifiedFilesAvailability == "not_provided"
            ? AbstentionProcessing.CreateCandidate(
                "total",
                "contract_detection",
                new DocumentScope("modified_files_collection", input.PackId),
                "modified_files_not_provided",
                [],
                "Modified File Evidence was not provided by the producer.",
                [],
                [],
                [])
            : null;

        return new CapabilityExecutionContextCandidate(
            profile.PluginId,
            profile.PluginVersion,
            profile.ProfileId,
            profile.ProfileVersion,
            profile.RuleSetId,
            profile.TaxonomyId,
            profile.TaxonomyVersion,
            input.CanonicalContextIdentity,
            input.ModifiedFilesAvailability,
            admitted,
            coverage,
            abstention);
    }

    private static AdmittedModifiedFile CreateAdmittedFile(
        string canonicalContextIdentity,
        InputModifiedFile file,
        int position)
    {
        var provenance = new AdmittedModifiedFileProvenance(
            file.Provenance.Provider,
            file.Provenance.Repository,
            file.Provenance.PullRequest,
            file.Provenance.Source);
        var identity = CreateIdentity(canonicalContextIdentity, file, position);
        return new AdmittedModifiedFile(
            identity,
            position,
            file.Path,
            file.ChangeStatus,
            provenance,
            new ModifiedFileScope(identity, position, file.Path));
    }

    private static string CreateIdentity(
        string canonicalContextIdentity,
        InputModifiedFile file,
        int position)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("canonical_context_identity", canonicalContextIdentity);
            writer.WriteNumber("position", position);
            writer.WriteString("path", file.Path);
            writer.WriteString("change_status", file.ChangeStatus);
            writer.WriteStartObject("provenance");
            writer.WriteString("provider", file.Provenance.Provider);
            writer.WriteString("repository", file.Provenance.Repository);
            writer.WriteNumber("pull_request", file.Provenance.PullRequest);
            writer.WriteString("source", file.Provenance.Source);
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        return Convert.ToHexStringLower(SHA256.HashData(stream.ToArray()));
    }
}

internal static class ContractDetectionCoverageProcessing
{
    public static ContractDetectionCoverageCandidate CreateCandidate(
        string availability,
        IReadOnlyList<ModifiedFileScope> totalScope) =>
        availability switch
        {
            "not_provided" when totalScope.Count == 0 => new(
                [],
                [],
                [],
                "unknown",
                "modified_files_not_provided"),
            "available" when totalScope.Count == 0 => new(
                [],
                [],
                [],
                "complete",
                null),
            "available" => new(
                totalScope,
                [],
                totalScope,
                "partial",
                "no_candidate_rules_registered"),
            _ => throw new InvalidDataException("Modified File Evidence availability is inconsistent.")
        };
}

internal sealed class ModifiedFileEvidenceAdmissionValidationAdapter : IProfileValidationAdapter
{
    public ProfileReasoningResult Validate(
        ExecutionProfile profile,
        ProfileReasoningResult result)
    {
        var candidate = result.CapabilityContextCandidate
            ?? throw new InvalidDataException("Modified File Evidence admission is missing.");
        if (candidate.PluginId != profile.PluginId
            || candidate.PluginVersion != profile.PluginVersion
            || candidate.ProfileId != profile.ProfileId
            || candidate.ProfileVersion != profile.ProfileVersion
            || candidate.RuleSetId != profile.RuleSetId
            || candidate.TaxonomyId != profile.TaxonomyId
            || candidate.TaxonomyVersion != profile.TaxonomyVersion
            || string.IsNullOrWhiteSpace(candidate.InputIdentity))
        {
            throw new InvalidDataException("The execution profile identity is inconsistent.");
        }

        var coverage = ContractDetectionCoverageValidation.Decide(
            candidate.ModifiedFilesAvailability,
            candidate.AdmittedModifiedFiles,
            candidate.Coverage);
        var abstentions = result.Abstentions;

        if (candidate.AbstentionCandidate is not null)
        {
            var decision = AbstentionValidation.Decide(candidate.AbstentionCandidate);
            if (decision.Status != "abstained" || decision.Abstention is null)
            {
                throw new InvalidDataException("Modified File Evidence abstention is invalid.");
            }

            abstentions = abstentions.Append(decision.Abstention).ToImmutableArray();
        }
        else if (candidate.ModifiedFilesAvailability == "not_provided")
        {
            throw new InvalidDataException("Modified File Evidence abstention is missing.");
        }

        return result with
        {
            Abstentions = abstentions,
            CapabilityContext = new CapabilityExecutionContext(
                candidate.PluginId,
                candidate.PluginVersion,
                candidate.ProfileId,
                candidate.ProfileVersion,
                candidate.RuleSetId,
                candidate.TaxonomyId,
                candidate.TaxonomyVersion,
                candidate.InputIdentity,
                candidate.ModifiedFilesAvailability,
                candidate.AdmittedModifiedFiles.Count,
                candidate.AdmittedModifiedFiles,
                coverage)
        };
    }
}

internal static class ContractDetectionCoverageValidation
{
    public static ContractDetectionCoverage Decide(
        string availability,
        IReadOnlyList<AdmittedModifiedFile> admitted,
        ContractDetectionCoverageCandidate candidate)
    {
        var total = candidate.TotalScope;
        var processed = candidate.ProcessedScope;
        var uncovered = candidate.UncoveredScope;
        var identities = admitted.Select(item => item.EvidenceId).ToArray();
        var scopeIdentities = total.Select(item => item.EvidenceId).ToArray();
        var valid = identities.SequenceEqual(scopeIdentities, StringComparer.Ordinal)
            && total.Select(item => item.Position).SequenceEqual(Enumerable.Range(0, total.Count))
            && processed.Count == 0
            && (availability, admitted.Count, candidate.Status, candidate.Cause) switch
            {
                ("not_provided", 0, "unknown", "modified_files_not_provided") => uncovered.Count == 0,
                ("available", 0, "complete", null) => uncovered.Count == 0,
                ("available", > 0, "partial", "no_candidate_rules_registered") =>
                    uncovered.Select(item => item.EvidenceId)
                        .SequenceEqual(identities, StringComparer.Ordinal),
                _ => false
            };
        if (!valid)
        {
            throw new InvalidDataException("Contract detection coverage is invalid.");
        }

        return new ContractDetectionCoverage(
            total,
            processed,
            uncovered,
            candidate.Status,
            candidate.Cause);
    }
}

internal sealed record CapabilityExecutionContextCandidate(
    string PluginId,
    string PluginVersion,
    string ProfileId,
    string ProfileVersion,
    string RuleSetId,
    string TaxonomyId,
    string TaxonomyVersion,
    string InputIdentity,
    string ModifiedFilesAvailability,
    IReadOnlyList<AdmittedModifiedFile> AdmittedModifiedFiles,
    ContractDetectionCoverageCandidate Coverage,
    AbstentionCandidate? AbstentionCandidate);

internal sealed record CapabilityExecutionContext(
    [property: JsonPropertyName("plugin_id")] string PluginId,
    [property: JsonPropertyName("plugin_version")] string PluginVersion,
    [property: JsonPropertyName("profile_id")] string ProfileId,
    [property: JsonPropertyName("profile_version")] string ProfileVersion,
    [property: JsonPropertyName("rule_set_id")] string RuleSetId,
    [property: JsonPropertyName("taxonomy_id")] string TaxonomyId,
    [property: JsonPropertyName("taxonomy_version")] string TaxonomyVersion,
    [property: JsonPropertyName("input_identity")] string InputIdentity,
    [property: JsonPropertyName("modified_files_availability")] string ModifiedFilesAvailability,
    [property: JsonPropertyName("admitted_modified_files_count")] int AdmittedModifiedFilesCount,
    [property: JsonPropertyName("admitted_modified_files")] IReadOnlyList<AdmittedModifiedFile> AdmittedModifiedFiles,
    [property: JsonPropertyName("contract_detection_coverage")] ContractDetectionCoverage Coverage);

internal sealed record AdmittedModifiedFile(
    [property: JsonPropertyName("evidence_id")] string EvidenceId,
    [property: JsonPropertyName("position")] int Position,
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("change_status")] string ChangeStatus,
    [property: JsonPropertyName("provenance")] AdmittedModifiedFileProvenance Provenance,
    [property: JsonPropertyName("scope")] ModifiedFileScope Scope);

internal sealed record AdmittedModifiedFileProvenance(
    [property: JsonPropertyName("provider")] string Provider,
    [property: JsonPropertyName("repository")] string Repository,
    [property: JsonPropertyName("pull_request")] int PullRequest,
    [property: JsonPropertyName("source")] string Source);

internal sealed record ModifiedFileScope(
    [property: JsonPropertyName("evidence_id")] string EvidenceId,
    [property: JsonPropertyName("position")] int Position,
    [property: JsonPropertyName("path")] string Path);

internal sealed record ContractDetectionCoverageCandidate(
    IReadOnlyList<ModifiedFileScope> TotalScope,
    IReadOnlyList<ModifiedFileScope> ProcessedScope,
    IReadOnlyList<ModifiedFileScope> UncoveredScope,
    string Status,
    string? Cause);

internal sealed record ContractDetectionCoverage(
    [property: JsonPropertyName("total_scope")] IReadOnlyList<ModifiedFileScope> TotalScope,
    [property: JsonPropertyName("processed_scope")] IReadOnlyList<ModifiedFileScope> ProcessedScope,
    [property: JsonPropertyName("uncovered_scope")] IReadOnlyList<ModifiedFileScope> UncoveredScope,
    [property: JsonPropertyName("coverage_status")] string Status,
    [property: JsonPropertyName("cause"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Cause);
