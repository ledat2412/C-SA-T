using Microsoft.Maui.Storage;

namespace MauiApp1.Services;

public sealed class AccessFlowService
{
    private const string AccessTokenKey = "access_token";
    private const string AccessSourceKey = "access_source";
    private const string AccessExpiryKey = "access_expires_at";
    private const string AccessEmailKey = "access_email";
    private const string AccessPackageIdKey = "access_package_id";
    private const string AccessPackageNameKey = "access_package_name";
    private const string AccessDeviceCodeKey = "access_device_code";

    private readonly ApiService _apiService;

    public AccessFlowService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        try
        {
            var secure = await SecureStorage.Default.GetAsync(AccessTokenKey);
            if (!string.IsNullOrWhiteSpace(secure))
                return secure;
        }
        catch
        {
        }

        var legacy = Preferences.Get(AccessTokenKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(legacy))
        {
            try
            {
                await SecureStorage.Default.SetAsync(AccessTokenKey, legacy);
                Preferences.Remove(AccessTokenKey);
            }
            catch
            {
            }
            return legacy;
        }

        return string.Empty;
    }

    private static async Task SetAccessTokenAsync(string token)
    {
        try
        {
            await SecureStorage.Default.SetAsync(AccessTokenKey, token);
            Preferences.Remove(AccessTokenKey);
        }
        catch
        {
            Preferences.Set(AccessTokenKey, token);
        }
    }

    private static void RemoveAccessToken()
    {
        try
        {
            SecureStorage.Default.Remove(AccessTokenKey);
        }
        catch
        {
        }
        Preferences.Remove(AccessTokenKey);
    }

    public async Task<AccessValidationState> ValidateCurrentAccessAsync()
    {
        var token = await GetAccessTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            return new AccessValidationState
            {
                IsValid = false,
                Message = "Chua co access token trong local."
            };
        }

        var source = Preferences.Get(AccessSourceKey, "qr");
        if (string.Equals(source, "test", StringComparison.OrdinalIgnoreCase))
        {
            var expiresAt = ReadExpiryFromPreferences();
            if (expiresAt.HasValue && expiresAt.Value > DateTime.UtcNow)
            {
                return new AccessValidationState
                {
                    IsValid = true,
                    Message = "Goi demo con hieu luc.",
                    Source = source,
                    ExpiresAtUtc = expiresAt
                };
            }

            await ClearAccessAsync();
            return new AccessValidationState
            {
                IsValid = false,
                Message = "Goi demo da het han."
            };
        }

        var apiResult = await _apiService.ValidateAccessAsync(token);
        if (apiResult.IsValid)
        {
            if (apiResult.HetHanLuc.HasValue)
                Preferences.Set(AccessExpiryKey, apiResult.HetHanLuc.Value.ToUniversalTime().ToString("O"));

            var resolvedSource = string.Equals(source, "package", StringComparison.OrdinalIgnoreCase) ? "package" : "qr";
            Preferences.Set(AccessSourceKey, resolvedSource);
            if (!string.IsNullOrWhiteSpace(apiResult.MaThietBi))
                Preferences.Set(AccessDeviceCodeKey, apiResult.MaThietBi);

            return new AccessValidationState
            {
                IsValid = true,
                Message = apiResult.Message,
                Source = resolvedSource,
                DeviceCode = apiResult.MaThietBi,
                ExpiresAtUtc = apiResult.HetHanLuc?.ToUniversalTime()
            };
        }

        await ClearAccessAsync();
        return new AccessValidationState
        {
            IsValid = false,
            Message = apiResult.Message
        };
    }

    public async Task<PackageAccessActivationState> RegisterPackageAccessBypassAsync(string email, int packageId)
    {
        var result = await _apiService.RegisterPackageAccessAsync(email, packageId, bypassPayment: true);
        if (!result.Success || string.IsNullOrWhiteSpace(result.AccessToken))
        {
            return new PackageAccessActivationState
            {
                Success = false,
                Message = result.Message,
                EmailSent = result.EmailSent,
                EmailStatusMessage = result.EmailStatusMessage
            };
        }

        await SetAccessTokenAsync(result.AccessToken);
        Preferences.Set(AccessSourceKey, "package");
        if (result.HetHanLuc.HasValue)
            Preferences.Set(AccessExpiryKey, result.HetHanLuc.Value.ToUniversalTime().ToString("O"));
        if (!string.IsNullOrWhiteSpace(result.Email))
            Preferences.Set(AccessEmailKey, result.Email);
        if (result.IdGoi.HasValue)
            Preferences.Set(AccessPackageIdKey, result.IdGoi.Value.ToString());
        if (!string.IsNullOrWhiteSpace(result.TenGoi))
            Preferences.Set(AccessPackageNameKey, result.TenGoi);
        if (!string.IsNullOrWhiteSpace(result.MaThietBi))
            Preferences.Set(AccessDeviceCodeKey, result.MaThietBi);

        var validation = await _apiService.ValidateAccessAsync(result.AccessToken);
        if (!validation.IsValid)
        {
            await ClearAccessAsync();
            return new PackageAccessActivationState
            {
                Success = false,
                Message = validation.Message,
                EmailSent = result.EmailSent,
                EmailStatusMessage = result.EmailStatusMessage
            };
        }

        return new PackageAccessActivationState
        {
            Success = true,
            Message = result.Message,
            AccessToken = result.AccessToken,
            QrTokenPayload = result.QrTokenPayload,
            Email = result.Email ?? email.Trim(),
            PackageId = result.IdGoi?.ToString() ?? packageId.ToString(),
            PackageName = result.TenGoi ?? string.Empty,
            ExpiresAtUtc = validation.HetHanLuc?.ToUniversalTime() ?? result.HetHanLuc?.ToUniversalTime(),
            EmailSent = result.EmailSent,
            EmailStatusMessage = result.EmailStatusMessage
        };
    }

    public async Task SaveQrAccessAsync(QrScanResult result)
    {
        if (!result.Success || string.IsNullOrWhiteSpace(result.AccessToken))
            return;

        await SetAccessTokenAsync(result.AccessToken);
        Preferences.Set(AccessSourceKey, result.IdGoi.HasValue ? "package" : "qr");

        if (result.HetHanLuc.HasValue)
            Preferences.Set(AccessExpiryKey, result.HetHanLuc.Value.ToUniversalTime().ToString("O"));

        if (!string.IsNullOrWhiteSpace(result.MaThietBi))
            Preferences.Set(AccessDeviceCodeKey, result.MaThietBi);
        if (result.IdGoi.HasValue)
            Preferences.Set(AccessPackageIdKey, result.IdGoi.Value.ToString());
        if (!string.IsNullOrWhiteSpace(result.TenGoi))
            Preferences.Set(AccessPackageNameKey, result.TenGoi);
    }

    public async Task<TestPackageActivationResult> ActivateTestPackageAsync(string email, string packageId, string packageName, int durationDays)
    {
        var accessToken = $"TEST-{Guid.NewGuid():N}".ToUpperInvariant();
        var expiresAtUtc = DateTime.UtcNow.AddDays(durationDays);

        await SetAccessTokenAsync(accessToken);
        Preferences.Set(AccessSourceKey, "test");
        Preferences.Set(AccessExpiryKey, expiresAtUtc.ToString("O"));
        Preferences.Set(AccessEmailKey, email.Trim());
        Preferences.Set(AccessPackageIdKey, packageId);
        Preferences.Set(AccessPackageNameKey, packageName);

        return new TestPackageActivationResult
        {
            AccessToken = accessToken,
            Email = email.Trim(),
            PackageId = packageId,
            PackageName = packageName,
            ExpiresAtUtc = expiresAtUtc
        };
    }

    public Task ClearAccessAsync()
    {
        RemoveAccessToken();
        Preferences.Remove(AccessSourceKey);
        Preferences.Remove(AccessExpiryKey);
        Preferences.Remove(AccessEmailKey);
        Preferences.Remove(AccessPackageIdKey);
        Preferences.Remove(AccessPackageNameKey);
        Preferences.Remove(AccessDeviceCodeKey);
        return Task.CompletedTask;
    }

    public async Task<AccessSummary> GetCurrentSummaryAsync()
    {
        return new AccessSummary
        {
            AccessToken = await GetAccessTokenAsync(),
            Source = Preferences.Get(AccessSourceKey, string.Empty),
            Email = Preferences.Get(AccessEmailKey, string.Empty),
            PackageId = Preferences.Get(AccessPackageIdKey, string.Empty),
            PackageName = Preferences.Get(AccessPackageNameKey, string.Empty),
            DeviceCode = Preferences.Get(AccessDeviceCodeKey, string.Empty),
            ExpiresAtUtc = ReadExpiryFromPreferences()
        };
    }

    private static DateTime? ReadExpiryFromPreferences()
    {
        var raw = Preferences.Get(AccessExpiryKey, string.Empty);
        if (DateTime.TryParse(raw, null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsed))
            return parsed.ToUniversalTime();

        return null;
    }
}

public sealed class AccessValidationState
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Source { get; set; }
    public string? DeviceCode { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
}

public sealed class TestPackageActivationResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PackageId { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}

public sealed class PackageAccessActivationState
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PackageId { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public DateTime? ExpiresAtUtc { get; set; }
    public string? QrTokenPayload { get; set; }
    public bool EmailSent { get; set; }
    public string? EmailStatusMessage { get; set; }
}

public sealed class AccessSummary
{
    public string AccessToken { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PackageId { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public string DeviceCode { get; set; } = string.Empty;
    public DateTime? ExpiresAtUtc { get; set; }
}
