using System.Net.Http.Headers;

namespace TeachMeHipaa;

/// <summary>Puts the current Access token on every API request.</summary>
internal sealed class AuthenticationHandler(ITokenProvider tokenProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var accessToken = await tokenProvider.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
