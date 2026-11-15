// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>AuditEventApiClientUpdatedFieldsItem</c> values.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(AuditEventApiClientUpdatedFieldsItemJsonConverter))]
public readonly record struct AuditEventApiClientUpdatedFieldsItem
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public AuditEventApiClientUpdatedFieldsItem(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>name</c>.</summary>
    public static AuditEventApiClientUpdatedFieldsItem Name { get; } = new("name");

    /// <summary><c>maxSeatsPerOrder</c>.</summary>
    public static AuditEventApiClientUpdatedFieldsItem MaxSeatsPerOrder { get; } = new("maxSeatsPerOrder");

    /// <summary><c>maxSpendPer30Days</c>.</summary>
    public static AuditEventApiClientUpdatedFieldsItem MaxSpendPer30Days { get; } = new("maxSpendPer30Days");

    /// <summary><c>returnUrlAllowlist</c>.</summary>
    public static AuditEventApiClientUpdatedFieldsItem ReturnUrlAllowlist { get; } = new("returnUrlAllowlist");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class AuditEventApiClientUpdatedFieldsItemJsonConverter : JsonConverter<AuditEventApiClientUpdatedFieldsItem>
{
    public override AuditEventApiClientUpdatedFieldsItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A AuditEventApiClientUpdatedFieldsItem must be a string."));

    public override void Write(Utf8JsonWriter writer, AuditEventApiClientUpdatedFieldsItem value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
