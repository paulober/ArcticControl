using ArcticControl.Core.Models;

namespace ArcticControl.Core.Contracts.Services;
public interface IGamesScannerService
{
    IEnumerable<InstalledGame> GetInstalledGames(bool force = false);
}
