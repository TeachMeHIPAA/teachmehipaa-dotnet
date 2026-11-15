// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary><c>requires_action</c>, <c>processing</c>, <c>invoiced</c>, <c>paid</c>, <c>failed</c>, <c>expired</c> or <c>refunded</c>.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(SeatOrderStatusJsonConverter))]
public readonly record struct SeatOrderStatus
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public SeatOrderStatus(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>requires_action</c>.</summary>
    public static SeatOrderStatus RequiresAction { get; } = new("requires_action");

    /// <summary><c>processing</c>.</summary>
    public static SeatOrderStatus Processing { get; } = new("processing");

    /// <summary><c>invoiced</c>.</summary>
    public static SeatOrderStatus Invoiced { get; } = new("invoiced");

    /// <summary><c>paid</c>.</summary>
    public static SeatOrderStatus Paid { get; } = new("paid");

    /// <summary><c>failed</c>.</summary>
    public static SeatOrderStatus Failed { get; } = new("failed");

    /// <summary><c>expired</c>.</summary>
    public static SeatOrderStatus Expired { get; } = new("expired");

    /// <summary><c>refunded</c>.</summary>
    public static SeatOrderStatus Refunded { get; } = new("refunded");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class SeatOrderStatusJsonConverter : JsonConverter<SeatOrderStatus>
{
    public override SeatOrderStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A SeatOrderStatus must be a string."));

    public override void Write(Utf8JsonWriter writer, SeatOrderStatus value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
