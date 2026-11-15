// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="EnrollmentService.PreviewImportAsync"/>.</summary>
public sealed class EnrollmentPreviewImportOptions
{
    /// <summary>UTF-8 CSV (RFC 4180: quoted fields, <c>""</c> escapes, CRLF/LF). Header row required. Required.</summary>
    public Stream File { get; set; } = Stream.Null;

    /// <summary>The <c>options</c> parameter.</summary>
    public ImportOptions? Options { get; set; }
}
