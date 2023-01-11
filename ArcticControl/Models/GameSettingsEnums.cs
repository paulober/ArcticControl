namespace ArcticControl.Models;
public enum GamingFlipMode : uint
{
    /// <summary>
    /// Application Default
    /// </summary>
    AppDefault = 1,
    /// <summary>
    /// Convert all sync flips to async on the next possible scanline.
    /// </summary>
    VSyncOff = 2,
    /// <summary>
    /// Convert all async flips to sync flips.
    /// </summary>
    VSync = 4,
    /// <summary>
    /// Reduce tearing effect with async flips
    /// </summary>
    SmoothSync = 8,
    /// <summary>
    /// Application unaware triple buffering (Speed Sync). Not avail. on Alchemist dGPUs.
    /// </summary>
    SpeedFrame = 16,
    /// <summary>
    /// Limit the game FPS to panel RR(Rendering-Rate). Does automaticly switch between VSync and VSyncOFF. (Smart VSync)
    /// </summary>
    CappedFPS = 32,
    Unknown = 0x80000000
};

public enum AnisotropicFilteringMode : uint
{
    // values do have parity with ctl_3d_anisotropic_types_t

    /// <summary>
    /// Application choice.
    /// </summary>
    AppChoice = 0,
    TwoX = 2,
    FourX = 4,
    EightX = 8,
    SixteenX = 16,
    // equivalent to MAX
    Unknown = 17
};

public enum CmaaMode : uint
{
    // values do have parity with ctl_3d_cmaa_types_t

    TurnOff = 0,
    OverrideMsaa = 1,
    EnhanceApplication = 2,
    // equivalent to MAX
    Unknown = 3
};
