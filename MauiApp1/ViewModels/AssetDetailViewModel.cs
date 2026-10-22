using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1.Helpers;
using MauiApp1.Models;
using MauiApp1.Services;
using Microsoft.Extensions.Logging;

namespace MauiApp1.ViewModels;

public partial class AssetDetailViewModel : ObservableObject, IQueryAttributable, ILifecycleAware
{
    private readonly IAssetRepository _repository;
    private readonly ILogger<AssetDetailViewModel> _logger;

    private int _id;
    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string _symbol = string.Empty;
    public string Symbol
    {
        get => _symbol;
        set => SetProperty(ref _symbol, value);
    }

    private decimal _amount;
    public decimal Amount
    {
        get => _amount;
        set
        {
            if (SetProperty(ref _amount, value))
            {
                OnPropertyChanged(nameof(TotalValue));
            }
        }
    }

    private decimal _price;
    internal Asset CurrentAsset;

    public decimal Price
    {
        get => _price;
        set
        {
            if (SetProperty(ref _price, value))
            {
                OnPropertyChanged(nameof(TotalValue));
            }
        }
    }

    public decimal TotalValue => Amount * Price;

    public AssetDetailViewModel(IAssetRepository repository, ILogger<AssetDetailViewModel> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Id", out var idObj) && int.TryParse(idObj?.ToString(), out int parsedId))
        {
            Id = parsedId;
        }

        if (query.TryGetValue("Name", out var nameObj))
        {
            Name = Uri.UnescapeDataString(nameObj?.ToString() ?? string.Empty);
        }

        if (query.TryGetValue("Symbol", out var symbolObj))
        {
            Symbol = Uri.UnescapeDataString(symbolObj?.ToString() ?? string.Empty);
        }

        if (query.TryGetValue("Amount", out var amountObj) && decimal.TryParse(amountObj?.ToString(), out decimal parsedAmount))
        {
            Amount = parsedAmount;
        }

        if (query.TryGetValue("Price", out var priceObj) && decimal.TryParse(priceObj?.ToString(), out decimal parsedPrice))
        {
            Price = parsedPrice;
        }

        _logger.LogInformation("Завантажено деталі активу для редагування: {Name} (Id={Id})", Name, Id);
        OnPropertyChanged(nameof(TotalValue));
    }

    [RelayCommand]
    private async Task SaveChangesAsync()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Symbol))
        {
            _logger.LogWarning("Спроба зберегти актив з порожньою назвою або тікером");
            await Shell.Current.DisplayAlert("Помилка валідації", "Назва та тікер активу не можуть бути порожніми.", "OK");
            return;
        }

        if (Amount < 0 || Price < 0)
        {
            _logger.LogWarning("Спроба зберегти від'ємні значення: Amount={Amount}, Price={Price}", Amount, Price);
            await Shell.Current.DisplayAlert("Помилка введення", "Кількість та ціна не можуть бути від'ємними.", "OK");
            return;
        }

        var asset = new Asset
        {
            Id = Id,
            Name = Name,
            Symbol = Symbol.ToUpper(),
            Amount = Amount,
            CurrentPrice = Price
        };

        var rowsAffected = await _repository.UpdateAsync(asset);

        if (rowsAffected > 0)
        {
            _logger.LogInformation("Успішно оновлено актив Id={Id}", Id);
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            _logger.LogError("Не вдалося оновити актив Id={Id} в базі даних (rowsAffected=0)", Id);
            // Відображаємо зрозуміле повідомлення без технічного стеку помилки
            await Shell.Current.DisplayAlert("Помилка операції", "Не вдалося зберегти зміни в базі даних. Спробуйте ще раз.", "OK");
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        var confirm = await Shell.Current.DisplayAlert("Підтвердження", $"Ви дійсно бажаєте видалити актив {Name}?", "Так", "Ні");
        if (!confirm)
            return;

        var asset = new Asset { Id = Id };
        var rowsAffected = await _repository.DeleteAsync(asset);

        if (rowsAffected > 0)
        {
            _logger.LogInformation("Користувач успішно видалив актив Id={Id}", Id);
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            _logger.LogError("Помилка при видаленні активу Id={Id}", Id);
            await Shell.Current.DisplayAlert("Помилка", "Не вдалося видалити актив. Спробуйте пізніше.", "OK");
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    public void OnAppearing()
    {
        _logger.LogDebug("AssetDetailPage відкрилася для активу Id={Id}", Id);
    }

    public void OnDisappearing()
    {
        _logger.LogDebug("AssetDetailPage закрито");
    }
}