using System.Text.Json;

namespace Eip.Tests;

public sealed class ArchitectureReviewOrchestratorTests
{
    [Fact]
    public async Task RendersOnlyValidatedFindingsWithoutReadingExecutionArtifact()
    {
        var root = CreateTemporaryDirectory();
        try
        {
            var reportPath = await WriteReportAsync(root, includeFinding: true);
            await File.WriteAllTextAsync(
                Path.Combine(root, "inference-execution.json"),
                "content that must never be consumed");

            var outputPath = await Cli.ArchitectureReviewOrchestrator.WriteAsync(
                reportPath,
                CancellationToken.None);
            var first = await File.ReadAllBytesAsync(outputPath);
            await Cli.ArchitectureReviewOrchestrator.WriteAsync(reportPath, CancellationToken.None);
            var second = await File.ReadAllBytesAsync(outputPath);
            var review = await File.ReadAllTextAsync(outputPath);

            Assert.Equal(first, second);
            Assert.Contains("Finding statement from the report.", review, StringComparison.Ordinal);
            Assert.Contains("`evidence-1`", review, StringComparison.Ordinal);
            Assert.Contains("Confidence: `moderate`", review, StringComparison.Ordinal);
            Assert.Contains("Applicability limits: does_not_establish_authority", review, StringComparison.Ordinal);
            Assert.DoesNotContain("content that must never be consumed", review, StringComparison.Ordinal);
            Assert.DoesNotContain("Claim", review, StringComparison.Ordinal);
            Assert.DoesNotContain("Hypothesis", review, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task ProducesValidInsufficientReviewWhenReportHasNoFindings()
    {
        var root = CreateTemporaryDirectory();
        try
        {
            var reportPath = await WriteReportAsync(root, includeFinding: false);

            var outputPath = await Cli.ArchitectureReviewOrchestrator.WriteAsync(
                reportPath,
                CancellationToken.None);
            var review = await File.ReadAllTextAsync(outputPath);

            Assert.Contains("Review status: `insufficient`", review, StringComparison.Ordinal);
            Assert.Contains("No validated Findings were available", review, StringComparison.Ordinal);
            Assert.Contains("reviewer retains the decision", review, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task SurfacesAbstentionCausesWhenReportHasNoFindings()
    {
        var root = CreateTemporaryDirectory();
        try
        {
            var report = new
            {
                report_id = "report-1",
                execution_id = "execution-1",
                status = "incomplete",
                findings = Array.Empty<object>(),
                abstentions = new[]
                {
                    new
                    {
                        type = "total",
                        condition = "required_evidence_missing",
                        missing_evidence_description = "Additional governed evidence is required."
                    }
                }
            };
            var reportPath = Path.Combine(root, "inference-report.json");
            await File.WriteAllTextAsync(reportPath, JsonSerializer.Serialize(report));

            var outputPath = await Cli.ArchitectureReviewOrchestrator.WriteAsync(
                reportPath,
                CancellationToken.None);
            var review = await File.ReadAllTextAsync(outputPath);

            Assert.Contains("Review status: `insufficient`", review, StringComparison.Ordinal);
            Assert.Contains("Type: `total`", review, StringComparison.Ordinal);
            Assert.Contains("Condition: required_evidence_missing", review, StringComparison.Ordinal);
            Assert.Contains(
                "Missing evidence: Additional governed evidence is required.",
                review,
                StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task RejectsInvalidReportWithoutPublishingPartialReview()
    {
        var root = CreateTemporaryDirectory();
        try
        {
            var reportPath = Path.Combine(root, "inference-report.json");
            await File.WriteAllTextAsync(reportPath, "{ invalid");

            var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
                Cli.ArchitectureReviewOrchestrator.WriteAsync(reportPath, CancellationToken.None));

            Assert.Equal("The inference report is invalid.", exception.Message);
            Assert.False(File.Exists(Path.Combine(root, "context-pack.md")));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private static async Task<string> WriteReportAsync(string root, bool includeFinding)
    {
        var findings = includeFinding
            ? new[]
            {
                new
                {
                    finding_id = "finding-1",
                    category = "context_availability",
                    statement = "Finding statement from the report.",
                    evidence_ids = new[] { "evidence-1" },
                    confidence = new { level = "moderate" },
                    uncertainty = new[] { "utility_not_verified" },
                    open_questions = new[] { "Does it apply?" },
                    applicability_limits = new[] { "does_not_establish_authority" },
                    status = "valid"
                }
            }
            : [];
        var report = new
        {
            report_id = "report-1",
            execution_id = "execution-1",
            status = "complete",
            findings
        };
        var path = Path.Combine(root, "inference-report.json");
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(report));
        return path;
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"eip-review-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
