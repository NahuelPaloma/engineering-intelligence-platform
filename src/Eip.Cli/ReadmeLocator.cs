using System.Text.Json;
using System.Text.Json.Serialization;

namespace Eip.Cli;

public static class ReadmeLocator
{
    private static readonly string[] AcceptedNames = ["README.md", "Readme.md", "readme.md"];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static async Task<string> WriteAsync(
        string manifestPath,
        string repositoryRoot,
        CancellationToken cancellationToken)
    {
        await using var manifestStream = File.OpenRead(manifestPath);
        var manifest = await JsonSerializer.DeserializeAsync<EvidenceManifest>(
            manifestStream,
            cancellationToken: cancellationToken)
            ?? throw new InvalidDataException("The evidence manifest is empty.");

        var root = Path.GetFullPath(repositoryRoot);
        var changedDirectories = manifest.ChangedFiles
            .Select(file => GetChangedDirectory(file.Path))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var candidates = new List<ReadmeCandidate>();
        var includedPaths = new HashSet<string>(StringComparer.Ordinal);

        foreach (var relativeDirectory in changedDirectories.Where(directory => directory.Length > 0))
        {
            AddReadmes(root, relativeDirectory, "changed file directory", candidates, includedPaths);
        }

        foreach (var relativeDirectory in changedDirectories.Where(directory => directory.Length > 0))
        {
            foreach (var ancestor in EnumerateAncestors(relativeDirectory))
            {
                var readmes = FindReadmes(root, ancestor);
                if (readmes.Length == 0)
                {
                    continue;
                }

                AddCandidates(readmes, "nearest ancestor of changed files", candidates, includedPaths);
                break;
            }
        }

        AddCandidates(FindReadmes(root, string.Empty), "repository root", candidates, includedPaths);

        var output = new ReadmeCandidates(candidates);
        var outputPath = Path.Combine(Path.GetDirectoryName(manifestPath)!, "readmes.json");
        var temporaryPath = $"{outputPath}.tmp";
        await using (var outputStream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(outputStream, output, JsonOptions, cancellationToken);
            await outputStream.WriteAsync("\n"u8.ToArray(), cancellationToken);
        }

        File.Move(temporaryPath, outputPath, true);
        return outputPath;
    }

    private static string GetChangedDirectory(string changedFilePath)
    {
        if (string.IsNullOrWhiteSpace(changedFilePath)
            || Path.IsPathRooted(changedFilePath))
        {
            throw new InvalidDataException("A changed file path is invalid.");
        }

        var segments = changedFilePath.Split('/');
        if (segments.Any(segment => segment is "" or "." or ".." || segment.Contains('\\')))
        {
            throw new InvalidDataException("A changed file path is invalid.");
        }

        return string.Join('/', segments.Take(segments.Length - 1));
    }

    private static IEnumerable<string> EnumerateAncestors(string relativeDirectory)
    {
        var segments = relativeDirectory.Split('/');
        for (var length = segments.Length - 1; length > 0; length--)
        {
            yield return string.Join('/', segments.Take(length));
        }
    }

    private static void AddReadmes(
        string root,
        string relativeDirectory,
        string reason,
        List<ReadmeCandidate> candidates,
        HashSet<string> includedPaths) =>
        AddCandidates(FindReadmes(root, relativeDirectory), reason, candidates, includedPaths);

    private static string[] FindReadmes(string root, string relativeDirectory)
    {
        var directory = relativeDirectory.Length == 0
            ? root
            : Path.Combine(root, relativeDirectory.Replace('/', Path.DirectorySeparatorChar));
        if (!Directory.Exists(directory))
        {
            return [];
        }

        var existingNames = Directory
            .EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .Where(name => name is not null)
            .ToHashSet(StringComparer.Ordinal);

        return AcceptedNames
            .Where(existingNames.Contains)
            .Select(name => relativeDirectory.Length == 0 ? name : $"{relativeDirectory}/{name}")
            .ToArray();
    }

    private static void AddCandidates(
        IEnumerable<string> paths,
        string reason,
        List<ReadmeCandidate> candidates,
        HashSet<string> includedPaths)
    {
        foreach (var path in paths)
        {
            if (includedPaths.Add(path))
            {
                candidates.Add(new ReadmeCandidate(path, reason));
            }
        }
    }
}

public sealed record ReadmeCandidates(
    [property: JsonPropertyName("candidate_readmes")] IReadOnlyList<ReadmeCandidate> CandidateReadmes);

public sealed record ReadmeCandidate(
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("reason")] string Reason);
