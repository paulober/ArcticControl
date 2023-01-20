using ArcticControl.ViewModels;

using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

namespace ArcticControl.Views;

// To learn more about WebView2, see https://docs.microsoft.com/microsoft-edge/webview2/.
public sealed partial class NewsPage : Page
{
    public NewsViewModel ViewModel
    {
        get;
    }

    public NewsPage()
    {
        ViewModel = App.GetService<NewsViewModel>();
        InitializeComponent();

        ViewModel.WebViewService.Initialize(WebView);
    }

    /*
    private async void WebView_OnNavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
        await sender.ExecuteScriptAsync("const elm = document.querySelector('.elementor-element-7980e40'); " +
                                           "elm.style.visibility = 'hidden';");
    }*/
}
