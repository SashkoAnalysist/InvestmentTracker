using MauiApp1.Models;

namespace MauiApp1.Services;

public interface IAssetRepository
{
    Task InitializeAsync();
    Task<List<Asset>> GetAllAsync();
    Task<Asset?> GetByIdAsync(int id);
    Task<int> InsertAsync(Asset asset);
    Task<int> UpdateAsync(Asset asset);
    Task<int> DeleteAsync(Asset asset);
}