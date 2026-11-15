// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>ImportPreviewRowsItemStatus</c> values.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(ImportPreviewRowsItemStatusJsonConverter))]
public readonly record struct ImportPreviewRowsItemStatus
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public ImportPreviewRowsItemStatus(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>valid</c>.</summary>
    public static ImportPreviewRowsItemStatus Valid { get; } = new("valid");

    /// <summary><c>invalid_email</c>.</summary>
    public static ImportPreviewRowsItemStatus InvalidEmail { get; } = new("invalid_email");

    /// <summary><c>duplicate_in_file</c>.</summary>
    public static ImportPreviewRowsItemStatus DuplicateInFile { get; } = new("duplicate_in_file");

    /// <summary><c>already_enrolled</c>.</summary>
    public static ImportPreviewRowsItemStatus AlreadyEnrolled { get; } = new("already_enrolled");

    /// <summary><c>already_invited</c>.</summary>
    public static ImportPreviewRowsItemStatus AlreadyInvited { get; } = new("already_invited");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class ImportPreviewRowsItemStatusJsonConverter : JsonConverter<ImportPreviewRowsItemStatus>
{
    public override ImportPreviewRowsItemStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A ImportPreviewRowsItemStatus must be a string."));

    public override void Write(Utf8JsonWriter writer, ImportPreviewRowsItemStatus value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
