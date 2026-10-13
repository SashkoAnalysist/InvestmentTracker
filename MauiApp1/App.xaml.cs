using MauiApp1.Views;

namespace MauiApp1;

public partial class App : Application
{
    private readonly PortfolioPage _portfolioPage;

    public App(PortfolioPage portfolioPage)
    {
        InitializeComponent();
        _portfolioPage = portfolioPage;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(_portfolioPage);
    }
}