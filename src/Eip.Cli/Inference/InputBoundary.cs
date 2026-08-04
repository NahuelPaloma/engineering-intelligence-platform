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
            return new InputBoundaryResult(
                root.GetProperty("pack_id").GetString()!,
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
            && segments.All(segment => segment.Length > 0 && segment is not "." and not "..");
    }

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
    IReadOnlyList<InputDocument> Documents);

internal sealed record InputDocument(
    string Path,
    string? Content,
    string? Error,
    string Status);
