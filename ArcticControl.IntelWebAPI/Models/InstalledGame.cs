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

    public InstalledGame(string name, string imagePath, string? version)
    {
        Name = name;
        Version = version;
        ImagePath = imagePath;
    }
}
