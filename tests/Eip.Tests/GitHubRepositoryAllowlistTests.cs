using System.Net;

namespace Eip.Tests;

public sealed class GitHubRepositoryAllowlistTests
{
    [Fact]
    public void AllowsConfiguredRepositoryCaseInsensitively()
    {
        var allowlist = Cli.GitHubRepositoryAllowlist.Parse("Example/Widgets");
        var reference = new Cli.GitHubPullRequestReference("example", "widgets", 123);

        allowlist.EnsureAuthorized(reference);
    }

    [Fact]
    public void RejectsRepositoryThatIsNotConfigured()
    {
        var allowlist = Cli.GitHubRepositoryAllowlist.Parse("example/other");
        var reference = new Cli.GitHubPullRequestReference("example", "widgets", 123);

        var exception = Assert.Throws<UnauthorizedAccessException>(
            () => allowlist.EnsureAuthorized(reference));

        Assert.Equal("The repository is not authorized for this pilot.", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MissingOrEmptyAllowlistRejectsAllRepositories(string? value)
    {
        var allowlist = Cli.GitHubRepositoryAllowlist.Parse(value);
        var reference = new Cli.GitHubPullRequestReference("example", "widgets", 123);

        Assert.Throws<UnauthorizedAccessException>(() => allowlist.EnsureAuthorized(reference));
    }

    [Fact]
    public async Task RejectedRepositoryDoesNotIssueHttpRequest()
    {
        var handler = new CountingHttpMessageHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.github.test/")
        };
        var command = new Cli.ReviewCommand(
            new Cli.GitHubEvidenceClient(httpClient),
            Cli.GitHubRepositoryAllowlist.Parse("example/other"),
            TimeProvider.System);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => command.ExecuteAsync(
            "https://github.com/example/widgets/pull/123",
            Path.GetTempPath(),
            Path.GetTempPath(),
            CancellationToken.None));

        Assert.Equal(0, handler.RequestCount);
    }

    private sealed class CountingHttpMessageHandler : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
