using System.Reflection;
using System.Runtime.InteropServices;

namespace TeachMeHipaa;

internal static class UserAgentHeader
{
    internal static readonly string SdkVersion = ReadSdkVersion();

    // ADR-0007: this header is the only thing the library says about its caller.
    public static string Build(AppInfo? app)
    {
        var agent = $"teachmehipaa-dotnet/{SdkVersion} ({RuntimeInformation.FrameworkDescription.Trim()})";
        return app is null ? agent : $"{agent} {Describe(app)}";
    }

    private static string Describe(AppInfo app)
    {
        var product = app.Version is null ? app.Name : $"{app.Name}/{app.Version}";
        return app.Url is null ? product : $"{product} (+{app.Url})";
    }

    private static string ReadSdkVersion()
    {
        var informational = typeof(UserAgentHeader).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";
        var buildMetadata = informational.IndexOf('+');
        return buildMetadata < 0 ? informational : informational.Substring(0, buildMetadata);
    }
}
