using ArcticControl.IntelWebAPI.Models;

namespace ArcticControl.IntelWebAPI.Contracts.Services;

public interface IWebArcDriversService
{
    Task<bool> PreloadWebArcDriverDataAsync();

    Task<IEnumerable<WebArcDriver>> GetWebArcDriverDataAsync();
}
