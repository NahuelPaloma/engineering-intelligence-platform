using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Eip.Cli;

public sealed class GitHubEvidenceClient(HttpClient httpClient)
{
    private const int PageSize = 100;

    public static HttpClient CreateHttpClient(string? token)
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri("https://api.github.com/")
        };
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        client.DefaultRequestHeaders.UserAgent.ParseAdd("eip-vs001-pilot/1.0");

        if (!string.IsNullOrWhiteSpace(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    public async Task<EvidenceManifest> CollectAsync(
        GitHubPullRequestReference reference,
        DateTimeOffset generatedAt,
        CancellationToken cancellationToken)
    {
        var prefix = $"repos/{Uri.EscapeDataString(reference.Owner)}/{Uri.EscapeDataString(reference.Repository)}";
        var pullRequest = await GetRequiredAsync<PullRequestResponse>(
            $"{prefix}/pulls/{reference.Number}",
            cancellationToken);
        if (pullRequest.User is null || pullRequest.Base is null || pullRequest.Head is null)
        {
            throw new HttpRequestException(
                $"GitHub returned an incomplete pull request response for {reference.Owner}/{reference.Repository}#{reference.Number}.");
        }

        var commits = await GetAllPagesAsync<CommitResponse>(
            $"{prefix}/pulls/{reference.Number}/commits",
            cancellationToken);
        var files = await GetAllPagesAsync<ChangedFileResponse>(
            $"{prefix}/pulls/{reference.Number}/files",
            cancellationToken);

        if (commits.Any(commit => commit.Commit is null || commit.Commit.Author is null))
        {
            throw new HttpRequestException(
                $"GitHub returned an incomplete commit response for {reference.Owner}/{reference.Repository}#{reference.Number}.");
        }

        return new EvidenceManifest(
            $"{reference.Owner}/{reference.Repository}",
            reference.Owner,
            reference.Number,
            pullRequest.Title,
            pullRequest.User.Login,
            pullRequest.Base.Sha,
            pullRequest.Head.Sha,
            commits.Select(commit => new CommitEvidence(
                commit.Sha,
                FirstLine(commit.Commit.Message),
                commit.Author?.Login ?? commit.Commit.Author.Name)).ToArray(),
            files.Select(file => new ChangedFileEvidence(
                file.Filename,
                file.Status,
                file.PreviousFilename,
                file.Additions,
                file.Deletions)).ToArray(),
            generatedAt);
    }

    private async Task<T> GetRequiredAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(path, cancellationToken);
        response.EnsureSuccessStatusCode();

        try
        {
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken)
                ?? throw new HttpRequestException($"GitHub returned an empty response for {path}.");
        }
        catch (JsonException exception)
        {
            throw new HttpRequestException($"GitHub returned an unparseable response for {path}.", exception);
        }
    }

    private async Task<IReadOnlyList<T>> GetAllPagesAsync<T>(
        string path,
        CancellationToken cancellationToken)
    {
        var result = new List<T>();

        for (var page = 1; ; page++)
        {
            var separator = path.Contains('?', StringComparison.Ordinal) ? '&' : '?';
            var items = await GetRequiredAsync<T[]>(
                $"{path}{separator}per_page={PageSize}&page={page}",
                cancellationToken);
            result.AddRange(items);

            if (items.Length < PageSize)
            {
                return result;
            }
        }
    }

    private static string FirstLine(string value) =>
        value.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;

    private sealed record PullRequestResponse(
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("user")] UserResponse? User,
        [property: JsonPropertyName("base")] GitReferenceResponse? Base,
        [property: JsonPropertyName("head")] GitReferenceResponse? Head);

    private sealed record UserResponse([property: JsonPropertyName("login")] string Login);

    private sealed record GitReferenceResponse([property: JsonPropertyName("sha")] string Sha);

    private sealed record CommitResponse(
        [property: JsonPropertyName("sha")] string Sha,
        [property: JsonPropertyName("commit")] CommitDetailResponse Commit,
        [property: JsonPropertyName("author")] UserResponse? Author);

    private sealed record CommitDetailResponse(
        [property: JsonPropertyName("message")] string Message,
        [property: JsonPropertyName("author")] CommitAuthorResponse Author);

    private sealed record CommitAuthorResponse([property: JsonPropertyName("name")] string Name);

    private sealed record ChangedFileResponse(
        [property: JsonPropertyName("filename")] string Filename,
        [property: JsonPropertyName("status")] string Status,
        [property: JsonPropertyName("previous_filename")] string? PreviousFilename,
        [property: JsonPropertyName("additions")] int Additions,
        [property: JsonPropertyName("deletions")] int Deletions);
}
