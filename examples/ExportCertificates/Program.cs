// Exports the Organization's Certificates from the Sandbox to a CSV file,
// waiting for the export Operation to finish.
//
//   TMH_CLIENT_ID=cli_... TMH_CLIENT_SECRET=tmh_cs_... \
//     dotnet run --project examples/ExportCertificates -- certificates.csv
using TeachMeHipaa;

var path = args.Length > 0 ? args[0] : "certificates.csv";
var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
{
    ClientId = RequiredVariable("TMH_CLIENT_ID"),
    ClientSecret = RequiredVariable("TMH_CLIENT_SECRET"),
    Environment = TeachMeHipaaEnvironment.Sandbox,
    Scopes = ["certificates:read"],
});

try
{
    var operation = await client.Certificates.ExportAsync(new CertificateExportOptions
    {
        Format = CertificatesExportRequestFormat.Csv,
    });
    var done = await operation.WaitAsync(timeout: TimeSpan.FromMinutes(2));
    var export = done.GetExportResult();

    // The CSV has CRLF line endings (RFC 4180); writing the string as is keeps them.
    await File.WriteAllTextAsync(path, export.Csv);
    Console.WriteLine($"Wrote {export.RowCount} Certificates to {path}");
    return 0;
}
catch (OperationFailedException error)
{
    Console.Error.WriteLine($"The export failed: {error.Operation.Error?.Detail ?? error.Message}");
    return 1;
}
catch (TeachMeHipaaException error) when (error.Code == "export_too_large")
{
    Console.Error.WriteLine("Too many Certificates for one file.");
    return 1;
}

static string RequiredVariable(string name) =>
    Environment.GetEnvironmentVariable(name) is { Length: > 0 } value
        ? value
        : throw new InvalidOperationException($"Set {name} to your Sandbox API client's credentials.");
