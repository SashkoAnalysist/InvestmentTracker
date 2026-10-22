using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1.Models;
using MauiApp1.Services;
using Moq;
using Xunit;

namespace MauiApp1.Tests;

// Тестована ViewModel без залежностей від UI MAUI (Shell, Preferences)
public partial class TestPortfolioViewModel : ObservableObject
{
    private readonly IAssetRepository _repository;

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

    public TestPortfolioViewModel(IAssetRepository repository)
    {
        _repository = repository;
    }

    [RelayCommand]
    public async Task LoadAssetsAsync()
    {
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
        catch
        {
            // Контрольоване перехоплення помилки (Лабораторна №4)
            Assets.Clear();
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
            await LoadAssetsAsync();
        }
    }
}

public class PortfolioViewModelTests
{
    private readonly Mock<IAssetRepository> _mockRepository;

    public PortfolioViewModelTests()
    {
        _mockRepository = new Mock<IAssetRepository>();
    }

    [Fact]
    public async Task LoadAssetsAsync_ShouldPopulateAssets_WhenRepositoryReturnsData()
    {
        // Arrange
        var testAssets = new List<Asset>
        {
            new Asset { Id = 1, Name = "Bitcoin", Symbol = "BTC", Amount = 1.5m, CurrentPrice = 60000m },
            new Asset { Id = 2, Name = "Ethereum", Symbol = "ETH", Amount = 10m, CurrentPrice = 3000m }
        };

        _mockRepository.Setup(repo => repo.GetAllAsync())
                       .ReturnsAsync(testAssets);

        var viewModel = new TestPortfolioViewModel(_mockRepository.Object);

        // Act
        await viewModel.LoadAssetsAsync();

        // Assert
        Assert.Equal(2, viewModel.Assets.Count);
        Assert.Equal("BTC", viewModel.Assets[0].Symbol);
        Assert.Equal("ETH", viewModel.Assets[1].Symbol);
        _mockRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task AddAssetAsync_ShouldInvokeInsert_AndClearFields()
    {
        // Arrange
        _mockRepository.Setup(repo => repo.InsertAsync(It.IsAny<Asset>()))
                       .ReturnsAsync(1);
        _mockRepository.Setup(repo => repo.GetAllAsync())
                       .ReturnsAsync(new List<Asset>());

        var viewModel = new TestPortfolioViewModel(_mockRepository.Object)
        {
            NewName = "Solana",
            NewSymbol = "SOL",
            NewAmount = 25,
            NewPrice = 140
        };

        // Act
        await viewModel.AddAssetCommand.ExecuteAsync(null);

        // Assert
        _mockRepository.Verify(repo => repo.InsertAsync(It.Is<Asset>(a => a.Name == "Solana" && a.Symbol == "SOL")), Times.Once);
        Assert.Empty(viewModel.NewName);
        Assert.Empty(viewModel.NewSymbol);
        Assert.Equal(0, viewModel.NewAmount);
    }

    [Fact]
    public async Task LoadAssetsAsync_ShouldHandleExceptionGracefully_WhenRepositoryThrows()
    {
        // Arrange
        _mockRepository.Setup(repo => repo.GetAllAsync())
                       .ThrowsAsync(new InvalidOperationException("Помилка SQLite"));

        var viewModel = new TestPortfolioViewModel(_mockRepository.Object);

        // Act
        var exception = await Record.ExceptionAsync(async () => await viewModel.LoadAssetsAsync());

        // Assert
        Assert.Null(exception);
        Assert.Empty(viewModel.Assets);
        Assert.False(viewModel.IsLoading);
    }

    [Theory]
    [InlineData("", "BTC", 1, 50000)]
    [InlineData("Bitcoin", "", 1, 50000)]
    [InlineData("   ", "   ", 1, 50000)]
    [InlineData("Cardano", "ADA", -5, 1)]
    public async Task AddAssetAsync_ShouldNotInsert_WhenInputDataIsInvalid(string name, string symbol, decimal amount, decimal price)
    {
        // Arrange
        var viewModel = new TestPortfolioViewModel(_mockRepository.Object)
        {
            NewName = name,
            NewSymbol = symbol,
            NewAmount = amount,
            NewPrice = price
        };

        // Act
        await viewModel.AddAssetCommand.ExecuteAsync(null);

        // Assert
        _mockRepository.Verify(repo => repo.InsertAsync(It.IsAny<Asset>()), Times.Never);
    }
}