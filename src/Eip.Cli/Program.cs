namespace Eip.Cli;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (args is not ["review", var pullRequestUrl])
        {
            Console.Error.WriteLine("Usage: vs001 review <github-pr-url>");
            return 2;
        }

        try
        {
            using var httpClient = GitHubEvidenceClient.CreateHttpClient(
                Environment.GetEnvironmentVariable("GITHUB_TOKEN")
                    ?? Environment.GetEnvironmentVariable("GH_TOKEN"));
            var allowlist = GitHubRepositoryAllowlist.Parse(
                Environment.GetEnvironmentVariable("EIP_GITHUB_REPOSITORIES"));
            var command = new ReviewCommand(
                new GitHubEvidenceClient(httpClient),
                allowlist,
                TimeProvider.System);
            var contextPackPath = await command.ExecuteAsync(
                pullRequestUrl,
                Environment.CurrentDirectory,
                Path.Combine(Environment.CurrentDirectory, "output"),
                CancellationToken.None);

            Console.WriteLine(contextPackPath);
            return 0;
        }
        catch (Exception exception) when (exception is ArgumentException or UnauthorizedAccessException
            or HttpRequestException or IOException or OperationCanceledException)
        {
            Console.Error.WriteLine($"Evidence collection failed: {exception.Message}");
            return 1;
        }
    }
}
