using ArcticControl.Models;

namespace ArcticControl.Contracts.Services;
public interface IIntelGraphicsControlService : IDisposable
{
    public bool IsDummy();
    public bool IsInitialized();
    public bool SetOverclockWaiver();
    public double GetOverclockPowerLimit();
    public double GetOverclockTemperatureLimit();
    public double GetOverclockGpuVoltageOffset();
    public double GetOverclockGpuFrequencyOffset();
    public bool SetOverclockPowerLimit(double newPowerLimit);
    public bool SetOverclockTemperatureLimit(double newGpuTemperatureLimit);
    public bool SetOverclockGpuVoltageOffset(double newGpuVoltageOffset);
    public bool SetOverclockGpuFrequencyOffset(double newGpuFrequencyOffset);
    public Tuple<double, double>? GetOverclockGpuLock();
    public bool SetOverclockGpuLock(double voltage, double frequency);
    public bool InitPowerDomains();
    public PowerProperties? GetPowerProperties();
    public PowerLimitsCombination? GetPowerLimits();

    public bool InitFansHandles();
    public FanProperties? GetFanProperties();

    public GamingFlipMode GetGamingFlipMode(string? app = null);
    public bool SetGamingFlipMode(GamingFlipMode gamingFlipMode, string? app = null);
    public AnisotropicFilteringMode GetAnisotropicFilteringMode(string? app = null);
    public bool SetAnisotropicFilteringMode(AnisotropicFilteringMode anisotropicMode, string? app = null);
    public CmaaMode GetCmaaMode(string? app = null);
    public bool SetCmaaMode(CmaaMode cmaaMode, string? app = null);
    public bool IsSharpeningFilterActive(string? app = null);
    public bool SetSharpeningFilter(bool on, string? app = null);

    public bool InitFrequencyDomains();
    public bool AreFrequencyDomainsInitialized();
    public FrequencyProperties? GetFrequencyProperties();
    public FrequencyState? GetFrequencyState();
    public Tuple<double, double>? GetMinMaxFrequency();
    public bool SetMinMaxFrequency(double minFreq, double maxFreq);
    
    public PCIeProperties? GetPCIeProperties();
}
