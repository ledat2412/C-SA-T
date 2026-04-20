using MauiApp1.Services;
using MauiApp1.Utils;
using MauiApp1.Views;

namespace MauiApp1;

public partial class App : Application
{
    private readonly IServiceProvider _services;
    private readonly DeviceHeartbeatService _heartbeatService;

    public App(
        IServiceProvider services,
        SQLiteService sqliteService,
        LocalizationService localizationService,
        DeviceHeartbeatService heartbeatService)
    {
        InitializeComponent();

        _services = services;
        _heartbeatService = heartbeatService;

        var savedBackendUrl = Preferences.Get(BackendUrlResolver.PreferenceKey, string.Empty);
        BackendUrlResolver.Configure(savedBackendUrl);

        var savedLanguage = Preferences.Get("ui_language", "vi");
        localizationService.SetLanguage(savedLanguage);

        _ = InitializeDatabaseAsync(sqliteService);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new NavigationPage(_services.GetRequiredService<AccessEntryPage>()));
        window.Activated += (_, _) => _heartbeatService.Start();
        window.Deactivated += (_, _) => _heartbeatService.Stop();
        window.Resumed += (_, _) => _heartbeatService.Start();
        window.Stopped += (_, _) => _heartbeatService.Stop();
        return window;
    }

    public Task ShowAccessEntryAsync()
    {
        return SetRootAsync(_services.GetRequiredService<AccessEntryPage>());
    }

    public Task ShowMainPageAsync()
    {
        return SetRootAsync(_services.GetRequiredService<HomePage>());
    }

    private Task SetRootAsync(Page page)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                var window = Windows.FirstOrDefault();
                if (window is not null)
                    window.Page = new NavigationPage(page);

                tcs.TrySetResult(true);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        });

        return tcs.Task;
    }

    private static async Task InitializeDatabaseAsync(SQLiteService sqliteService)
    {
        try
        {
            await sqliteService.InitAsync();
            await sqliteService.CleanupOldCacheAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[App] SQLite init error: {ex.Message}");
        }
    }
}
