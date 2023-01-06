using System.Diagnostics;
using ArcticControl.Contracts.Services;
using ArcticControl.Helpers;

namespace ArcticControl;

public sealed partial class MainWindow : WindowEx
{
    public MainWindow()
    {
        InitializeComponent();

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
        Content = null;
        Title = "AppDisplayName".GetLocalized();

        // set min window width
        var manager = WindowManager.Get(this);
        manager.MinWidth = 1200;
        manager.MaxWidth = 1600;
    }

    private void WindowEx_Closed(object sender, Microsoft.UI.Xaml.WindowEventArgs args)
    {
        // TODO: direct invoke of dispose() method ; change if WindowsAppSDK calls disposables
        App.GetService<IIntelGraphicsControlService>().Dispose();
    }
}
