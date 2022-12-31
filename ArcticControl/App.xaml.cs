﻿using ArcticControl.Activation;
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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;

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
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // add logging
            services.AddLogging();

            // Http client factory
            // HttpClient is intended to be instantiated once per application, rather than per-use. See Remarks.
            // https://learn.microsoft.com/de-de/dotnet/api/system.net.http.httpclient?view=net-6.0#remarks
            services.AddHttpClient("Intel", client =>
            {
                client.BaseAddress = new Uri("https://www.intel.com/");
            });

            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers
            services.AddTransient<IActivationHandler, AppNotificationActivationHandler>();

            // Services
            services.AddSingleton<IAppNotificationService, AppNotificationService>();
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<IWebViewService, WebViewService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Core Services
            services.AddSingleton<ISampleDataService, SampleDataService>();
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<IGamesScannerService, GamesScannerService>();

            // Intel Web Api Service
            services.AddSingleton<IWebArcDriversService, WebArcDriversService>();

            // Views and ViewModels
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SettingsPage>();
            services.AddTransient<NewsViewModel>();
            services.AddTransient<NewsPage>();
            services.AddTransient<PerformanceViewModel>();
            services.AddTransient<PerformancePage>();
            services.AddTransient<GamesViewModel>();
            services.AddTransient<GamesPage>();
            services.AddTransient<GamesDetailViewModel>();
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

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        App.GetService<IAppNotificationService>().Show(string.Format("AppNotificationSamplePayload".GetLocalized(), AppContext.BaseDirectory));

        await App.GetService<IActivationService>().ActivateAsync(args);
    }
}
