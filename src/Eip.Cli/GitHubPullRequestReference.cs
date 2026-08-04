namespace Eip.Cli;

public sealed record GitHubPullRequestReference(string Owner, string Repository, int Number)
{
    public static GitHubPullRequestReference Parse(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal)
            || !string.Equals(uri.Host, "github.com", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("The Pull Request URL must use https://github.com.", nameof(value));
        }

        var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments is not [var owner, var repository, "pull", var number]
            || !int.TryParse(number, out var pullRequestNumber)
            || pullRequestNumber <= 0)
        {
            throw new ArgumentException(
                "Expected a URL in the form https://github.com/<owner>/<repository>/pull/<number>.",
                nameof(value));
        }

        return new GitHubPullRequestReference(owner, repository, pullRequestNumber);
    }
}
