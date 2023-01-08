using ArcticControl.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArcticControl.Views;

public sealed partial class GamesPage : Page
{
    public GamesViewModel ViewModel
    {
        get;
    }

    public GamesPage()
    {
        ViewModel = App.GetService<GamesViewModel>();
        InitializeComponent();
    }

    private void GridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        // TOOD: unter development
        return;

/*
        if (ViewModel.ItemClickCommand.CanExecute(e.ClickedItem))
        {
            ViewModel.ItemClickCommand.Execute(e.ClickedItem);
        }
*/
    }

    private void InstalledGame_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        => ViewModel.InstalledGame_PointerEntered(sender, e);

    private void InstalledGame_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        => ViewModel.InstalledGame_PointerExited(sender, e);
}
