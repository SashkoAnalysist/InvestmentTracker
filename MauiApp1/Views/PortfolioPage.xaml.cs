using MauiApp1.ViewModels;

namespace MauiApp1.Views;

public partial class PortfolioPage : ContentPage
{
    public PortfolioPage(PortfolioViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}