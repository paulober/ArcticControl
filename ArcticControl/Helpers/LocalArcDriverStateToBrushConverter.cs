using ArcticControl.IntelWebAPI.Models;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace ArcticControl.Helpers;
internal class LocalArcDriverStateToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value != null && value is LocalArcDriverState localState)
        {
            return LocalStateToBrush(localState);
        }

        throw new ArgumentException("ExceptionLocalArcDriverStateToBrushConverterParameterMustBeAnLocalArcDriverState");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }

    public static Brush LocalStateToBrush(LocalArcDriverState localState)
    {
        return localState switch
        {
            LocalArcDriverState.Old => new SolidColorBrush(Microsoft.UI.Colors.DarkOrange),
            LocalArcDriverState.New => new SolidColorBrush(Microsoft.UI.Colors.DarkSlateBlue),
            LocalArcDriverState.Current => new SolidColorBrush(Microsoft.UI.Colors.Chartreuse),
            _ => new SolidColorBrush(Microsoft.UI.Colors.Cyan),
        };
    }
}
