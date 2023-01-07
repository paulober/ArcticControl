using ArcticControlGPUInterop;

namespace ArcticControl.Contracts.Services;
public interface IIntelGraphicsControlService : IDisposable
{
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
    public bool InitPowerDomains();
    public PowerProperties? GetPowerProperties();
    public PowerLimitsCombination? GetPowerLimits();

    public bool InitFansHandles();
}
