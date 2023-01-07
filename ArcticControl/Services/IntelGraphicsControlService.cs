using System.Diagnostics;
using ArcticControl.Contracts.Services;
using ArcticControlGPUInterop;
using Microsoft.AppCenter.Crashes;

namespace ArcticControl.Services;
public class IntelGraphicsControlService: IIntelGraphicsControlService, IDisposable
{
    private readonly bool _initialized;
    private readonly GPUInterop _gpuInterop;

    public IntelGraphicsControlService()
    {
        _gpuInterop = new();
        _initialized = _gpuInterop.InitCtlApi();
    }

    public bool IsInitialized() => _initialized;

    public bool SetOverclockWaiver()
    {
        if (!_initialized)
        {
            return false;
        }

        try
        {
            var result = _gpuInterop.SetOverclockWaiver();
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - SetOverclockWaiver");
            Crashes.TrackError(ex);
        }

        return false;
    }

    public double GetOverclockPowerLimit()
    {
        if (!_initialized)
        {
            return 0.0;
        }

        try
        {
            var powerLimit = _gpuInterop.GetOverclockPowerLimit();
            return powerLimit;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - GetOverclockPowerLimit");
            Crashes.TrackError(ex);
        }

        return 0.0;
    }

    public double GetOverclockTemperatureLimit()
    {
        if (!_initialized)
        {
            return 0.0;
        }

        try
        {
            var overclockTempLimit = _gpuInterop.GetOverclockTemperatureLimit();
            return overclockTempLimit;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - GetOverclockTemperatureLimit");
            Crashes.TrackError(ex);
        }

        return 0.0;
    }

    public double GetOverclockGpuVoltageOffset()
    {
        if (!_initialized)
        {
            return 0.0;
        }

        try
        {
            var gpuVoltageOffset = _gpuInterop.GetOverclockGPUVoltageOffset();
            return gpuVoltageOffset;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - GetOverclockGPUVoltageOffset");
            Crashes.TrackError(ex);
        }

        return 0.0;
    }

    public double GetOverclockGpuFrequencyOffset()
    {
        if (!_initialized)
        {
            return 0.0;
        }

        try
        {
            var gpuFrequencyOffset = _gpuInterop.GetOverclockGPUFrequencyOffset();
            return gpuFrequencyOffset;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - GetOverclockGPUFrequencyOffset");
            Crashes.TrackError(ex);
        }

        return 0.0;
    }

    public bool SetOverclockPowerLimit(double newPowerLimit)
    {
        if (!_initialized)
        {
            return false;
        }

        try
        {
            var result = _gpuInterop.SetOverclockPowerLimit(newPowerLimit);
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - SetOverclockPowerLimit");
            Crashes.TrackError(ex);
        }

        return false;
    }

    public bool SetOverclockTemperatureLimit(double newGpuTemperatureLimit)
    {
        if (!_initialized)
        {
            return false;
        }

        try
        {
            var result = _gpuInterop.SetOverclockTemperatureLimit(newGpuTemperatureLimit);
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - SetOverclockTemperatureLimit");
            Crashes.TrackError(ex);
        }

        return false;
    }

    public bool SetOverclockGpuVoltageOffset(double newGpuVoltageOffset)
    {
        if (!_initialized)
        {
            return false;
        }

        try
        {
            var result = _gpuInterop.SetOverclockGPUVoltageOffset(newGpuVoltageOffset);
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - SetOverclockGPUVoltageOffset");
            Crashes.TrackError(ex);
        }

        return false;
    }

    public bool SetOverclockGpuFrequencyOffset(double newGpuFrequencyOffset)
    {
        if (!_initialized)
        {
            return false;
        }

        try
        {
            var result = _gpuInterop.SetOverclockGPUVoltageOffset(newGpuFrequencyOffset);
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - SetOverclockGPUFrequencyOffset");
            Crashes.TrackError(ex);
        }

        return false;
    }

    public bool InitPowerDomains()
    {
        if (!_initialized)
        {
            return false;
        }

        try
        {
            var result = _gpuInterop.InitPowerDomains();
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - InitPowerDomains");
            Crashes.TrackError(ex);
        }

        return default;
    }

    public PowerProperties? GetPowerProperties()
    {
        if (!_initialized)
        {
            return null;
        }

        try
        {
            var result = _gpuInterop.GetPowerProperties();
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - GetPowerProperties");
            Crashes.TrackError(ex);
        }

        return default;
    }

    public PowerLimitsCombination? GetPowerLimits()
    {
        if (!_initialized)
        {
            return null;
        }

        try
        {
            var result = _gpuInterop.GetPowerLimits();
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - GetPowerLimits");
            Crashes.TrackError(ex);
        }

        return default;
    }

    public bool InitFansHandles()
    {
        if (!_initialized)
        {
            return false;
        }

        try
        {
            var result = _gpuInterop.InitFansHandles();

            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - InitFansHandles");
            Crashes.TrackError(ex);
        }

        return false;
    }

    public FanProperties? GetFanProperties()
    {
        if (!_initialized)
        {
            return default;
        }

        try
        {
            var fanProps = _gpuInterop.GetFanProperties();
            return fanProps;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - GetFanProperties");
            Crashes.TrackError(ex);
        }

        return default;
    }

    /// <summary>
    /// DON'T CALL THIS METHOD MANUALLY.
    /// </summary>
    public void Dispose() => _gpuInterop.Dispose();
}
