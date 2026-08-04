using System.Net;
using System.Text;
using System.Text.Json;

namespace Eip.Tests;

public sealed class ReviewCommandTests
{
    [Fact]
    public async Task WritesMinimalEvidenceManifest()
    {
        var responses = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["/repos/example/widgets/pulls/123"] = """
                {
                  "title": "Add widget endpoint",
                  "user": { "login": "octocat" },
                  "base": { "sha": "base-sha" },
                  "head": { "sha": "head-sha" }
                }
                """,
            ["/repos/example/widgets/pulls/123/commits?per_page=100&page=1"] = """
                [
                  {
                    "sha": "commit-sha",
                    "commit": {
                      "message": "feat: add widget endpoint\n\nDetails",
                      "author": { "name": "Octo Cat" }
                    },
                    "author": { "login": "octocat" }
                  }
                ]
                """,
            ["/repos/example/widgets/pulls/123/files?per_page=100&page=1"] = """
                [
                  {
                    "filename": "src/widgets.cs",
                    "status": "modified",
                    "previous_filename": null,
                    "additions": 12,
                    "deletions": 3
                  }
                ]
                """
        };
        using var httpClient = new HttpClient(new StubHttpMessageHandler(responses))
        {
            BaseAddress = new Uri("https://api.github.test/")
        };
        var command = new Cli.ReviewCommand(
            new Cli.GitHubEvidenceClient(httpClient),
            Cli.GitHubRepositoryAllowlist.Parse("example/widgets"),
            new FixedTimeProvider(new DateTimeOffset(2026, 8, 3, 12, 0, 0, TimeSpan.Zero)));
        var outputRoot = Path.Combine(Path.GetTempPath(), $"eip-tests-{Guid.NewGuid():N}");

