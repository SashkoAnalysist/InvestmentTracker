using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1.Helpers;
using MauiApp1.Models;
using MauiApp1.Services;
using MauiApp1.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MauiApp1.ViewModels;

public partial class PortfolioViewModel : ObservableObject, ILifecycleAware
{
    private const string DraftKey = "Draft_Asset_Name";
    private readonly IAssetRepository _repository;

    public ObservableCollection<Asset> Assets { get; } = new();

    [ObservableProperty]
    private string _newName = string.Empty;

    [ObservableProperty]
    private string _newSymbol = string.Empty;

    [ObservableProperty]
    private decimal _newAmount;

    [ObservableProperty]
    private decimal _newPrice;

    [ObservableProperty]
    private bool _isLoading;

    public PortfolioViewModel(IAssetRepository repository)
    {
        _repository = repository;
    }

    [RelayCommand]
    public async Task LoadAssetsAsync()
    {
        if (IsLoading)
            return;

        try
        {
            IsLoading = true;
            var items = await _repository.GetAllAsync();

            Assets.Clear();
            foreach (var item in items)
            {
                Assets.Add(item);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Помилка завантаження даних із SQLite: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddAssetAsync()
    {
        if (string.IsNullOrWhiteSpace(NewName) || string.IsNullOrWhiteSpace(NewSymbol))
            return;

        var asset = new Asset
        {
            Name = NewName,
            Symbol = NewSymbol.ToUpper(),
            Amount = NewAmount,
            CurrentPrice = NewPrice
        };

        await _repository.InsertAsync(asset);

        NewName = string.Empty;
        NewSymbol = string.Empty;
        NewAmount = 0;
        NewPrice = 0;

        Preferences.Default.Remove(DraftKey);

        await LoadAssetsAsync();
    }

    [RelayCommand]
    private async Task SelectAssetAsync(Asset? selectedAsset)
    {
        if (selectedAsset is null)
            return;

        var route = $"{nameof(AssetDetailPage)}?" +
                    $"Id={selectedAsset.Id}&" +
                    $"Name={Uri.EscapeDataString(selectedAsset.Name)}&" +
                    $"Symbol={Uri.EscapeDataString(selectedAsset.Symbol)}&" +
                    $"Amount={selectedAsset.Amount}&" +
                    $"Price={selectedAsset.CurrentPrice}";

        await Shell.Current.GoToAsync(route);
    }

    public async void OnAppearing()
    {
        Debug.WriteLine("--> [LifecycleAware] PortfolioPage OnAppearing");

        if (string.IsNullOrEmpty(NewName))
        {
            var savedDraft = Preferences.Default.Get(DraftKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(savedDraft))
            {
                NewName = savedDraft;
                Debug.WriteLine($"--> [Preferences] Відновлено чорновик: {savedDraft}");
            }
        }

        await LoadAssetsAsync();
    }

    public void OnDisappearing()
    {
        Debug.WriteLine("--> [LifecycleAware] PortfolioPage OnDisappearing");

        if (!string.IsNullOrWhiteSpace(NewName))
        {
            Preferences.Default.Set(DraftKey, NewName);
            Debug.WriteLine($"--> [Preferences] Збережено чорновик: {NewName}");
        }
    }
}