namespace ArcticControl.Models;

public struct GamesSettingsParamerter
{
    /// <summary>
    /// False if editing global settings.
    /// </summary>
    public bool IsGame;

    // will be ignored if IsGame is set to false

    /// <summary>
    /// Does the app use global settings?
    /// </summary>
    public bool UseGlobal;
    /// <summary>
    /// The game as exe. For example Cyberpunk2077.exe
    /// </summary>
    public string GameExe;
}
