// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>ListEnrollmentsArchived</c> values.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(ListEnrollmentsArchivedJsonConverter))]
public readonly record struct ListEnrollmentsArchived
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public ListEnrollmentsArchived(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>true</c>.</summary>
    public static ListEnrollmentsArchived True { get; } = new("true");

    /// <summary><c>false</c>.</summary>
    public static ListEnrollmentsArchived False { get; } = new("false");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class ListEnrollmentsArchivedJsonConverter : JsonConverter<ListEnrollmentsArchived>
{
    public override ListEnrollmentsArchived Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A ListEnrollmentsArchived must be a string."));

    public override void Write(Utf8JsonWriter writer, ListEnrollmentsArchived value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
