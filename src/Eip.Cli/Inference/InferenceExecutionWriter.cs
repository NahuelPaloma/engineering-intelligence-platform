using System.Text.Json;

namespace Eip.Cli.Inference;

internal static class InferenceExecutionWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static async Task<string> WriteAllAsync(
        string localContextPath,
        InferenceExecution execution,
        InferenceReport? report,
        CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(localContextPath)
            ?? throw new InvalidDataException("The local context location is invalid.");
        var outputPath = Path.Combine(directory, "inference-execution.json");
        var reportPath = Path.Combine(directory, "inference-report.json");
        var temporaryPath = $"{outputPath}.tmp";
        var temporaryReportPath = $"{reportPath}.tmp";
        var reportPublished = false;

        try
        {
            if (report is not null)
            {
                await WriteAsync(temporaryReportPath, report, cancellationToken);
            }

            await using (var stream = File.Create(temporaryPath))
            {
                await JsonSerializer.SerializeAsync(stream, execution, JsonOptions, cancellationToken);
                await stream.WriteAsync("\n"u8.ToArray(), cancellationToken);
            }

            if (report is not null)
            {
                File.Move(temporaryReportPath, reportPath, true);
                reportPublished = true;
            }

            File.Move(temporaryPath, outputPath, true);
            return outputPath;
        }
        catch (OperationCanceledException)
        {
            TryDeleteTemporaryFile(temporaryPath);
            TryDeleteTemporaryFile(temporaryReportPath);
            if (reportPublished)
            {
                TryDeleteTemporaryFile(reportPath);
            }
            throw;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            TryDeleteTemporaryFile(temporaryPath);
            TryDeleteTemporaryFile(temporaryReportPath);
            if (reportPublished)
            {
                TryDeleteTemporaryFile(reportPath);
            }
            throw new IOException("The inference execution could not be written.");
        }
    }

    private static async Task WriteAsync<T>(
        string path,
        T value,
        CancellationToken cancellationToken)
    {
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, value, JsonOptions, cancellationToken);
        await stream.WriteAsync("\n"u8.ToArray(), cancellationToken);
    }

    private static void TryDeleteTemporaryFile(string temporaryPath)
    {
        try
        {
            File.Delete(temporaryPath);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // Preserve the original failure without exposing filesystem details.
        }
    }
}
