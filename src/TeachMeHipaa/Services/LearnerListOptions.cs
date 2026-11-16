// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="LearnerService.ListAsync"/>.</summary>
public sealed class LearnerListOptions
{
    /// <summary>How many objects to return, between 1 and 100. Defaults to 20.</summary>
    public long? Limit { get; set; }

    /// <summary>The <c>nextCursor</c> from the previous page.</summary>
    public string? Cursor { get; set; }

    /// <summary>The <c>email</c> parameter.</summary>
    public string? Email { get; set; }

    /// <summary>The integration's own id for the learner (unique per organization), or the SCIM <c>externalId</c> when none was set.</summary>
    public string? ExternalId { get; set; }

    /// <summary>The <c>status</c> parameter.</summary>
    public LearnerStatus? Status { get; set; }

    /// <summary>The course. Defaults to the organization's default course.</summary>
    public string? CourseId { get; set; }
}
