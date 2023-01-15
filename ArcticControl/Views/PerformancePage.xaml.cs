using System.Diagnostics;
using ArcticControl.ViewModels;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.Foundation.Metadata;
using ArcticControl.Helpers;

namespace ArcticControl.Views;

public sealed partial class PerformancePage : Page
{
    public PerformanceViewModel ViewModel { get; }

    // To ignore updates by Driver on loading of the page
    private bool _waiverEnabled = false;

    public PerformancePage()
    {
        ViewModel = App.GetService<PerformanceViewModel>();
        InitializeComponent();

        if (!UACChecker.IsAdministrator())
        {
            NotAdminWarningInfo.IsOpen = true;
        }

        NoDriverErrorInfo.IsOpen = ViewModel.IsNoArcDriverInstalled();
    }

    private void EnableRevertButton()
    {
        if (RevertButton != null)
        {
            RevertButton.IsEnabled = true;
        }
    }

    private void DisableRevertButton()
    {
        if (RevertButton != null)
        {
            RevertButton.IsEnabled = false;
        }
    }

    private void EnableApplyButton()
    {
        if (ApplyButton != null)
        {
            ApplyButton.IsEnabled = true;
        }
    }

    private void DisableApplyButton()
    {
        if (ApplyButton != null)
        {
            ApplyButton.IsEnabled = false;
        }
    }

    private void TelemetryOverlayToggleButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is ToggleButton tb)
        {
            tb.Content = tb.IsChecked ?? false ? "ON" : "OFF";
        }
    }

    private void ApplySettingsOnSystemBootToggle_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is ToggleButton tb)
        {
            tb.Content = (tb.IsChecked ?? false) ? "ON" : "OFF";
        }
    }

    private async Task CheckWaiver()
    {
        if (ViewModel.WaiverSigned || !_waiverEnabled)
        {
            if (ViewModel.WaiverSigned)
            {
                EnableApplyButton();
            }

            await Task.CompletedTask;
            return;
        }
        // set it now to true to avoid build paralel runs of this method
        // block which will all try to start a ContentDialog async
        ViewModel.WaiverSigned = true;

        ContentDialog dialog = new()
        {
            Content = "The tuning of your GPU can cause instability or damage your components. You do all changes to these settings on your own risk. The author of this application does not take responsibility for any damage to your hardware. (It is required that you run this application as administrator if you want the settings to take effect!)",
            PrimaryButtonText = "Acept & Continue",
            CloseButtonText = "Cancel"
        };

        if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
        {
            // dialog.XamlRoot = xamlRoot;
            dialog.XamlRoot = App.MainWindow.Content.XamlRoot;
        }

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.None)
        {
            ViewModel.WaiverSigned = false;
            DisableApplyButton();
        }
        else
        {
            EnableApplyButton();
            ViewModel.SetOverclockWaiver();
        }

        await Task.CompletedTask;
    }

    /*private async void GPUPerformanceBoostSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        GPUPerformanceBoostLabel.Text = e.NewValue.ToString();
        EnableRevertButton();
        await CheckWaiver();
    }*/
    
    private async void FrequencyMinimumNumberBox_OnValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        EnableRevertButton();
        await CheckWaiver();
    }
    
    private async void FrequencyMaximumNumberBox_OnValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        EnableRevertButton();
        await CheckWaiver();
    }

    private async void GPUFrequencyOffsetSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        GPUFrequencyOffsetLabel.Text = $"+{Math.Round(e.NewValue, 2)} MHz";
        EnableRevertButton();
        await CheckWaiver();
    }

    private async void GPUVoltageOffsetSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        GPUVoltageOffsetLabel.Text = $"+{e.NewValue} mV";
        EnableRevertButton();
        await CheckWaiver();
    }

    private async void GPUPowerLimitSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        GPUPowerLimitLabel.Text = $"{e.NewValue} W";
        EnableRevertButton();
        await CheckWaiver();
    }

    private async void GPUTemperatureLimitSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        GPUTemperatureLimitLabel.Text = $"{e.NewValue} °C";
        EnableRevertButton();
        EnableApplyButton();
        await CheckWaiver();
    }

    private void ResetToDefaultsButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        /*GPUPerformanceBoostSlider.Value = 0;
        GPUVoltageOffsetSlider.Value = 0;
        GPUPowerLimitSlider.Value= 0;
        GPUTemperatureLimitSlider.Value= 0;*/
        FanSpeedControlDropDownButton.Content = "Automatic";

        ViewModel.ResetToDefaultSliderValues();

        // TODO: maybe not do this
        DisableRevertButton();
    }

    private void FanSpeedControlItem_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem mfi)
        {
            FanSpeedControlDropDownButton.Content = mfi.Text;
            if (mfi.Text == "Fixed")
            {
                ViewModel.LoadFanSpeedSlider = true;
                ViewModel.FanSpeedFixed = true;
                FanSpeedSlider.Visibility = Visibility.Visible;
            }
            else
            {
                ViewModel.FanSpeedFixed = false;
                FanSpeedSlider.Visibility = Visibility.Collapsed;
            }
        }
    }

    private async void FanSpeedSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        FanSpeedLabel.Text = $"{e.NewValue}%";
        EnableRevertButton();
        await CheckWaiver();
    }

    private void GridView_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        _waiverEnabled = true;
    }

    private async void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.ApplyChanges())
        {
            ApplyButton.IsEnabled = false;
            DisableApplyButton();
        }
        else
        {
            await CheckWaiver();
        }
    }

    private void StartMonitoringButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.IsTickTimerStarted())
        {
            // The await causes the handler to return immediately.
            //Task.Run(() => StartBackgroundTickTimer());
            // Now update the UI with the results.
            // ...
            ViewModel.StartBackgroundTickTimer(DispatcherQueue.GetForCurrentThread());
        }
    }
}
