// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>One of the API's kinds of <c>PaymentMethodSavedBy</c>.</summary>
/// <remarks>
/// Known kinds: <see cref="PaymentMethodSavedByApiClient"/>, <see cref="PaymentMethodSavedByUser"/>, <see cref="UnknownPaymentMethodSavedBy"/>.
/// Chosen by <c>type</c>; any other value reads as <see cref="UnknownPaymentMethodSavedBy"/>.
/// </remarks>
[JsonConverter(typeof(PaymentMethodSavedByJsonConverter))]
public abstract record PaymentMethodSavedBy : TeachMeHipaaEntity
{
    private protected PaymentMethodSavedBy()
    {
    }
}

internal sealed class PaymentMethodSavedByJsonConverter : JsonConverter<PaymentMethodSavedBy>
{
    public override PaymentMethodSavedBy? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var json = document.RootElement;
        var tag = json.ValueKind == JsonValueKind.Object && json.TryGetProperty("type", out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
        return tag switch
        {
            "api_client" => json.Deserialize(TeachMeHipaaJsonContext.Default.PaymentMethodSavedByApiClient),
            "user" => json.Deserialize(TeachMeHipaaJsonContext.Default.PaymentMethodSavedByUser),
            _ => json.Deserialize(TeachMeHipaaJsonContext.Default.UnknownPaymentMethodSavedBy),
        };
    }

    public override void Write(Utf8JsonWriter writer, PaymentMethodSavedBy value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case PaymentMethodSavedByApiClient member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.PaymentMethodSavedByApiClient);
                break;
            case PaymentMethodSavedByUser member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.PaymentMethodSavedByUser);
                break;
            case UnknownPaymentMethodSavedBy member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.UnknownPaymentMethodSavedBy);
                break;
        }
    }
}
