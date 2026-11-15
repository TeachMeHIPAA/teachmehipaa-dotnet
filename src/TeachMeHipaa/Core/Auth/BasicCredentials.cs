using System.Net;
using System.Text;

namespace TeachMeHipaa;

internal static class BasicCredentials
{
    // RFC 6749 §2.3.1: Basic credentials are form-encoded before base64.
    public static string Encode(string clientId, string clientSecret) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes($"{WebUtility.UrlEncode(clientId)}:{WebUtility.UrlEncode(clientSecret)}"));
}
