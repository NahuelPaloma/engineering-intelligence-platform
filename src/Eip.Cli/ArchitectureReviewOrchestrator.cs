using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Eip.Cli;

internal static class ArchitectureReviewOrchestrator
{
    public static async Task<string> WriteAsync(
        string inferenceReportPath,
        CancellationToken cancellationToken)
    {
        InferenceReportInput report;
        try
        {
            await using var stream = File.OpenRead(inferenceReportPath);
            report = await JsonSerializer.DeserializeAsync<InferenceReportInput>(
                stream,
                cancellationToken: cancellationToken)
                ?? throw new InvalidDataException("The inference report is invalid.");
        }
        catch (JsonException)
        {
            throw new InvalidDataException("The inference report is invalid.");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new InvalidDataException("The inference report could not be read.");
        }

        Validate(report);
        var directory = Path.GetDirectoryName(inferenceReportPath)
            ?? throw new InvalidDataException("The inference report location is invalid.");
        var outputPath = Path.Combine(directory, "context-pack.md");
        var temporaryPath = $"{outputPath}.tmp";
        var content = Render(report);
        try
        {
            await File.WriteAllTextAsync(
                temporaryPath,
                content,
                new UTF8Encoding(false),
                cancellationToken);
            File.Move(temporaryPath, outputPath, true);
            return outputPath;
        }
        catch (OperationCanceledException)
        {
            TryDelete(temporaryPath);
            throw;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            TryDelete(temporaryPath);
            throw new IOException("The architecture review could not be written.");
        }
    }

    private static void Validate(InferenceReportInput report)
    {
        if (string.IsNullOrWhiteSpace(report.ReportId)
            || string.IsNullOrWhiteSpace(report.ExecutionId)
            || report.Status is not ("complete" or "incomplete")
            || report.Findings is null
            || report.Findings.Any(item => item.Status != "valid"
                || string.IsNullOrWhiteSpace(item.FindingId)
                || string.IsNullOrWhiteSpace(item.Category)
                || string.IsNullOrWhiteSpace(item.Statement)
                || item.Confidence is null
                || string.IsNullOrWhiteSpace(item.Confidence.Level)
                || item.Uncertainty is null
                || item.OpenQuestions is null
                || item.EvidenceIds is null
                || item.ApplicabilityLimits is null))
        {
            throw new InvalidDataException("The inference report is invalid.");
        }
    }

    private static string Render(InferenceReportInput report)
    {
        var findings = report.Findings!;
        var reviewStatus = findings.Count == 0
            ? "insufficient"
            : report.Status == "complete" ? "complete" : "partial";
        var builder = new StringBuilder();
        builder.AppendLine("# Architecture Review Context Pack");
        builder.AppendLine();
        builder.AppendLine(CultureInfo.InvariantCulture, $"- Review status: `{reviewStatus}`");
        builder.AppendLine(CultureInfo.InvariantCulture, $"- Inference report: `{report.ReportId}`");
        builder.AppendLine(CultureInfo.InvariantCulture, $"- Execution: `{report.ExecutionId}`");
        builder.AppendLine();
        builder.AppendLine("## Findings");
        builder.AppendLine();
        if (findings.Count == 0)
        {
            builder.AppendLine("No validated Findings were available for this review.");
        }
        else
        {
            foreach (var finding in findings)
            {
                builder.AppendLine(CultureInfo.InvariantCulture, $"### {finding.Category} — `{finding.FindingId}`");
                builder.AppendLine();
                builder.AppendLine(finding.Statement);
                builder.AppendLine();
                builder.AppendLine(CultureInfo.InvariantCulture, $"- Confidence: `{finding.Confidence!.Level}`");
                builder.AppendLine(CultureInfo.InvariantCulture, $"- Evidence: {FormatReferences(finding.EvidenceIds!)}");
                builder.AppendLine(CultureInfo.InvariantCulture, $"- Uncertainty: {FormatValues(finding.Uncertainty!)}");
                builder.AppendLine(CultureInfo.InvariantCulture, $"- Open questions: {FormatValues(finding.OpenQuestions!)}");
                builder.AppendLine(CultureInfo.InvariantCulture, $"- Applicability limits: {FormatValues(finding.ApplicabilityLimits!)}");
                builder.AppendLine();
            }
        }

        if (findings.Count == 0 && report.Abstentions is { Count: > 0 })
        {
            builder.AppendLine("## Abstention");
            builder.AppendLine();
            foreach (var abstention in report.Abstentions)
            {
                builder.AppendLine(CultureInfo.InvariantCulture, $"- Type: `{abstention.Type}`");
                builder.AppendLine(CultureInfo.InvariantCulture, $"- Condition: {abstention.Condition}");
                if (!string.IsNullOrWhiteSpace(abstention.MissingEvidenceDescription))
                {
                    builder.AppendLine(
                        CultureInfo.InvariantCulture,
                        $"- Missing evidence: {abstention.MissingEvidenceDescription}");
                }

                builder.AppendLine();
            }
        }

        builder.AppendLine("## Decision boundary");
        builder.AppendLine();
        builder.AppendLine("This context pack does not approve or reject the change. The reviewer retains the decision.");
        return builder.ToString();
    }

