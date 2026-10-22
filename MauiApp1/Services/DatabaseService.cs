using MauiApp1.Models;
using Microsoft.Extensions.Logging;
using SQLite;

namespace MauiApp1.Services;

public class DatabaseService : IAssetRepository
{
    private SQLiteAsyncConnection? _database;
    private readonly string _dbPath;
    private readonly ILogger<DatabaseService> _logger;

    public DatabaseService(ILogger<DatabaseService> logger)
    {
        _logger = logger;
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "investment_tracker.db3");
        _logger.LogInformation("Ініціалізовано шлях до бази даних SQLite: {DbPath}", _dbPath);
    }

    public async Task InitializeAsync()
    {
        if (_database is not null)
            return;

        try
        {
            _logger.LogDebug("Спроба підключення та створення таблиці Assets у SQLite...");
            _database = new SQLiteAsyncConnection(_dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
            await _database.CreateTableAsync<Asset>();
            _logger.LogInformation("Таблицю Assets успішно перевірено/створено в SQLite");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Критична помилка ініціалізації бази даних за шляхом {DbPath}", _dbPath);
            throw;
        }
    }

    public async Task<List<Asset>> GetAllAsync()
    {
        try
        {
            await InitializeAsync();
            var items = await _database!.Table<Asset>().ToListAsync();
            _logger.LogInformation("Успішно завантажено {Count} активів із бази даних SQLite", items.Count);
            return items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка читання списку активів із бази даних SQLite");
            return new List<Asset>(); // Контрольоване повернення порожнього списку замість падіння
        }
    }

    public async Task<Asset?> GetByIdAsync(int id)
    {
        try
        {
            await InitializeAsync();
            var item = await _database!.Table<Asset>().FirstOrDefaultAsync(a => a.Id == id);
            if (item is null)
            {
                _logger.LogWarning("Актив із Id={Id} не знайдено в базі даних", id);
            }
            return item;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка отримання активу з Id={Id} із SQLite", id);
            return null;
        }
    }

    public async Task<int> InsertAsync(Asset asset)
    {
        try
        {
            await InitializeAsync();
            var result = await _database!.InsertAsync(asset);
            _logger.LogInformation("Успішно додано новий актив: {Name} ({Symbol}) з Id={Id}", asset.Name, asset.Symbol, asset.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка вставки нового активу {Name} ({Symbol}) у SQLite", asset.Name, asset.Symbol);
            return 0; // 0 сигналізує про невдачу операції
        }
    }

    public async Task<int> UpdateAsync(Asset asset)
    {
        try
        {
            await InitializeAsync();
            var result = await _database!.UpdateAsync(asset);
            _logger.LogInformation("Оновлено дані для активу Id={Id}: {Name} ({Symbol})", asset.Id, asset.Name, asset.Symbol);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка оновлення активу з Id={Id} у SQLite", asset.Id);
            return 0;
        }
    }

    public async Task<int> DeleteAsync(Asset asset)
    {
        try
        {
            await InitializeAsync();
            var result = await _database!.DeleteAsync(asset);
            _logger.LogInformation("Видалено актив із Id={Id} з бази даних SQLite", asset.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка видалення активу з Id={Id} із SQLite", asset.Id);
            return 0;
        }
    }
}