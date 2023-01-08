using ArcticControlGPUInterop;

namespace ArcticControl.Helpers;

public static class AnisotropicFilteringModeExtension
{
    public static string ToReadableString(this AnisotropicFilteringMode afm)
    {
        return afm switch
        {
            AnisotropicFilteringMode.AppChoice => "Application Choice",
            AnisotropicFilteringMode.TwoX => "2x",
            AnisotropicFilteringMode.FourX => "4x",
            AnisotropicFilteringMode.EightX => "8x",
            AnisotropicFilteringMode.SixteenX => "16x",
            _ => "Unknown",
        };
    }

    public static AnisotropicFilteringMode ToAnisotropicFilteringMode(this string gfmStr)
    {
        return gfmStr switch
        {
            "Application Choice" => AnisotropicFilteringMode.AppChoice,
            "2x" => AnisotropicFilteringMode.TwoX,
            "4x" => AnisotropicFilteringMode.FourX,
            "8x" => AnisotropicFilteringMode.EightX,
            "16x" => AnisotropicFilteringMode.SixteenX,
            _ => AnisotropicFilteringMode.Unknown,
        };
    }
}