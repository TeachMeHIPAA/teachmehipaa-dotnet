// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>Where to send the buyer next: a checkout <c>url</c> to redirect to, or a hosted invoice.</summary>
/// <remarks>
/// Known kinds: <see cref="SeatOrderCreatedNextActionInvoice"/>, <see cref="SeatOrderCreatedNextActionRedirect"/>, <see cref="UnknownSeatOrderCreatedNextAction"/>.
/// Chosen by <c>type</c>; any other value reads as <see cref="UnknownSeatOrderCreatedNextAction"/>.
/// </remarks>
[JsonConverter(typeof(SeatOrderCreatedNextActionJsonConverter))]
public abstract record SeatOrderCreatedNextAction : TeachMeHipaaEntity
{
    private protected SeatOrderCreatedNextAction()
    {
    }
}

internal sealed class SeatOrderCreatedNextActionJsonConverter : JsonConverter<SeatOrderCreatedNextAction>
{
    public override SeatOrderCreatedNextAction? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var json = document.RootElement;
        var tag = json.ValueKind == JsonValueKind.Object && json.TryGetProperty("type", out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
        return tag switch
        {
            "invoice" => json.Deserialize(TeachMeHipaaJsonContext.Default.SeatOrderCreatedNextActionInvoice),
            "redirect" => json.Deserialize(TeachMeHipaaJsonContext.Default.SeatOrderCreatedNextActionRedirect),
            _ => json.Deserialize(TeachMeHipaaJsonContext.Default.UnknownSeatOrderCreatedNextAction),
        };
    }

    public override void Write(Utf8JsonWriter writer, SeatOrderCreatedNextAction value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case SeatOrderCreatedNextActionInvoice member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.SeatOrderCreatedNextActionInvoice);
                break;
            case SeatOrderCreatedNextActionRedirect member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.SeatOrderCreatedNextActionRedirect);
                break;
            case UnknownSeatOrderCreatedNextAction member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.UnknownSeatOrderCreatedNextAction);
                break;
        }
    }
}