    private static string FormatReferences(IReadOnlyList<string> values) =>
        values.Count == 0 ? "none" : string.Join(", ", values.Select(value => $"`{value}`"));

    private static string FormatValues(IReadOnlyList<string> values) =>
        values.Count == 0 ? "none" : string.Join("; ", values);

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // Preserve the original failure without exposing filesystem details.
        }
    }

    private sealed record InferenceReportInput(
        [property: System.Text.Json.Serialization.JsonPropertyName("report_id")] string? ReportId,
        [property: System.Text.Json.Serialization.JsonPropertyName("execution_id")] string? ExecutionId,
        [property: System.Text.Json.Serialization.JsonPropertyName("status")] string? Status,
        [property: System.Text.Json.Serialization.JsonPropertyName("findings")] IReadOnlyList<FindingInput>? Findings,
        [property: System.Text.Json.Serialization.JsonPropertyName("abstentions")] IReadOnlyList<AbstentionInput>? Abstentions);

    private sealed record FindingInput(
        [property: System.Text.Json.Serialization.JsonPropertyName("finding_id")] string? FindingId,
        [property: System.Text.Json.Serialization.JsonPropertyName("category")] string? Category,
        [property: System.Text.Json.Serialization.JsonPropertyName("statement")] string? Statement,
        [property: System.Text.Json.Serialization.JsonPropertyName("evidence_ids")] IReadOnlyList<string>? EvidenceIds,
        [property: System.Text.Json.Serialization.JsonPropertyName("confidence")] ConfidenceInput? Confidence,
        [property: System.Text.Json.Serialization.JsonPropertyName("uncertainty")] IReadOnlyList<string>? Uncertainty,
        [property: System.Text.Json.Serialization.JsonPropertyName("open_questions")] IReadOnlyList<string>? OpenQuestions,
        [property: System.Text.Json.Serialization.JsonPropertyName("applicability_limits")] IReadOnlyList<string>? ApplicabilityLimits,
        [property: System.Text.Json.Serialization.JsonPropertyName("status")] string? Status);

    private sealed record ConfidenceInput(
        [property: System.Text.Json.Serialization.JsonPropertyName("level")] string? Level);

    private sealed record AbstentionInput(
        [property: System.Text.Json.Serialization.JsonPropertyName("type")] string? Type,
        [property: System.Text.Json.Serialization.JsonPropertyName("condition")] string? Condition,
        [property: System.Text.Json.Serialization.JsonPropertyName("missing_evidence_description")] string? MissingEvidenceDescription);
}
