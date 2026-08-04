using System.Text.Json;

namespace Eip.Tests;

public sealed class ReadmeMetadataExtractorTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    [Fact]
    public async Task ExtractsH1AndFollowingDescriptiveParagraphWithEvidence()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var inputs = await WriteInputsAsync(
                root,
                new Cli.ReadmeDocument(
                    "README.md",
                    "# Example Service\n\nProvides example processing capabilities.\n",
                    null));

            var outputPath = await Cli.ReadmeMetadataExtractor.WriteAsync(
                inputs.ContentsPath,
                CancellationToken.None);

            using var output = JsonDocument.Parse(await File.ReadAllTextAsync(outputPath));
            var document = output.RootElement.GetProperty("documents")[0];
            Assert.Equal("Example Service", document.GetProperty("name").GetString());
            Assert.Equal(
                "Provides example processing capabilities.",
                document.GetProperty("purpose").GetString());
            Assert.Equal("extracted", document.GetProperty("status").GetString());
            var evidence = document.GetProperty("evidence").EnumerateArray().ToArray();
            AssertEvidence(evidence[0], "name", "Example Service", 1, 1);
            AssertEvidence(
                evidence[1],
                "purpose",
                "Provides example processing capabilities.",
                3,
                3);
            await AssertInputsUnchangedAsync(inputs);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task ExtractsPurposeButReportsMissingNameWhenReadmeHasNoTitle()
    {
        var result = await ExtractSingleAsync("Provides standalone processing.\n");

        Assert.Equal(JsonValueKind.Null, result.GetProperty("name").ValueKind);
        Assert.Equal("Provides standalone processing.", result.GetProperty("purpose").GetString());
        Assert.Equal("missing_name", result.GetProperty("status").GetString());
    }

    [Theory]
    [InlineData("Title: Explicit Service\n\nProvides explicit processing.\n", "Explicit Service", 1, 1)]
    [InlineData("Setext Service\n==============\n\nProvides setext processing.\n", "Setext Service", 1, 2)]
    public async Task ExtractsExplicitTitleWhenAtxH1IsAbsent(
        string content,
        string expectedName,
        int expectedStart,
        int expectedEnd)
    {
        var result = await ExtractSingleAsync(content);

        Assert.Equal(expectedName, result.GetProperty("name").GetString());
        var nameEvidence = result.GetProperty("evidence")[0];
        AssertEvidence(nameEvidence, "name", expectedName, expectedStart, expectedEnd);
    }

    [Fact]
    public async Task EmptyReadmeIsInsufficient()
    {
        var result = await ExtractSingleAsync(string.Empty);

        Assert.Equal(JsonValueKind.Null, result.GetProperty("name").ValueKind);
        Assert.Equal(JsonValueKind.Null, result.GetProperty("purpose").ValueKind);
        Assert.Equal("insufficient", result.GetProperty("status").GetString());
        Assert.Empty(result.GetProperty("evidence").EnumerateArray());
    }

    [Fact]
    public async Task IgnoresBadgesBeforeFirstH1AndDoesNotUseLaterHeadings()
    {
        var result = await ExtractSingleAsync(
            "[![Build](build.svg)](build)\n\n# Primary Service\n\nPrimary purpose.\n\n# Other Heading\n\nOther text.\n");

        Assert.Equal("Primary Service", result.GetProperty("name").GetString());
        Assert.Equal("Primary purpose.", result.GetProperty("purpose").GetString());
        var evidence = result.GetProperty("evidence").EnumerateArray().ToArray();
        AssertEvidence(evidence[0], "name", "Primary Service", 3, 3);
        AssertEvidence(evidence[1], "purpose", "Primary purpose.", 5, 5);
    }

    [Fact]
    public async Task JoinsMultilineUnicodeParagraphAndPreservesEvidenceRange()
    {
        var result = await ExtractSingleAsync(
            "# Servicio de Pagos\n\nProcesa pagos en múltiples monedas\ny conserva trazabilidad explícita.\n");

        Assert.Equal("Servicio de Pagos", result.GetProperty("name").GetString());
        Assert.Equal(
            "Procesa pagos en múltiples monedas y conserva trazabilidad explícita.",
            result.GetProperty("purpose").GetString());
        var purposeEvidence = result.GetProperty("evidence")[1];
        AssertEvidence(
            purposeEvidence,
            "purpose",
            "Procesa pagos en múltiples monedas y conserva trazabilidad explícita.",
            3,
            4);
    }

    [Fact]
    public async Task ReportsMissingPurposeWithoutCrossingNextHeading()
    {
        var result = await ExtractSingleAsync("# Example Service\n\n## Installation\n\nRun the installer.\n");

        Assert.Equal("Example Service", result.GetProperty("name").GetString());
        Assert.Equal(JsonValueKind.Null, result.GetProperty("purpose").ValueKind);
        Assert.Equal("missing_purpose", result.GetProperty("status").GetString());
    }

    [Fact]
    public async Task IgnoresHeadingLikeTextInsideFencedCodeBlocks()
    {
        var result = await ExtractSingleAsync(
            "```bash\n# configure the token before running\nexport GITHUB_TOKEN=x\n```\n\n"
                + "# Real Title\n\nReal purpose.\n");

        Assert.Equal("Real Title", result.GetProperty("name").GetString());
        Assert.Equal("Real purpose.", result.GetProperty("purpose").GetString());
        var evidence = result.GetProperty("evidence").EnumerateArray().ToArray();
        AssertEvidence(evidence[0], "name", "Real Title", 6, 6);
        AssertEvidence(evidence[1], "purpose", "Real purpose.", 8, 8);
    }

    [Fact]
    public async Task DocumentErrorFromPreviousIncrementIsInsufficient()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var inputs = await WriteInputsAsync(
                root,
                new Cli.ReadmeDocument(
                    "missing/README.md",
                    null,
                    "The document could not be read."));

            var outputPath = await Cli.ReadmeMetadataExtractor.WriteAsync(
                inputs.ContentsPath,
                CancellationToken.None);

            using var output = JsonDocument.Parse(await File.ReadAllTextAsync(outputPath));
            var document = output.RootElement.GetProperty("documents")[0];
            Assert.Equal("insufficient", document.GetProperty("status").GetString());
            Assert.Equal(JsonValueKind.Null, document.GetProperty("name").ValueKind);
            Assert.Equal(JsonValueKind.Null, document.GetProperty("purpose").ValueKind);
            Assert.Empty(document.GetProperty("evidence").EnumerateArray());
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task RepeatedExecutionIsByteForByteDeterministicAndPreservesOrder()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var inputs = await WriteInputsAsync(
                root,
                new Cli.ReadmeDocument("b/README.md", "# B\n\nPurpose B.\n", null),
                new Cli.ReadmeDocument("a/README.md", "# A\n\nPurpose A.\n", null));

            var outputPath = await Cli.ReadmeMetadataExtractor.WriteAsync(
                inputs.ContentsPath,
                CancellationToken.None);
            var first = await File.ReadAllBytesAsync(outputPath);
            await Cli.ReadmeMetadataExtractor.WriteAsync(inputs.ContentsPath, CancellationToken.None);
            var second = await File.ReadAllBytesAsync(outputPath);

            Assert.Equal(first, second);
            using var output = JsonDocument.Parse(second);
            var documents = output.RootElement.GetProperty("documents").EnumerateArray().ToArray();
            Assert.Equal("b/README.md", documents[0].GetProperty("path").GetString());
            Assert.Equal("a/README.md", documents[1].GetProperty("path").GetString());
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Theory]
    [InlineData("{ not json")]
    [InlineData("{}")]
    [InlineData("{\"documents\":[{\"content\":\"# Name\"}]}")]
    public async Task RejectsMalformedInput(string json)
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var packDirectory = Path.Combine(root, "output", "pack-id");
            Directory.CreateDirectory(packDirectory);
            var path = Path.Combine(packDirectory, "readme-contents.json");
            await File.WriteAllTextAsync(path, json);

            await Assert.ThrowsAsync<InvalidDataException>(() =>
                Cli.ReadmeMetadataExtractor.WriteAsync(path, CancellationToken.None));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private static async Task<JsonElement> ExtractSingleAsync(string content)
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var inputs = await WriteInputsAsync(
                root,
                new Cli.ReadmeDocument("README.md", content, null));
            var outputPath = await Cli.ReadmeMetadataExtractor.WriteAsync(
                inputs.ContentsPath,
                CancellationToken.None);
            using var output = JsonDocument.Parse(await File.ReadAllTextAsync(outputPath));
            return output.RootElement.GetProperty("documents")[0].Clone();
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private static void AssertEvidence(
        JsonElement evidence,
        string field,
        string text,
        int start,
        int end)
    {
        Assert.Equal(field, evidence.GetProperty("field").GetString());
        Assert.Equal(text, evidence.GetProperty("text").GetString());
        Assert.Equal(start, evidence.GetProperty("source_line_start").GetInt32());
        Assert.Equal(end, evidence.GetProperty("source_line_end").GetInt32());
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"eip-readme-metadata-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private static async Task<InputFiles> WriteInputsAsync(
        string root,
        params Cli.ReadmeDocument[] documents)
    {
        var packDirectory = Path.Combine(root, "output", "pack-id");
        Directory.CreateDirectory(packDirectory);
        var manifestPath = Path.Combine(packDirectory, "manifest.json");
        var readmesPath = Path.Combine(packDirectory, "readmes.json");
        var contentsPath = Path.Combine(packDirectory, "readme-contents.json");
        await File.WriteAllTextAsync(manifestPath, "{\"unchanged\":\"manifest\"}\n");
        await File.WriteAllTextAsync(readmesPath, "{\"unchanged\":\"readmes\"}\n");
        await File.WriteAllTextAsync(
            contentsPath,
            JsonSerializer.Serialize(new Cli.ReadmeDocuments(documents), JsonOptions));

        return new InputFiles(
            manifestPath,
            readmesPath,
            contentsPath,
            await File.ReadAllBytesAsync(manifestPath),
            await File.ReadAllBytesAsync(readmesPath),
            await File.ReadAllBytesAsync(contentsPath));
    }

    private static async Task AssertInputsUnchangedAsync(InputFiles inputs)
    {
        Assert.Equal(inputs.ManifestBytes, await File.ReadAllBytesAsync(inputs.ManifestPath));
        Assert.Equal(inputs.ReadmesBytes, await File.ReadAllBytesAsync(inputs.ReadmesPath));
        Assert.Equal(inputs.ContentsBytes, await File.ReadAllBytesAsync(inputs.ContentsPath));
    }

    private sealed record InputFiles(
        string ManifestPath,
        string ReadmesPath,
        string ContentsPath,
        byte[] ManifestBytes,
        byte[] ReadmesBytes,
        byte[] ContentsBytes);
}
