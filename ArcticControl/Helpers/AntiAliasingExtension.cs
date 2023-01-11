using ArcticControl.Models;

namespace ArcticControl.Helpers;

public static class AntiAliasingExtension
{
    public static string ToReadableString(this CmaaMode afm)
    {
        return afm switch
        {
            CmaaMode.TurnOff => "Force Off",
            CmaaMode.OverrideMsaa => "Force On",
            CmaaMode.EnhanceApplication => "Enhance Application",
            _ => "Unknown",
        };
    }

    public static CmaaMode ToCmaaMode(this string gfmStr)
    {
        return gfmStr switch
        {
            "Force Off" => CmaaMode.TurnOff,
            "Force On" => CmaaMode.OverrideMsaa,
            "Enhance Application" => CmaaMode.EnhanceApplication,
            _ => CmaaMode.Unknown,
        };
    }
}