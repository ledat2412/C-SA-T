using Microsoft.Maui.Devices;

namespace MauiApp1.Utils;

public static class BackendUrlResolver
{
    public static string GetBaseUrl()
    {
#if ANDROID
        return DeviceInfo.DeviceType == DeviceType.Virtual
            ? "https://10.0.2.2:7123/"
            : "https://127.0.0.1:7123/";
#else
        return "https://localhost:7123/";
#endif
    }

    public static string BuildUrl(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return GetBaseUrl();

        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return path;
        }

        return GetBaseUrl() + path.TrimStart('/');
    }
}
