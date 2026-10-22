using MauiApp1.Helpers;
using MauiApp1.ViewModels;

namespace MauiApp1.Views;

public partial class PortfolioPage : ContentPage
{
    private readonly PortfolioViewModel _viewModel;

    public PortfolioPage(PortfolioViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel is ILifecycleAware lifecycleAware)
        {
            lifecycleAware.OnAppearing();
        }

        // Автоматично підтягуємо оновлені дані з SQLite при кожному поверненні на екран
        await _viewModel.LoadAssetsAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (_viewModel is ILifecycleAware lifecycleAware)
        {
            lifecycleAware.OnDisappearing();
        }
    }
}