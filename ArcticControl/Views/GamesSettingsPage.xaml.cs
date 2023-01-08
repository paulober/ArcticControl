using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using ArcticControl.Contracts.Services;
using ArcticControl.ViewModels;

using CommunityToolkit.WinUI.UI.Animations;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ArcticControl.Views;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class GamesSettingsPage : Page
{
    public GamesSettingsViewModel ViewModel
    {
        get;
    }
    
    public GamesSettingsPage()
    {
        ViewModel = App.GetService<GamesSettingsViewModel>();
        
        // Load UI
        this.InitializeComponent();
        
        // Bind UI elements
        ViewModel.TearingEffectMitigationMenuFlyout = TearingEffectMitigationMenuFlyout;
        ViewModel.TearingEffectMitigationDropdownBtn = TearingEffectMitigationDropdownBtn;
        ViewModel.AnisotropicFilteringMenuFlyout = AnisotropicFilteringMenuFlyout;
        ViewModel.AnisotropicFilteringDropdownBtn = AnisotropicFilteringDropdownBtn;
        ViewModel.AntiAliasingMenuFlyout = AntiAliasingMenuFlyout;
        ViewModel.AntiAliasingDropdownBtn = AntiAliasingDropdownBtn;
        
        // Loading stuff - does onNavigatedTo in viewmodel do
        //ViewModel.LoadTearingEffectMitigationStuff();
        //ViewModel.LoadAnisotropicFilteringStuff();
        //ViewModel.LoadAntiAliasingStuff();
    }
    
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        this.RegisterElementForConnectedAnimation("animationKeyContentGrid", GameDetailImage);
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);
        if (e.NavigationMode == NavigationMode.Back)
        {
            var navigationService = App.GetService<INavigationService>();

            if (ViewModel.Item != null)
            {
                // TODO: implement animation properly like the GameDetails page does
                //navigationService.SetListDataItemForNextConnectedAnimation(ViewModel.Item);
            }
        }
    }
}
