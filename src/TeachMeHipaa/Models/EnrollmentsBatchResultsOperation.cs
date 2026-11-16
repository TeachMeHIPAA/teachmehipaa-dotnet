// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The operation that was run.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(EnrollmentsBatchResultsOperationJsonConverter))]
public readonly record struct EnrollmentsBatchResultsOperation
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public EnrollmentsBatchResultsOperation(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>remind</c>.</summary>
    public static EnrollmentsBatchResultsOperation Remind { get; } = new("remind");

    /// <summary><c>resendInvite</c>.</summary>
    public static EnrollmentsBatchResultsOperation ResendInvite { get; } = new("resendInvite");

    /// <summary><c>cancel</c>.</summary>
    public static EnrollmentsBatchResultsOperation Cancel { get; } = new("cancel");

    /// <summary><c>recertify</c>.</summary>
    public static EnrollmentsBatchResultsOperation Recertify { get; } = new("recertify");

    /// <summary><c>archive</c>.</summary>
    public static EnrollmentsBatchResultsOperation Archive { get; } = new("archive");

    /// <summary><c>unarchive</c>.</summary>
    public static EnrollmentsBatchResultsOperation Unarchive { get; } = new("unarchive");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class EnrollmentsBatchResultsOperationJsonConverter : JsonConverter<EnrollmentsBatchResultsOperation>
{
    public override EnrollmentsBatchResultsOperation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A EnrollmentsBatchResultsOperation must be a string."));

    public override void Write(Utf8JsonWriter writer, EnrollmentsBatchResultsOperation value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
