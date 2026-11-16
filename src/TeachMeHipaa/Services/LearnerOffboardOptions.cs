// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="LearnerService.OffboardAsync"/>.</summary>
public sealed class LearnerOffboardOptions
{
    /// <summary>Sent as <c>If-Match</c> when set: the ETag you read the object with, or the object itself. If the object changed since, the call fails with <see cref="PreconditionFailedException"/>.</summary>
    public ETag? IfMatch { get; set; }

    /// <summary>The course. Defaults to the organization's default course.</summary>
    public string? CourseId { get; set; }
}
