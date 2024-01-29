using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ArcticControl.Models;

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
            control.ValueField.Text = ((PerformanceValueDataObject)e.NewValue).Value;
        }
    }
}
