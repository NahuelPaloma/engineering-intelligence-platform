namespace Eip.Cli;

public sealed class GitHubRepositoryAllowlist
{
    private readonly HashSet<string> repositories;

    private GitHubRepositoryAllowlist(HashSet<string> repositories)
    {
        this.repositories = repositories;
    }

    public static GitHubRepositoryAllowlist Parse(string? value)
    {
        var repositories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(value))
        {
            return new GitHubRepositoryAllowlist(repositories);
        }

        foreach (var item in value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            var segments = item.Split('/');
            if (segments is not [var owner, var repository]
                || string.IsNullOrWhiteSpace(owner)
                || string.IsNullOrWhiteSpace(repository)
                || owner.Any(char.IsWhiteSpace)
                || repository.Any(char.IsWhiteSpace))
            {
                throw new ArgumentException(
                    "EIP_GITHUB_REPOSITORIES must contain comma-separated owner/repository values.",
                    nameof(value));
            }

            repositories.Add($"{owner}/{repository}");
        }

        return new GitHubRepositoryAllowlist(repositories);
    }

    public void EnsureAuthorized(GitHubPullRequestReference reference)
    {
        if (!repositories.Contains($"{reference.Owner}/{reference.Repository}"))
        {
            throw new UnauthorizedAccessException(
                "The repository is not authorized for this pilot.");
        }
    }
}
