namespace Eip.Tests;

public sealed class GitHubPullRequestReferenceTests
{
    [Fact]
    public void ParsesCanonicalGitHubPullRequestUrl()
    {
        var reference = Cli.GitHubPullRequestReference.Parse(
            "https://github.com/example/widgets/pull/123");

        Assert.Equal("example", reference.Owner);
        Assert.Equal("widgets", reference.Repository);
        Assert.Equal(123, reference.Number);
    }

    [Theory]
    [InlineData("http://github.com/example/widgets/pull/123")]
    [InlineData("https://gitlab.com/example/widgets/pull/123")]
    [InlineData("https://github.com/example/widgets/issues/123")]
    [InlineData("https://github.com/example/widgets/pull/not-a-number")]
    public void RejectsUnsupportedUrl(string value)
    {
        Assert.Throws<ArgumentException>(() => Cli.GitHubPullRequestReference.Parse(value));
    }
}
