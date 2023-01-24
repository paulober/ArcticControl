using System.Diagnostics;
using ArcticControl.Contracts.Services;
using ArcticControl.Contracts.ViewModels;
using ArcticControl.Helpers;
using ArcticControl.Models;
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

    private Visibility _xessSupportedFlagSupportedText = Visibility.Collapsed;

    public Visibility XessSupportedFlagSupportedText
    {
        get => _xessSupportedFlagSupportedText;
        set => SetProperty(ref _xessSupportedFlagSupportedText, value);
    }

    private Visibility _xessSupportedFlagUnsupportedText = Visibility.Collapsed;

    public Visibility XessSupportedFlagUnsupportedText
    {
        get => _xessSupportedFlagUnsupportedText;
        set => SetProperty(ref _xessSupportedFlagUnsupportedText, value);
    }

    private void SetXessState(bool supported)
    {
        if (supported)
        {
            XessSupportedFlagSupportedText = Visibility.Visible;
            XessSupportedFlagUnsupportedText = Visibility.Collapsed;
        }
        else
        {
            XessSupportedFlagSupportedText = Visibility.Collapsed;
            XessSupportedFlagUnsupportedText = Visibility.Visible;
        }
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
            string? applicationName = null;
            if (_navigationParameter is { IsGame: true, InstalledGame.ExePath: { } })
            {
                applicationName = _navigationParameter.Value.InstalledGame?.ExePath;
            }
            
            _ = _igcs.SetGamingFlipMode(mfi.Text.ToGamingFlipMode(), applicationName);
            LoadTearingEffectMitigationStuff();
        }
    }

    public void LoadTearingEffectMitigationStuff()
    {
        if (TearingEffectMitigationDropdownBtn == null)
        {
            return;
        }

        string? applicationName = null;
        if (_navigationParameter is { IsGame: true, InstalledGame.ExePath: { } })
        {
            applicationName = _navigationParameter.Value.InstalledGame?.ExePath;
        }
        var gfm = _igcs.GetGamingFlipMode(applicationName);
        TearingEffectMitigationDropdownBtn.Content = gfm.ToReadableString();
    }

    public void AnisotropicFilteringFlyoutItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem mfi)
        {
            string? applicationName = null;
            if (_navigationParameter is { IsGame: true, InstalledGame.ExePath: { } })
            {
                applicationName = _navigationParameter.Value.InstalledGame?.ExePath;
            }
            _ = _igcs.SetAnisotropicFilteringMode(mfi.Text.ToAnisotropicFilteringMode(), applicationName);
            LoadAnisotropicFilteringStuff();
        }
    }

    public void LoadAnisotropicFilteringStuff()
    {
        if (AnisotropicFilteringDropdownBtn == null)
        {
            return;
        }

        string? applicationName = null;
        if (_navigationParameter is { IsGame: true, InstalledGame.ExePath: { } })
        {
            applicationName = _navigationParameter.Value.InstalledGame?.ExePath;
        }
        var afm = _igcs.GetAnisotropicFilteringMode(applicationName);
        AnisotropicFilteringDropdownBtn.Content = afm.ToReadableString();
    }

    public void AntiAliasingFlyoutItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem mfi)
        {
            string? applicationName = null;
            if (_navigationParameter is { IsGame: true, InstalledGame.ExePath: { } })
            {
                applicationName = _navigationParameter.Value.InstalledGame?.ExePath;
            }
            _ = _igcs.SetCmaaMode(mfi.Text.ToCmaaMode(), applicationName);
            LoadAntiAliasingStuff();
        }
    }
    
    public void LoadAntiAliasingStuff()
    {
        if (AntiAliasingDropdownBtn == null)
        {
            return;
        }

        string? applicationName = null;
        if (_navigationParameter is { IsGame: true, InstalledGame.ExePath: { } })
        {
            applicationName = _navigationParameter.Value.InstalledGame?.ExePath;
        }
        // TODO: return if is game but not exepath, because then this function should not run!
        var cm = _igcs.GetCmaaMode(applicationName);
        AntiAliasingDropdownBtn.Content = cm.ToReadableString();
    }
    
    public void SharpeningFilter_ToggleSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        SharpeningFilterSwitchEnabled = false;
        string? applicationName = null;
        if (_navigationParameter is { IsGame: true, InstalledGame.ExePath: { } })
        {
            applicationName = _navigationParameter.Value.InstalledGame?.ExePath;
        }
        if (!_igcs.SetSharpeningFilter(IsSharpeningFilterActive, applicationName))
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
                    
                    // enable input controls as now game-specific settings can be made
                    // TODO: enable when feature is supported
                    InputControlsEnabled = true;
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
                SetXessState(param.InstalledGame?.XeSS ?? false);

                GameDetailElementsVisibility = Visibility.Visible;
                
                // if no game-exe is provided GPUInterop cannot set or get game specific settings
                if (string.IsNullOrEmpty(param.InstalledGame?.ExePath))
                {
                    InputControlsEnabled = false;
                    //DisEnableAllInputs(false);
                }
                else
                {
                    // TODO: enable when feature supported
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
            string? applicationName = null;
            if (_navigationParameter is { IsGame: true, InstalledGame.ExePath: { } })
            {
                applicationName = _navigationParameter.Value.InstalledGame?.ExePath;
            }
            IsSharpeningFilterActive = _igcs.IsSharpeningFilterActive(applicationName);
        }
    }

    public void OnNavigatedFrom()
    {
        
    }
}
