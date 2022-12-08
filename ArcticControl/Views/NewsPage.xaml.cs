using ArcticControl.ViewModels;

using Microsoft.UI.Xaml.Controls;

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
}
