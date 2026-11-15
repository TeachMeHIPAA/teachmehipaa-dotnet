// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>PaymentMethodSavedByUser</c> object.</summary>
/// <remarks>One kind of <see cref="PaymentMethodSavedBy"/>.</remarks>
public sealed record PaymentMethodSavedByUser : PaymentMethodSavedBy
{
    /// <summary>Always <c>user</c>.</summary>
    [JsonPropertyName("type")]
    public string Type { get; } = "user";

    /// <summary>The <c>name</c> field.</summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
