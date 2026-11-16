// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The action that was accepted: <c>remind</c> or <c>resendInvite</c>.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(EnrollmentActionAcceptedActionJsonConverter))]
public readonly record struct EnrollmentActionAcceptedAction
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public EnrollmentActionAcceptedAction(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>remind</c>.</summary>
    public static EnrollmentActionAcceptedAction Remind { get; } = new("remind");

    /// <summary><c>resendInvite</c>.</summary>
    public static EnrollmentActionAcceptedAction ResendInvite { get; } = new("resendInvite");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class EnrollmentActionAcceptedActionJsonConverter : JsonConverter<EnrollmentActionAcceptedAction>
{
    public override EnrollmentActionAcceptedAction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A EnrollmentActionAcceptedAction must be a string."));

    public override void Write(Utf8JsonWriter writer, EnrollmentActionAcceptedAction value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
