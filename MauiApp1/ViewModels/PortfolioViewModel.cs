using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1.Models;
using System.Collections.ObjectModel;

namespace MauiApp1.ViewModels;

public partial class PortfolioViewModel : ObservableObject
{
    // Колекція для збереження та відображення активів у списку
    public ObservableCollection<Asset> Assets { get; } = new();

    // Поля введення форми
    [ObservableProperty]
    private string _newName = string.Empty;

    [ObservableProperty]
    private string _newSymbol = string.Empty;

    [ObservableProperty]
    private decimal _newAmount;

    [ObservableProperty]
    private decimal _newPrice;

    // Команда для додавання нового запису
    [RelayCommand]
    private void AddAsset()
    {
        if (string.IsNullOrWhiteSpace(NewName) || string.IsNullOrWhiteSpace(NewSymbol))
            return;

        Assets.Add(new Asset
        {
            Name = NewName,
            Symbol = NewSymbol.ToUpper(),
            Amount = NewAmount,
            CurrentPrice = NewPrice
        });

        // Очищення полів вводу після додавання
        NewName = string.Empty;
        NewSymbol = string.Empty;
        NewAmount = 0;
        NewPrice = 0;
    }
}
