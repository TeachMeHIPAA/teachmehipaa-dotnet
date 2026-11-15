// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>Learners by status: <c>learners</c>, <c>compliant</c>, <c>expiringSoon</c>, <c>expired</c>, <c>inProgress</c>, <c>notStarted</c> and <c>inviteBounced</c>.</summary>
public sealed record ComplianceSummaryTotals : TeachMeHipaaEntity
{
    /// <summary>The <c>learners</c> field.</summary>
    [JsonPropertyName("learners")]
    public long Learners { get; init; }

    /// <summary>The <c>compliant</c> field.</summary>
    [JsonPropertyName("compliant")]
    public long Compliant { get; init; }

    /// <summary>The <c>expiringSoon</c> field.</summary>
    [JsonPropertyName("expiringSoon")]
    public long ExpiringSoon { get; init; }

    /// <summary>The <c>expired</c> field.</summary>
    [JsonPropertyName("expired")]
    public long Expired { get; init; }

    /// <summary>The <c>inProgress</c> field.</summary>
    [JsonPropertyName("inProgress")]
    public long InProgress { get; init; }

    /// <summary>The <c>notStarted</c> field.</summary>
    [JsonPropertyName("notStarted")]
    public long NotStarted { get; init; }

    /// <summary>The <c>inviteBounced</c> field.</summary>
    [JsonPropertyName("inviteBounced")]
    public long InviteBounced { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
