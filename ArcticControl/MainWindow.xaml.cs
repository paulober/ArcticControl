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
    }
}
