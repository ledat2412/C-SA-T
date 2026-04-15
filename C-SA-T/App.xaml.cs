using MauiApp1.Services;
using MauiApp1.Views.Maps;

namespace MauiApp1;

public partial class App : Application
{
    private readonly Page _rootPage;

    public App(PoiMapPage poiMapPage, SQLiteService sqliteService, LocalizationService localizationService)
    {
        InitializeComponent();

        var savedLanguage = Preferences.Get("ui_language", "vi");
        localizationService.SetLanguage(savedLanguage);

        _ = InitializeDatabaseAsync(sqliteService);
        _rootPage = new NavigationPage(poiMapPage);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(_rootPage);
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
