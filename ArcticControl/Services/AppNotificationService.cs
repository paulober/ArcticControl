using System.Collections.Specialized;
using System.Web;

using ArcticControl.Contracts.Services;
using ArcticControl.ViewModels;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using WinRT.Interop;

namespace ArcticControl.Notifications;

public class AppNotificationService : IAppNotificationService
{
    private readonly INavigationService _navigationService;

    public AppNotificationService(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    ~AppNotificationService()
    {
        Unregister();
    }

    public void Initialize()
    {
        AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;

        AppNotificationManager.Default.Register();
    }

    public void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
    {
        // TODO: Handle notification invocations when your app is already running.

        // Navigate to a specific page based on the notification arguments.
        if (ParseArguments(args.Argument)["action"] == "Settings")
        {
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                _navigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
            });
        }

        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            App.MainWindow.ShowMessageDialogAsync("TODO: Handle notification invocations when your app is already running.", "Notification Invoked");

            App.MainWindow.BringToFront();
        });
    }

    /// <summary>
    /// !IMPORTANT! Does accept toast xml as payload not messages. Use ShowMessage to show messages instead.
    /// </summary>
    /// <param name="payload">Toast/App notification xml.</param>
    /// <returns></returns>
    public bool Show(string payload)
    {
        var appNotification = new AppNotification(payload);

        AppNotificationManager.Default.Show(appNotification);

        return appNotification.Id != 0;
    }

    public bool ShowMessage(string message)
    {
        var appNotification = new AppNotificationBuilder()
            .AddText(message)
            .BuildNotification();

        AppNotificationManager.Default.Show(appNotification);

        return appNotification.Id != 0;
    }

    public bool ShowWithActionAndProgressBar(string payload, string filePath, string status, string title, double pbValue, string valueStringOverride)
    {
        var appNotification = new AppNotificationBuilder()
            .AddButton(new AppNotificationButton("Install").SetInvokeUri(new Uri(string.Concat("file://", filePath))))
            .AddButton(new AppNotificationButton("Open in explorer").SetInvokeUri(new Uri(string.Concat("file://", filePath.AsSpan(0, filePath.LastIndexOf("/"))))))
            .AddText(payload)
            .AddProgressBar(new AppNotificationProgressBar()
            {
                Status = status,
                Title = title,
                Value = pbValue,
                ValueStringOverride = valueStringOverride
            }).BuildNotification();


        AppNotificationManager.Default.Show(appNotification);

        return appNotification.Id != 0;
    }

    public NameValueCollection ParseArguments(string arguments)
    {
        return HttpUtility.ParseQueryString(arguments);
    }

    public void Unregister()
    {
        AppNotificationManager.Default.Unregister();
    }
}
