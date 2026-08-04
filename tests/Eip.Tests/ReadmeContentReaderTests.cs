using System.Text;
using System.Text.Json;

namespace Eip.Tests;

public sealed class ReadmeContentReaderTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    [Fact]
    public async Task ReadsOnlyListedDocumentsInOriginalOrderWithoutChangingContent()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            const string rootContent = "# Root\r\n\r\n  Keep spacing  \r\n";
            const string paymentsContent = "# Payments\n\nÁrbol de pagos\n";
            WriteFile(root, "README.md", rootContent);
            WriteFile(root, "src/payments/README.md", paymentsContent);
            WriteFile(root, "unrelated/README.md", "must not be returned");
            var readmesPath = await WriteReadmesAsync(
                root,
                new Cli.ReadmeCandidate("src/payments/README.md", "changed file directory"),
                new Cli.ReadmeCandidate("README.md", "repository root"));
            var readmesBefore = await File.ReadAllBytesAsync(readmesPath);

            var outputPath = await Cli.ReadmeContentReader.WriteAsync(
                readmesPath,
                root,
                CancellationToken.None);

            using var document = JsonDocument.Parse(await File.ReadAllTextAsync(outputPath));
            var documents = document.RootElement.GetProperty("documents").EnumerateArray().ToArray();

            Assert.Equal(2, documents.Length);
            Assert.Equal("src/payments/README.md", documents[0].GetProperty("path").GetString());
            Assert.Equal(paymentsContent, documents[0].GetProperty("content").GetString());
            Assert.False(documents[0].TryGetProperty("error", out _));
            Assert.Equal("README.md", documents[1].GetProperty("path").GetString());
            Assert.Equal(rootContent, documents[1].GetProperty("content").GetString());
            Assert.False(documents[1].TryGetProperty("error", out _));
            Assert.DoesNotContain(
                documents,
                item => item.GetProperty("path").GetString() == "unrelated/README.md");
            Assert.Equal(readmesBefore, await File.ReadAllBytesAsync(readmesPath));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task RecordsPerDocumentErrorAndContinuesReading()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            WriteFile(root, "README.md", "available");
            var readmesPath = await WriteReadmesAsync(
                root,
                new Cli.ReadmeCandidate("missing/README.md", "changed file directory"),
                new Cli.ReadmeCandidate("README.md", "repository root"));

            var outputPath = await Cli.ReadmeContentReader.WriteAsync(
                readmesPath,
                root,
                CancellationToken.None);

            using var document = JsonDocument.Parse(await File.ReadAllTextAsync(outputPath));
            var documents = document.RootElement.GetProperty("documents").EnumerateArray().ToArray();

            Assert.Equal(2, documents.Length);
            Assert.Equal("missing/README.md", documents[0].GetProperty("path").GetString());
            Assert.Equal("The document could not be read.", documents[0].GetProperty("error").GetString());
            Assert.False(documents[0].TryGetProperty("content", out _));
            Assert.Equal("available", documents[1].GetProperty("content").GetString());
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task RecordsErrorForPathOutsideRepository()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var readmesPath = await WriteReadmesAsync(
                root,
                new Cli.ReadmeCandidate("../README.md", "repository root"));

            var outputPath = await Cli.ReadmeContentReader.WriteAsync(
                readmesPath,
                root,
                CancellationToken.None);

            using var document = JsonDocument.Parse(await File.ReadAllTextAsync(outputPath));
            var result = document.RootElement.GetProperty("documents")[0];
            Assert.Equal("The document could not be read.", result.GetProperty("error").GetString());
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task RejectsIntermediateDirectorySymlinkAndContinuesWithValidCandidate()
    {
        var root = CreateTemporaryDirectory();
        var externalRoot = CreateTemporaryDirectory();

        try
        {
            const string externalContent = "external content must not be read";
            WriteFile(externalRoot, "README.md", externalContent);
            WriteFile(root, "README.md", "valid repository content");
            Directory.CreateSymbolicLink(Path.Combine(root, "linked"), externalRoot);
            var readmesPath = await WriteReadmesAsync(
                root,
                new Cli.ReadmeCandidate("linked/README.md", "changed file directory"),
                new Cli.ReadmeCandidate("README.md", "repository root"));
            var readmesBefore = await File.ReadAllBytesAsync(readmesPath);

            var outputPath = await Cli.ReadmeContentReader.WriteAsync(
                readmesPath,
                root,
                CancellationToken.None);

            var output = await File.ReadAllTextAsync(outputPath);
            using var document = JsonDocument.Parse(output);
            var documents = document.RootElement.GetProperty("documents").EnumerateArray().ToArray();

            Assert.Equal(2, documents.Length);
            Assert.Equal("The document could not be read.", documents[0].GetProperty("error").GetString());
            Assert.False(documents[0].TryGetProperty("content", out _));
            Assert.Equal("valid repository content", documents[1].GetProperty("content").GetString());
            Assert.DoesNotContain(externalContent, output, StringComparison.Ordinal);
            Assert.Equal(readmesBefore, await File.ReadAllBytesAsync(readmesPath));
        }
        finally
        {
            Directory.Delete(root, true);
            Directory.Delete(externalRoot, true);
        }
    }

    [Fact]
    public async Task RejectsFinalFileSymlinkWithoutReadingExternalContent()
    {
        var root = CreateTemporaryDirectory();
        var externalRoot = CreateTemporaryDirectory();

        try
        {
            const string externalContent = "external file content must not be read";
            var externalPath = Path.Combine(externalRoot, "README.md");
            await File.WriteAllTextAsync(externalPath, externalContent);
            File.CreateSymbolicLink(Path.Combine(root, "README.md"), externalPath);
            var readmesPath = await WriteReadmesAsync(
                root,
                new Cli.ReadmeCandidate("README.md", "repository root"));

            var outputPath = await Cli.ReadmeContentReader.WriteAsync(
                readmesPath,
                root,
                CancellationToken.None);

            var output = await File.ReadAllTextAsync(outputPath);
            using var document = JsonDocument.Parse(output);
            var result = document.RootElement.GetProperty("documents")[0];
            Assert.Equal("The document could not be read.", result.GetProperty("error").GetString());
            Assert.False(result.TryGetProperty("content", out _));
            Assert.DoesNotContain(externalContent, output, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, true);
            Directory.Delete(externalRoot, true);
        }
    }

    [Fact]
    public async Task RecordsErrorInsteadOfAbortingWhenAnotherCandidateHasAnIllegalPathCharacter()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            WriteFile(root, "README.md", "available");
            var readmesPath = await WriteReadmesAsync(
                root,
                new Cli.ReadmeCandidate("bad\0dir/README.md", "changed file directory"),
                new Cli.ReadmeCandidate("README.md", "repository root"));

            var outputPath = await Cli.ReadmeContentReader.WriteAsync(
                readmesPath,
                root,
                CancellationToken.None);

            using var document = JsonDocument.Parse(await File.ReadAllTextAsync(outputPath));
            var documents = document.RootElement.GetProperty("documents").EnumerateArray().ToArray();

            Assert.Equal(2, documents.Length);
            Assert.Equal("The document could not be read.", documents[0].GetProperty("error").GetString());
            Assert.Equal("available", documents[1].GetProperty("content").GetString());
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task RecordsErrorForContentThatIsNotValidUtf8EvenWithARecognizedByteOrderMark()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var path = Path.Combine(root, "README.md");
            await File.WriteAllTextAsync(path, "Hello UTF-16", new UnicodeEncoding(bigEndian: false, byteOrderMark: true));
            var readmesPath = await WriteReadmesAsync(
                root,
                new Cli.ReadmeCandidate("README.md", "repository root"));

            var outputPath = await Cli.ReadmeContentReader.WriteAsync(
                readmesPath,
                root,
                CancellationToken.None);

            using var document = JsonDocument.Parse(await File.ReadAllTextAsync(outputPath));
            var result = document.RootElement.GetProperty("documents")[0];
            Assert.Equal("The document could not be read.", result.GetProperty("error").GetString());
            Assert.False(result.TryGetProperty("content", out _));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task ThrowsInvalidDataExceptionWhenReadmesFileIsNotValidJson()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var packDirectory = Path.Combine(root, "output", "pack-id");
            Directory.CreateDirectory(packDirectory);
            var readmesPath = Path.Combine(packDirectory, "readmes.json");
            await File.WriteAllTextAsync(readmesPath, "{ not json");

            await Assert.ThrowsAsync<InvalidDataException>(() => Cli.ReadmeContentReader.WriteAsync(
                readmesPath,
                root,
                CancellationToken.None));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task ThrowsInvalidDataExceptionWhenReadmesFileIsMissingCandidateReadmes()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var packDirectory = Path.Combine(root, "output", "pack-id");
            Directory.CreateDirectory(packDirectory);
            var readmesPath = Path.Combine(packDirectory, "readmes.json");
            await File.WriteAllTextAsync(readmesPath, "{}");

            await Assert.ThrowsAsync<InvalidDataException>(() => Cli.ReadmeContentReader.WriteAsync(
                readmesPath,
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
        var path = Path.Combine(Path.GetTempPath(), $"eip-readme-content-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private static void WriteFile(string root, string relativePath, string content)
    {
        var path = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    private static async Task<string> WriteReadmesAsync(
        string root,
        params Cli.ReadmeCandidate[] candidates)
    {
        var packDirectory = Path.Combine(root, "output", "pack-id");
        Directory.CreateDirectory(packDirectory);
        var path = Path.Combine(packDirectory, "readmes.json");
        await File.WriteAllTextAsync(
            path,
            JsonSerializer.Serialize(new Cli.ReadmeCandidates(candidates), JsonOptions));
        return path;
    }
}
