// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary><c>hosted_checkout</c>, <c>saved_payment_method</c> or <c>invoice</c>.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(SeatOrderPaymentMethodJsonConverter))]
public readonly record struct SeatOrderPaymentMethod
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public SeatOrderPaymentMethod(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>hosted_checkout</c>.</summary>
    public static SeatOrderPaymentMethod HostedCheckout { get; } = new("hosted_checkout");

    /// <summary><c>saved_payment_method</c>.</summary>
    public static SeatOrderPaymentMethod SavedPaymentMethod { get; } = new("saved_payment_method");

    /// <summary><c>invoice</c>.</summary>
    public static SeatOrderPaymentMethod Invoice { get; } = new("invoice");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class SeatOrderPaymentMethodJsonConverter : JsonConverter<SeatOrderPaymentMethod>
{
    public override SeatOrderPaymentMethod Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A SeatOrderPaymentMethod must be a string."));

    public override void Write(Utf8JsonWriter writer, SeatOrderPaymentMethod value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
