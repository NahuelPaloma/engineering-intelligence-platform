using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace Eip.Cli.Inference;

internal static class InferencePipeline
{
    internal const string EmptyRuleSetId = "capability-002-empty-rules-v1";

    private static readonly IReadOnlyList<InferenceStage> EmptyStages =
    [
        new("input_boundary", "completed"),
        new("claim_processing", "not_implemented"),
        new("hypothesis_processing", "not_implemented"),
        new("finding_processing", "not_implemented"),
        new("report_builder", "not_implemented")
    ];

    public static async Task<string> ExecuteAsync(
        string localContextPath,
        CancellationToken cancellationToken)
    {
        var input = await InputBoundary.ReadAsync(localContextPath, cancellationToken);
        var execution = new InferenceExecution(
            CreateExecutionId(input.PackId),
            input.PackId,
            EmptyRuleSetId,
            "no_inferences",
            EmptyStages,
            new InferenceCounts(0, 0, 0, 0, 0));

        return await InferenceExecutionWriter.WriteAsync(
            localContextPath,
            execution,
            cancellationToken);
    }

    private static string CreateExecutionId(string packId)
    {
        var identity = $"{packId}\n{InputBoundary.ContractId}\n{EmptyRuleSetId}";
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(identity)));
    }
}

internal sealed record InferenceExecution(
    [property: JsonPropertyName("execution_id")] string ExecutionId,
    [property: JsonPropertyName("input_pack_id")] string InputPackId,
    [property: JsonPropertyName("rule_set_id")] string RuleSetId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("stages")] IReadOnlyList<InferenceStage> Stages,
    [property: JsonPropertyName("counts")] InferenceCounts Counts);

internal sealed record InferenceStage(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("status")] string Status);

internal sealed record InferenceCounts(
    [property: JsonPropertyName("evidence")] int Evidence,
    [property: JsonPropertyName("claims")] int Claims,
    [property: JsonPropertyName("hypotheses")] int Hypotheses,
    [property: JsonPropertyName("findings")] int Findings,
    [property: JsonPropertyName("abstentions")] int Abstentions);
