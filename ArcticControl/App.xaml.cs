using System.Diagnostics;
using ArcticControl.Activation;
using ArcticControl.Contracts.Services;
using ArcticControl.Core.Contracts.Services;
using ArcticControl.Core.Services;
using ArcticControl.Helpers;
using ArcticControl.IntelWebAPI.Contracts.Services;
using ArcticControl.IntelWebAPI.Services;
using ArcticControl.Models;
using ArcticControl.Notifications;
using ArcticControl.Services;
using ArcticControl.ViewModels;
using ArcticControl.Views;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.Windows.ApplicationModel.WindowsAppRuntime;
using Windows.Globalization;

namespace ArcticControl;

public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host { get; }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public App()
    {
        if (DeploymentManager.GetStatus().Status != DeploymentStatus.Ok 
            || SystemInformation.Instance.IsFirstRun 
            || SystemInformation.Instance.IsAppUpdated)
        {
            Debug.WriteLine("[App]: DeploymentManager status: " + DeploymentManager.GetStatus().Status);
            var initializeTask = Task.Run(DeploymentManager.Initialize);
            initializeTask.Wait();

            if (initializeTask.Result.Status == DeploymentStatus.Ok)
            {
                Debug.WriteLine("Installed Windows App Runtime successfully!");
            }
        }

        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureAppConfiguration(appConfig => 
        {
            appConfig.AddUserSecrets<App>();
        }).
        ConfigureServices((context, services) =>
        {
            // add logging
            services.AddLogging();
            
            // place for AppCenter sdk configuration
            AppCenter.SetCountryCode(new GeographicRegion().CodeTwoLetter);
            AppCenter.Configure(context.Configuration["AppCenterSecret"]);

            // Http client factory
            // HttpClient is intended to be instantiated once per application, rather than per-use. See Remarks.
            // https://learn.microsoft.com/de-de/dotnet/api/system.net.http.httpclient?view=net-6.0#remarks
            services.AddHttpClient("Intel", client =>
            {
                client.BaseAddress = new Uri("https://www.intel.com/");
            });
            services.AddHttpClient("AppRepository", client =>
            {
                client.BaseAddress = new Uri("https://arcticcontrol.paulober.dev/app/");
            });

            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers
            services.AddTransient<IActivationHandler, AppNotificationActivationHandler>();

            // Services
            services.AddSingleton<IAppNotificationService, AppNotificationService>();
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddSingleton<IPrivacyConsentDisplayService, PrivacyConsentDisplayService>();
            services.AddSingleton<IUpdateAvailableDisplayService, UpdateAvailableDisplayService>();
            services.AddTransient<IWebViewService, WebViewService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Core Services
            services.AddSingleton<ISampleDataService, SampleDataService>();
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<IGamesScannerService, GamesScannerService>();

            // Intel Services
            if (InstalledDriverHelper.IsIntelGraphicsDriverInstalled())
            {
                services.AddSingleton<IIntelGraphicsControlService, IntelGraphicsControlService>();
            }
            else
            {
                services.AddSingleton<IIntelGraphicsControlService, DummyIntelGraphicsControlService>();
            }
            services.AddSingleton<IWebArcDriversService, WebArcDriversService>();

            // Views and ViewModels (changes here must be reflected in PagesService.cs)
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SettingsPage>();
            services.AddTransient<NewsViewModel>();
            services.AddTransient<NewsPage>();
            services.AddTransient<PerformanceViewModel>();
            services.AddTransient<PerformancePage>();
            services.AddTransient<GamesViewModel>();
            services.AddTransient<GamesPage>();
            services.AddTransient<GamesDetailViewModel>();
            services.AddTransient<GamesSettingsViewModel>();
            services.AddTransient<DriversViewModel>();
            services.AddTransient<DriversPage>();
            services.AddTransient<GamesDetailPage>();
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();
            services.AddTransient<StartPageViewModel>();
            services.AddTransient<StartPage>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();

        App.GetService<IAppNotificationService>().Initialize();

        UnhandledException += App_UnhandledException;
    }

    private async void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        if (await Crashes.IsEnabledAsync())
        {
            Crashes.TrackError(e.Exception);
        }
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        // App.GetService<IAppNotificationService>().Show(string.Format("AppNotificationSamplePayload".GetLocalized(), AppContext.BaseDirectory));

        await App.GetService<IActivationService>().ActivateAsync(args);
    }
}
