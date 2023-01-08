using ArcticControl.Core.Models;

namespace ArcticControl.Models;

public struct GamesSettingsParameter
{
    /// <summary>
    /// False if editing global settings.
    /// </summary>
    public bool IsGame;

    // will be ignored if IsGame is set to false

    // <summary>
    // Does the app use global settings?
    // </summary>
    //public bool UseGlobal;

    /// <summary>
    /// Must be provided if IsGame == true!
    /// </summary>
    public string? ImagePath;

    public InstalledGame? InstalledGame;
}
