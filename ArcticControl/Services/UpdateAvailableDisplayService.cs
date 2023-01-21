using Windows.Foundation.Metadata;
using ArcticControl.Contracts.Services;
using ArcticControl.Helpers;
using ArcticControl.ViewModels;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml.Controls;

namespace ArcticControl.Services;

internal class UpdateAvailableDisplayService : IUpdateAvailableDisplayService
{
    private static bool shown = false;
    private readonly INavigationService _navigationService;

    public UpdateAvailableDisplayService(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }
    
    public async Task ShowIfAppropriateAsync()
    {
        if (!shown && (!SystemInformation.Instance.IsFirstRun && !SystemInformation.Instance.IsAppUpdated))
        {
            var updateAvailable = await AppUpdateHelper.CheckForUpdates() != string.Empty;

            if (updateAvailable)
            {
                var dialog = new ContentDialog
                {
                    Title = "App update",
                    PrimaryButtonText = "Go to Settings",
                    CloseButtonText = "Later",
                    DefaultButton = ContentDialogButton.Primary,
                    Content = "NewUpdateAvailableDialog_Content".GetLocalized()
                };
            
                // Use this code to associate the dialog to the appropriate AppWindow by setting
                // the dialog's XamlRoot to the same XamlRoot as an element that is already present in the AppWindow.
                if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
                {
                    dialog.XamlRoot = App.MainWindow.Content.XamlRoot;
                }
            
                var result = await dialog.ShowAsync();
                // avoid relaunching UI if app gets
                // activated via notification or so while running
                // needed because if conditions done with or to allow
                shown = true;

                if (result == ContentDialogResult.Primary)
                {
                    App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                    {
                        _navigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
                    });
                }
            }
            else
            {
                shown = true;
            }
        }
        
        await Task.CompletedTask;
    }
}