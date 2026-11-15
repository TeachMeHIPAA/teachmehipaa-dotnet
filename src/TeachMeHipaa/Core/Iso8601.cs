using System.Globalization;

namespace TeachMeHipaa;

/// <summary>The platform's timestamp format: JavaScript's <c>Date.toISOString()</c>, always UTC with milliseconds.</summary>
internal static class Iso8601
{
    public const string UtcMillisecondsFormat = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";

    public static string Format(DateTimeOffset value) =>
        value.UtcDateTime.ToString(UtcMillisecondsFormat, CultureInfo.InvariantCulture);
}
