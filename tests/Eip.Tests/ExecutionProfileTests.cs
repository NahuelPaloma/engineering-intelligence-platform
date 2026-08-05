using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Eip.Tests;

public sealed class ExecutionProfileTests
{
    private static readonly string[] UnitCollectionNames =
    [
        "evidence",
        "claims",
        "hypotheses",
        "findings",
        "contradictions",
        "abstentions"
    ];

    [Fact]
    public async Task Capability002DefaultAndExplicitProfileProduceIdenticalArtifacts()
    {
        var defaultRoot = CreateTemporaryDirectory();
        var explicitRoot = CreateTemporaryDirectory();

        try
        {
            var defaultContext = await WriteLocalContextAsync(defaultRoot);
            var explicitContext = await WriteLocalContextAsync(explicitRoot);

            var defaultExecution = await Cli.Inference.InferencePipeline.ExecuteAsync(
                defaultContext,
                CancellationToken.None);
            var explicitExecution = await Cli.Inference.InferencePipeline.ExecuteAsync(
                explicitContext,
                Cli.Inference.ExecutionProfileRegistry.Capability002ProfileId,
                CancellationToken.None);

            Assert.Equal(
                await File.ReadAllBytesAsync(defaultExecution),
                await File.ReadAllBytesAsync(explicitExecution));
            Assert.Equal(
                await File.ReadAllBytesAsync(Path.Combine(defaultRoot, "inference-report.json")),
                await File.ReadAllBytesAsync(Path.Combine(explicitRoot, "inference-report.json")));
        }
        finally
        {
            Directory.Delete(defaultRoot, true);
            Directory.Delete(explicitRoot, true);
        }
    }

    [Fact]
    public async Task EmptyCapability003ProfileUsesSharedPipelineAndProducesNoUnits()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var context = await WriteLocalContextAsync(root);
            var output = await Cli.Inference.InferencePipeline.ExecuteAsync(
                context,
                Cli.Inference.ExecutionProfileRegistry.Capability003ProfileId,
                CancellationToken.None);

            using var execution = JsonDocument.Parse(await File.ReadAllBytesAsync(output));
            var rootElement = execution.RootElement;
            Assert.Equal("reasoning_completed", rootElement.GetProperty("status").GetString());
            Assert.Equal("capability-003-empty-rules-v1", rootElement.GetProperty("rule_set_id").GetString());
            Assert.All(
                UnitCollectionNames,
                property => Assert.Empty(rootElement.GetProperty(property).EnumerateArray()));
            Assert.All(
                rootElement.GetProperty("counts").EnumerateObject(),
                count => Assert.Equal(0, count.Value.GetInt32()));
            Assert.True(File.Exists(Path.Combine(root, "inference-report.json")));
            Assert.False(File.Exists(Path.Combine(root, "contract-change-report.json")));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task ProfilesAreIsolatedAndHaveDifferentExecutionIdentities()
    {
        var capability002Root = CreateTemporaryDirectory();
        var capability003Root = CreateTemporaryDirectory();

        try
        {
            var capability002 = Cli.Inference.ExecutionProfileRegistry.Resolve(
                Cli.Inference.ExecutionProfileRegistry.Capability002ProfileId);
            var capability003 = Cli.Inference.ExecutionProfileRegistry.Resolve(
                Cli.Inference.ExecutionProfileRegistry.Capability003ProfileId);
            Assert.Equal("capability-002", capability002.PluginId);
            Assert.Equal("1", capability002.PluginVersion);
            Assert.Equal("capability-003", capability003.PluginId);
            Assert.Equal("1", capability003.PluginVersion);
            Assert.NotEqual(capability002.ProfileId, capability003.ProfileId);
            Assert.NotEqual(capability002.RuleSetId, capability003.RuleSetId);
            Assert.NotEqual(capability002.TaxonomyId, capability003.TaxonomyId);
            Assert.NotEmpty(capability002.Rules);
            Assert.Empty(capability003.Rules);
            Assert.Empty(
                capability002.Rules.Select(rule => rule.RuleId)
                    .Intersect(capability003.Rules.Select(rule => rule.RuleId), StringComparer.Ordinal));
            Assert.Same(
                Assert.Single(capability002.ValidationAdapters),
                capability003.ValidationAdapters[0]);
            Assert.Equal(2, capability003.ValidationAdapters.Length);

            var capability002Output = await Cli.Inference.InferencePipeline.ExecuteAsync(
                await WriteLocalContextAsync(capability002Root),
                capability002.ProfileId,
                CancellationToken.None);
            var capability003Output = await Cli.Inference.InferencePipeline.ExecuteAsync(
                await WriteLocalContextAsync(capability003Root),
                capability003.ProfileId,
                CancellationToken.None);
            using var capability002Execution = JsonDocument.Parse(
                await File.ReadAllBytesAsync(capability002Output));
            using var capability003Execution = JsonDocument.Parse(
                await File.ReadAllBytesAsync(capability003Output));

            Assert.NotEqual(
                capability002Execution.RootElement.GetProperty("execution_id").GetString(),
                capability003Execution.RootElement.GetProperty("execution_id").GetString());
        }
        finally
        {
            Directory.Delete(capability002Root, true);
            Directory.Delete(capability003Root, true);
        }
    }

