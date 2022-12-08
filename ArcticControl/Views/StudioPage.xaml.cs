using ArcticControl.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace ArcticControl.Views;

public sealed partial class StudioPage : Page
{
    public StudioViewModel ViewModel
    {
        get;
    }

    public StudioPage()
    {
        ViewModel = App.GetService<StudioViewModel>();
        InitializeComponent();
    }
}
