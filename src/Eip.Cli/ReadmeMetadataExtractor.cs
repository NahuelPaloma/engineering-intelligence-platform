using System.Text.Json;
using System.Text.Json.Serialization;

namespace Eip.Cli;

public static class ReadmeMetadataExtractor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static async Task<string> WriteAsync(
        string readmeContentsPath,
        CancellationToken cancellationToken)
    {
        ReadmeDocuments input;
        await using (var inputStream = File.OpenRead(readmeContentsPath))
        {
            try
            {
                input = await JsonSerializer.DeserializeAsync<ReadmeDocuments>(
                    inputStream,
                    cancellationToken: cancellationToken)
                    ?? throw new InvalidDataException("The README content file is empty.");
            }
            catch (JsonException exception)
            {
                throw new InvalidDataException("The README content file is not valid JSON.", exception);
            }
        }

        if (input.Documents is null
            || input.Documents.Any(document => document is null || string.IsNullOrWhiteSpace(document.Path)))
        {
            throw new InvalidDataException("The README content file is malformed.");
        }

        var documents = input.Documents.Select(Extract).ToArray();
        var output = new ReadmeMetadataDocuments(documents);
        var outputPath = Path.Combine(
            Path.GetDirectoryName(readmeContentsPath)!,
            "readme-metadata.json");
        var temporaryPath = $"{outputPath}.tmp";
        await using (var outputStream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(outputStream, output, JsonOptions, cancellationToken);
            await outputStream.WriteAsync("\n"u8.ToArray(), cancellationToken);
        }

        File.Move(temporaryPath, outputPath, true);
        return outputPath;
    }

    private static ReadmeMetadataDocument Extract(ReadmeDocument document)
    {
        if (document.Content is null || document.Error is not null)
        {
            return CreateResult(document.Path, null, null, []);
        }

        var lines = SplitLines(document.Content);
        var name = FindName(lines);
        var purpose = FindPurpose(lines, name?.LineEnd ?? 0);
        var evidence = new List<ReadmeMetadataEvidence>(2);

        if (name is not null)
        {
            evidence.Add(new ReadmeMetadataEvidence("name", name.Text, name.LineStart, name.LineEnd));
        }

        if (purpose is not null)
        {
            evidence.Add(new ReadmeMetadataEvidence(
                "purpose",
                purpose.Text,
                purpose.LineStart,
                purpose.LineEnd));
        }

        return CreateResult(document.Path, name?.Text, purpose?.Text, evidence);
    }

    private static ReadmeMetadataDocument CreateResult(
        string path,
        string? name,
        string? purpose,
        IReadOnlyList<ReadmeMetadataEvidence> evidence)
    {
        var status = (name, purpose) switch
        {
            (not null, not null) => "extracted",
            (null, not null) => "missing_name",
            (not null, null) => "missing_purpose",
            _ => "insufficient"
        };

        return new ReadmeMetadataDocument(path, name, purpose, status, evidence);
    }

    private static string[] SplitLines(string content) =>
        content.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Split('\n');

    private static ExtractedField? FindName(string[] lines)
    {
        var fenced = ComputeFencedLines(lines);

        for (var index = 0; index < lines.Length; index++)
        {
            if (fenced[index])
            {
                continue;
            }

            var value = TryParseAtxH1(lines[index]);
            if (value is not null)
            {
                return new ExtractedField(value, index + 1, index + 1);
            }
        }

        for (var index = 0; index < lines.Length; index++)
        {
            if (fenced[index])
            {
                continue;
            }

            var line = lines[index].Trim();
            if (line.StartsWith("Title:", StringComparison.OrdinalIgnoreCase))
            {
                var value = line["Title:".Length..].Trim();
                if (value.Length > 0)
                {
                    return new ExtractedField(value, index + 1, index + 1);
                }
            }

            if (line.Length > 0
                && index + 1 < lines.Length
                && IsSetextH1Underline(lines[index + 1]))
            {
                return new ExtractedField(line, index + 1, index + 2);
            }
        }

        return null;
    }

    private static bool[] ComputeFencedLines(string[] lines)
    {
        var fenced = new bool[lines.Length];
        string? activeFence = null;

        for (var index = 0; index < lines.Length; index++)
        {
            var trimmed = lines[index].TrimStart();
            if (activeFence is null)
            {
                if (trimmed.StartsWith("```", StringComparison.Ordinal))
                {
                    activeFence = "```";
                    fenced[index] = true;
                }
                else if (trimmed.StartsWith("~~~", StringComparison.Ordinal))
                {
                    activeFence = "~~~";
                    fenced[index] = true;
                }
            }
            else
            {
                fenced[index] = true;
                if (trimmed.StartsWith(activeFence, StringComparison.Ordinal))
                {
                    activeFence = null;
                }
            }
        }

        return fenced;
    }

    private static ExtractedField? FindPurpose(string[] lines, int titleLineEnd)
    {
        var index = titleLineEnd;
        while (index < lines.Length && (string.IsNullOrWhiteSpace(lines[index]) || IsBadge(lines[index])))
        {
            index++;
        }

        if (index >= lines.Length || IsHeading(lines[index]) || IsStructuralLine(lines[index]))
        {
            return null;
        }

        var start = index;
        var paragraph = new List<string>();
        while (index < lines.Length
            && !string.IsNullOrWhiteSpace(lines[index])
            && !IsHeading(lines[index])
            && !IsStructuralLine(lines[index]))
        {
            paragraph.Add(lines[index].Trim());
            index++;
        }

        var text = string.Join(' ', paragraph).Trim();
        return text.Length == 0 ? null : new ExtractedField(text, start + 1, index);
    }

    private static string? TryParseAtxH1(string line)
    {
        var trimmed = line.Trim();
        if (!trimmed.StartsWith("# ", StringComparison.Ordinal))
        {
            return null;
        }

        var value = trimmed[2..].Trim().TrimEnd('#').TrimEnd();
        return value.Length == 0 ? null : value;
    }

    private static bool IsHeading(string line)
    {
        var trimmed = line.TrimStart();
        return trimmed.StartsWith('#') || IsSetextH1Underline(line);
    }

    private static bool IsSetextH1Underline(string line)
    {
        var trimmed = line.Trim();
        return trimmed.Length > 0 && trimmed.All(character => character == '=');
    }

    private static bool IsBadge(string line)
    {
        var trimmed = line.Trim();
        return trimmed.StartsWith("![", StringComparison.Ordinal)
            || trimmed.StartsWith("[![", StringComparison.Ordinal)
            || trimmed.StartsWith("<img", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsStructuralLine(string line)
    {
        var trimmed = line.TrimStart();
        return trimmed.StartsWith("```", StringComparison.Ordinal)
            || trimmed.StartsWith("~~~", StringComparison.Ordinal)
            || trimmed.StartsWith('>')
            || trimmed.StartsWith("- ", StringComparison.Ordinal)
            || trimmed.StartsWith("* ", StringComparison.Ordinal)
            || trimmed.StartsWith("+ ", StringComparison.Ordinal)
            || (trimmed.Length > 2 && char.IsDigit(trimmed[0]) && trimmed[1..].StartsWith(". ", StringComparison.Ordinal));
    }

    private sealed record ExtractedField(string Text, int LineStart, int LineEnd);
}

public sealed record ReadmeMetadataDocuments(
    [property: JsonPropertyName("documents")] IReadOnlyList<ReadmeMetadataDocument> Documents);

public sealed record ReadmeMetadataDocument(
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("purpose")] string? Purpose,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("evidence")] IReadOnlyList<ReadmeMetadataEvidence> Evidence);

public sealed record ReadmeMetadataEvidence(
    [property: JsonPropertyName("field")] string Field,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("source_line_start")] int SourceLineStart,
    [property: JsonPropertyName("source_line_end")] int SourceLineEnd);
