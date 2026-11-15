// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The order's status.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(SeatOrderCreatedStatusJsonConverter))]
public readonly record struct SeatOrderCreatedStatus
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public SeatOrderCreatedStatus(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>requires_action</c>.</summary>
    public static SeatOrderCreatedStatus RequiresAction { get; } = new("requires_action");

    /// <summary><c>processing</c>.</summary>
    public static SeatOrderCreatedStatus Processing { get; } = new("processing");

    /// <summary><c>invoiced</c>.</summary>
    public static SeatOrderCreatedStatus Invoiced { get; } = new("invoiced");

    /// <summary><c>paid</c>.</summary>
    public static SeatOrderCreatedStatus Paid { get; } = new("paid");

    /// <summary><c>failed</c>.</summary>
    public static SeatOrderCreatedStatus Failed { get; } = new("failed");

    /// <summary><c>expired</c>.</summary>
    public static SeatOrderCreatedStatus Expired { get; } = new("expired");

    /// <summary><c>refunded</c>.</summary>
    public static SeatOrderCreatedStatus Refunded { get; } = new("refunded");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class SeatOrderCreatedStatusJsonConverter : JsonConverter<SeatOrderCreatedStatus>
{
    public override SeatOrderCreatedStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A SeatOrderCreatedStatus must be a string."));

    public override void Write(Utf8JsonWriter writer, SeatOrderCreatedStatus value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
