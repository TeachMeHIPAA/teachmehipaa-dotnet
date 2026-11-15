// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="LearnerService.UpdateAsync"/>.</summary>
/// <remarks>Sent as a JSON merge patch: a property left unset is left out, so the field keeps its value, and one set to null clears the field.</remarks>
public sealed class LearnerUpdateOptions
{
    /// <summary>Creates the parameters of a conditional update.</summary>
    /// <param name="ifMatch">The ETag you read the object with, or the object itself. Sent as <c>If-Match</c>.</param>
    public LearnerUpdateOptions(ETag ifMatch) => IfMatch = ifMatch;

    /// <summary>Sent as <c>If-Match</c>: the ETag you read the object with, or the object itself. If the object changed since, the call fails with <see cref="PreconditionFailedException"/>. Required.</summary>
    [JsonIgnore]
    public ETag IfMatch { get; set; }

    /// <summary>The learner's full name.</summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonConverter(typeof(OptionalJsonConverter<string>))]
    public Optional<string> Name { get; set; }

    /// <summary>Your own id for the learner. Set it to <c>null</c> to clear it.</summary>
    [JsonPropertyName("externalId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonConverter(typeof(OptionalJsonConverter<string>))]
    public Optional<string?> ExternalId { get; set; }

    /// <summary>The course. Defaults to the organization's default course.</summary>
    [JsonIgnore]
    public string? CourseId { get; set; }
}
