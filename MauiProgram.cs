using MauiApp1.Services;
using MauiApp1.Views;
using MauiApp1.Views.Auth;
using MauiApp1.Views.Maps;
using Microsoft.Extensions.Logging;

namespace MauiApp1;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiMaps()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddSingleton<MySqlService>();

        builder.Services.AddSingleton<TaiKhoanService>();
        builder.Services.AddSingleton<GianHangService>();
        builder.Services.AddSingleton<MonAnService>();
        builder.Services.AddSingleton<HinhAnhChiNhanhService>();
        builder.Services.AddSingleton<PoiService>();

        builder.Services.AddSingleton<LoginPage>();
        builder.Services.AddTransient<GianHangPage>();
        builder.Services.AddTransient<MonAnPage>();
        builder.Services.AddTransient<PoiMapPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}