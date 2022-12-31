namespace ArcticControl.Helpers;
public static class StringExtension
{
    /// <summary>
    /// Does truncate string to max length.
    /// </summary>
    /// <returns>If string is null then returns string.Empty.</returns>
    public static string Truncate(this string? value, int maxLength, string truncationSuffix = "")
    {
        return value?.Length > maxLength
            ? value[..maxLength] + truncationSuffix
            : value ?? string.Empty;
    }
}
