// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using ArcticControl.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ArcticControl.Views;
/// <summary>
/// Startpage of the Application.
/// </summary>
public sealed partial class StartPage : Page
{
    public StartPageViewModel ViewModel
    {
        get;
    }

    public StartPage()
    {
        ViewModel = App.GetService<StartPageViewModel>();
        this.InitializeComponent();
    }
}
