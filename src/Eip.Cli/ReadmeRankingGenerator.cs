using System.Text.Json;
using System.Text.Json.Serialization;

namespace Eip.Cli;

public static class ReadmeRankingGenerator
{
    private const string SameDirectoryDiscoveryReason = "changed file directory";
    private const string NearestAncestorDiscoveryReason = "nearest ancestor of changed files";
    private const string RepositoryRootDiscoveryReason = "repository root";

    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.Ordinal)
    {
        "extracted",
        "missing_name",
        "missing_purpose",
        "insufficient"
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static async Task<string> WriteAsync(
        string metadataPath,
        string readmesPath,
        CancellationToken cancellationToken)
    {
        var metadata = await ReadJsonAsync<ReadmeMetadataDocuments>(
            metadataPath,
            "README metadata",
            cancellationToken);
        var readmes = await ReadJsonAsync<ReadmeCandidates>(
            readmesPath,
            "README candidates",
            cancellationToken);

        ValidateInputs(metadata, readmes);

        var sameDirectories = readmes.CandidateReadmes
            .Where(candidate => candidate.Reason is SameDirectoryDiscoveryReason)
            .Select(candidate => GetDirectory(candidate.Path))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var ancestorDirectories = readmes.CandidateReadmes
            .Where(candidate => candidate.Reason is NearestAncestorDiscoveryReason)
            .Select(candidate => GetDirectory(candidate.Path))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var documents = readmes.CandidateReadmes
            .Select(candidate => Rank(candidate, sameDirectories, ancestorDirectories))
            .OrderByDescending(document => document.Score)
            .ThenBy(document => document.Path, StringComparer.Ordinal)
            .ToArray();
        var output = new ReadmeRankingDocuments(documents);
        var outputPath = Path.Combine(Path.GetDirectoryName(metadataPath)!, "readme-ranking.json");
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

    private static void ValidateInputs(ReadmeMetadataDocuments metadata, ReadmeCandidates readmes)
    {
        if (metadata.Documents is null
            || readmes.CandidateReadmes is null
            || metadata.Documents.Any(document => document is null
                || string.IsNullOrWhiteSpace(document.Path)
                || !AllowedStatuses.Contains(document.Status))
            || readmes.CandidateReadmes.Any(candidate => candidate is null
                || string.IsNullOrWhiteSpace(candidate.Path)
                || candidate.Reason is not (SameDirectoryDiscoveryReason
                    or NearestAncestorDiscoveryReason
                    or RepositoryRootDiscoveryReason)))
        {
            throw new InvalidDataException("The README ranking input is malformed.");
        }

        var metadataPaths = metadata.Documents.Select(document => document.Path).ToArray();
        var candidatePaths = readmes.CandidateReadmes.Select(candidate => candidate.Path).ToArray();
        if (metadataPaths.Distinct(StringComparer.Ordinal).Count() != metadataPaths.Length
            || candidatePaths.Distinct(StringComparer.Ordinal).Count() != candidatePaths.Length
            || !metadataPaths.Order(StringComparer.Ordinal).SequenceEqual(
                candidatePaths.Order(StringComparer.Ordinal),
                StringComparer.Ordinal))
        {
            throw new InvalidDataException("The README ranking inputs are inconsistent.");
        }
    }

    private static ReadmeRankingDocument Rank(
        ReadmeCandidate candidate,
        IReadOnlyList<string> sameDirectories,
        IReadOnlyList<string> ancestorDirectories)
    {
        return candidate.Reason switch
        {
            SameDirectoryDiscoveryReason => new ReadmeRankingDocument(
                candidate.Path,
                100,
                "same_directory"),
            NearestAncestorDiscoveryReason => new ReadmeRankingDocument(
                candidate.Path,
                CalculateAncestorScore(
                    GetDirectory(candidate.Path),
                    sameDirectories,
                    ancestorDirectories),
                "nearest_ancestor"),
            RepositoryRootDiscoveryReason => new ReadmeRankingDocument(
                candidate.Path,
                10,
                "repository_root"),
            _ => throw new InvalidDataException("The README discovery reason is not supported.")
        };
    }

    private static int CalculateAncestorScore(
        string ancestorDirectory,
        IReadOnlyList<string> sameDirectories,
        IReadOnlyList<string> ancestorDirectories)
    {
        var distanceFromChangedDirectory = sameDirectories
            .Where(directory => IsStrictAncestor(ancestorDirectory, directory))
            .Select(directory => GetDepth(directory) - GetDepth(ancestorDirectory))
            .DefaultIfEmpty(int.MaxValue)
            .Min();
        if (distanceFromChangedDirectory != int.MaxValue)
        {
            return Math.Max(0, 100 - (distanceFromChangedDirectory * 20));
        }

        var levelsAboveNearest = ancestorDirectories
            .Where(directory => IsStrictAncestor(ancestorDirectory, directory))
            .Select(directory => GetDepth(directory) - GetDepth(ancestorDirectory))
            .DefaultIfEmpty(0)
            .Max();

        return Math.Max(0, 80 - (levelsAboveNearest * 20));
    }

    private static bool IsStrictAncestor(string ancestor, string directory)
    {
        if (ancestor.Length == 0)
        {
            return directory.Length > 0;
        }

        return directory.StartsWith($"{ancestor}/", StringComparison.Ordinal);
    }

    private static int GetDepth(string directory) =>
        directory.Length == 0 ? 0 : directory.Count(character => character == '/') + 1;

    private static string GetDirectory(string path)
    {
        var separator = path.LastIndexOf('/');
        return separator < 0 ? string.Empty : path[..separator];
    }
}

public sealed record ReadmeRankingDocuments(
    [property: JsonPropertyName("documents")] IReadOnlyList<ReadmeRankingDocument> Documents);

public sealed record ReadmeRankingDocument(
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("score")] int Score,
    [property: JsonPropertyName("reason")] string Reason);
