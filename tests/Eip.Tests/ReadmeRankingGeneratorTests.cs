using System.Text.Json;

namespace Eip.Tests;

public sealed class ReadmeRankingGeneratorTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    [Fact]
    public async Task RanksSingleSameDirectoryReadme()
    {
        var result = await RankSingleAsync("src/payments/README.md", "changed file directory");

        Assert.Equal("src/payments/README.md", result.GetProperty("path").GetString());
        Assert.Equal(100, result.GetProperty("score").GetInt32());
        Assert.Equal("same_directory", result.GetProperty("reason").GetString());
    }

    [Fact]
    public async Task RanksRepositoryRootReadmeAtTen()
    {
        var result = await RankSingleAsync("README.md", "repository root");

        Assert.Equal(10, result.GetProperty("score").GetInt32());
        Assert.Equal("repository_root", result.GetProperty("reason").GetString());
    }

    [Fact]
    public async Task ReducesScoreForEachHigherAncestorLevel()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var inputs = await WriteInputsAsync(
                root,
                [
                    Metadata("src/payments/processing/README.md", "extracted"),
                    Metadata("src/payments/README.md", "extracted"),
                    Metadata("src/README.md", "extracted"),
                    Metadata("README.md", "extracted")
                ],
                [
                    Candidate("src/payments/processing/README.md", "changed file directory"),
                    Candidate("src/payments/README.md", "nearest ancestor of changed files"),
                    Candidate("src/README.md", "nearest ancestor of changed files"),
                    Candidate("README.md", "repository root")
                ]);

            var outputPath = await Cli.ReadmeRankingGenerator.WriteAsync(
                inputs.MetadataPath,
                inputs.ReadmesPath,
                CancellationToken.None);

            using var output = JsonDocument.Parse(await File.ReadAllTextAsync(outputPath));
            var documents = output.RootElement.GetProperty("documents").EnumerateArray().ToArray();
            AssertRanking(documents[0], "src/payments/processing/README.md", 100, "same_directory");
            AssertRanking(documents[1], "src/payments/README.md", 80, "nearest_ancestor");
            AssertRanking(documents[2], "src/README.md", 60, "nearest_ancestor");
            AssertRanking(documents[3], "README.md", 10, "repository_root");
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task BreaksScoreTiesByOrdinalPath()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var inputs = await WriteInputsAsync(
                root,
                [Metadata("z/README.md", "extracted"), Metadata("A/README.md", "extracted")],
                [
                    Candidate("z/README.md", "changed file directory"),
                    Candidate("A/README.md", "changed file directory")
                ]);

            var outputPath = await Cli.ReadmeRankingGenerator.WriteAsync(
                inputs.MetadataPath,
                inputs.ReadmesPath,
                CancellationToken.None);

            using var output = JsonDocument.Parse(await File.ReadAllTextAsync(outputPath));
            var documents = output.RootElement.GetProperty("documents").EnumerateArray().ToArray();
            Assert.Equal("A/README.md", documents[0].GetProperty("path").GetString());
            Assert.Equal("z/README.md", documents[1].GetProperty("path").GetString());
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Theory]
    [InlineData("insufficient")]
    [InlineData("missing_name")]
    [InlineData("missing_purpose")]
    public async Task RankingDoesNotDependOnMetadataExtractionStatus(string status)
    {
        var result = await RankSingleAsync(
            "src/payments/README.md",
            "changed file directory",
            status);

        Assert.Equal(100, result.GetProperty("score").GetInt32());
        Assert.Equal("same_directory", result.GetProperty("reason").GetString());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RejectsDuplicateInputs(bool duplicateMetadata)
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var metadataDocument = Metadata("README.md", "extracted");
            var duplicateCandidate = Candidate("README.md", "repository root");
            var inputs = await WriteInputsAsync(
                root,
                duplicateMetadata ? [metadataDocument, metadataDocument] : [metadataDocument],
                duplicateMetadata ? [duplicateCandidate] : [duplicateCandidate, duplicateCandidate]);

            await Assert.ThrowsAsync<InvalidDataException>(() => Cli.ReadmeRankingGenerator.WriteAsync(
                inputs.MetadataPath,
                inputs.ReadmesPath,
                CancellationToken.None));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Theory]
    [InlineData("metadata", "{ not json")]
    [InlineData("metadata", "{}")]
    [InlineData("readmes", "{ not json")]
    [InlineData("readmes", "{}")]
    public async Task RejectsInvalidJsonOrMissingProperties(string target, string json)
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var inputs = await WriteInputsAsync(
                root,
                [Metadata("README.md", "extracted")],
                [Candidate("README.md", "repository root")]);
            await File.WriteAllTextAsync(
                target == "metadata" ? inputs.MetadataPath : inputs.ReadmesPath,
                json);

            await Assert.ThrowsAsync<InvalidDataException>(() => Cli.ReadmeRankingGenerator.WriteAsync(
                inputs.MetadataPath,
                inputs.ReadmesPath,
                CancellationToken.None));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task RepeatedExecutionIsDeterministicAndLeavesInputsUnchanged()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var inputs = await WriteInputsAsync(
                root,
                [Metadata("README.md", "extracted"), Metadata("src/README.md", "missing_purpose")],
                [
                    Candidate("README.md", "repository root"),
                    Candidate("src/README.md", "nearest ancestor of changed files")
                ]);
            var metadataBefore = await File.ReadAllBytesAsync(inputs.MetadataPath);
            var readmesBefore = await File.ReadAllBytesAsync(inputs.ReadmesPath);

            var outputPath = await Cli.ReadmeRankingGenerator.WriteAsync(
                inputs.MetadataPath,
                inputs.ReadmesPath,
                CancellationToken.None);
            var first = await File.ReadAllBytesAsync(outputPath);
            await Cli.ReadmeRankingGenerator.WriteAsync(
                inputs.MetadataPath,
                inputs.ReadmesPath,
                CancellationToken.None);
            var second = await File.ReadAllBytesAsync(outputPath);

            Assert.Equal(first, second);
            Assert.Equal(metadataBefore, await File.ReadAllBytesAsync(inputs.MetadataPath));
            Assert.Equal(readmesBefore, await File.ReadAllBytesAsync(inputs.ReadmesPath));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private static async Task<JsonElement> RankSingleAsync(
        string path,
        string reason,
        string status = "extracted")
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var inputs = await WriteInputsAsync(
                root,
                [Metadata(path, status)],
                [Candidate(path, reason)]);
            var outputPath = await Cli.ReadmeRankingGenerator.WriteAsync(
                inputs.MetadataPath,
                inputs.ReadmesPath,
                CancellationToken.None);
            using var output = JsonDocument.Parse(await File.ReadAllTextAsync(outputPath));
            return output.RootElement.GetProperty("documents")[0].Clone();
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private static Cli.ReadmeMetadataDocument Metadata(string path, string status) =>
        new(path, null, null, status, []);

    private static Cli.ReadmeCandidate Candidate(string path, string reason) => new(path, reason);

    private static void AssertRanking(JsonElement document, string path, int score, string reason)
    {
        Assert.Equal(path, document.GetProperty("path").GetString());
        Assert.Equal(score, document.GetProperty("score").GetInt32());
        Assert.Equal(reason, document.GetProperty("reason").GetString());
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"eip-readme-ranking-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private static async Task<InputFiles> WriteInputsAsync(
        string root,
        IReadOnlyList<Cli.ReadmeMetadataDocument> metadata,
        IReadOnlyList<Cli.ReadmeCandidate> candidates)
    {
        var packDirectory = Path.Combine(root, "output", "pack-id");
        Directory.CreateDirectory(packDirectory);
        var metadataPath = Path.Combine(packDirectory, "readme-metadata.json");
        var readmesPath = Path.Combine(packDirectory, "readmes.json");
        await File.WriteAllTextAsync(
            metadataPath,
            JsonSerializer.Serialize(new Cli.ReadmeMetadataDocuments(metadata), JsonOptions));
        await File.WriteAllTextAsync(
            readmesPath,
            JsonSerializer.Serialize(new Cli.ReadmeCandidates(candidates), JsonOptions));
        return new InputFiles(metadataPath, readmesPath);
    }

    private sealed record InputFiles(string MetadataPath, string ReadmesPath);
}
