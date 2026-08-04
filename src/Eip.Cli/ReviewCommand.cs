using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Eip.Cli;

public sealed class ReviewCommand(
    GitHubEvidenceClient github,
    GitHubRepositoryAllowlist allowlist,
    TimeProvider timeProvider)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public async Task<string> ExecuteAsync(
        string pullRequestUrl,
        string repositoryRoot,
        string outputRoot,
        CancellationToken cancellationToken)
    {
        var reference = GitHubPullRequestReference.Parse(pullRequestUrl);
        allowlist.EnsureAuthorized(reference);
        var manifest = await github.CollectAsync(
            reference,
            timeProvider.GetUtcNow(),
            cancellationToken);
        var packId = CreatePackId(manifest);
        var directory = Path.Combine(outputRoot, packId);
        var manifestPath = Path.Combine(directory, "manifest.json");
        var temporaryPath = Path.Combine(directory, "manifest.json.tmp");

        Directory.CreateDirectory(directory);
        await using (var stream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(stream, manifest, JsonOptions, cancellationToken);
            await stream.WriteAsync("\n"u8.ToArray(), cancellationToken);
        }

        File.Move(temporaryPath, manifestPath, true);
        var readmesPath = await ReadmeLocator.WriteAsync(manifestPath, repositoryRoot, cancellationToken);
        await ReadmeContentReader.WriteAsync(readmesPath, repositoryRoot, cancellationToken);
        return manifestPath;
    }

    private static string CreatePackId(EvidenceManifest manifest)
    {
        var identity = $"{manifest.Repository}\n{manifest.PullRequest}\n{manifest.BaseSha}\n{manifest.HeadSha}";
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(identity)));
    }
}
