// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="EnrollmentService.PreviewImportAsync"/>.</summary>
public sealed class EnrollmentPreviewImportOptions
{
    /// <summary>Creates the parameters with the file to upload.</summary>
    /// <param name="file">UTF-8 CSV (RFC 4180: quoted fields, <c>""</c> escapes, CRLF/LF). Header row required.</param>
    public EnrollmentPreviewImportOptions(FileUpload file) => File = file;

    /// <summary>UTF-8 CSV (RFC 4180: quoted fields, <c>""</c> escapes, CRLF/LF). Header row required. Required.</summary>
    public FileUpload File { get; set; }

    /// <summary>The <c>options</c> parameter.</summary>
    public ImportOptions? Options { get; set; }
}
