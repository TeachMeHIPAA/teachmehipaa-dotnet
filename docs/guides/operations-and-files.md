# Operations and files

Exports, imports, batch writes, Test deliveries and redrives return an
Operation: work that may finish after the request that started it returns.
This guide covers waiting for Operations, exporting and importing CSV
files, and downloading Certificate PDFs.

Samples assume a `client` created as in
[Getting started](getting-started.md#3-make-your-first-call).

## Waiting for an Operation

Methods that start an Operation return an `Operation` with a `Name`,
`Done`, and, once done, a `Response` or an `Error`. `WaitAsync` polls until
it is done, backing off from `interval` (default 1 s) to 10 s, and returns
the finished Operation. It throws `OperationTimeoutException` after
`timeout` (default 5 minutes), or `OperationFailedException` if the
Operation finished with an error; both carry the `Operation`. An Operation
that is already done, as exports always are, returns without a request.

An export's `GetExportResult` hands back the CSV file:

<!-- sample: operations -->
```csharp
var export = await client.Learners.ExportAsync(new LearnerExportOptions
{
    Format = LearnersExportRequestFormat.Csv,
    CourseId = "hipaa-2026",
});
var done = await export.WaitAsync(timeout: TimeSpan.FromMinutes(1), interval: TimeSpan.FromSeconds(2));
ExportResult csv = done.GetExportResult();
Console.WriteLine($"{csv.FileName} {csv.RowCount} {csv.Csv.Length}");
```

To check on an Operation later, from another process, keep its `Name`:
`Operation.OperationIdFromName(operation.Name)` gives the id that
`Operations.GetAsync` takes.

## Exporting Certificates

`Certificates.ExportAsync`, `Enrollments.ExportAsync` and
`Learners.ExportAsync` each produce one CSV file (RFC 4180, CRLF line
endings) in `ExportResult.Csv`. Write the string as it is, so the line
endings stay as they are:

<!-- sample: certificates-export -->
```csharp
var operation = await client.Certificates.ExportAsync(new CertificateExportOptions
{
    Format = CertificatesExportRequestFormat.Csv,
});
ExportResult export = (await operation.WaitAsync()).GetExportResult();
await File.WriteAllTextAsync(export.FileName ?? "certificates.csv", export.Csv);
Console.WriteLine($"{export.RowCount} Certificates exported");
```

An export too large for one file fails with an `InvalidRequestException`
whose `Code` is `export_too_large`. `Learners.ExportAsync` covers one
Course, and can be narrowed further to one `Status`.

## Importing Enrollments from a CSV file

A CSV file with an `email` column, and optionally `name` and `courseId`,
can enroll up to 500 people at once. `Enrollments.PreviewImportAsync`
checks the file without inviting anyone; `Enrollments.ImportAsync` invites
everyone in it and returns an Operation. Both take the file in their
options' constructor as a `FileUpload`: a file path, a `byte[]` or a
`Stream`, each of which converts to one implicitly, or
`FileUpload.FromBytes(bytes, "roster.csv")` to name it. The file is read
once per call, so a retry resends the same bytes.

<!-- sample: csv-import -->
```csharp
var preview = await client.Enrollments.PreviewImportAsync(
    new EnrollmentPreviewImportOptions("roster.csv") { Options = new() { CourseId = "hipaa-2026" } });
Console.WriteLine($"{preview.ValidRows} of {preview.TotalRows} rows are valid");

var import = await client.Enrollments.ImportAsync(new EnrollmentImportOptions("roster.csv"));
await import.WaitAsync();
```

## Downloading Certificate PDFs

`Certificates.DownloadPdfAsync` returns the whole file as a
`BinaryResponse` (`Data`, `ContentType`, `FileName`).
`DownloadPdfStreamingAsync` returns once the headers arrive, so you read
the file from the connection as it comes; dispose it to release the
connection.

<!-- sample: pdf-download -->
```csharp
var pdf = await client.Certificates.DownloadPdfAsync("cert_01JD8F");
await File.WriteAllBytesAsync(pdf.FileName ?? "certificate.pdf", pdf.Data);

await using var stream = await client.Certificates.DownloadPdfStreamingAsync("cert_01JD8F");
await using var copy = File.Create("certificate-copy.pdf");
await stream.Content.CopyToAsync(copy);
```

Revoked and expired Certificates download watermarked.
