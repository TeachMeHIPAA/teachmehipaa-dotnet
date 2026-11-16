namespace TeachMeHipaa;

/// <summary>
/// The file an export Operation produced, from <see cref="Operation.GetExportResult"/>:
/// <c>Enrollments.ExportAsync</c>, <c>Certificates.ExportAsync</c> or <c>Learners.ExportAsync</c>.
/// </summary>
/// <param name="Csv">The whole CSV file as text: RFC 4180, CRLF line endings.</param>
/// <param name="FileName">A suggested file name, such as <c>enrollments-org_acme.csv</c>; null when the API leaves it out.</param>
/// <param name="RowCount">The number of data rows, not counting the header; null when the API leaves it out.</param>
public sealed record ExportResult(string Csv, string? FileName, long? RowCount)
{
    /// <summary>The file name, row count and CSV length; never the CSV itself.</summary>
    /// <returns>A description safe to log.</returns>
    // The CSV holds Learners' names and emails, and nothing the library hands
    // out should put them in a log (ADR-0007).
    public override string ToString() =>
        $"ExportResult {{ FileName = {FileName}, RowCount = {RowCount}, Csv = ({Csv.Length} characters) }}";
}
