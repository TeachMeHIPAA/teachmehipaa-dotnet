// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>AuditEventLearnerUpdatedFieldsItem</c> values.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(AuditEventLearnerUpdatedFieldsItemJsonConverter))]
public readonly record struct AuditEventLearnerUpdatedFieldsItem
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public AuditEventLearnerUpdatedFieldsItem(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>name</c>.</summary>
    public static AuditEventLearnerUpdatedFieldsItem Name { get; } = new("name");

    /// <summary><c>externalId</c>.</summary>
    public static AuditEventLearnerUpdatedFieldsItem ExternalId { get; } = new("externalId");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class AuditEventLearnerUpdatedFieldsItemJsonConverter : JsonConverter<AuditEventLearnerUpdatedFieldsItem>
{
    public override AuditEventLearnerUpdatedFieldsItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A AuditEventLearnerUpdatedFieldsItem must be a string."));

    public override void Write(Utf8JsonWriter writer, AuditEventLearnerUpdatedFieldsItem value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
