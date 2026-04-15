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
}
