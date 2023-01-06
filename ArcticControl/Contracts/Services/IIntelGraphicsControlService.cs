using ArcticControlGPUInterop;

namespace ArcticControl.Contracts.Services;
public interface IIntelGraphicsControlService : IDisposable
{
    public bool IsInitialized();
    public bool SetOverclockWaiver();
    public double GetOverclockPowerLimit();
    public double GetOverclockTemperatureLimit();
    public double GetOverclockGPUVoltageOffset();
    public double GetOverclockGPUFrequencyOffset();
    public bool SetOverclockPowerLimit(double newPowerLimit);
    public bool SetOverclockTemperatureLimit(double newGPUTemperatureLimit);
    public bool SetOverclockGPUVoltageOffset(double newGPUVoltageOffset);
    public bool SetOverclockGPUFrequencyOffset(double newGPUFrequencyOffset);
    public bool InitPowerDomains();
    public PowerProperties? GetPowerProperties();
    public PowerLimitsCombination? GetPowerLimits();
}
