namespace TeachMeHipaa;

/// <summary>
/// Ends the handler pipeline in a caller's <see cref="HttpClient"/>. It asks
/// for the client on every request, so one from <c>IHttpClientFactory</c> can
/// be a fresh one each time and pick up the factory's rotated handlers.
/// </summary>
internal sealed class HttpClientSendingHandler(Func<HttpClient> httpClient) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        httpClient().SendAsync(
            request,
            RequestProperties.StreamResponse.Get(request) == true ? HttpCompletionOption.ResponseHeadersRead : HttpCompletionOption.ResponseContentRead,
            cancellationToken);
}
