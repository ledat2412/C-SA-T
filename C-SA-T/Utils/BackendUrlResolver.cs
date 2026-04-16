using Microsoft.Maui.Devices;

namespace MauiApp1.Utils;

public static class BackendUrlResolver
{
    public const string PreferenceKey = "backend_base_url";

    private const string EmulatorBaseUrl = "https://10.0.2.2:7123/";
    private const string AndroidDeviceFallbackBaseUrl = "https://127.0.0.1:7123/";
    private const string DesktopBaseUrl = "https://localhost:7123/";

    private static string? _configuredBaseUrl;

    public static void Configure(string? configuredBaseUrl)
    {
        _configuredBaseUrl = NormalizeOverrideBaseUrl(configuredBaseUrl);
    }

    public static string GetBaseUrl()
    {
        var configured = _configuredBaseUrl ?? NormalizeOverrideBaseUrl(Environment.GetEnvironmentVariable("MAUI_BACKEND_URL"));
        if (!string.IsNullOrWhiteSpace(configured))
            return configured;

#if ANDROID
        return DeviceInfo.DeviceType == DeviceType.Virtual
            ? EmulatorBaseUrl
            : AndroidDeviceFallbackBaseUrl;
#else
        return DesktopBaseUrl;
#endif
    }

    public static string BuildUrl(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return GetBaseUrl();

        var trimmedPath = path.Trim();
        if (Uri.TryCreate(trimmedPath, UriKind.Absolute, out var absoluteUri) &&
            (absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps))
        {
            return RewriteLoopbackUrl(absoluteUri);
        }

        return new Uri(new Uri(GetBaseUrl()), trimmedPath.TrimStart('/')).ToString();
    }

    public static string? NormalizeOverrideBaseUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
            return null;

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return null;

        return trimmed.EndsWith("/", StringComparison.Ordinal) ? trimmed : trimmed + "/";
    }

    private static string RewriteLoopbackUrl(Uri absoluteUri)
    {
        if (!IsLoopbackHost(absoluteUri.Host))
            return absoluteUri.ToString();

        var baseUri = new Uri(GetBaseUrl());
        var builder = new UriBuilder(absoluteUri)
        {
            Scheme = baseUri.Scheme,
            Host = baseUri.Host,
            Port = baseUri.IsDefaultPort ? -1 : baseUri.Port
        };

        return builder.Uri.ToString();
    }

    private static bool IsLoopbackHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
            return false;

        return host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
               host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
               host.Equals("0.0.0.0", StringComparison.OrdinalIgnoreCase) ||
               host.Equals("::1", StringComparison.OrdinalIgnoreCase);
    }
}
