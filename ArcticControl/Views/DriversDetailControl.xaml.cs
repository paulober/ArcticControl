using System.Diagnostics;
using ArcticControl.Contracts.Services;
using ArcticControl.Helpers;
using ArcticControl.IntelWebAPI.Models;
using ArcticControl.Services;
using ArcticControl.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Storage;

namespace ArcticControl.Views;

public sealed partial class DriversDetailControl : UserControl
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAppNotificationService _appNotificationService;
    private readonly INavigationService _navigationService;

    public WebArcDriver? ListDetailsMenuItem
    {
        get => GetValue(ListDetailsMenuItemProperty) as WebArcDriver;
        set => SetValue(ListDetailsMenuItemProperty, value);
    }

    public static readonly DependencyProperty ListDetailsMenuItemProperty = DependencyProperty.Register("ListDetailsMenuItem", typeof(WebArcDriver), typeof(DriversDetailControl), new PropertyMetadata(null, OnListDetailsMenuItemPropertyChanged));

    public DriversDetailControl()
    {
        _httpClientFactory = App.GetService<IHttpClientFactory>();
        _appNotificationService = App.GetService<IAppNotificationService>();
        _navigationService= App.GetService<INavigationService>();
        InitializeComponent();
    }

    private static void OnListDetailsMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DriversDetailControl control)
        {
            control.ForegroundElement.ChangeView(0, 0, 1);
            if (e.NewValue != null)
            {
                control.NoDriverSelectedPane.Visibility = Visibility.Collapsed;
                control.DriverDetailsPane.Visibility = Visibility.Visible;

                control.DownloadProgressBar.IsIndeterminate = true;
                control.DownloadProgressBar.ShowPaused = false;
                control.DownloadProgressBar.ShowError = false;

                if (e.NewValue is WebArcDriver wac)
                {
                    if (wac.DriverVersion.LocalState == LocalArcDriverState.Current)
                    {
                        /*control.DownloadDriverButton.Background = new SolidColorBrush(Microsoft.UI.Colors.Green);
                        control.DownloadDriverButton.Content = "Installed";*/

                        control.DownloadButtonDefaultContent.Visibility = Visibility.Collapsed;
                        control.DownloadButtonInstalledText.Visibility = Visibility.Visible;
                        control.DownloadDriverButton.IsEnabled = false;
                    }
                    else
                    {
                        control.DownloadButtonDefaultContent.Visibility = Visibility.Visible;
                        control.DownloadButtonInstalledText.Visibility = Visibility.Collapsed;
                        control.DownloadDriverButton.IsEnabled = true;
                    }

                    // (causes commexception if done in xaml) <- does now work in xaml
                    // but leave commented out just to have a quick fix when something changes
                    //control.DriverIcon.Foreground = LocalArcDriverStateToBrushConverter.LocalStateToBrush(wac.DriverVersion.LocalState);
                }
            } 
            else
            {
                control.DriverDetailsPane.Visibility = Visibility.Collapsed;
                control.NoDriverSelectedPane.Visibility = Visibility.Visible;
            }
        }
    }

    private async void DownloadDriverButton_Click(object sender, RoutedEventArgs e)
    {
        // TODO: move code into singleton service to don't interrupt on page change but sync with button progressbar or notification from service
        try
        {
            using var client = _httpClientFactory.CreateClient();

            DownloadProgressBar.Visibility = Visibility.Visible;
            var downloadStream = await client.GetStreamAsync(ListDetailsMenuItem?.DownloadUri);

            var localFile = await DownloadsFolder.CreateFileAsync(
                ListDetailsMenuItem?.DownloadUri?.Segments.Last());

            // write
            using var fsStream = await localFile.OpenStreamForWriteAsync();
            await downloadStream.CopyToAsync(fsStream);

            DownloadProgressBar.Visibility = Visibility.Collapsed;

            _appNotificationService.ShowWithActionAndProgressBar(
                "Intel® Graphics Driver " + ListDetailsMenuItem?.DriverVersion,
                localFile.Path.Replace('\\', '/'),
                "Finished",
                localFile.DisplayName, 
                1.0,
                "100%");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error downloading driver: " + ex.Message);
            DownloadProgressBar.ShowError = true;

            _appNotificationService.ShowMessage("Download failed! File may already exits...");
        }
    }

    private void ReleaseNotesButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is HyperlinkButton hlB)
        {
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                _navigationService.NavigateTo(typeof(NewsViewModel).FullName!, ListDetailsMenuItem?.ReleaseNotesUri);
            });
        }
    }
}
