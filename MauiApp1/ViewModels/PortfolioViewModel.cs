using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1.Helpers;
using MauiApp1.Models;
using MauiApp1.Services;
using MauiApp1.Views;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MauiApp1.ViewModels;

public partial class PortfolioViewModel : ObservableObject, ILifecycleAware
{
    private const string DraftKey = "AssetDraft_Name";

    private readonly IAssetRepository _repository;
    private readonly IAuthService _authService;
    private readonly ILogger<PortfolioViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<Asset> _assets = new();

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

    // Властивість для відображення кнопок видалення (доступно лише Admin)
    [ObservableProperty]
    private bool _canDelete;

    // Інформаційний рядок поточного користувача
    [ObservableProperty]
    private string _currentUserInfo = string.Empty;

    public PortfolioViewModel(IAssetRepository repository, IAuthService authService, ILogger<PortfolioViewModel> logger)
    {
        _repository = repository;
        _authService = authService;
        _logger = logger;
    }

    public async void OnAppearing()
    {
        Debug.WriteLine("--> [LifecycleAware] PortfolioPage OnAppearing");

        UpdateUserRoleState();

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

    public void UpdateUserRoleState()
    {
        CanDelete = _authService.IsAdmin;
        CurrentUserInfo = _authService.CurrentUser != null
            ? $"Користувач: {_authService.CurrentUser.Username} ({_authService.CurrentUser.Role})"
            : "Гість (Viewer)";
    }

    [RelayCommand]
    public async Task LoadAssetsAsync()
    {
        try
        {
            IsLoading = true;
            UpdateUserRoleState();

            var items = await _repository.GetAllAsync();
            Assets.Clear();
            foreach (var item in items)
            {
                Assets.Add(item);
            }
            _logger.LogInformation("Завантажено {Count} активів.", Assets.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка під час завантаження активів.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task AddAssetAsync()
    {
        if (string.IsNullOrWhiteSpace(NewName) || string.IsNullOrWhiteSpace(NewSymbol) || NewAmount <= 0 || NewPrice <= 0)
        {
            return;
        }

        var asset = new Asset
        {
            Name = NewName,
            Symbol = NewSymbol.ToUpper(),
            Amount = NewAmount,
            CurrentPrice = NewPrice
        };

        var result = await _repository.InsertAsync(asset);
        if (result > 0)
        {
            NewName = string.Empty;
            NewSymbol = string.Empty;
            NewAmount = 0;
            NewPrice = 0;
            Preferences.Default.Remove(DraftKey);
            await LoadAssetsAsync();
        }
    }

    // Крок 3 методички: Перевірка ролі безпосередньо в тілі команди ViewModel
    [RelayCommand]
    public async Task DeleteAssetAsync(Asset? asset)
    {
        if (asset == null) return;

        if (!_authService.IsAdmin)
        {
            _logger.LogWarning("Спроба несанкціонованого видалення користувачем без ролі Admin.");
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Доступ заборонено", "Операція видалення доступна лише для ролі Admin!", "OK");
            }
            return;
        }

        var deleted = await _repository.DeleteAsync(asset);
        if (deleted > 0)
        {
            Assets.Remove(asset);
            _logger.LogInformation("Актив {Symbol} успішно видалено адміністратором.", asset.Symbol);
        }
    }

    [RelayCommand]
    public async Task SelectAssetAsync(Asset? asset)
    {
        if (asset == null) return;
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync(nameof(AssetDetailPage), new Dictionary<string, object>
            {
                { "Asset", asset }
            });
        }
    }
}