namespace TeachMeHipaa;

/// <summary>
/// A file the API is sending, such as a Certificate PDF, read from the
/// connection as it arrives rather than held in memory. Dispose it to
/// release the connection.
/// </summary>
/// <remarks>
/// The client's timeout covers the wait for the response headers; reads from
/// <see cref="Content"/> are bounded by the cancellation token you pass them.
/// </remarks>
public sealed class BinaryStreamResponse : IDisposable, IAsyncDisposable
{
    private readonly HttpResponseMessage _response;

    internal BinaryStreamResponse(HttpResponseMessage response, Stream content)
    {
        _response = response;
        Content = content;
        ContentType = response.Content?.Headers.ContentType?.MediaType;
        FileName = ResponseReader.FileName(response);
        LastResponse = TeachMeHipaaResponse.From(response);
    }

    /// <summary>The file's bytes, unchanged, as they arrive. Read it once.</summary>
    public Stream Content { get; }

    /// <summary>The file's media type, such as <c>application/pdf</c>.</summary>
    public string? ContentType { get; }

    /// <summary>The file name the API suggested in <c>Content-Disposition</c>, if any.</summary>
    public string? FileName { get; }

    /// <summary>What the API said about the response: status, headers and request id.</summary>
    public TeachMeHipaaResponse LastResponse { get; }

    /// <summary>Closes the stream and releases the connection.</summary>
    public void Dispose()
    {
        Content.Dispose();
        _response.Dispose();
    }

    /// <summary>Closes the stream and releases the connection.</summary>
    /// <returns>A completed task.</returns>
    public ValueTask DisposeAsync()
    {
        Dispose();
        return default;
    }
}
