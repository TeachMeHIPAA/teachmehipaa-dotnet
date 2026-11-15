// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>One of the API's kinds of <c>CancelEnrollmentResponse</c>.</summary>
/// <remarks>
/// Known kinds: <see cref="EnrollmentCancelRefunded"/>, <see cref="Enrollment"/>.
/// Chosen by which of their fields is present, else <see cref="Enrollment"/>.
/// </remarks>
[JsonConverter(typeof(CancelEnrollmentResponseJsonConverter))]
public abstract record CancelEnrollmentResponse : TeachMeHipaaEntity
{
    private protected CancelEnrollmentResponse()
    {
    }
}

internal sealed class CancelEnrollmentResponseJsonConverter : JsonConverter<CancelEnrollmentResponse>
{
    public override CancelEnrollmentResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var json = document.RootElement;
        if (json.ValueKind == JsonValueKind.Object && json.TryGetProperty("refund", out _))
        {
            return json.Deserialize(TeachMeHipaaJsonContext.Default.EnrollmentCancelRefunded);
        }

        return json.Deserialize(TeachMeHipaaJsonContext.Default.Enrollment);
    }

    public override void Write(Utf8JsonWriter writer, CancelEnrollmentResponse value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case EnrollmentCancelRefunded member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.EnrollmentCancelRefunded);
                break;
            case Enrollment member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.Enrollment);
                break;
        }
    }
}
