using FFImageLoading.Maui;
using Microsoft.Extensions.Logging;
using TMDB.MobileClient.Services;
using TMDB.MobileClient.ViewModels;

namespace TMDB.MobileClient;

public static class MauiProgram
{
    public static IServiceProvider? ServiceProvider;

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseFFImageLoading()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif
        builder.Services.AddSingleton<ApiClientService>();
        builder.Services.AddSingleton<NavigationService>();

        builder.Services.AddTransient<MainPageViewModel>();
        builder.Services.AddTransient<HomePageViewModel>();

        var app = builder.Build();
        ServiceProvider = app.Services;
        return app;
    }

}