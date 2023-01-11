using ArcticControl.Contracts.Services;
using ArcticControl.Helpers;
using ArcticControl.Views;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Navigation;

namespace ArcticControl.ViewModels;

public class ShellViewModel : ObservableRecipient
{
    private bool _isBackEnabled;
    private object? _selected;

    public INavigationService NavigationService
    {
        get;
    }

    public INavigationViewService NavigationViewService
    {
        get;
    }

    public bool IsBackEnabled
    {
        get => _isBackEnabled;
        set => SetProperty(ref _isBackEnabled, value);
    }

    public object? Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }

    private bool _arcDriverInstalled = false;
    public bool ArcDriverInstalled
    {
        get => _arcDriverInstalled;
        set => SetProperty(ref _arcDriverInstalled, value);
    }

    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;

        // TODO: remove this by fixing performance page crashes users pc with c++ exceptions and "System.IO.FileNotFoundException"
        ArcDriverInstalled = InstalledDriverHelper.IsIntelGraphicsDriverInstalled();
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = NavigationService.CanGoBack;

        if (e.SourcePageType == typeof(SettingsPage))
        {
            Selected = NavigationViewService.SettingsItem;
            return;
        }

        var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }
    }
}
