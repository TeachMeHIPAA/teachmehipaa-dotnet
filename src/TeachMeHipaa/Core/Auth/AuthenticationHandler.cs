using System.Net;
using System.Net.Http.Headers;

namespace TeachMeHipaa;

/// <summary>
/// Puts the current Access token on every API request. When the API answers
/// 401 <c>unauthorized</c> (a token revoked mid-flight), it re-mints once and
/// replays the request. It sits above retries, so the replay never uses one
/// up; a 401 comes before the API acts, so replaying even a PATCH is safe.
/// </summary>
internal sealed class AuthenticationHandler(ITokenProvider tokenProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var replay = await ReplayableRequest.CaptureAsync(request, cancellationToken).ConfigureAwait(false);
        var accessToken = await tokenProvider.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        var response = await SendWithAsync(replay, accessToken, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode != HttpStatusCode.Unauthorized
            || await ResponseReader.ReadProblemCodeAsync(response, cancellationToken).ConfigureAwait(false) != "unauthorized")
        {
            return response;
        }

        response.Dispose();
        tokenProvider.Invalidate(accessToken);
        accessToken = await tokenProvider.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        return await SendWithAsync(replay, accessToken, cancellationToken).ConfigureAwait(false);
    }

    private Task<HttpResponseMessage> SendWithAsync(ReplayableRequest replay, string accessToken, CancellationToken cancellationToken) =>
        base.SendAsync(replay.Create(new AuthenticationHeaderValue("Bearer", accessToken)), cancellationToken);
}
