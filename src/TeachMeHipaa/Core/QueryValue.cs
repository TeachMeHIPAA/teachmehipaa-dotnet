using System.Globalization;

namespace TeachMeHipaa;

/// <summary>Writes typed query parameters the way the API parses them.</summary>
internal static class QueryValue
{
    public static string? From(long? value) => value?.ToString(CultureInfo.InvariantCulture);

    public static string? From(DateTimeOffset? value) =>
        value is { } timestamp ? Iso8601.Format(timestamp) : null;
}
