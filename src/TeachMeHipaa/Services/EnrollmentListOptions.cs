// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="EnrollmentService.ListAsync"/>.</summary>
public sealed class EnrollmentListOptions
{
    /// <summary>How many objects to return, between 1 and 1000. Defaults to 20.</summary>
    public long? Limit { get; set; }

    /// <summary>The <c>nextCursor</c> from the previous page.</summary>
    public string? Cursor { get; set; }

    /// <summary>Filter on the derived effectiveStatus.</summary>
    public ListEnrollmentsStatus? Status { get; set; }

    /// <summary>The <c>courseId</c> parameter.</summary>
    public string? CourseId { get; set; }

    /// <summary>The <c>archived</c> parameter.</summary>
    public ListEnrollmentsArchived? Archived { get; set; }

    /// <summary>Opt-in enrichment. With <c>priorCertificate</c>, an item carries <c>priorCertificate</c> (object, or null when the server confirmed there is none). The key is omitted on an item whose answer is unknown, and on every item of the page if the certificate lookup failed; clients must treat a missing key as unknown, never as null.</summary>
    public ListEnrollmentsInclude? Include { get; set; }
}
