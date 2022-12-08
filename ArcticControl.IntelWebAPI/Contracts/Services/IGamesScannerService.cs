using ArcticControl.Core.Models;

namespace ArcticControl.IntelWebAPI.Contracts.Services;
public interface IGamesScannerService
{
    IEnumerable<InstalledGame> GetInstalledGames(bool force = false);
}
