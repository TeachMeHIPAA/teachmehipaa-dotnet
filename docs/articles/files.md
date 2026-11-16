---
title: Import and export files
description: Export Certificates, Enrollments and Learners to CSV, import a roster CSV with FileUpload, and download Certificate PDFs whole or streamed.
ms.topic: how-to
ms.date: 11/16/2026
---

# Import and export files

The API moves files in three shapes: CSV exports that arrive inside an
Operation, CSV imports sent as multipart uploads, and Certificate PDFs.
The examples assume a `client` built as in the
[quickstart](quickstart.md#read-compliance-and-list-learners).

## Export to a CSV file

`Certificates.ExportAsync`, `Enrollments.ExportAsync` and
`Learners.ExportAsync` each produce one RFC 4180 CSV file with CRLF line
endings. `GetExportResult` on the finished Operation returns it as an
`ExportResult` with `Csv`, `FileName` and `RowCount`. Write `Csv`
unchanged so the line endings survive:

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

`Learners.ExportAsync` covers one Course, and its `Status` option narrows
the file to one Learner status. An export too large for one file throws
`InvalidRequestException` with `Code` `export_too_large`.

## Import Enrollments from a CSV file

A CSV file with an `email` column, plus optional `name` and `courseId`
columns, enrolls up to 500 people in one call.
`Enrollments.PreviewImportAsync` validates the file and invites no one;
`Enrollments.ImportAsync` invites everyone in it and returns an
Operation.

<!-- sample: csv-import -->
```csharp
var preview = await client.Enrollments.PreviewImportAsync(
    new EnrollmentPreviewImportOptions("roster.csv") { Options = new() { CourseId = "hipaa-2026" } });
Console.WriteLine($"{preview.ValidRows} of {preview.TotalRows} rows are valid");

var import = await client.Enrollments.ImportAsync(new EnrollmentImportOptions("roster.csv"));
await import.WaitAsync();
```

Both options classes take the file in their constructor as a `FileUpload`.
A file path, a `byte[]` and a `Stream` each convert to one implicitly.
`FileUpload.FromBytes(bytes, "roster.csv")` and
`FileUpload.FromStream(stream, "roster.csv")` also set the file name sent
with the upload. The client reads the file once per call, so a retry
resends the same bytes.

## Download a Certificate PDF

`Certificates.DownloadPdfAsync` buffers the whole file into a
`BinaryResponse` with `Data`, `ContentType` and `FileName`.
`DownloadPdfStreamingAsync` returns once the headers arrive, and you read
the body from the connection as it comes. Dispose the stream response to
release the connection.

<!-- sample: pdf-download -->
```csharp
var pdf = await client.Certificates.DownloadPdfAsync("cert_01JD8F");
await File.WriteAllBytesAsync(pdf.FileName ?? "certificate.pdf", pdf.Data);

await using var stream = await client.Certificates.DownloadPdfStreamingAsync("cert_01JD8F");
await using var copy = File.Create("certificate-copy.pdf");
await stream.Content.CopyToAsync(copy);
```

Revoked and expired Certificates download with a watermark.

## Next steps

- [Wait for long-running operations](long-running-operations.md)
- [Handle errors and retries](handle-errors.md)
