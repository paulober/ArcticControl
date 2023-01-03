using ArcticControl.Contracts.Services;
using ArcticControl.Views.Dialogs;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation.Metadata;

namespace ArcticControl.Services;
internal class PrivacyConsentDisplayService : IPrivacyConsentDisplayService
{
    private static bool shown = false;
    private const string PrivacyPolicySettingsKey = "PrivacyPolicy:Consented";
    private readonly ILocalSettingsService _localSettingsService;

    public PrivacyConsentDisplayService(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
    }

    private async Task<bool> CheckIfPrivacyPolicyDenied() 
        => !await _localSettingsService.ReadSettingAsync<bool>(PrivacyPolicySettingsKey);

    public async Task ShowIfAppropriateAsync()
    {
        
        if (
            !shown && 
            (System.Diagnostics.Debugger.IsAttached 
            || SystemInformation.Instance.IsFirstRun 
            || SystemInformation.Instance.IsAppUpdated 
            || await CheckIfPrivacyPolicyDenied()))
        {
            var dialog = new ContentDialog
            {
                Title = "Privacy Policy agreement",
                PrimaryButtonText = "Accept",
                CloseButtonText = "Refuse",
                DefaultButton = ContentDialogButton.Primary,
                Content = new PrivacyConsentDialog()
            };

            // Use this code to associate the dialog to the appropriate AppWindow by setting
            // the dialog's XamlRoot to the same XamlRoot as an element that is already present in the AppWindow.
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
            {
                dialog.XamlRoot = App.MainWindow.Content.XamlRoot;
            }

            var result = await dialog.ShowAsync();
            // avoid relaunching UI if app gets
            // activated via notification or so while runnung
            // needed because if conditions done with or to allow
            // if IsAppUpdate even if PrivacyPolicy is allready accepted
            shown = true;

            switch (result)
            {
                case ContentDialogResult.Primary:
                    await _localSettingsService.SaveSettingAsync(PrivacyPolicySettingsKey, true);
                    if (AppCenter.Configured)
                    {
                        AppCenter.Start(typeof(Analytics), typeof(Crashes));
                        // ensure Crashes and Analytics are enabled
                        await Crashes.SetEnabledAsync(true);
                        await Analytics.SetEnabledAsync(true);
                    }
                    break;
                default:
                    await _localSettingsService.SaveSettingAsync(PrivacyPolicySettingsKey, false);
                    Application.Current.Exit();
                    break;
            }
        }

        await Task.CompletedTask;
    }
}
