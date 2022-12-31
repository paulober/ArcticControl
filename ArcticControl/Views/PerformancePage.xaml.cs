using System.Diagnostics;
using System.Linq;
using ArcticControl.Core.Models;
using ArcticControl.ViewModels;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;

namespace ArcticControl.Views;

public sealed partial class PerformancePage : Page
{
    public PerformanceViewModel ViewModel { get; }

    public PerformancePage()
    {
        ViewModel = App.GetService<PerformanceViewModel>();
        InitializeComponent();
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

    private void TelemetryOverlayToggleButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is ToggleButton tb)
        {
            tb.Content = (tb.IsChecked ?? false) ? "ON" : "OFF";
        }
    }

    private void ApplySettingsOnSystemBootToggle_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is ToggleButton tb)
        {
            tb.Content = (tb.IsChecked ?? false) ? "ON" : "OFF";
        }
    }

    private void GPUPerformanceBoostSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        GPUPerformanceBoostLabel.Text = e.NewValue.ToString();
        EnableRevertButton();
    }

    private void GPUVoltageOffsetSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        GPUVoltageOffsetLabel.Text = $"+{e.NewValue} mV";
        EnableRevertButton();
    }

    private void GPUPowerLimitSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        GPUPowerLimitLabel.Text = $"{e.NewValue} W";
        EnableRevertButton();
    }

    private void GPUTemperatureLimitSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        GPUTemperatureLimitLabel.Text = $"{e.NewValue} °C";
        EnableRevertButton();
    }

    private void ResetToDefaultsButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        GPUPerformanceBoostSlider.Value = 0;
        GPUVoltageOffsetSlider.Value = 0;
        GPUPowerLimitSlider.Value= 0;
        GPUTemperatureLimitSlider.Value= 0;
        FanSpeedControlDropDownButton.Content = "Automatic";
        DisableRevertButton();
    }

    private void FanSpeedControlItem_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem mfi)
        {
            FanSpeedControlDropDownButton.Content = mfi.Text;
        }
    }

    private void GridView_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // The await causes the handler to return immediately.
        //Task.Run(() => StartBackgroundTickTimer());
        // Now update the UI with the results.
        // ...
        ViewModel.StartBackgroundTickTimer(DispatcherQueue.GetForCurrentThread());
    }
}
