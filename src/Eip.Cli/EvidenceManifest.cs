using System.Text.Json.Serialization;

namespace Eip.Cli;

public sealed record EvidenceManifest(
    [property: JsonPropertyName("repository")] string Repository,
    [property: JsonPropertyName("owner")] string Owner,
    [property: JsonPropertyName("pull_request")] int PullRequest,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("author")] string Author,
    [property: JsonPropertyName("base_sha")] string BaseSha,
    [property: JsonPropertyName("head_sha")] string HeadSha,
    [property: JsonPropertyName("commits")] IReadOnlyList<CommitEvidence> Commits,
    [property: JsonPropertyName("changed_files")] IReadOnlyList<ChangedFileEvidence> ChangedFiles,
    [property: JsonPropertyName("generated_at")] DateTimeOffset GeneratedAt);

public sealed record CommitEvidence(
    [property: JsonPropertyName("sha")] string Sha,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("author")] string Author);

public sealed record ChangedFileEvidence(
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("previous_path")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? PreviousPath,
    [property: JsonPropertyName("additions")] int Additions,
    [property: JsonPropertyName("deletions")] int Deletions);
