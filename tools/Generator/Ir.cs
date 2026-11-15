using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa.Generator;

// The parts of openapi/sdk-ir.json the generator reads. The vendored
// sdk-ir.schema.json is the full contract; these records only name fields,
// and the enums name the values the schema allows for each kind.

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
    IrRequirement IdempotencyKey,
    IrRequirement IfMatch,
    IrRequirement IfNoneMatch,
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

public sealed record IrConstraint(IrConstraintKind Kind, IReadOnlyList<string> Path, string? RequiredProperty, JsonElement? Value);

public sealed record IrResponse(IrResponseKind Kind, TypeRef? Type, string? ContentType);

public sealed record IrSecurity(IrSecurityKind Kind);

public sealed record TypeRef(TypeRefKind Kind, string? Type, string? Name, TypeRef? Items, TypeRef? Values);

public sealed record IrType(
    string Name,
    IrTypeKind Kind,
    string? Description,
    IReadOnlyList<IrField>? Fields,
    IReadOnlyList<string>? Values,
    IrDiscriminator? Discriminator);

public sealed record IrField(string Name, string? Description, bool Required, bool Nullable, TypeRef Type);

public sealed record IrDiscriminator(
    IrDiscriminatorKind Kind,
    string? Property,
    IReadOnlyDictionary<string, string> Mapping,
    string? Fallback);

public sealed record IrEvents(string Envelope, string UnknownEvent);

/// <summary>An <c>IrType</c>'s <c>kind</c>.</summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1720", Justification = "Named after the IR's kind values.")]
public enum IrTypeKind
{
    Object,
    Enum,
    Union,
}

/// <summary>A <c>TypeRef</c>'s <c>kind</c>.</summary>
public enum TypeRefKind
{
    Primitive,
    Ref,
    Array,
    Map,
}

/// <summary>Whether a method takes a header: <c>idempotencyKey</c>, <c>ifMatch</c> and <c>ifNoneMatch</c>.</summary>
public enum IrRequirement
{
    Required,
    Optional,
    None,
}

/// <summary>An <c>IrMethod</c> response's <c>kind</c>.</summary>
public enum IrResponseKind
{
    Model,
    Page,
    Binary,
    Operation,
    Empty,
    WriteResult,
}

/// <summary>An <c>IrMethod</c> security's <c>kind</c>.</summary>
public enum IrSecurityKind
{
    None,
    Bearer,
}

/// <summary>How a union tells its members apart.</summary>
public enum IrDiscriminatorKind
{
    Property,
    Presence,
}

/// <summary>A request body constraint's <c>kind</c>.</summary>
public enum IrConstraintKind
{
    Const,
    Variant,
}

// Enum values are read by name, ignoring case, so "writeResult" is
// WriteResult; the schema check has already refused any other value.
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, UseStringEnumConverter = true)]
[JsonSerializable(typeof(IrDocument))]
internal sealed partial class IrJsonContext : JsonSerializerContext
{
}
