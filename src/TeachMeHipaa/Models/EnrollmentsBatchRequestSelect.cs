// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The enrollments to act on: a list of <c>enrollmentIds</c> (up to 500), or a <c>filter</c>.</summary>
/// <remarks>
/// Known kinds: <see cref="EnrollmentsBatchRequestSelectEnrollmentIds"/>, <see cref="EnrollmentsBatchRequestSelectFilter"/>.
/// Chosen by which of their fields is present.
/// </remarks>
[JsonConverter(typeof(EnrollmentsBatchRequestSelectJsonConverter))]
public abstract record EnrollmentsBatchRequestSelect : TeachMeHipaaEntity
{
    private protected EnrollmentsBatchRequestSelect()
    {
    }
}

internal sealed class EnrollmentsBatchRequestSelectJsonConverter : JsonConverter<EnrollmentsBatchRequestSelect>
{
    public override EnrollmentsBatchRequestSelect? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var json = document.RootElement;
        if (json.ValueKind == JsonValueKind.Object && json.TryGetProperty("enrollmentIds", out _))
        {
            return json.Deserialize(TeachMeHipaaJsonContext.Default.EnrollmentsBatchRequestSelectEnrollmentIds);
        }

        if (json.ValueKind == JsonValueKind.Object && json.TryGetProperty("filter", out _))
        {
            return json.Deserialize(TeachMeHipaaJsonContext.Default.EnrollmentsBatchRequestSelectFilter);
        }

        throw new JsonException("A EnrollmentsBatchRequestSelect has none of enrollmentIds, filter.");
    }

    public override void Write(Utf8JsonWriter writer, EnrollmentsBatchRequestSelect value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case EnrollmentsBatchRequestSelectEnrollmentIds member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.EnrollmentsBatchRequestSelectEnrollmentIds);
                break;
            case EnrollmentsBatchRequestSelectFilter member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.EnrollmentsBatchRequestSelectFilter);
                break;
        }
    }
}
