// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>AuditEventPaymentMethod</c> values.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(AuditEventPaymentMethodJsonConverter))]
public readonly record struct AuditEventPaymentMethod
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public AuditEventPaymentMethod(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>hosted_checkout</c>.</summary>
    public static AuditEventPaymentMethod HostedCheckout { get; } = new("hosted_checkout");

    /// <summary><c>saved_payment_method</c>.</summary>
    public static AuditEventPaymentMethod SavedPaymentMethod { get; } = new("saved_payment_method");

    /// <summary><c>invoice</c>.</summary>
    public static AuditEventPaymentMethod Invoice { get; } = new("invoice");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class AuditEventPaymentMethodJsonConverter : JsonConverter<AuditEventPaymentMethod>
{
    public override AuditEventPaymentMethod Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A AuditEventPaymentMethod must be a string."));

    public override void Write(Utf8JsonWriter writer, AuditEventPaymentMethod value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
