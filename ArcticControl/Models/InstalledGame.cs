namespace ArcticControl.Models;
public class InstalledGame
{

    public string ImagePath
    {
        get; set;
    }

    public string? ExePath
    {
        get;
        set;
    }

    public string? SteamAppId
    {
        get;
        set;
    }

    public string? EpicGamesLaunchPath
    {
        get;
        set;
    }

    // TODO: when supporting more game provider reove string.empty
    public string GetIdenitfier() => !string.IsNullOrEmpty(SteamAppId) ? "Steam" + SteamAppId : string.Empty;

    public InstalledGame(
        string imagePath,
        string exePath = null,
        string steamAppId = null,
        string epicGamesLaunchPath = null)
    {
        ImagePath = imagePath;
        SteamAppId = steamAppId;
        ExePath = exePath;
        EpicGamesLaunchPath = epicGamesLaunchPath;
    }
}
