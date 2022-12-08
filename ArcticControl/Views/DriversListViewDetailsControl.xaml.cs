using System.Collections.ObjectModel;
using ArcticControl.IntelWebAPI.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ArcticControl.Views;

public sealed partial class DriversListViewDetailsControl : UserControl
{
    public WebArcDriver? SelectedDriver
    {
        get;
        set;
    }

    private ObservableCollection<WebArcDriver> NewDrivers
    {
        get => (ObservableCollection<WebArcDriver>)GetValue(NewItemsSourceProperty);
        set => SetValue(NewItemsSourceProperty, value);
    }

    private ObservableCollection<WebArcDriver> OldDrivers
    {
        get => (ObservableCollection<WebArcDriver>)GetValue(OldItemsSourceProperty);
        set => SetValue(OldItemsSourceProperty, value);
    }

    public static readonly DependencyProperty NewItemsSourceProperty = DependencyProperty.RegisterAttached("NewDriversSrc", typeof(ObservableCollection<WebArcDriver>), typeof(DriversListViewDetailsControl), new PropertyMetadata(null, OnNewItemsSourcePropertyChanged));
    public static readonly DependencyProperty OldItemsSourceProperty = DependencyProperty.RegisterAttached("OldDriverSrc", typeof(ObservableCollection<WebArcDriver>), typeof(DriversListViewDetailsControl), new PropertyMetadata(null, OnOldItemsSourcePropertyChanged));

    public DriversListViewDetailsControl()
    {
        this.InitializeComponent();
    }

    private void NewDriversListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
        {
            // connect with other list view
            OldDriversListView.SelectedItem = null;

            DriverSelectionChanged((WebArcDriver)e.AddedItems[0]);
        }
    }

    private void OldDriversListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
        {
            // connect with other list view
            NewDriversListView.SelectedItem = null;

            DriverSelectionChanged((WebArcDriver)e.AddedItems[0]);
        }
    }

    private void DriverSelectionChanged(WebArcDriver selectedDriver)
    {
        this.SelectedDriver = selectedDriver;
    }

    private static void OnNewItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DriversListViewDetailsControl control)
        {
        }
    }

    private static void OnOldItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DriversListViewDetailsControl control)
        {
        }
    }
}