    [Fact]
    public void ProfileConstructionRejectsAdapterOutsideSelectedRuleSet()
    {
        var capability003 = Cli.Inference.ExecutionProfileRegistry.Resolve(
            Cli.Inference.ExecutionProfileRegistry.Capability003ProfileId);

        Assert.Throws<ArgumentException>(() =>
            new Cli.Inference.ExecutionProfile(
                capability003.PluginId,
                capability003.PluginVersion,
                capability003.ProfileId,
                capability003.ProfileVersion,
                capability003.RuleSetId,
                capability003.TaxonomyId,
                capability003.TaxonomyVersion,
                capability003.InputAdapters,
                [],
                [new Cli.Inference.DocumentAvailabilityRuleAdapter()],
                capability003.ValidationAdapters));
    }

    [Fact]
    public async Task ProfileRuleSetAndTaxonomyMateriallyAffectIdentity()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var input = await Cli.Inference.InputBoundary.ReadAsync(
                await WriteLocalContextAsync(root),
                CancellationToken.None);
            var original = Cli.Inference.ExecutionProfileRegistry.Resolve(
                Cli.Inference.ExecutionProfileRegistry.Capability003ProfileId);
            var changedTaxonomy = new Cli.Inference.ExecutionProfile(
                original.PluginId,
                original.PluginVersion,
                original.ProfileId,
                original.ProfileVersion,
                original.RuleSetId,
                "different-taxonomy",
                original.TaxonomyVersion,
                original.InputAdapters,
                original.Rules,
                original.DomainRuleAdapters,
                original.ValidationAdapters);
            var changedRuleSet = new Cli.Inference.ExecutionProfile(
                original.PluginId,
                original.PluginVersion,
                original.ProfileId,
                original.ProfileVersion,
                "different-rule-set",
                original.TaxonomyId,
                original.TaxonomyVersion,
                original.InputAdapters,
                original.Rules,
                original.DomainRuleAdapters,
                original.ValidationAdapters);

