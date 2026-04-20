using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;

namespace MauiApp1.Services;

public sealed class ClientDeviceIdentityService
{
    private const string ClientDeviceIdKey = "client_device_id";

    public string GetOrCreateClientDeviceId()
    {
        var existing = Preferences.Get(ClientDeviceIdKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(existing))
            return existing;

        var generated = $"APP-CLIENT-{Guid.NewGuid():N}".ToUpperInvariant();
        Preferences.Set(ClientDeviceIdKey, generated);
        return generated;
    }

    public DeviceMetadata GetDeviceMetadata()
    {
        try
        {
            return new DeviceMetadata
            {
                Platform = DeviceInfo.Current.Platform.ToString(),
                Model = Truncate(DeviceInfo.Current.Model, 128),
                Manufacturer = Truncate(DeviceInfo.Current.Manufacturer, 128),
                AppVersion = Truncate(AppInfo.Current.VersionString, 32)
            };
        }
        catch
        {
            return new DeviceMetadata();
        }
    }

    private static string? Truncate(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        return value.Length <= max ? value : value[..max];
    }
}

public sealed class DeviceMetadata
{
    public string? Platform { get; set; }
    public string? Model { get; set; }
    public string? Manufacturer { get; set; }
    public string? AppVersion { get; set; }
}
