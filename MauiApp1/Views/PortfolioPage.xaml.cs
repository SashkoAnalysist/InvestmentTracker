using MauiApp1.Helpers;
using MauiApp1.Models;
using MauiApp1.Services;
using MauiApp1.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace MauiApp1.Views;

public partial class PortfolioPage : ContentPage
{
    private readonly PortfolioViewModel _viewModel;
    private readonly IAuthService _authService;

    public PortfolioPage(PortfolioViewModel viewModel, IAuthService authService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _authService = authService;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel is ILifecycleAware lifecycle)
        {
            lifecycle.OnAppearing();
        }
        await _viewModel.LoadAssetsAsync();
    }

    private async void OnSwitchRoleClicked(object sender, EventArgs e)
    {
        string action = await DisplayActionSheet("Оберіть користувача для входу:", "Скасувати", null, "admin (Роль: Admin)", "viewer (Роль: Viewer)");

        if (action == "admin (Роль: Admin)")
        {
            await _authService.LoginAsync("admin", "admin123");
        }
        else if (action == "viewer (Роль: Viewer)")
        {
            await _authService.LoginAsync("viewer", "user123");
        }

        if (_viewModel is ILifecycleAware lifecycle)
        {
            lifecycle.OnAppearing();
        }
        await _viewModel.LoadAssetsAsync();
    }

    private async void OnDetailsClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Asset asset)
        {
            try
            {
                var services = Handler?.MauiContext?.Services ?? IPlatformApplication.Current?.Services;
                if (services == null) return;

                var detailVm = services.GetRequiredService<AssetDetailViewModel>();
                detailVm.CurrentAsset = asset;

                var detailPage = new AssetDetailPage(detailVm);
                await Navigation.PushAsync(detailPage);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", ex.Message, "OK");
            }
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Asset asset)
        {
            if (!_authService.IsAdmin)
            {
                await DisplayAlert("Доступ обмежено", "Ви увійшли як Viewer. Видалення записів дозволено тільки адміністратору!", "OK");
                return;
            }

            await _viewModel.DeleteAssetAsync(asset);
        }
    }
}