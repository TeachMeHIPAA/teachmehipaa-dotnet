// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>What to do to each enrollment: <c>remind</c>, <c>resendInvite</c>, <c>cancel</c>, <c>recertify</c>, <c>archive</c> or <c>unarchive</c>.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(EnrollmentsBatchRequestOperationJsonConverter))]
public readonly record struct EnrollmentsBatchRequestOperation
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public EnrollmentsBatchRequestOperation(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>remind</c>.</summary>
    public static EnrollmentsBatchRequestOperation Remind { get; } = new("remind");

    /// <summary><c>resendInvite</c>.</summary>
    public static EnrollmentsBatchRequestOperation ResendInvite { get; } = new("resendInvite");

    /// <summary><c>cancel</c>.</summary>
    public static EnrollmentsBatchRequestOperation Cancel { get; } = new("cancel");

    /// <summary><c>recertify</c>.</summary>
    public static EnrollmentsBatchRequestOperation Recertify { get; } = new("recertify");

    /// <summary><c>archive</c>.</summary>
    public static EnrollmentsBatchRequestOperation Archive { get; } = new("archive");

    /// <summary><c>unarchive</c>.</summary>
    public static EnrollmentsBatchRequestOperation Unarchive { get; } = new("unarchive");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class EnrollmentsBatchRequestOperationJsonConverter : JsonConverter<EnrollmentsBatchRequestOperation>
{
    public override EnrollmentsBatchRequestOperation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A EnrollmentsBatchRequestOperation must be a string."));

    public override void Write(Utf8JsonWriter writer, EnrollmentsBatchRequestOperation value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
