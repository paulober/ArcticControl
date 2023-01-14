using ArcticControl.Contracts.Services;
using ArcticControl.Models;
using ArcticControlGPUInterop;

namespace ArcticControl.Services;
public class DummyIntelGraphicsControlService : IIntelGraphicsControlService
{
    public bool IsDummy() => true;
    public bool AreFrequencyDomainsInitialized() { return default; }
    public void Dispose() { return; }
    public AnisotropicFilteringMode GetAnisotropicFilteringMode(string? app = null) { return AnisotropicFilteringMode.Unknown; }
    public CmaaMode GetCmaaMode(string? app = null) { return CmaaMode.Unknown; }
    public FanProperties? GetFanProperties() { return null; }
    public FrequencyProperties? GetFrequencyProperties() { return null; }
    public FrequencyState? GetFrequencyState() { return null; }
    public GamingFlipMode GetGamingFlipMode(string? app = null) { return GamingFlipMode.Unknown; }
    public Tuple<double, double>? GetMinMaxFrequency() { return null; }
    public double GetOverclockGpuFrequencyOffset() { return 0.0; }
    public double GetOverclockGpuVoltageOffset() { return 0.0; }
    public double GetOverclockPowerLimit() { return 0.0; }
    public double GetOverclockTemperatureLimit() { return 0.0; }
    public PCIeProperties? GetPCIeProperties() { return null; }
    public PowerLimitsCombination? GetPowerLimits() { return null; }
    public PowerProperties? GetPowerProperties() { return null; }
    public bool InitFansHandles() { return false; }
    public bool InitFrequencyDomains() { return false; }
    public bool InitPowerDomains() { return false; }
    public bool IsInitialized() { return false; }
    public bool IsSharpeningFilterActive(string? app = null) { return false; }
    public bool SetAnisotropicFilteringMode(AnisotropicFilteringMode anisotropicMode, string? app = null) { return false; }
    public bool SetCmaaMode(CmaaMode cmaaMode, string? app = null) { return false; }
    public bool SetGamingFlipMode(GamingFlipMode gamingFlipMode, string? app = null) { return false; }
    public bool SetMinMaxFrequency(double minFreq, double maxFreq) { return false; }
    public bool SetOverclockGpuFrequencyOffset(double newGpuFrequencyOffset) { return false; }
    public bool SetOverclockGpuVoltageOffset(double newGpuVoltageOffset) { return false; }
    public bool SetOverclockPowerLimit(double newPowerLimit) { return false; }
    public bool SetOverclockTemperatureLimit(double newGpuTemperatureLimit) { return false; }
    public bool SetOverclockWaiver() { return false; }
    public bool SetSharpeningFilter(bool on, string? app = null) { return false; }
}
