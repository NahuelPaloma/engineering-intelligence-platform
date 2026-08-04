using System.Security;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Eip.Cli;

public static class ReadmeContentReader
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static async Task<string> WriteAsync(
        string readmesPath,
        string repositoryRoot,
        CancellationToken cancellationToken)
    {
        ReadmeCandidates readmes;
        await using (var readmesStream = File.OpenRead(readmesPath))
        {
            try
            {
                readmes = await JsonSerializer.DeserializeAsync<ReadmeCandidates>(
                    readmesStream,
                    cancellationToken: cancellationToken)
                    ?? throw new InvalidDataException("The README candidate file is empty.");
            }
            catch (JsonException exception)
            {
                throw new InvalidDataException("The README candidate file is not valid JSON.", exception);
            }
        }

        if (readmes.CandidateReadmes is null)
        {
            throw new InvalidDataException("The README candidate file is missing candidate_readmes.");
        }

        var root = Path.GetFullPath(repositoryRoot);
        var documents = new List<ReadmeDocument>(readmes.CandidateReadmes.Count);

        foreach (var candidate in readmes.CandidateReadmes)
        {
            documents.Add(await ReadDocumentAsync(candidate.Path, root, cancellationToken));
        }

        var output = new ReadmeDocuments(documents);
        var outputPath = Path.Combine(Path.GetDirectoryName(readmesPath)!, "readme-contents.json");
        var temporaryPath = $"{outputPath}.tmp";
        await using (var outputStream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(outputStream, output, JsonOptions, cancellationToken);
            await outputStream.WriteAsync("\n"u8.ToArray(), cancellationToken);
        }

        File.Move(temporaryPath, outputPath, true);
        return outputPath;
    }

    private static async Task<ReadmeDocument> ReadDocumentAsync(
        string candidatePath,
        string root,
        CancellationToken cancellationToken)
    {
        if (!TryGetSegments(candidatePath, out var segments))
        {
            return ReadmeDocument.Failed(candidatePath);
        }

        try
        {
            if (!TryResolvePhysicalPath(root, segments, out var path))
            {
                return ReadmeDocument.Failed(candidatePath);
            }

            var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
            var content = StrictUtf8.GetString(bytes);
            return ReadmeDocument.Succeeded(candidatePath, content);
        }
        catch (Exception exception) when (exception is IOException
            or UnauthorizedAccessException
            or DecoderFallbackException
            or ArgumentException
            or NotSupportedException
            or SecurityException)
        {
            return ReadmeDocument.Failed(candidatePath);
        }
    }

    private static bool TryGetSegments(string candidatePath, out string[] segments)
    {
        segments = [];
        if (string.IsNullOrWhiteSpace(candidatePath) || Path.IsPathRooted(candidatePath))
        {
            return false;
        }

        segments = candidatePath.Split('/');
        if (segments.Any(segment => segment is "" or "." or ".." || segment.Contains('\\'))
            || segments[^1] is not ("README.md" or "Readme.md" or "readme.md"))
        {
            return false;
        }

        return true;
    }

    private static bool TryResolvePhysicalPath(string root, string[] segments, out string path)
    {
        path = string.Empty;
        var rootDirectory = new DirectoryInfo(root);
        if (!IsNormalDirectory(rootDirectory))
        {
            return false;
        }

        var currentDirectory = root;
        foreach (var segment in segments[..^1])
        {
            currentDirectory = Path.Combine(currentDirectory, segment);
            if (!IsNormalDirectory(new DirectoryInfo(currentDirectory)))
            {
                return false;
            }
        }

        path = Path.Combine(currentDirectory, segments[^1]);
        var file = new FileInfo(path);
        return file.Exists && !IsLinkOrReparsePoint(file);
    }

    private static bool IsNormalDirectory(DirectoryInfo directory) =>
        directory.Exists && !IsLinkOrReparsePoint(directory);

    private static bool IsLinkOrReparsePoint(FileSystemInfo entry) =>
        entry.LinkTarget is not null || (entry.Attributes & FileAttributes.ReparsePoint) != 0;
}

public sealed record ReadmeDocuments(
    [property: JsonPropertyName("documents")] IReadOnlyList<ReadmeDocument> Documents);

public sealed record ReadmeDocument(
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("content")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Content,
    [property: JsonPropertyName("error")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Error)
{
    public static ReadmeDocument Succeeded(string path, string content) => new(path, content, null);

    public static ReadmeDocument Failed(string path) =>
        new(path, null, "The document could not be read.");
}
