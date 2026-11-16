// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="CertificateService.ExportAsync"/>.</summary>
public sealed class CertificateExportOptions
{
    /// <summary>The file format. Only <c>csv</c> today. Required.</summary>
    [JsonPropertyName("format")]
    public CertificatesExportRequestFormat Format { get; set; }
}