            var identity = Cli.Inference.ExecutionIdentity.Create(input.CanonicalContextIdentity, original);
            Assert.NotEqual(
                identity,
                Cli.Inference.ExecutionIdentity.Create(input.CanonicalContextIdentity, changedTaxonomy));
            Assert.NotEqual(
                identity,
                Cli.Inference.ExecutionIdentity.Create(input.CanonicalContextIdentity, changedRuleSet));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void RegistryContainsOnlyTheTwoExplicitProfiles()
    {
        Assert.IsAssignableFrom<FrozenDictionary<string, Cli.Inference.ExecutionProfile>>(
            Cli.Inference.ExecutionProfileRegistry.RegisteredProfiles);
        Assert.Equal(2, Cli.Inference.ExecutionProfileRegistry.RegisteredProfiles.Count);
        Assert.Equal(
            Cli.Inference.ExecutionProfileRegistry.Capability002ProfileId,
            Cli.Inference.ExecutionProfileRegistry.Resolve(
                Cli.Inference.ExecutionProfileRegistry.Capability002ProfileId).ProfileId);
        Assert.Equal(
            Cli.Inference.ExecutionProfileRegistry.Capability003ProfileId,
            Cli.Inference.ExecutionProfileRegistry.Resolve(
                Cli.Inference.ExecutionProfileRegistry.Capability003ProfileId).ProfileId);
        Assert.Throws<ArgumentException>(() =>
            Cli.Inference.ExecutionProfileRegistry.Resolve("unregistered-profile"));
    }

    [Fact]
    public async Task PluginProfileAndPluginVersionAreIndependentIdentityTerms()
    {
        var root = CreateTemporaryDirectory();

        try
        {
            var input = await Cli.Inference.InputBoundary.ReadAsync(
                await WriteLocalContextAsync(root),
                CancellationToken.None);
            var original = Cli.Inference.ExecutionProfileRegistry.Resolve(
                Cli.Inference.ExecutionProfileRegistry.Capability003ProfileId);
            var samePluginDifferentProfile = CopyProfile(original, profileId: "another-profile");
            var differentPluginSimilarProfile = CopyProfile(original, pluginId: "another-plugin");
            var differentPluginVersion = CopyProfile(original, pluginVersion: "2");
            var identity = Cli.Inference.ExecutionIdentity.Create(input.CanonicalContextIdentity, original);

            Assert.NotEqual(
                identity,
                Cli.Inference.ExecutionIdentity.Create(input.CanonicalContextIdentity, samePluginDifferentProfile));
            Assert.NotEqual(
                identity,
                Cli.Inference.ExecutionIdentity.Create(input.CanonicalContextIdentity, differentPluginSimilarProfile));
            Assert.NotEqual(
                identity,
                Cli.Inference.ExecutionIdentity.Create(input.CanonicalContextIdentity, differentPluginVersion));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private static Cli.Inference.ExecutionProfile CopyProfile(
        Cli.Inference.ExecutionProfile source,
        string? pluginId = null,
        string? pluginVersion = null,
        string? profileId = null) =>
        new(
            pluginId ?? source.PluginId,
            pluginVersion ?? source.PluginVersion,
            profileId ?? source.ProfileId,
            source.ProfileVersion,
            source.RuleSetId,
            source.TaxonomyId,
            source.TaxonomyVersion,
            source.InputAdapters,
            source.Rules,
            source.DomainRuleAdapters,
            source.ValidationAdapters);

    private static async Task<string> WriteLocalContextAsync(string root)
    {
        var context = new JsonObject
        {
            ["repository"] = "example/widgets",
            ["pull_request"] = 123,
            ["pack_id"] = "f166a136da542904223312b67fbb42ba5d1436fd29a399f7956d02ef50525bdd",
            ["modified_files"] = new JsonArray(),
            ["documents"] = new JsonArray
            {
                new JsonObject
                {
                    ["path"] = "README.md",
                    ["score"] = 10,
                    ["reason"] = "repository_root",
                    ["name"] = "Example",
                    ["purpose"] = "Provides an example.",
                    ["content"] = "# Example\n\nProvides an example.\n",
                    ["status"] = "extracted",
                    ["evidence"] = new JsonArray()
                }
            }
        };
        var path = Path.Combine(root, "local-context.json");
        await File.WriteAllTextAsync(
            path,
            context.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");
        return path;
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"eip-profile-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
