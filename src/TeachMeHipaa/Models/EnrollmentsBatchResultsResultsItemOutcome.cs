// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>EnrollmentsBatchResultsResultsItemOutcome</c> values.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(EnrollmentsBatchResultsResultsItemOutcomeJsonConverter))]
public readonly record struct EnrollmentsBatchResultsResultsItemOutcome
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public EnrollmentsBatchResultsResultsItemOutcome(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>succeeded</c>.</summary>
    public static EnrollmentsBatchResultsResultsItemOutcome Succeeded { get; } = new("succeeded");

    /// <summary><c>skipped</c>.</summary>
    public static EnrollmentsBatchResultsResultsItemOutcome Skipped { get; } = new("skipped");

    /// <summary><c>failed</c>.</summary>
    public static EnrollmentsBatchResultsResultsItemOutcome Failed { get; } = new("failed");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class EnrollmentsBatchResultsResultsItemOutcomeJsonConverter : JsonConverter<EnrollmentsBatchResultsResultsItemOutcome>
{
    public override EnrollmentsBatchResultsResultsItemOutcome Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A EnrollmentsBatchResultsResultsItemOutcome must be a string."));

    public override void Write(Utf8JsonWriter writer, EnrollmentsBatchResultsResultsItemOutcome value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
