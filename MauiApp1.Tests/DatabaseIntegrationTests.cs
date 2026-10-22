using MauiApp1.Models;
using SQLite;
using Xunit;

namespace MauiApp1.Tests;

public class DatabaseIntegrationTests : IDisposable
{
    private readonly string _testDbPath;
    private readonly SQLiteAsyncConnection _database;

    public DatabaseIntegrationTests()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_db_{Guid.NewGuid()}.db3");
        _database = new SQLiteAsyncConnection(_testDbPath);
        _database.CreateTableAsync<Asset>().Wait();
    }

    [Fact]
    public async Task Database_FullCrudLifecycle_ShouldWorkCorrectly()
    {
        var asset = new Asset
        {
            Name = "Cardano",
            Symbol = "ADA",
            Amount = 100m,
            CurrentPrice = 0.5m
        };

        var insertResult = await _database.InsertAsync(asset);
        Assert.Equal(1, insertResult);
        Assert.True(asset.Id > 0);

        var list = await _database.Table<Asset>().ToListAsync();
        Assert.Single(list);
        Assert.Equal("Cardano", list[0].Name);

        var itemToUpdate = await _database.Table<Asset>().FirstOrDefaultAsync(a => a.Id == asset.Id);
        Assert.NotNull(itemToUpdate);
        itemToUpdate.Amount = 250m;
        var updateResult = await _database.UpdateAsync(itemToUpdate);
        Assert.Equal(1, updateResult);

        var verified = await _database.Table<Asset>().FirstOrDefaultAsync(a => a.Id == asset.Id);
        Assert.Equal(250m, verified.Amount);

        var deleteResult = await _database.DeleteAsync(verified);
        Assert.Equal(1, deleteResult);

        var afterDelete = await _database.Table<Asset>().ToListAsync();
        Assert.Empty(afterDelete);
    }

    public void Dispose()
    {
        _database.CloseAsync().Wait();
        if (File.Exists(_testDbPath))
        {
            try { File.Delete(_testDbPath); } catch { }
        }
    }
}
