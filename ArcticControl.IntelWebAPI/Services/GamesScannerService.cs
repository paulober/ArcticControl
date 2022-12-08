using System.Diagnostics;
using System.Text.RegularExpressions;
using ArcticControl.Core.Models;
using ArcticControl.IntelWebAPI.Contracts.Services;

namespace ArcticControl.Services;
public class GamesScannerService : IGamesScannerService
{
    private const string STEAM_LIBRARIES_CONFIG_PATH = "C:\\Program Files (x86)\\Steam\\steamapps\\libraryfolders.vdf";
    private const string STEAM_LIBRARY_CACHE_PATH = "C:\\Program Files (x86)\\Steam\\appcache\\librarycache";

    private List<InstalledGame>? installedGames;

    private static IEnumerable<string> GetInstalledSteamAppIds()
    {
        try
        {
            var config = File.ReadAllText(STEAM_LIBRARIES_CONFIG_PATH);

            var appsRegex = new Regex("\"apps\"\\n.*?{\\n(?:.*?\"(?:\\d*?)\".*?\"(?:\\d*?)\"\\n)*.*?}");

            var appsLists = appsRegex.Matches(config);
            if (appsLists == null)
            {
                throw new Exception("Could not find installed steam apps");
            }

            var appIdRegex = new Regex("(?:.*?\"(?<appId>\\d*?)\".*?\"(?:\\d*?)\"\\n)");

            List<string> result = new();
            foreach (var apps in appsLists.Cast<Match>())
            {
                var appIds = appIdRegex.Matches(apps.Value);
                if (appIds == null)
                {
                    throw new Exception("Could not find installed steam apps");
                }
                result.AddRange(appIds.Select(appId => appId.Groups["appId"].Value));
            }

            return result;
        }
        catch (Exception ex)
        {
            Debug.Write("Could not load steam libraries!: ");
            Debug.WriteLine(ex.Message);
        }

        return Array.Empty<string>();
    }

    public IEnumerable<InstalledGame> GetInstalledSteamGames()
    {
        try
        {
            var steamAppIds = GetInstalledSteamAppIds();

            if (steamAppIds != null && steamAppIds.Any())
            {
                var result = new List<InstalledGame>();
                foreach (var appId in steamAppIds)
                {
                    var imagePath = Path.Combine(STEAM_LIBRARY_CACHE_PATH, appId + "_library_600x900.jpg");
                    if (File.Exists(imagePath))
                    {
                        result.Add(new InstalledGame("Name", imagePath, "version"));
                    }
                }

                return result;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error loading steam library: " + ex.Message);
        }

        return Array.Empty<InstalledGame>();
    }

    public IEnumerable<InstalledGame> GetInstalledGames(bool force = false)
    {
        if (installedGames == null || force)
        {
            installedGames = new List<InstalledGame>();

            var steamGames = GetInstalledSteamGames();
            // add other sources below

            installedGames.AddRange(steamGames);
        }

        return installedGames;
    }

    public GamesScannerService()
    {
    }
}
