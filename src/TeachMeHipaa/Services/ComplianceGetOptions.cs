// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="ComplianceService.GetAsync"/>.</summary>
public sealed class ComplianceGetOptions
{
    /// <summary>The course. Defaults to the organization's default course.</summary>
    public string? CourseId { get; set; }
}
