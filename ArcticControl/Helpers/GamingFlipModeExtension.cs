using ArcticControl.Models;

namespace ArcticControl.Helpers;
public static class GamingFlipModeExtension
{
    public static string ToReadableString(this GamingFlipMode gfm)
    {
        return gfm switch
        {
            GamingFlipMode.AppDefault => "Application Choice",
            GamingFlipMode.VSyncOff => "VSync off",
            GamingFlipMode.VSync => "VSync",
            GamingFlipMode.SmoothSync => "Smooth Sync",
            GamingFlipMode.SpeedFrame => "Speed Sync",
            GamingFlipMode.CappedFPS => "Smart VSync",
            _ => "Unknown",
        };
    }

    public static GamingFlipMode ToGamingFlipMode(this string gfmStr)
    {
        return gfmStr switch
        {
            "Application Choice" => GamingFlipMode.AppDefault,
            "VSync off" => GamingFlipMode.VSyncOff,
            "VSync" => GamingFlipMode.VSync,
            "Smooth Sync" => GamingFlipMode.SmoothSync,
            "Speed Sync" => GamingFlipMode.SpeedFrame,
            "Smart VSync" => GamingFlipMode.CappedFPS,
            _ => GamingFlipMode.Unknown,
        };
    }
}
