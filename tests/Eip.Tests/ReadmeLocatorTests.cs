using System.Text.Json;

namespace Eip.Tests;

public sealed class ReadmeLocatorTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    [Fact]
    public async Task WritesCandidatesInRequiredPriorityWithoutUnrelatedDirectories()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            WriteFile(root, "README.md");
            WriteFile(root, "src/Readme.md");
            WriteFile(root, "src/payments/readme.md");
            WriteFile(root, "unrelated/README.md");
            var manifestPath = await WriteManifestAsync(
                root,
                "src/payments/payment.cs",
                "src/orders/order.cs");
            var manifestBefore = await File.ReadAllBytesAsync(manifestPath);

            var outputPath = await Cli.ReadmeLocator.WriteAsync(
                manifestPath,
                root,
                CancellationToken.None);

            using var document = JsonDocument.Parse(await File.ReadAllTextAsync(outputPath));
            var candidates = document.RootElement
                .GetProperty("candidate_readmes")
                .EnumerateArray()
                .Select(candidate => (
                    candidate.GetProperty("path").GetString(),
                    candidate.GetProperty("reason").GetString()))
                .ToArray();

            Assert.Equal(
                [
                    ("src/payments/readme.md", "changed file directory"),
                    ("src/Readme.md", "nearest ancestor of changed files"),
                    ("README.md", "repository root")
                ],
                candidates);
            Assert.DoesNotContain(candidates, candidate => candidate.Item1 == "unrelated/README.md");
            Assert.Equal(manifestBefore, await File.ReadAllBytesAsync(manifestPath));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task WritesEmptyCandidatesWhenNoRelatedReadmeExists()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var manifestPath = await WriteManifestAsync(root, "src/payment.cs");

            var outputPath = await Cli.ReadmeLocator.WriteAsync(
                manifestPath,
                root,
                CancellationToken.None);

            using var document = JsonDocument.Parse(await File.ReadAllTextAsync(outputPath));
            Assert.Empty(document.RootElement.GetProperty("candidate_readmes").EnumerateArray());
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task RejectsChangedFilePathOutsideRepository()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var manifestPath = await WriteManifestAsync(root, "../README.md");

            await Assert.ThrowsAsync<InvalidDataException>(() => Cli.ReadmeLocator.WriteAsync(
                manifestPath,
                root,
                CancellationToken.None));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"eip-readme-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private static void WriteFile(string root, string relativePath)
    {
        var path = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "content that the locator must not read");
    }

    private static async Task<string> WriteManifestAsync(string root, params string[] changedFiles)
    {
        var manifest = new Cli.EvidenceManifest(
            "example/widgets",
            "example",
            123,
            "Change widgets",
            "octocat",
            "base-sha",
            "head-sha",
            [],
            changedFiles
                .Select(path => new Cli.ChangedFileEvidence(path, "modified", null, 1, 0))
                .ToArray(),
            new DateTimeOffset(2026, 8, 3, 12, 0, 0, TimeSpan.Zero));
        var packDirectory = Path.Combine(root, "output", "pack-id");
        Directory.CreateDirectory(packDirectory);
        var manifestPath = Path.Combine(packDirectory, "manifest.json");
        await File.WriteAllTextAsync(
            manifestPath,
            JsonSerializer.Serialize(manifest, JsonOptions));
        return manifestPath;
    }
}
