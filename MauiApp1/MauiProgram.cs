using MauiApp1.Helpers;
using MauiApp1.Services;
using MauiApp1.ViewModels;
using MauiApp1.Views;
using Microsoft.Extensions.Logging;

namespace MauiApp1;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // 1. Глобальні перехоплювачі непередбачених збоїв
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            var path = Path.Combine(FileSystem.AppDataDirectory, "app_logs.txt");
            var crashLog = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [CRITICAL] [UnhandledException] {ex?.Message}{Environment.NewLine}{ex?.StackTrace}{Environment.NewLine}";
            try { File.AppendAllText(path, crashLog); } catch { }
        };

        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "app_logs.txt");
            var crashLog = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [CRITICAL] [UnobservedTask] {args.Exception.Message}{Environment.NewLine}{args.Exception.StackTrace}{Environment.NewLine}";
            try { File.AppendAllText(path, crashLog); } catch { }
            args.SetObserved();
        };

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // 2. Налаштування системи логування
        builder.Logging.ClearProviders();
#if DEBUG
        builder.Logging.AddDebug();
#endif
        builder.Logging.AddFileLogger();

        // 3. Реєстрація репозиторію та сервісів (Singleton)
        builder.Services.AddSingleton<IAssetRepository, DatabaseService>();

        // 4. Реєстрація ViewModel та Pages
        builder.Services.AddSingleton<PortfolioViewModel>();
        builder.Services.AddSingleton<PortfolioPage>();

        builder.Services.AddTransient<AssetDetailViewModel>();
        builder.Services.AddTransient<AssetDetailPage>();

        return builder.Build();
    }
}