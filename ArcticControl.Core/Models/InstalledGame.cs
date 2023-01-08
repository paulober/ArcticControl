namespace ArcticControl.Core.Models;
public class InstalledGame
{
    public string Name
    {
        get; set;
    }

    public string? Version
    {
        get; set; 
    }

    public string ImagePath
    {
        get; set;
    }

    public string? ExePath
    {
        get;
        set;
    }

    public InstalledGame(string name, string imagePath, string? version = null, string? exePath = null)
    {
        Name = name;
        Version = version;
        ImagePath = imagePath;
        ExePath = exePath;
    }
}
