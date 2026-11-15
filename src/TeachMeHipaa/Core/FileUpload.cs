namespace TeachMeHipaa;

/// <summary>
/// A file to upload, such as an Enrollment import's CSV: a path, a stream or
/// bytes. Each call reads it once, so a retry resends the same bytes under
/// the same Idempotency-Key.
/// </summary>
/// <remarks>
/// Convert from any of the three implicitly:
/// <c>new EnrollmentImportOptions("roster.csv")</c>,
/// <c>new EnrollmentImportOptions(stream)</c> or
/// <c>new EnrollmentImportOptions(bytes)</c>.
/// </remarks>
public sealed class FileUpload
{
    private readonly string? _path;
    private readonly Stream? _stream;
    private readonly ReadOnlyMemory<byte> _bytes;
    private readonly string? _fileName;

    private FileUpload(string? path, Stream? stream, ReadOnlyMemory<byte> bytes, string? fileName)
    {
        _path = path;
        _stream = stream;
        _bytes = bytes;
        _fileName = fileName;
    }

    /// <summary>The file at <paramref name="path"/>, sent under its own name.</summary>
    /// <param name="path">The file's path. One containing a line break is refused as file contents passed by mistake.</param>
    /// <returns>The upload, read when the call is made.</returns>
    public static FileUpload FromPath(string path) => new(path ?? string.Empty, null, default, null);

    /// <summary>The rest of <paramref name="stream"/>, from its current position. The stream is left open.</summary>
    /// <param name="stream">The file's contents.</param>
    /// <param name="fileName">The name to send; a <see cref="FileStream"/>'s own name, else a default such as <c>file.csv</c>.</param>
    /// <returns>The upload, read when the call is made.</returns>
    public static FileUpload FromStream(Stream stream, string? fileName = null) =>
        new(null, stream ?? throw new ArgumentNullException(nameof(stream)), default, fileName ?? (stream as FileStream)?.Name);

    /// <summary>A file's bytes.</summary>
    /// <param name="bytes">The file's contents.</param>
    /// <param name="fileName">The name to send; a default such as <c>file.csv</c> when null.</param>
    /// <returns>The upload.</returns>
    public static FileUpload FromBytes(ReadOnlyMemory<byte> bytes, string? fileName = null) => new(null, null, bytes, fileName);

    /// <summary>The file at <paramref name="path"/>.</summary>
    /// <param name="path">The file's path.</param>
    public static implicit operator FileUpload(string path) => FromPath(path);

    // A null converts to null, which the method then refuses, rather than to an empty file.

    /// <summary>The rest of <paramref name="stream"/>.</summary>
    /// <param name="stream">The file's contents.</param>
    public static implicit operator FileUpload(Stream stream) => stream is null ? null! : FromStream(stream);

    /// <summary>A file's bytes.</summary>
    /// <param name="bytes">The file's contents.</param>
    public static implicit operator FileUpload(byte[] bytes) => bytes is null ? null! : FromBytes(bytes);

    /// <summary>
    /// Reads the file for one call. Errors surface here, before anything is
    /// sent, rather than as a connection failure from inside the pipeline.
    /// </summary>
    internal async Task<FileContents> ReadAsync(string defaultFileName, string parameterName, CancellationToken cancellationToken)
    {
        if (_path is not null)
        {
            if (_path.Length == 0 || _path.IndexOfAny(['\r', '\n']) >= 0)
            {
                throw new ArgumentException(
                    "A FileUpload string is a file path, and this one is empty or has a line break. Pass the contents with FileUpload.FromBytes instead.",
                    parameterName);
            }

            using var file = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
            return new FileContents(await CopyAsync(file, cancellationToken).ConfigureAwait(false), Path.GetFileName(_path));
        }

        var name = _fileName is null ? defaultFileName : Path.GetFileName(_fileName);
        if (_stream is null)
        {
            return new FileContents(_bytes.ToArray(), name);
        }

        return new FileContents(await CopyAsync(_stream, cancellationToken).ConfigureAwait(false), name);
    }

    private static async Task<byte[]> CopyAsync(Stream source, CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        await source.CopyToAsync(buffer, 81920, cancellationToken).ConfigureAwait(false);
        return buffer.ToArray();
    }
}

/// <summary>A <see cref="FileUpload"/> as one call read it.</summary>
internal sealed record FileContents(byte[] Data, string FileName);
