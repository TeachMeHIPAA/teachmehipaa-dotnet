namespace TeachMeHipaa;

/// <summary>Ends the handler pipeline in a caller's <see cref="HttpClient"/>, such as one from <c>IHttpClientFactory</c>.</summary>
internal sealed class HttpClientSendingHandler(HttpClient httpClient) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        httpClient.SendAsync(request, cancellationToken);
}
