using ArcticControl.Models;

namespace ArcticControl.Contracts.Services;
public interface IGamesScannerService
{
    Task<IEnumerable<InstalledGame>> GetInstalledGames(bool force = false);
    Task<string> LaunchSteamGame(string appId, bool searchExe = false);
    Task LaunchEpicGames(string appLaunchPath);
}
