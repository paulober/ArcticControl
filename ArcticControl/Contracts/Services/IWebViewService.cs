﻿using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

namespace ArcticControl.Contracts.Services;

public interface IWebViewService
{
    Uri? Source
    {
        get;
    }

    bool CanGoBack
    {
        get;
    }

    bool CanGoForward
    {
        get;
    }

    event EventHandler<CoreWebView2WebErrorStatus>? NavigationCompleted;

    void Initialize(WebView2 webView);

    void GoHome();
    
    void GoBack();

    void GoForward();

    void Reload();

    void UnregisterEvents();
}
