using Microsoft.UI.Xaml.Data;

namespace ArcticControl.Helpers;
/// <summary>
/// Does convert the ArcDriverVersion.IsLatest bool to the glyph for update or normal
/// </summary>
internal class BoolToGlyphConverter : IValueConverter
{
    // TODO: check target type to avoid wrong usage of converter
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isLatest)
        {
            return BoolToGlyph(isLatest);
        }

        throw new ArgumentException("ExceptionBoolToGlyphConverterParameterMustBeABool");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => value.Equals("\xe898");

    private static string BoolToGlyph(bool isLatest)
    {
        return isLatest ? "\xe898" : "\xe74c" /* is equal to &#xe74c;*/;
    }
}
