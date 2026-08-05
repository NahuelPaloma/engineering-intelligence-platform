using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Eip.Cli;

public static class LocalContextBuilder
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.Ordinal)
    {
        "extracted",
        "missing_name",
        "missing_purpose",
        "insufficient"
    };

    private static readonly HashSet<string> AllowedRankingReasons = new(StringComparer.Ordinal)
    {
        "same_directory",
        "nearest_ancestor",
        "repository_root"
    };

    private static readonly Dictionary<string, string> ChangedFileStatusMappings =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["added"] = "added",
            ["modified"] = "modified",
            ["removed"] = "deleted",
            ["renamed"] = "renamed"
        };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static async Task<string> WriteAsync(
        string manifestPath,
        string readmesPath,
        string contentsPath,
        string metadataPath,
        string rankingPath,
        CancellationToken cancellationToken)
    {
        EnsureSameDirectory(manifestPath, readmesPath, contentsPath, metadataPath, rankingPath);

        var manifest = await ReadJsonAsync<EvidenceManifest>(manifestPath, "manifest", cancellationToken);
        var readmes = await ReadJsonAsync<ReadmeCandidates>(readmesPath, "README candidates", cancellationToken);
        var contents = await ReadJsonAsync<ReadmeDocuments>(contentsPath, "README contents", cancellationToken);
        var metadata = await ReadJsonAsync<ReadmeMetadataDocuments>(metadataPath, "README metadata", cancellationToken);
        var ranking = await ReadJsonAsync<ReadmeRankingDocuments>(rankingPath, "README ranking", cancellationToken);

        ValidateManifest(manifest);
        ValidateDocuments(readmes, contents, metadata, ranking);

        var modifiedFiles = BuildModifiedFiles(manifest);

        var contentsByPath = contents.Documents.ToDictionary(document => document.Path, StringComparer.Ordinal);
        var metadataByPath = metadata.Documents.ToDictionary(document => document.Path, StringComparer.Ordinal);
        var documents = ranking.Documents.Select(ranked =>
        {
            var content = contentsByPath[ranked.Path];
            var extracted = metadataByPath[ranked.Path];
            return new LocalContextDocument(
                ranked.Path,
                ranked.Score,
                ranked.Reason,
                extracted.Name,
                extracted.Purpose,
                content.Content,
                content.Error,
                extracted.Status,
                extracted.Evidence);
        }).ToArray();
        var output = new LocalContext(
            manifest.Repository,
            manifest.PullRequest,
            CreatePackId(manifest),
            modifiedFiles,
            documents);
        var outputPath = Path.Combine(Path.GetDirectoryName(manifestPath)!, "local-context.json");
        var temporaryPath = $"{outputPath}.tmp";

        await using (var outputStream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(outputStream, output, JsonOptions, cancellationToken);
            await outputStream.WriteAsync("\n"u8.ToArray(), cancellationToken);
        }

        File.Move(temporaryPath, outputPath, true);
        return outputPath;
    }

    private static async Task<T> ReadJsonAsync<T>(
        string path,
        string description,
        CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        try
        {
            return await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: cancellationToken)
                ?? throw new InvalidDataException($"The {description} file is empty.");
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException($"The {description} file is not valid JSON.", exception);
        }
    }

    private static void EnsureSameDirectory(params string[] paths)
    {
        var directories = paths
            .Select(path => Path.GetFullPath(Path.GetDirectoryName(path) ?? string.Empty))
            .Distinct(StringComparer.Ordinal)
            .Count();
        if (directories != 1)
        {
            throw new InvalidDataException("The local context inputs are incompatible.");
        }
    }

    private static void ValidateManifest(EvidenceManifest manifest)
    {
        if (string.IsNullOrWhiteSpace(manifest.Repository)
            || manifest.PullRequest <= 0
            || string.IsNullOrWhiteSpace(manifest.BaseSha)
            || string.IsNullOrWhiteSpace(manifest.HeadSha)
            || manifest.Commits is null
            || manifest.ChangedFiles is null)
        {
            throw new InvalidDataException("The manifest is malformed.");
        }
    }

    private static List<LocalContextModifiedFile> BuildModifiedFiles(EvidenceManifest manifest)
    {
        var modifiedFiles = new List<LocalContextModifiedFile>();
        var statusesByPath = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var changedFile in manifest.ChangedFiles)
        {
            if (changedFile is null
                || InvalidRepositoryRelativePath(changedFile.Path)
                || string.IsNullOrWhiteSpace(changedFile.Status)
                || !ChangedFileStatusMappings.TryGetValue(changedFile.Status, out var changeStatus))
            {
                throw new InvalidDataException("The manifest contains an invalid changed file.");
            }

            if (statusesByPath.TryGetValue(changedFile.Path, out var existingStatus))
            {
                if (existingStatus != changeStatus)
                {
                    throw new InvalidDataException("The manifest contains ambiguous changed files.");
                }

                continue;
            }

            statusesByPath.Add(changedFile.Path, changeStatus);
            modifiedFiles.Add(new LocalContextModifiedFile(
                changedFile.Path,
                changeStatus,
                new LocalContextModifiedFileProvenance(
                    "github",
                    manifest.Repository,
                    manifest.PullRequest,
                    "manifest.changed_files")));
        }

        return modifiedFiles;
    }

    private static void ValidateDocuments(
        ReadmeCandidates readmes,
        ReadmeDocuments contents,
        ReadmeMetadataDocuments metadata,
        ReadmeRankingDocuments ranking)
    {
        if (readmes.CandidateReadmes is null
            || contents.Documents is null
            || metadata.Documents is null
            || ranking.Documents is null
            || readmes.CandidateReadmes.Any(candidate => candidate is null || InvalidPath(candidate.Path))
            || contents.Documents.Any(document => document is null
                || InvalidPath(document.Path)
                || (document.Content is null) == (document.Error is null))
            || metadata.Documents.Any(document => document is null
                || InvalidPath(document.Path)
                || !AllowedStatuses.Contains(document.Status)
                || document.Evidence is null)
            || ranking.Documents.Any(document => document is null
                || InvalidPath(document.Path)
                || document.Score < 0
                || !AllowedRankingReasons.Contains(document.Reason)))
        {
            throw new InvalidDataException("The local context input is malformed.");
        }

        var expectedPaths = UniquePaths(readmes.CandidateReadmes.Select(candidate => candidate.Path));
        var contentPaths = UniquePaths(contents.Documents.Select(document => document.Path));
        var metadataPaths = UniquePaths(metadata.Documents.Select(document => document.Path));
        var rankingPaths = UniquePaths(ranking.Documents.Select(document => document.Path));
        if (!expectedPaths.SetEquals(contentPaths)
            || !expectedPaths.SetEquals(metadataPaths)
            || !expectedPaths.SetEquals(rankingPaths))
        {
            throw new InvalidDataException("The local context inputs are inconsistent.");
        }
    }

    private static HashSet<string> UniquePaths(IEnumerable<string> paths)
    {
        var values = paths.ToArray();
        var unique = values.ToHashSet(StringComparer.Ordinal);
        if (unique.Count != values.Length)
        {
            throw new InvalidDataException("The local context inputs contain duplicate documents.");
        }

        return unique;
    }

    private static bool InvalidPath(string path) => string.IsNullOrWhiteSpace(path);

    private static bool InvalidRepositoryRelativePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)
            || path[0] == '/'
            || path.Contains('\\')
            || path.Any(char.IsControl)
            || LooksLikeWindowsDrivePath(path))
        {
            return true;
        }

        var segments = path.Split('/');
        return segments.Any(segment => segment.Length == 0 || segment is "." or "..");
    }

    private static bool LooksLikeWindowsDrivePath(string path) =>
        path.Length >= 3
        && char.IsAsciiLetter(path[0])
        && path[1] == ':'
        && path[2] == '/';

    private static string CreatePackId(EvidenceManifest manifest)
    {
        var identity = $"{manifest.Repository}\n{manifest.PullRequest}\n{manifest.BaseSha}\n{manifest.HeadSha}";
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(identity)));
    }
}

public sealed record LocalContext(
    [property: JsonPropertyName("repository")] string Repository,
    [property: JsonPropertyName("pull_request")] int PullRequest,
    [property: JsonPropertyName("pack_id")] string PackId,
    [property: JsonPropertyName("modified_files")] IReadOnlyList<LocalContextModifiedFile> ModifiedFiles,
    [property: JsonPropertyName("documents")] IReadOnlyList<LocalContextDocument> Documents);

public sealed record LocalContextModifiedFile(
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("change_status")] string ChangeStatus,
    [property: JsonPropertyName("provenance")] LocalContextModifiedFileProvenance Provenance);

public sealed record LocalContextModifiedFileProvenance(
    [property: JsonPropertyName("provider")] string Provider,
    [property: JsonPropertyName("repository")] string Repository,
    [property: JsonPropertyName("pull_request")] int PullRequest,
    [property: JsonPropertyName("source")] string Source);

public sealed record LocalContextDocument(
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("score")] int Score,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("purpose")] string? Purpose,
    [property: JsonPropertyName("content")] string? Content,
    [property: JsonPropertyName("error")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Error,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("evidence")] IReadOnlyList<ReadmeMetadataEvidence> Evidence);
