using System.Collections.ObjectModel;
using System.Windows.Input;

using ArcticControl.Contracts.Services;
using ArcticControl.Contracts.ViewModels;
using ArcticControl.Core.Contracts.Services;
using ArcticControl.Helpers;
using ArcticControl.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace ArcticControl.ViewModels;

public class GamesViewModel : ObservableRecipient, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly ISampleDataService _sampleDataService;
    private readonly IGamesScannerService _gamesScannerService;

    public ICommand ItemClickCommand
    {
        get;
    }

    public ObservableCollection<InstalledGame> Source { get; } = new ObservableCollection<InstalledGame>();

    private bool _arcDriverInstalled = false;
    public bool ArcDriverInstalled
    {
        get => _arcDriverInstalled;
        set => SetProperty(ref _arcDriverInstalled, value);
    }
    
    public GamesViewModel(
        INavigationService navigationService, 
        ISampleDataService sampleDataService, 
        IGamesScannerService gamesScannerService)
    {
        _navigationService = navigationService;
        _sampleDataService = sampleDataService;
        _gamesScannerService = gamesScannerService;

        ItemClickCommand = new RelayCommand<InstalledGame>(OnItemClick);
        
        ArcDriverInstalled = InstalledDriverHelper.IsIntelGraphicsDriverInstalled();
    }

    public async void OnNavigatedTo(object parameter)
    {
        Source.Clear();

        var data = await Task.Run(() => _gamesScannerService.GetInstalledGames());

        foreach (var game in data)
        {
            Source.Add(game);
        }
    }

    public void OnNavigatedFrom()
    {
    }

    private void OnItemClick(InstalledGame? clickedItem)
    {
        if (clickedItem == null)
        {
            return;
        }

        _navigationService.SetListDataItemForNextConnectedAnimation(clickedItem);
        _navigationService.NavigateTo(typeof(GamesSettingsViewModel).FullName!, new GamesSettingsParameter
        {
            IsGame = true,
            ImagePath = clickedItem.ImagePath,
            InstalledGame = clickedItem
        });
    }

    public void InstalledGame_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        /*if (sender is Grid g)
        {
            g.RenderTransform = new CompositeTransform() { ScaleX = 1.1, ScaleY = 1.1 };
        } */
        if (sender is not Image img)
        {
            return;
        }

        //img.RenderTransform = new CompositeTransform() { ScaleX = 1.1, ScaleY = 1.1 };
        img.RenderTransformOrigin = new Windows.Foundation.Point(0.5,0.5);
        img.RenderTransform = new ScaleTransform() { ScaleX = 1.1, ScaleY = 1.1};
    }

    public void InstalledGame_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is not Image img)
        {
            return;
        }

        //img.RenderTransform = new CompositeTransform() { ScaleX = 1, ScaleY = 1 };
        img.RenderTransformOrigin = new Windows.Foundation.Point(0, 0);
        img.RenderTransform = new ScaleTransform() { ScaleX = 1, ScaleY = 1, CenterX = 1, CenterY = 1 };
    }
    
    public void GlobalSettingsButton_OnClick(object sender, RoutedEventArgs e)
    {
        _navigationService.NavigateTo(
            typeof(GamesSettingsViewModel).FullName!, 
            new GamesSettingsParameter{ IsGame = false});
    }
}
