using MauiApp1.Helpers;
using MauiApp1.ViewModels;

namespace MauiApp1.Views;

public partial class AssetDetailPage : ContentPage
{
    private readonly AssetDetailViewModel _viewModel;

    public AssetDetailPage(AssetDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel is ILifecycleAware lifecycleAware)
        {
            lifecycleAware.OnAppearing();
        }
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