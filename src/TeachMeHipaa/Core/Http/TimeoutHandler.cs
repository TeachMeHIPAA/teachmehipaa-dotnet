using System.Globalization;

namespace TeachMeHipaa;

/// <summary>
/// Bounds each HTTP attempt, token mints included, reading the body inside
/// the same time limit. A failure to get a whole response becomes an
/// <see cref="ApiConnectionException"/> (or <see cref="ApiTimeoutException"/>)
/// here, so everything above it, retries included, sees one kind of error.
/// </summary>
internal sealed class TimeoutHandler(TimeSpan defaultTimeout, TimeProvider timeProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var timeout = RequestProperties.Timeout.Get(request) ?? defaultTimeout;
#if NET8_0_OR_GREATER
        using var timeoutSource = new CancellationTokenSource(timeout, timeProvider);
#else
        using var timeoutSource = timeProvider.CreateCancellationTokenSource(timeout);
#endif
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token);
        HttpResponseMessage? response = null;
        try
        {
            response = await base.SendAsync(request, linked.Token).ConfigureAwait(false);
            await BufferAsync(response, linked.Token).ConfigureAwait(false);
            return response;
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            // Also a caller's HttpClient giving up after its own Timeout.
            response?.Dispose();
            var seconds = timeout.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture);
            throw new ApiTimeoutException(
                $"No response from TeachMeHIPAA within {seconds} s ({request.Method} {request.RequestUri?.AbsolutePath}).",
                exception);
        }
        catch (Exception exception) when (exception is HttpRequestException or IOException)
        {
            response?.Dispose();
            throw new ApiConnectionException(
                $"Could not reach TeachMeHIPAA ({request.Method} {request.RequestUri?.AbsolutePath}): {exception.Message}",
                exception);
        }
    }

    private static async Task BufferAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content is null)
        {
            return;
        }
#if NET5_0_OR_GREATER
        // Reading the bytes buffers the content, so later reads replay them.
        await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
#else
        var load = response.Content.LoadIntoBufferAsync();
        var cancelled = Task.Delay(Timeout.Infinite, cancellationToken);
        if (await Task.WhenAny(load, cancelled).ConfigureAwait(false) == cancelled)
        {
            throw new OperationCanceledException(cancellationToken);
        }

        await load.ConfigureAwait(false);
#endif
    }
}
