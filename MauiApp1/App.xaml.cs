using System.Diagnostics;

namespace MauiApp1;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        Debug.WriteLine("--> [App Lifecycle] CreateWindow викликано");
        var window = new Window(new AppShell());

        window.Created += (s, e) => Debug.WriteLine("--> [App Lifecycle] Window Created");
        window.Activated += (s, e) => Debug.WriteLine("--> [App Lifecycle] Window Activated");
        window.Deactivated += (s, e) => Debug.WriteLine("--> [App Lifecycle] Window Deactivated (Sleep)");
        window.Stopped += (s, e) => Debug.WriteLine("--> [App Lifecycle] Window Stopped");
        window.Resumed += (s, e) => Debug.WriteLine("--> [App Lifecycle] Window Resumed");

        return window;
    }

    protected override void OnStart()
    {
        base.OnStart();
        Debug.WriteLine("--> [App Lifecycle] OnStart викликано");
    }

    protected override void OnSleep()
    {
        base.OnSleep();
        Debug.WriteLine("--> [App Lifecycle] OnSleep викликано");
    }

    protected override void OnResume()
    {
        base.OnResume();
        Debug.WriteLine("--> [App Lifecycle] OnResume викликано");
    }
}