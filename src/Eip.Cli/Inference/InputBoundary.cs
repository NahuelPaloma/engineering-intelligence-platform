using System.Buffers;
using System.Security.Cryptography;
using System.Text.Json;

namespace Eip.Cli.Inference;

internal static class InputBoundary
{
    internal const string ContractId = "local-context-v1";

    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.Ordinal)
    {
        "extracted",
        "missing_name",
        "missing_purpose",
        "insufficient"
    };

    private static readonly HashSet<string> AllowedReasons = new(StringComparer.Ordinal)
    {
        "same_directory",
        "nearest_ancestor",
        "repository_root"
    };

    private static readonly HashSet<string> AllowedEvidenceFields = new(StringComparer.Ordinal)
    {
        "name",
        "purpose"
    };

    private static readonly HashSet<string> AllowedChangeStatuses = new(StringComparer.Ordinal)
    {
        "added",
        "modified",
        "deleted",
        "renamed"
    };

    public static async Task<InputBoundaryResult> ReadAsync(
        string localContextPath,
        CancellationToken cancellationToken)
    {
        byte[] bytes;
        try
        {
            bytes = await File.ReadAllBytesAsync(localContextPath, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new InvalidDataException("The local context could not be read.");
        }

        try
        {
            using var document = JsonDocument.Parse(bytes);
            var root = document.RootElement;
            ValidateRoot(root);
            var modifiedFilesProvided = root.TryGetProperty("modified_files", out _);
            return new InputBoundaryResult(
                root.GetProperty("pack_id").GetString()!,
                CreateCanonicalContextIdentity(root, modifiedFilesProvided),
                modifiedFilesProvided ? "available" : "not_provided",
                ReadModifiedFiles(root),
                ReadDocuments(root.GetProperty("documents")));
        }
        catch (JsonException)
        {
            throw new InvalidDataException("The local context is not valid JSON.");
        }
    }

    private static void ValidateRoot(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object
            || !RequiredString(root, "repository")
            || !RequiredPositiveInteger(root, "pull_request")
            || !RequiredPackId(root)
            || !root.TryGetProperty("documents", out var documents)
            || documents.ValueKind != JsonValueKind.Array
            || !SupportedContractVersion(root))
        {
            throw new InvalidDataException("The local context is incompatible.");
        }

        var paths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in documents.EnumerateArray())
        {
            ValidateDocument(item);
            if (!paths.Add(item.GetProperty("path").GetString()!))
            {
                throw new InvalidDataException("The local context contains ambiguous document identities.");
            }
        }

        ValidateModifiedFiles(root);
    }

    private static void ValidateModifiedFiles(JsonElement root)
    {
        if (!root.TryGetProperty("modified_files", out var modifiedFiles))
        {
            return;
        }

        if (modifiedFiles.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidDataException("The local context is incompatible.");
        }

        var paths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in modifiedFiles.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object
                || !RequiredRelativePath(item, "path")
                || !RequiredAllowedString(item, "change_status", AllowedChangeStatuses)
                || !item.TryGetProperty("provenance", out var provenance)
                || provenance.ValueKind != JsonValueKind.Object
                || !RequiredString(provenance, "provider")
                || !RequiredString(provenance, "repository")
                || !RequiredPositiveInteger(provenance, "pull_request")
                || !RequiredString(provenance, "source")
                || provenance.GetProperty("repository").GetString()
                    != root.GetProperty("repository").GetString()
                || provenance.GetProperty("pull_request").GetInt32()
                    != root.GetProperty("pull_request").GetInt32()
                || provenance.GetProperty("source").GetString() != "manifest.changed_files")
            {
                throw new InvalidDataException("The local context is incompatible.");
            }

            if (!paths.Add(item.GetProperty("path").GetString()!))
            {
                throw new InvalidDataException("The local context contains ambiguous modified file identities.");
            }
        }
    }

    private static InputModifiedFile[] ReadModifiedFiles(JsonElement root)
    {
        if (!root.TryGetProperty("modified_files", out var modifiedFiles))
        {
            return [];
        }

        return modifiedFiles
            .EnumerateArray()
            .Select(item => new InputModifiedFile(
                item.GetProperty("path").GetString()!,
                item.GetProperty("change_status").GetString()!,
                new InputModifiedFileProvenance(
                    item.GetProperty("provenance").GetProperty("provider").GetString()!,
                    item.GetProperty("provenance").GetProperty("repository").GetString()!,
                    item.GetProperty("provenance").GetProperty("pull_request").GetInt32(),
                    item.GetProperty("provenance").GetProperty("source").GetString()!)))
            .ToArray();
    }

    private static InputDocument[] ReadDocuments(JsonElement documents) =>
        documents
            .EnumerateArray()
            .Select(document => new InputDocument(
                document.GetProperty("path").GetString()!,
                document.GetProperty("content").ValueKind == JsonValueKind.Null
                    ? null
                    : document.GetProperty("content").GetString(),
                document.TryGetProperty("error", out var error) ? error.GetString() : null,
                document.GetProperty("status").GetString()!))
            .OrderBy(document => document.Path, StringComparer.Ordinal)
            .ToArray();

    private static string CreateCanonicalContextIdentity(
        JsonElement root,
        bool modifiedFilesProvided)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteString("repository", root.GetProperty("repository").GetString());
            writer.WriteNumber("pull_request", root.GetProperty("pull_request").GetInt32());
            writer.WriteString("pack_id", root.GetProperty("pack_id").GetString());
            writer.WriteString(
                "modified_files_availability",
                modifiedFilesProvided ? "available" : "not_provided");
            if (modifiedFilesProvided)
            {
                WriteCanonicalModifiedFiles(writer, root.GetProperty("modified_files"));
            }

            WriteCanonicalDocuments(writer, root.GetProperty("documents"));
            writer.WriteEndObject();
        }

        return Convert.ToHexStringLower(SHA256.HashData(buffer.WrittenSpan));
    }

    private static void WriteCanonicalModifiedFiles(Utf8JsonWriter writer, JsonElement modifiedFiles)
    {
        writer.WritePropertyName("modified_files");
        writer.WriteStartArray();
        foreach (var item in modifiedFiles.EnumerateArray())
        {
            writer.WriteStartObject();
            writer.WriteString("path", item.GetProperty("path").GetString());
            writer.WriteString("change_status", item.GetProperty("change_status").GetString());
            writer.WritePropertyName("provenance");
            writer.WriteStartObject();
            var provenance = item.GetProperty("provenance");
            writer.WriteString("provider", provenance.GetProperty("provider").GetString());
            writer.WriteString("repository", provenance.GetProperty("repository").GetString());
            writer.WriteNumber("pull_request", provenance.GetProperty("pull_request").GetInt32());
            writer.WriteString("source", provenance.GetProperty("source").GetString());
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static void WriteCanonicalDocuments(Utf8JsonWriter writer, JsonElement documents)
    {
        writer.WritePropertyName("documents");
        writer.WriteStartArray();
        foreach (var document in documents.EnumerateArray())
        {
            writer.WriteStartObject();
            writer.WriteString("path", document.GetProperty("path").GetString());
            writer.WriteNumber("score", document.GetProperty("score").GetInt32());
            writer.WriteString("reason", document.GetProperty("reason").GetString());
            WriteNullableString(writer, "name", document.GetProperty("name"));
            WriteNullableString(writer, "purpose", document.GetProperty("purpose"));
            WriteNullableString(writer, "content", document.GetProperty("content"));
            if (document.TryGetProperty("error", out var error))
            {
                writer.WriteString("error", error.GetString());
            }

            writer.WriteString("status", document.GetProperty("status").GetString());
            writer.WritePropertyName("evidence");
            writer.WriteStartArray();
            foreach (var evidence in document.GetProperty("evidence").EnumerateArray())
            {
                writer.WriteStartObject();
                writer.WriteString("field", evidence.GetProperty("field").GetString());
                writer.WriteString("text", evidence.GetProperty("text").GetString());
                writer.WriteNumber(
                    "source_line_start",
                    evidence.GetProperty("source_line_start").GetInt32());
                writer.WriteNumber(
                    "source_line_end",
                    evidence.GetProperty("source_line_end").GetInt32());
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static void WriteNullableString(
        Utf8JsonWriter writer,
        string propertyName,
        JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Null)
        {
            writer.WriteNull(propertyName);
        }
        else
        {
            writer.WriteString(propertyName, value.GetString());
        }
    }

    private static bool SupportedContractVersion(JsonElement root)
    {
        if (!root.TryGetProperty("contract_version", out var version))
        {
            return true;
        }

        return version.ValueKind == JsonValueKind.Number
            && version.TryGetInt32(out var value)
            && value == 1;
    }

    private static void ValidateDocument(JsonElement document)
    {
        if (document.ValueKind != JsonValueKind.Object
            || !RequiredRelativePath(document, "path")
            || !RequiredNonNegativeInteger(document, "score")
            || !RequiredAllowedString(document, "reason", AllowedReasons)
            || !RequiredNullableString(document, "name")
            || !RequiredNullableString(document, "purpose")
            || !RequiredNullableString(document, "content")
            || !RequiredAllowedString(document, "status", AllowedStatuses)
            || !document.TryGetProperty("evidence", out var evidence)
            || evidence.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidDataException("The local context is incompatible.");
        }

        var contentIsNull = document.GetProperty("content").ValueKind == JsonValueKind.Null;
        var hasError = document.TryGetProperty("error", out var error);
        if (contentIsNull != hasError
            || (hasError && (error.ValueKind != JsonValueKind.String
                || string.IsNullOrWhiteSpace(error.GetString()))))
        {
            throw new InvalidDataException("The local context is incompatible.");
        }

        foreach (var item in evidence.EnumerateArray())
        {
            ValidateEvidence(item);
        }
    }

    private static void ValidateEvidence(JsonElement evidence)
    {
        if (evidence.ValueKind != JsonValueKind.Object
            || !RequiredAllowedString(evidence, "field", AllowedEvidenceFields)
            || !RequiredString(evidence, "text")
            || !RequiredPositiveInteger(evidence, "source_line_start")
            || !RequiredPositiveInteger(evidence, "source_line_end")
            || evidence.GetProperty("source_line_end").GetInt32()
                < evidence.GetProperty("source_line_start").GetInt32())
        {
            throw new InvalidDataException("The local context is incompatible.");
        }
    }

    private static bool RequiredString(JsonElement parent, string propertyName) =>
        parent.TryGetProperty(propertyName, out var value)
        && value.ValueKind == JsonValueKind.String
        && !string.IsNullOrWhiteSpace(value.GetString());

    private static bool RequiredRelativePath(JsonElement parent, string propertyName)
    {
        if (!RequiredString(parent, propertyName))
        {
            return false;
        }

        var path = parent.GetProperty(propertyName).GetString()!;
        var segments = path.Split('/');
        return path[0] != '/'
            && !path.Contains('\\')
            && !path.Any(char.IsControl)
            && !LooksLikeWindowsDrivePath(path)
            && segments.All(segment => segment.Length > 0 && segment is not "." and not "..");
    }

    private static bool LooksLikeWindowsDrivePath(string path) =>
        path.Length >= 3
        && char.IsAsciiLetter(path[0])
        && path[1] == ':'
        && path[2] == '/';

    private static bool RequiredNullableString(JsonElement parent, string propertyName) =>
        parent.TryGetProperty(propertyName, out var value)
        && value.ValueKind is JsonValueKind.String or JsonValueKind.Null;

    private static bool RequiredPositiveInteger(JsonElement parent, string propertyName) =>
        parent.TryGetProperty(propertyName, out var value)
        && value.ValueKind == JsonValueKind.Number
        && value.TryGetInt32(out var number)
        && number > 0;

    private static bool RequiredNonNegativeInteger(JsonElement parent, string propertyName) =>
        parent.TryGetProperty(propertyName, out var value)
        && value.ValueKind == JsonValueKind.Number
        && value.TryGetInt32(out var number)
        && number >= 0;

    private static bool RequiredAllowedString(
        JsonElement parent,
        string propertyName,
        HashSet<string> allowed) =>
        RequiredString(parent, propertyName)
        && allowed.Contains(parent.GetProperty(propertyName).GetString()!);

    private static bool RequiredPackId(JsonElement root)
    {
        if (!RequiredString(root, "pack_id"))
        {
            return false;
        }

        var packId = root.GetProperty("pack_id").GetString()!;
        return packId.Length == 64
            && packId.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f');
    }
}

internal sealed record InputBoundaryResult(
    string PackId,
    string CanonicalContextIdentity,
    string ModifiedFilesAvailability,
    IReadOnlyList<InputModifiedFile> ModifiedFiles,
    IReadOnlyList<InputDocument> Documents);

internal sealed record InputModifiedFile(
    string Path,
    string ChangeStatus,
    InputModifiedFileProvenance Provenance);

internal sealed record InputModifiedFileProvenance(
    string Provider,
    string Repository,
    int PullRequest,
    string Source);

internal sealed record InputDocument(
    string Path,
    string? Content,
    string? Error,
    string Status);
