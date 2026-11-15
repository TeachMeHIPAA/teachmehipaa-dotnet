using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>
/// How a Test delivery or a redrive ended, from <see cref="Operation.GetDeliveryResult"/>
/// on the Operation that <c>WebhookSubscriptions.TestAsync</c> or <c>RedriveAsync</c> returns.
/// </summary>
/// <remarks>
/// Known kinds: <see cref="WebhookDeliverySent"/>, <see cref="WebhookDeliveryFailed"/>,
/// <see cref="WebhookDeliverySettled"/>. Chosen by <c>_tag</c>; any other value
/// reads as <see cref="UnknownWebhookDeliveryResult"/>, so handle kinds you
/// don't recognize.
/// </remarks>
// The spec leaves these Operations' response untyped; the webhook worker
// settles them with one of these (api-sdk-surface.md §8).
[JsonConverter(typeof(WebhookDeliveryResultJsonConverter))]
public abstract record WebhookDeliveryResult
{
    private protected WebhookDeliveryResult()
    {
    }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}

/// <summary>The endpoint accepted the Delivery.</summary>
public sealed record WebhookDeliverySent : WebhookDeliveryResult
{
    /// <summary>Always <c>sent</c>.</summary>
    [JsonPropertyName("_tag")]
    public string Tag { get; } = "sent";

    /// <summary>The endpoint's HTTP status.</summary>
    [JsonPropertyName("responseStatus")]
    public long ResponseStatus { get; init; }
}

/// <summary>Delivery stopped for good; <see cref="Reason"/> says why.</summary>
public sealed record WebhookDeliveryFailed : WebhookDeliveryResult
{
    /// <summary>Always <c>failed</c>.</summary>
    [JsonPropertyName("_tag")]
    public string Tag { get; } = "failed";

    /// <summary>Why delivery stopped, such as <c>endpoint_gone</c>.</summary>
    [JsonPropertyName("reason")]
    public string Reason { get; init; } = string.Empty;

    /// <summary>The endpoint's last HTTP status, or null when it never answered.</summary>
    [JsonPropertyName("responseStatus")]
    public long? ResponseStatus { get; init; }
}

/// <summary>The Delivery had already ended before this Operation; <see cref="Status"/> says how.</summary>
public sealed record WebhookDeliverySettled : WebhookDeliveryResult
{
    /// <summary>Always <c>settled</c>.</summary>
    [JsonPropertyName("_tag")]
    public string Tag { get; } = "settled";

    /// <summary>How the Delivery ended, such as <c>delivered</c>.</summary>
    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;
}

/// <summary>
/// A <see cref="WebhookDeliveryResult"/> of a kind this version of the library
/// doesn't know. Its other fields are kept in <see cref="WebhookDeliveryResult.ExtensionData"/>.
/// </summary>
public sealed record UnknownWebhookDeliveryResult : WebhookDeliveryResult
{
    /// <summary>The kind, as the API sent it.</summary>
    [JsonPropertyName("_tag")]
    public string Tag { get; init; } = string.Empty;
}

// STJ's own polymorphism throws on an unknown discriminator, so the union
// reads its members by hand, as the generated unions do.
internal sealed class WebhookDeliveryResultJsonConverter : JsonConverter<WebhookDeliveryResult>
{
    public override WebhookDeliveryResult? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var json = document.RootElement;
        var tag = json.ValueKind == JsonValueKind.Object && json.TryGetProperty("_tag", out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
        return tag switch
        {
            "sent" => json.Deserialize(CoreJsonContext.Default.WebhookDeliverySent),
            "failed" => json.Deserialize(CoreJsonContext.Default.WebhookDeliveryFailed),
            "settled" => json.Deserialize(CoreJsonContext.Default.WebhookDeliverySettled),
            _ => json.Deserialize(CoreJsonContext.Default.UnknownWebhookDeliveryResult),
        };
    }

    public override void Write(Utf8JsonWriter writer, WebhookDeliveryResult value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case WebhookDeliverySent sent:
                JsonSerializer.Serialize(writer, sent, CoreJsonContext.Default.WebhookDeliverySent);
                break;
            case WebhookDeliveryFailed failed:
                JsonSerializer.Serialize(writer, failed, CoreJsonContext.Default.WebhookDeliveryFailed);
                break;
            case WebhookDeliverySettled settled:
                JsonSerializer.Serialize(writer, settled, CoreJsonContext.Default.WebhookDeliverySettled);
                break;
            case UnknownWebhookDeliveryResult unknown:
                JsonSerializer.Serialize(writer, unknown, CoreJsonContext.Default.UnknownWebhookDeliveryResult);
                break;
        }
    }
}
