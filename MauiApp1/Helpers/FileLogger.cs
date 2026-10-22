using Microsoft.Extensions.Logging;

namespace MauiApp1.Helpers;

public class FileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly string _filePath;
    private static readonly object _lock = new();

    public FileLogger(string categoryName)
    {
        _categoryName = categoryName;
        _filePath = Path.Combine(FileSystem.AppDataDirectory, "app_logs.txt");
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        var logRecord = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{logLevel}] [{_categoryName}] {message}";

        if (exception != null)
        {
            logRecord += $"{Environment.NewLine}Exception: {exception.GetType().FullName}: {exception.Message}";
            logRecord += $"{Environment.NewLine}StackTrace: {exception.StackTrace}";
        }

        lock (_lock)
        {
            try
            {
                File.AppendAllText(_filePath, logRecord + Environment.NewLine);
            }
            catch
            {
                // Уникаємо аварійного завершення, якщо виник збій доступу до файлу
            }
        }
    }
}

public class FileLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new FileLogger(categoryName);
    public void Dispose() { }
}

public static class FileLoggerExtensions
{
    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder)
    {
        builder.AddProvider(new FileLoggerProvider());
        return builder;
    }
}