// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>One of the API's kinds of <c>AuditEventActor</c>.</summary>
/// <remarks>
/// Known kinds: <see cref="AuditEventActorAgent"/>, <see cref="AuditEventActorClient"/>, <see cref="AuditEventActorScim"/>, <see cref="AuditEventActorUser"/>, <see cref="UnknownAuditEventActor"/>.
/// Chosen by <c>type</c>; any other value reads as <see cref="UnknownAuditEventActor"/>.
/// </remarks>
[JsonConverter(typeof(AuditEventActorJsonConverter))]
public abstract record AuditEventActor : TeachMeHipaaEntity
{
    private protected AuditEventActor()
    {
    }
}

internal sealed class AuditEventActorJsonConverter : JsonConverter<AuditEventActor>
{
    public override AuditEventActor? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var json = document.RootElement;
        var tag = json.ValueKind == JsonValueKind.Object && json.TryGetProperty("type", out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
        return tag switch
        {
            "agent" => json.Deserialize(TeachMeHipaaJsonContext.Default.AuditEventActorAgent),
            "client" => json.Deserialize(TeachMeHipaaJsonContext.Default.AuditEventActorClient),
            "scim" => json.Deserialize(TeachMeHipaaJsonContext.Default.AuditEventActorScim),
            "user" => json.Deserialize(TeachMeHipaaJsonContext.Default.AuditEventActorUser),
            _ => json.Deserialize(TeachMeHipaaJsonContext.Default.UnknownAuditEventActor),
        };
    }

    public override void Write(Utf8JsonWriter writer, AuditEventActor value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case AuditEventActorAgent member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.AuditEventActorAgent);
                break;
            case AuditEventActorClient member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.AuditEventActorClient);
                break;
            case AuditEventActorScim member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.AuditEventActorScim);
                break;
            case AuditEventActorUser member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.AuditEventActorUser);
                break;
            case UnknownAuditEventActor member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.UnknownAuditEventActor);
                break;
        }
    }
}
