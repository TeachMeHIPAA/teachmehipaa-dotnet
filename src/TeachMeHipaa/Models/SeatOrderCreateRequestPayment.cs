// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>How to pay: <c>hosted_checkout</c> with a <c>returnUrl</c>, <c>saved_payment_method</c> with the card's <c>paymentMethodId</c>, or <c>invoice</c>. A <c>returnUrl</c> must be https and on your API client's allowlist.</summary>
/// <remarks>
/// Known kinds: <see cref="SeatOrderCreateRequestPaymentHostedCheckout"/>, <see cref="SeatOrderCreateRequestPaymentInvoice"/>, <see cref="SeatOrderCreateRequestPaymentSavedPaymentMethod"/>, <see cref="UnknownSeatOrderCreateRequestPayment"/>.
/// Chosen by <c>method</c>; any other value reads as <see cref="UnknownSeatOrderCreateRequestPayment"/>.
/// </remarks>
[JsonConverter(typeof(SeatOrderCreateRequestPaymentJsonConverter))]
public abstract record SeatOrderCreateRequestPayment : TeachMeHipaaEntity
{
    private protected SeatOrderCreateRequestPayment()
    {
    }
}

internal sealed class SeatOrderCreateRequestPaymentJsonConverter : JsonConverter<SeatOrderCreateRequestPayment>
{
    public override SeatOrderCreateRequestPayment? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var json = document.RootElement;
        var tag = json.ValueKind == JsonValueKind.Object && json.TryGetProperty("method", out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
        return tag switch
        {
            "hosted_checkout" => json.Deserialize(TeachMeHipaaJsonContext.Default.SeatOrderCreateRequestPaymentHostedCheckout),
            "invoice" => json.Deserialize(TeachMeHipaaJsonContext.Default.SeatOrderCreateRequestPaymentInvoice),
            "saved_payment_method" => json.Deserialize(TeachMeHipaaJsonContext.Default.SeatOrderCreateRequestPaymentSavedPaymentMethod),
            _ => json.Deserialize(TeachMeHipaaJsonContext.Default.UnknownSeatOrderCreateRequestPayment),
        };
    }

    public override void Write(Utf8JsonWriter writer, SeatOrderCreateRequestPayment value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case SeatOrderCreateRequestPaymentHostedCheckout member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.SeatOrderCreateRequestPaymentHostedCheckout);
                break;
            case SeatOrderCreateRequestPaymentInvoice member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.SeatOrderCreateRequestPaymentInvoice);
                break;
            case SeatOrderCreateRequestPaymentSavedPaymentMethod member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.SeatOrderCreateRequestPaymentSavedPaymentMethod);
                break;
            case UnknownSeatOrderCreateRequestPayment member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.UnknownSeatOrderCreateRequestPayment);
                break;
        }
    }
}
