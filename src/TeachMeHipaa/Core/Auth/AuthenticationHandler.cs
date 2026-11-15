using System.Net;
using System.Net.Http.Headers;

namespace TeachMeHipaa;

/// <summary>
/// Puts the current Access token on every API request. When the API answers
/// 401 <c>unauthorized</c> (a token revoked mid-flight), it re-mints once and
/// replays the request, unless the provider hands back the same token. It
/// sits above retries, so the replay never uses one up, but the request and
/// its replay share one <see cref="RetryBudget"/>, so the call as a whole
/// retries no more than its limit. A 401 comes before the API acts, so
/// replaying even a PATCH is safe.
/// Public operations go out without a token, so none is minted for them.
/// </summary>
internal sealed class AuthenticationHandler(ITokenProvider tokenProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (RequestProperties.Authenticated.Get(request) == false)
        {
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        RequestProperties.RetryBudget.Set(request, new RetryBudget());
        var replay = await ReplayableRequest.CaptureAsync(request, cancellationToken).ConfigureAwait(false);
        var accessToken = await tokenProvider.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        var response = await SendWithAsync(replay, accessToken, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode != HttpStatusCode.Unauthorized
            || await ResponseReader.ReadProblemCodeAsync(response, cancellationToken).ConfigureAwait(false) != "unauthorized")
        {
            return response;
        }

        tokenProvider.Invalidate(accessToken);
        var renewed = await tokenProvider.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        // A provider that can't renew, such as a static token, hands back the
        // same token, which the API would only refuse again.
        if (renewed == accessToken)
        {
            return response;
        }

        response.Dispose();
        return await SendWithAsync(replay, renewed, cancellationToken).ConfigureAwait(false);
    }

    private Task<HttpResponseMessage> SendWithAsync(ReplayableRequest replay, string accessToken, CancellationToken cancellationToken) =>
        base.SendAsync(replay.Create(new AuthenticationHeaderValue("Bearer", accessToken)), cancellationToken);
}
