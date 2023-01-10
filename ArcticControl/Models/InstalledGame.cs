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

    // ReSharper disable once InconsistentNaming
    public bool XeSS
    {
        get;
        set;
    } = false;

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

    // TODO: when supporting more game provider remove string.empty
    public string GetIdentifier() => !string.IsNullOrEmpty(SteamAppId) ? "Steam_" + SteamAppId : 
        (!string.IsNullOrEmpty(EpicGamesLaunchPath) ? "EpicGames_" + EpicGamesLaunchPath :string.Empty);

    public InstalledGame(
        string imagePath,
        bool xeSs = false,
        string exePath = null,
        string steamAppId = null,
        string epicGamesLaunchPath = null)
    {
        ImagePath = imagePath;
        this.XeSS = xeSs;
        SteamAppId = steamAppId;
        ExePath = exePath;
        EpicGamesLaunchPath = epicGamesLaunchPath;
    }
}
