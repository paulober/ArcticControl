using System.Reflection;
using System.Windows.Input;

using ArcticControl.Contracts.Services;
using ArcticControl.Core.Helpers;
using ArcticControl.Helpers;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel;

namespace ArcticControl.ViewModels;

public class SettingsViewModel : ObservableRecipient
{
    private const double MinGPUPowerMaxLimit = 228.0d;

    private readonly IThemeSelectorService _themeSelectorService;
    private readonly ILocalSettingsService _localSettingsService;
    private ElementTheme _elementTheme;
    private string _versionDescription;

    public ElementTheme ElementTheme
    {
        get => _elementTheme;
        set => SetProperty(ref _elementTheme, value);
    }

    public string VersionDescription
    {
        get => _versionDescription;
        set => SetProperty(ref _versionDescription, value);
    }

    public ICommand SwitchThemeCommand
    {
        get;
    }

    private Visibility _gpuPowerMaxLimitInputErrorVis = Visibility.Collapsed;

    public Visibility GPUPowerMaxLimitInputErrorVis
    {
        get => _gpuPowerMaxLimitInputErrorVis;
        set => SetProperty(ref _gpuPowerMaxLimitInputErrorVis, value);
    }

    private double _gpuPowerMaxLimit;
    public double GPUPowerMaxLimit
    {
        get => _gpuPowerMaxLimit;
        set => SetProperty(ref _gpuPowerMaxLimit, value);
    }

    public SettingsViewModel(IThemeSelectorService themeSelectorService, ILocalSettingsService localSettingsService)
    {
        _themeSelectorService = themeSelectorService;
        _localSettingsService = localSettingsService;
        _elementTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();

        SwitchThemeCommand = new RelayCommand<ElementTheme>(
            async (param) =>
            {
                if (ElementTheme != param)
                {
                    ElementTheme = param;
                    await _themeSelectorService.SetThemeAsync(param);
                }
            });
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

    public async void GPUPowerMaxLimitComboBox_TextSubmitted(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
    {
        double newValue;
        if (!double.TryParse(args.Text, out newValue))
        {
            return;
        }

        if (newValue < MinGPUPowerMaxLimit || newValue > 400.0)
        {
            sender.BorderBrush = new SolidColorBrush(Colors.Red);
            sender.BorderThickness = new Thickness(1);
            GPUPowerMaxLimitInputErrorVis = Visibility.Visible;
            return;
        }
        else
        {
            GPUPowerMaxLimitInputErrorVis = Visibility.Collapsed;
            sender.BorderThickness = new Thickness(0);
        }

        await _localSettingsService.SaveSettingAsync(LocalSettingsKeys.GPUPowerMaxLimit, newValue);
    }

    public async void SaveMaxGPUPowerLimitButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (GPUPowerMaxLimit < MinGPUPowerMaxLimit || GPUPowerMaxLimit > 400.0)
        {
            GPUPowerMaxLimitInputErrorVis = Visibility.Visible;
            return;
        }
        else
        {
            GPUPowerMaxLimitInputErrorVis = Visibility.Collapsed;
        }

        await _localSettingsService.SaveSettingAsync(LocalSettingsKeys.GPUPowerMaxLimit, GPUPowerMaxLimit);
    }

    public async void GPUPowerMaxLimitNumberBox_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is NumberBox box)
        {
            var settingsVal = await _localSettingsService.ReadSettingAsync<double>(LocalSettingsKeys.GPUPowerMaxLimit);
            if (settingsVal < MinGPUPowerMaxLimit)
            {
                settingsVal = MinGPUPowerMaxLimit;
            }
            box.Value = settingsVal;
        }
    }
}
