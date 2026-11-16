// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="LearnerService.ExportAsync"/>.</summary>
public sealed class LearnerExportOptions
{
    /// <summary>The file format. Only <c>csv</c> today. Required.</summary>
    [JsonPropertyName("format")]
    public LearnersExportRequestFormat Format { get; set; }

    /// <summary>The course to export. Defaults to the organization's default course.</summary>
    [JsonPropertyName("courseId")]
    public string? CourseId { get; set; }

    /// <summary>Only export learners with this status.</summary>
    [JsonPropertyName("status")]
    public LearnerStatus? Status { get; set; }
}
