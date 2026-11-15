namespace TeachMeHipaa;

/// <summary>
/// A token provider that gets its tokens from one API base URL, such as a
/// <see cref="PartnerGrantTokenProvider"/>. A client refuses such a provider
/// unless the client calls the same base URL, so a Sandbox client never
/// hands out tokens refreshed against Live. Implement it on your own
/// <see cref="ITokenProvider"/> to opt into the same check.
/// </summary>
public interface ITokenEndpointBound
{
    /// <summary>The API base URL the provider gets its tokens from, such as <c>https://api.tme.so/v1</c>.</summary>
    Uri BaseUrl { get; }
}
