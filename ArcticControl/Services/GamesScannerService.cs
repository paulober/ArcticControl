using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using Windows.Storage;
using Windows.Storage.Search;
using ArcticControl.Models;
using ArcticControl.Contracts.Services;
using Newtonsoft.Json;
using Icon = System.Drawing.Icon;

namespace ArcticControl.Services;
public partial class GamesScannerService : IGamesScannerService
{
    private const string STEAM_LIBRARIES_CONFIG_PATH = "C:\\Program Files (x86)\\Steam\\steamapps\\libraryfolders.vdf";
    private const string STEAM_LIBRARY_CACHE_PATH = "C:\\Program Files (x86)\\Steam\\appcache\\librarycache";
    private const string EPICGAMES_MANIFESTS_PATH = "C:\\ProgramData\\Epic\\EpicGamesLauncher\\Data\\Manifests";

    private List<InstalledGame>? _installedGames;

    private async Task<List<InstalledGame>> AlternativeSteamScannerAsync()
    {
        try
        {
            var config = await File.ReadAllTextAsync(STEAM_LIBRARIES_CONFIG_PATH);

            var libraryPathsRegex = SteamLibraryPathsRegex();
            var libraryPaths = libraryPathsRegex.Matches(config);
            if (libraryPaths.Any())
            {
                var result = new List<InstalledGame>();
                
                foreach (Match libPath in libraryPaths)
                {
                    var libraryFolder =
                        await StorageFolder.GetFolderFromPathAsync(Path.Combine(libPath.Groups["path"].Value.Replace("\\\\", "\\"),
                            "steamapps"));

                    QueryOptions options = new(CommonFileQuery.DefaultQuery, new[] { ".acf" })
                    {
                        FolderDepth = FolderDepth.Shallow
                    };
                    var filesInLib = await libraryFolder.CreateFileQueryWithOptions(options).GetFilesAsync();
                    var manifestFiles = filesInLib.Where(f => f.Name.Contains("appmanifest_"));

                    foreach (var manifest in manifestFiles)
                    {
                        try
                        {
                            var appIdRegex = AppIdRegex();
                            var installDirRegex = InstallDirRegex();

                            var manifestContent = await FileIO.ReadTextAsync(manifest);

                            var appId = appIdRegex.Match(manifestContent).Groups["appId"].Value;
                            var installDir = installDirRegex.Match(manifestContent).Groups["installdir"].Value;

                            var gameFolder = await (await libraryFolder.GetFolderAsync("common")).GetFolderAsync(installDir);
                            var xessSupported = await DoesGameSupportXeSsAsync(gameFolder.Path);

                            var imagePath = Path.Combine(STEAM_LIBRARY_CACHE_PATH, appId + "_library_600x900.jpg");
                            if (File.Exists(imagePath))
                            {
                                result.Add(new InstalledGame(imagePath, steamAppId: appId, xeSs: xessSupported));
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("[GameScannerService]: Could not load 1 steam game: " + ex.Message);
                        }
                    }
                }

                return result;
            }
        }
        catch (Exception ex)
        {
            Debug.Write("Could not load steam libraries!: " + ex.Message);
        }
        return new List<InstalledGame>();
    }
    
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

    private IEnumerable<InstalledGame> GetInstalledSteamGames()
    {
        try
        {
            var steamAppIds = GetInstalledSteamAppIds().ToArray();

            if (steamAppIds.Any())
            {
                /*return (from appId in steamAppIds 
                    select Path.Combine(STEAM_LIBRARY_CACHE_PATH, appId + "_library_600x900.jpg") 
                    into imagePath where File.Exists(imagePath) 
                    select new InstalledGame("Name", imagePath, "version")).ToList();*/
                var result = new List<InstalledGame>();
                foreach (var appId in steamAppIds)
                {
                    var imagePath = Path.Combine(STEAM_LIBRARY_CACHE_PATH, appId + "_library_600x900.jpg");
                    if (File.Exists(imagePath))
                    {
                        result.Add(new InstalledGame(imagePath, steamAppId: appId));
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

    private Bitmap PngFromIcon(Icon icon)
    {
        return Bitmap.FromHicon(icon.Handle);
    }

    private async Task<string> CacheIconForEpicGamesGame(string exePath)
    {
        var icon = Icon.ExtractAssociatedIcon(exePath);

        if (icon != null)
        {
            var imagesCacheFolder =
                await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("EpicGamesImagesCache", CreationCollisionOption.OpenIfExists);
            var pngFile = await imagesCacheFolder.CreateFileAsync(Path.GetFileNameWithoutExtension(exePath) + ".bmp", CreationCollisionOption.ReplaceExisting);
            await using var writeStream = await pngFile.OpenStreamForWriteAsync();
            PngFromIcon(icon).Save(writeStream, ImageFormat.Bmp);

            return pngFile.Path;
        }

        return string.Empty;
    }

    private async Task<bool> DoesGameSupportXeSsAsync(string installLocation)
    {
        /*var xessDlls = 0;

        QueryOptions qOpts = new(CommonFileQuery.DefaultQuery, new[] { ".dll" })
        {
            // for performance reasons but could be changed to deep if games don't get detected
            // as XeSS utilizing games
            FolderDepth = FolderDepth.Shallow
        };
        var dllsQuerry = gameDirectory.CreateFileQueryWithOptions(qOpts);
        var dlls = await dllsQuerry.GetFilesAsync();
        if (dlls.Count < 3)
        {
            qOpts.FolderDepth = FolderDepth.Deep;
            dllsQuerry.ApplyNewQueryOptions(qOpts);
            dlls = await dllsQuerry.GetFilesAsync();
        }

        if (dlls != null)
        {
            // TODO: maybe get xess version by libxess.dll <- FileVersionInfo.GetVersionInfo()
            xessDlls = dlls.Count(dll => XeSsRegex().IsMatch(dll.Name));
        }*/
        var xessDllsCount = Directory.GetFiles(installLocation, "*xess.dll", SearchOption.AllDirectories).Length;

        await Task.CompletedTask;
        return xessDllsCount > 0;
    }

    private async Task<IEnumerable<InstalledGame>> GetInstalledEpicGames()
    {
        try
        {
            /*return (
                from file in Directory.EnumerateFiles(EPICGAMES_MANIFESTS_PATH, "*.item") 
                select File.ReadAllText(file) into content 
                select JsonConvert.DeserializeObject<EpicGamesItem>(content) into item 
                where item.LaunchExecutable.Length >= 2 
                let imagePath = await CacheIconForEpicGamesGame(Path.Combine(item.InstallLocation, item.LaunchExecutable)) 
                select new InstalledGame(imagePath, exePath: item.LaunchExecutable)).ToList();*/
            var result = new List<InstalledGame>();

            foreach (var file in Directory.EnumerateFiles(EPICGAMES_MANIFESTS_PATH, "*.item"))
            {
                var content = await File.ReadAllTextAsync(file);
                var item = JsonConvert.DeserializeObject<EpicGamesItem>(content);

                // skip dlcs and stuff like that
                if (item.LaunchExecutable.Length < 2)
                {
                    continue;
                }

                // icon
                var imagePath =
                    await CacheIconForEpicGamesGame(Path.Combine(item.InstallLocation, item.LaunchExecutable));
                
                // XeSS support
                var xeSsSupported = false;
                // Path.getfullpath to normalize the paths provided by the epicgames items which contains als kinds of slashes
                var gameDirectory = await StorageFolder.GetFolderFromPathAsync(Path.GetFullPath(item.InstallLocation));
                if (gameDirectory != null)
                {
                    xeSsSupported = await DoesGameSupportXeSsAsync(gameDirectory.Path);
                }
                
                result.Add(new InstalledGame(
                    imagePath,
                    xeSs: xeSsSupported,
                    exePath: item.LaunchExecutable, 
                    epicGamesLaunchPath: item.MainGameCatalogNamespace
                                         +"%3A"+item.MainGameCatalogItemId
                                         +"%3A"+item.MainGameAppName));
            }

            return result;
        }
        catch (Exception ex)
        {
            Debug.Write("Could not load epic games items!: ");
            Debug.WriteLine(ex.Message);
        }

        return Array.Empty<InstalledGame>();
    }

    public async Task<IEnumerable<InstalledGame>> GetInstalledGames(bool force = false)
    {
        if (_installedGames == null || force)
        {
            _installedGames = new List<InstalledGame>();

            //var steamGames = GetInstalledSteamGames();
            var steamGames = await AlternativeSteamScannerAsync();
            var epicGames = await GetInstalledEpicGames();

            _installedGames.AddRange(steamGames);
            _installedGames.AddRange(epicGames);
        }

        return _installedGames;
    }

    public GamesScannerService()
    {
    }

    /// <summary>
    /// Launch a steam game and get exe name.
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="searchExe"></param>
    /// <returns></returns>
    public async Task<string> LaunchSteamGame(string appId, bool searchExe = false)
    {
        if (string.IsNullOrEmpty(appId) || !appId.All(char.IsDigit))
        {
            await Task.CompletedTask;
            return string.Empty;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "steam://rungameid/" + appId,
                UseShellExecute = true
            });

            if (searchExe)
            {
                Thread.Sleep(8000);

                // very slow!!
                var procs = Process.GetProcesses().Where(p =>
                {
                    if (p.ProcessName.Contains("steam"))
                    {
                        return false;
                    }

                    // fast test
                    /*if (!p.ProcessName.Contains("Raft"))
                    {
                        return false;
                    }*/

                    try
                    {
                        foreach (ProcessModule module in p.Modules)
                        {
                            if (module.ModuleName.Contains("steam_api"))
                            {
                                return true;
                            }
                        }
                    }
                    catch (Win32Exception)
                    {
                        Debug.WriteLine("Could not enum modules for: " + p.ProcessName);
                    }
                    catch (InvalidOperationException)
                    {
                        Debug.WriteLine("Could not enum modules for: " + p.ProcessName);
                    }

                    return false;
                }).ToList();
                if (procs.Any())
                {
                    await Task.CompletedTask;
                    return procs[0].MainModule?.ModuleName ?? string.Empty;
                }

                /* slower thant first approach
                var processes = Process.GetProcessesByName("steam").Where(p =>
                {
                    try
                    {
                        if (p.MainModule.ModuleName == "steam.exe")
                        {
                            return true;
                        }
                    }
                    catch (Win32Exception ex)
                    {
                        return false;
                    }
                    return false;
                }).ToArray();

                if (processes.Length == 1)
                {
                    // found steam proc which launches the games
                    var steamPid = processes.First().Id;
                    string procName = string.Empty;

                    foreach (var proc in Process.GetProcesses())
                    {
                        using (var obj = new ManagementObject($"win32_process.handle='{proc.Id}'"))
                        {
                            try
                            {
                                obj.Get();
                                int parentPid = Convert.ToInt32(obj["ParentProcessId"]);
                                if (parentPid == steamPid && !proc.MainModule.ModuleName.Contains("steam"))
                                {
                                    Debug.WriteLine("Game: " + proc.MainModule.ModuleName + " Pid: " + proc.Id);
                                    procName = proc.MainModule.ModuleName;
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("[GameScannerService]: Error searching for game!");
                            }
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("[GameScannerService]: unable to detect steam process!");
                }
                */
            }
        }
        catch (Win32Exception ex)
        {
            Debug.WriteLine("Could not launch steam game: " + ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            Debug.WriteLine("Could not launch steam game: " + ex.Message);
        }

        await Task.CompletedTask;
        return string.Empty;
    }

    public async Task LaunchEpicGames(string appLaunchPath)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "com.epicgames.launcher://apps/" + appLaunchPath + "?action=launch&silent=true",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Could not start epic games: " + ex.Message);
        }

        await Task.CompletedTask;
    }

    [GeneratedRegex("(?:^|/)(?:ig|lib)xess\\.dll$", RegexOptions.Multiline)]
    private static partial Regex XeSsRegex();
    [GeneratedRegex("^(?:.*)\"appid\"(?:.*)\"(?<appId>.*)\"$", RegexOptions.Multiline)]
    private static partial Regex AppIdRegex();
    [GeneratedRegex("^(?:.*)\"installdir\"(?:.*)\"(?<installdir>.*)\"$", RegexOptions.Multiline)]
    private static partial Regex InstallDirRegex();
    [GeneratedRegex("^(?:.*)\"path\"(?:.*)\"(?<path>.*)\"$", RegexOptions.Multiline)]
    private static partial Regex SteamLibraryPathsRegex();
}
