// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using ArcticControl.Core.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ArcticControl.Views;

public sealed partial class PerformanceValueTemplate : UserControl
{
    public PerformanceValueDataObject Obj
    {
        get => (PerformanceValueDataObject)GetValue(PerformanceValueDataObjProperty);
        set => SetValue(PerformanceValueDataObjProperty, value);
    }

    public static readonly DependencyProperty PerformanceValueDataObjProperty 
        = DependencyProperty.Register(
            "Obj", 
            typeof(PerformanceValueDataObject), 
            typeof(PerformanceValueTemplate), 
            new PropertyMetadata(null, ObjPropertyChanged));

    public PerformanceValueTemplate()
    {
        this.InitializeComponent();
        DataContext = this;
    }

    private static void ObjPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PerformanceValueTemplate control)
        {
            control.ForegroundElement.UpdateLayout();
            control.ValueField.Text = (e.NewValue as PerformanceValueDataObject).Value;
        }
    }
}
