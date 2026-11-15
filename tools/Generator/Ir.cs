using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa.Generator;

// The parts of openapi/sdk-ir.json the generator reads. The vendored
// sdk-ir.schema.json is the full contract; these records only name fields.

public sealed record IrDocument(
    int IrVersion,
    IrSource Source,
    IReadOnlyList<IrResource> Resources,
    IReadOnlyList<IrType> Types,
    IrEvents Events);

public sealed record IrSource(string Release, string Sha256);

public sealed record IrResource(IReadOnlyList<string> Path, IReadOnlyList<IrMethod> Methods);

public sealed record IrMethod(
    string Name,
    string OperationId,
    string HttpMethod,
    string? Summary,
    string? Description,
    IReadOnlyList<IrParam> HeaderParams,
    string IdempotencyKey,
    bool OrgScoped,
    IrPagination? Pagination,
    IrPath Path,
    IReadOnlyList<IrParam> PathParams,
    IReadOnlyList<IrParam> QueryParams,
    IrRequestBody? RequestBody,
    IrResponse Response,
    bool RetrySafe,
    IrSecurity Security);

public sealed record IrPath(string Template, string? CustomVerb);

public sealed record IrParam(string Name, string? Description, bool Required, TypeRef Type);

public sealed record IrPagination(string CursorParam, string Style, TypeRef ItemType);

public sealed record IrRequestBody(
    IReadOnlyList<IrBinaryPart> BinaryParts,
    IReadOnlyList<IrConstraint> Constraints,
    string ContentType,
    bool Required,
    TypeRef Type);

public sealed record IrBinaryPart(string Name, string ContentType);

public sealed record IrConstraint(string Kind, IReadOnlyList<string> Path, string? RequiredProperty, JsonElement? Value);

public sealed record IrResponse(string Kind, TypeRef? Type, string? ContentType);

public sealed record IrSecurity(string Kind);

public sealed record TypeRef(string Kind, string? Type, string? Name, TypeRef? Items, TypeRef? Values);

public sealed record IrType(
    string Name,
    string Kind,
    string? Description,
    IReadOnlyList<IrField>? Fields,
    IReadOnlyList<string>? Values,
    IrDiscriminator? Discriminator);

public sealed record IrField(string Name, string? Description, bool Required, bool Nullable, TypeRef Type);

public sealed record IrDiscriminator(
    string Kind,
    string? Property,
    IReadOnlyDictionary<string, string> Mapping,
    string? Fallback);

public sealed record IrEvents(string Envelope, string UnknownEvent);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(IrDocument))]
internal sealed partial class IrJsonContext : JsonSerializerContext
{
}
