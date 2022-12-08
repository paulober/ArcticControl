using ArcticControl.Helpers;
using ArcticControl.IntelWebAPI.Models;
using ArcticControl.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;

namespace ArcticControl.Views;

public sealed partial class DriversPage : Page
{
    public DriversViewModel ViewModel
    {
        get;
    }

    public DriversPage()
    {
        ViewModel = App.GetService<DriversViewModel>();
        InitializeComponent();

        // listen to NewWebArcDrivers as this collection gets always loaded at least 1 drivers into it
        ViewModel.NewWebArcDrivers.CollectionChanged += NewWebArcDrivers_CollectionChanged;
        //defect: ViewModel.OldWebArcDrivers.CollectionChanged += OldWebArcDrivers_CollectionChanged;
    }

    private void NewWebArcDrivers_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (LoadingContainer.Visibility == Microsoft.UI.Xaml.Visibility.Visible)
        {
            LoadingContainer.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }

        // unsubscribe after LoadingContainer handeling has been done
        ViewModel.NewWebArcDrivers.CollectionChanged -= NewWebArcDrivers_CollectionChanged;
    }

    private void OldWebArcDrivers_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // DEFECT: does not work correctly in WinUI 3 suspend to future update

        // adjust the headers width to the
        // actualwidth (width of the widest element in expander plus minus some expander calculations)
        // to avoid horizontally expanding when content inside is wider then the rest of the stackpanel
        //OldDriversExpanderHeader.Width = OldDriversExpander.ActualWidth;
    }

    /* for ListDetailsView of CommunityToolkit
    private void OnViewStateChanged(object sender, ListDetailsViewState e)
    {
        if (e == ListDetailsViewState.Both)
        {
            ViewModel.EnsureItemSelected();
        }
    }*/
}
