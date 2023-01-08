using System.Diagnostics;
using ArcticControl.Contracts.Services;
using ArcticControl.Contracts.ViewModels;
using ArcticControl.Core.Contracts.Services;
using ArcticControl.Core.Models;
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
    private readonly IGamesScannerService _gss;

    private GamesSettingsParameter? _navigationParameter;

    public InstalledGame? Item => _navigationParameter?.InstalledGame;

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

    // TODO: does for whatever reason not work to bind multiple (or a single) IsEnabled to this
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

    private string _imagePath = "/Assets/Square44x44Logo.targetsize-256.png";

    public string ImagePath
    {
        get => _imagePath;
        set => SetProperty(ref _imagePath, value);
    }

    private Visibility _gameDetailElementsVis = Visibility.Collapsed;

    public Visibility GameDetailElementsVisibility
    {
        get => _gameDetailElementsVis;
        set => SetProperty(ref _gameDetailElementsVis, value);
    }

    public GamesSettingsViewModel(
        IIntelGraphicsControlService graphicsControlService,
        IGamesScannerService gamesScannerService)
    {
        _igcs = graphicsControlService;
        _gss = gamesScannerService;
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
        if (InputControlsEnabled)
        {
            SharpeningFilterSwitchEnabled = true;
        }
    }
    
    public async void LaunchButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!_navigationParameter.HasValue)
        {
            return;
        }

        var isGame = _navigationParameter?.IsGame ?? false;
        var steamAppId = _navigationParameter?.InstalledGame?.SteamAppId;
        var epicGamesLaunchPath = _navigationParameter?.InstalledGame?.EpicGamesLaunchPath;
        if (isGame)
        {
            if (!string.IsNullOrEmpty(steamAppId))
            {
                var result = await Task.Run(() => 
                    _gss.LaunchSteamGame(steamAppId, _navigationParameter?.InstalledGame?.ExePath == null));
                if (!string.IsNullOrEmpty(result) && _navigationParameter?.InstalledGame != null)
                {
                    Debug.WriteLine("Found steam game ExeName: " + result);
                    _navigationParameter.Value.InstalledGame.ExePath = result;
                }
            }
            else if (!string.IsNullOrEmpty(epicGamesLaunchPath))
            {
                await Task.Run(() => _gss.LaunchEpicGames(epicGamesLaunchPath));
            }
        }
    }

    /*private void DisEnableAllInputs(bool enable)
    {
        if (AnisotropicFilteringDropdownBtn != null)
        {
            AnisotropicFilteringDropdownBtn.IsEnabled = enable;
        }
        if (AntiAliasingDropdownBtn != null)
        {
            AntiAliasingDropdownBtn.IsEnabled = enable;
        }
        if (TearingEffectMitigationDropdownBtn != null)
        {
            TearingEffectMitigationDropdownBtn.IsEnabled = enable;
        }

        SharpeningFilterSwitchEnabled = false;
    }*/

    public void OnNavigatedTo(object parameter)
    {
        if (parameter is GamesSettingsParameter param)
        {
            _navigationParameter = param;
            GameDetailElementsVisibility = Visibility.Visible;
            
            LoadTearingEffectMitigationStuff();
            LoadAnisotropicFilteringStuff();
            LoadAntiAliasingStuff();

            // load sharpening filter stuff
            IsSharpeningFilterActive = _igcs.IsSharpeningFilterActive();

            if (param.IsGame)
            {
                // TODO: maybe change
                // game detail view require image-path
                if (string.IsNullOrEmpty(param.InstalledGame?.ImagePath))
                {
                    throw new OperationCanceledException();
                }
                ImagePath = param.InstalledGame?.ImagePath!;

                GameDetailElementsVisibility = Visibility.Visible;
                
                // if no game-exe is provided GPUInterop cannot set or get game specific settings
                if (string.IsNullOrEmpty(param.InstalledGame?.ExePath))
                {
                    InputControlsEnabled = false;
                    //DisEnableAllInputs(false);
                }
                else
                {
                    InputControlsEnabled = true;
                    //DisEnableAllInputs(true);
                }
            }
            else
            {
                GameDetailElementsVisibility = Visibility.Collapsed;
                InputControlsEnabled = true;
                //DisEnableAllInputs(true);
            }
        }
        else
        {
            GameDetailElementsVisibility = Visibility.Collapsed;
            
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
