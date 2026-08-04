using System.Text.Json;

namespace Eip.Cli.Inference;

internal static class InferenceExecutionWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static async Task<string> WriteAsync(
        string localContextPath,
        InferenceExecution execution,
        CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(localContextPath)
            ?? throw new InvalidDataException("The local context location is invalid.");
        var outputPath = Path.Combine(directory, "inference-execution.json");
        var temporaryPath = $"{outputPath}.tmp";

        try
        {
            await using (var stream = File.Create(temporaryPath))
            {
                await JsonSerializer.SerializeAsync(stream, execution, JsonOptions, cancellationToken);
                await stream.WriteAsync("\n"u8.ToArray(), cancellationToken);
            }

            File.Move(temporaryPath, outputPath, true);
            return outputPath;
        }
        catch (OperationCanceledException)
        {
            TryDeleteTemporaryFile(temporaryPath);
            throw;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            TryDeleteTemporaryFile(temporaryPath);
            throw new IOException("The inference execution could not be written.");
        }
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
