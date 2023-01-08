using ArcticControl.Contracts.Services;
using ArcticControl.Contracts.ViewModels;
using ArcticControl.Helpers;
using ArcticControl.Models;
using ArcticControlGPUInterop;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArcticControl.ViewModels;

public class GamesSettingsViewModel : ObservableRecipient, INavigationAware
{
    private readonly IIntelGraphicsControlService _igcs;

    private GamesSettingsParamerter? _navigationParameter;

    // TODO: load MenuFlyout items dynamicly (depnding on supported ones reported by driver) in the future
    public MenuFlyout? TearingEffectMitigationMenuFlyout
    {
        get;
        set;
    }

    public DropDownButton? TearingEffectMitigationDropdownBtn
    {
        get; set;
    }
    
    public MenuFlyout? AnisotropicFilteringMenuFlyout
    {
        get;
        set;
    }

    public DropDownButton? AnisotropicFilteringDropdownBtn
    {
        get; set;
    }
    
    public MenuFlyout? AntiAliasingMenuFlyout
    {
        get;
        set;
    }

    public DropDownButton? AntiAliasingDropdownBtn
    {
        get; set;
    }

    private bool _isSharpeningFilterActive = false;

    public bool IsSharpeningFilterActive
    {
        get => _isSharpeningFilterActive;
        set => SetProperty(ref _isSharpeningFilterActive, value);
    }

    private bool _sharpeningFilterSwitchEnabled = true;

    public bool SharpeningFilterSwitchEnabled
    {
        get => _sharpeningFilterSwitchEnabled;
        set => SetProperty(ref _sharpeningFilterSwitchEnabled, value);
    }

    private bool _inputControlsEnabled = true;
    public bool InputControlsEnabled
    {
        get => _inputControlsEnabled;
        set
        {
            SharpeningFilterSwitchEnabled = value;
            SetProperty(ref _inputControlsEnabled, value);
        }
    }

    public GamesSettingsViewModel(IIntelGraphicsControlService graphicsControlService)
    {
        _igcs = graphicsControlService;
    }
    
    public void TearingEffectReductionFlyoutItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem mfi)
        {
            _ = _igcs.SetGamingFlipMode(mfi.Text.ToGamingFlipMode());
            LoadTearingEffectMitigationStuff();
        }
    }

    public void LoadTearingEffectMitigationStuff()
    {
        if (TearingEffectMitigationDropdownBtn == null)
        {
            return;
        }

        var gfm = _igcs.GetGamingFlipMode();
        TearingEffectMitigationDropdownBtn.Content = gfm.ToReadableString();
    }

    public void AnisotropicFilteringFlyoutItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem mfi)
        {
            _ = _igcs.SetAnisotropicFilteringMode(mfi.Text.ToAnisotropicFilteringMode());
            LoadAnisotropicFilteringStuff();
        }
    }

    public void LoadAnisotropicFilteringStuff()
    {
        if (AnisotropicFilteringDropdownBtn == null)
        {
            return;
        }

        var afm = _igcs.GetAnisotropicFilteringMode();
        AnisotropicFilteringDropdownBtn.Content = afm.ToReadableString();
    }

    public void AntiAliasingFlyoutItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem mfi)
        {
            _ = _igcs.SetCmaaMode(mfi.Text.ToCmaaMode());
            LoadAntiAliasingStuff();
        }
    }
    
    public void LoadAntiAliasingStuff()
    {
        if (AntiAliasingDropdownBtn == null)
        {
            return;
        }

        var cm = _igcs.GetCmaaMode();
        AntiAliasingDropdownBtn.Content = cm.ToReadableString();
    }
    
    public void SharpeningFilter_ToggleSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        SharpeningFilterSwitchEnabled = false;
        if (!_igcs.SetSharpeningFilter(IsSharpeningFilterActive))
        {
            // sharpening filter hasn't been set successfully
            IsSharpeningFilterActive = !IsSharpeningFilterActive;
            // TODO: maybe also load after change like on other settings to ensure the driver accepted the value
        }
        SharpeningFilterSwitchEnabled = true;
    }

    public void OnNavigatedTo(object parameter)
    {
        if (parameter is GamesSettingsParamerter param)
        {
            _navigationParameter = param;
        }
        else
        {
            LoadTearingEffectMitigationStuff();
            LoadAnisotropicFilteringStuff();
            LoadAntiAliasingStuff();

            // load sharpening filter stuff
            IsSharpeningFilterActive = _igcs.IsSharpeningFilterActive();
        }
    }

    public void OnNavigatedFrom()
    {
        
    }
}