        try
        {
            var manifestPath = await command.ExecuteAsync(
                "https://github.com/example/widgets/pull/123",
                outputRoot,
                outputRoot,
                CancellationToken.None);

            Assert.True(File.Exists(manifestPath));
            using var document = JsonDocument.Parse(await File.ReadAllTextAsync(manifestPath));
            var root = document.RootElement;
            var propertyNames = root.EnumerateObject().Select(property => property.Name).ToArray();

            Assert.Equal(
                [
                    "repository",
                    "owner",
                    "pull_request",
                    "title",
                    "author",
                    "base_sha",
                    "head_sha",
                    "commits",
                    "changed_files",
                    "generated_at"
                ],
                propertyNames);
            Assert.Equal("example/widgets", root.GetProperty("repository").GetString());
            Assert.Equal("base-sha", root.GetProperty("base_sha").GetString());
            Assert.Equal("head-sha", root.GetProperty("head_sha").GetString());
            Assert.Single(root.GetProperty("commits").EnumerateArray());
            Assert.Single(root.GetProperty("changed_files").EnumerateArray());
            var readmesPath = Path.Combine(Path.GetDirectoryName(manifestPath)!, "readmes.json");
            Assert.True(File.Exists(readmesPath));
            var contentsPath = Path.Combine(Path.GetDirectoryName(manifestPath)!, "readme-contents.json");
            Assert.True(File.Exists(contentsPath));
            var metadataPath = Path.Combine(Path.GetDirectoryName(manifestPath)!, "readme-metadata.json");
            Assert.True(File.Exists(metadataPath));
            var rankingPath = Path.Combine(Path.GetDirectoryName(manifestPath)!, "readme-ranking.json");
            Assert.True(File.Exists(rankingPath));
            var localContextPath = Path.Combine(Path.GetDirectoryName(manifestPath)!, "local-context.json");
            Assert.True(File.Exists(localContextPath));
        }
        finally
        {
            if (Directory.Exists(outputRoot))
            {
                Directory.Delete(outputRoot, true);
            }
        }
    }

    [Fact]
    public async Task FailsCleanlyWhenPullRequestResponseIsIncomplete()
    {
        var responses = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["/repos/example/widgets/pulls/123"] = """
                {
                  "title": "Add widget endpoint",
                  "user": null,
                  "base": { "sha": "base-sha" },
                  "head": { "sha": "head-sha" }
                }
                """
        };
        using var httpClient = new HttpClient(new StubHttpMessageHandler(responses))
        {
            BaseAddress = new Uri("https://api.github.test/")
        };
        var command = new Cli.ReviewCommand(
            new Cli.GitHubEvidenceClient(httpClient),
            Cli.GitHubRepositoryAllowlist.Parse("example/widgets"),
            new FixedTimeProvider(new DateTimeOffset(2026, 8, 3, 12, 0, 0, TimeSpan.Zero)));
        var outputRoot = Path.Combine(Path.GetTempPath(), $"eip-tests-{Guid.NewGuid():N}");

        try
        {
            await Assert.ThrowsAsync<HttpRequestException>(() => command.ExecuteAsync(
                "https://github.com/example/widgets/pull/123",
                outputRoot,
                outputRoot,
                CancellationToken.None));
        }
        finally
        {
            if (Directory.Exists(outputRoot))
            {
                Directory.Delete(outputRoot, true);
            }
        }
    }

    [Fact]
    public async Task FailsCleanlyWhenCommitResponseIsIncomplete()
    {
        var responses = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["/repos/example/widgets/pulls/123"] = """
                {
                  "title": "Add widget endpoint",
                  "user": { "login": "octocat" },
                  "base": { "sha": "base-sha" },
                  "head": { "sha": "head-sha" }
                }
                """,
            ["/repos/example/widgets/pulls/123/commits?per_page=100&page=1"] = """
                [
                  {
                    "sha": "commit-sha",
                    "commit": null,
                    "author": { "login": "octocat" }
                  }
                ]
                """,
            ["/repos/example/widgets/pulls/123/files?per_page=100&page=1"] = "[]"
        };
        using var httpClient = new HttpClient(new StubHttpMessageHandler(responses))
        {
            BaseAddress = new Uri("https://api.github.test/")
        };
        var command = new Cli.ReviewCommand(
            new Cli.GitHubEvidenceClient(httpClient),
            Cli.GitHubRepositoryAllowlist.Parse("example/widgets"),
            new FixedTimeProvider(new DateTimeOffset(2026, 8, 3, 12, 0, 0, TimeSpan.Zero)));
        var outputRoot = Path.Combine(Path.GetTempPath(), $"eip-tests-{Guid.NewGuid():N}");

        try
        {
            await Assert.ThrowsAsync<HttpRequestException>(() => command.ExecuteAsync(
                "https://github.com/example/widgets/pull/123",
                outputRoot,
                outputRoot,
                CancellationToken.None));
        }
        finally
        {
            if (Directory.Exists(outputRoot))
            {
                Directory.Delete(outputRoot, true);
            }
        }
    }

    [Fact]
    public async Task FailsCleanlyWhenResponseBodyIsNotJson()
    {
        var responses = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["/repos/example/widgets/pulls/123"] = "<html>not json</html>"
        };
        using var httpClient = new HttpClient(new StubHttpMessageHandler(responses))
        {
            BaseAddress = new Uri("https://api.github.test/")
        };
        var command = new Cli.ReviewCommand(
            new Cli.GitHubEvidenceClient(httpClient),
            Cli.GitHubRepositoryAllowlist.Parse("example/widgets"),
            new FixedTimeProvider(new DateTimeOffset(2026, 8, 3, 12, 0, 0, TimeSpan.Zero)));
        var outputRoot = Path.Combine(Path.GetTempPath(), $"eip-tests-{Guid.NewGuid():N}");

        try
        {
            await Assert.ThrowsAsync<HttpRequestException>(() => command.ExecuteAsync(
                "https://github.com/example/widgets/pull/123",
                outputRoot,
                outputRoot,
                CancellationToken.None));
        }
        finally
        {
            if (Directory.Exists(outputRoot))
            {
                Directory.Delete(outputRoot, true);
            }
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }

    private sealed class StubHttpMessageHandler(IReadOnlyDictionary<string, string> responses)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.Method != HttpMethod.Get)
            {
                throw new InvalidOperationException(
                    $"Evidence collection must only issue read-only GitHub requests; got {request.Method}.");
            }

            var path = request.RequestUri?.PathAndQuery
                ?? throw new InvalidOperationException("The request URI is required.");
            var response = responses.TryGetValue(path, out var content)
                ? new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(content, Encoding.UTF8, "application/json")
                }
                : new HttpResponseMessage(HttpStatusCode.NotFound);

            return Task.FromResult(response);
        }
    }
}
