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
            if (powerLimit != null)
            {
                return (double)powerLimit;
            }
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
            if (overclockTempLimit != null)
            {
                return (double)overclockTempLimit;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - GetOverclockTemperatureLimit");
            Crashes.TrackError(ex);
        }

        return 0.0;
    }

    public double GetOverclockGPUVoltageOffset()
    {
        if (!_initialized)
        {
            return 0.0;
        }

        try
        {
            var gpuVoltageOffset = _gpuInterop.GetOverclockGPUVoltageOffset();
            if (gpuVoltageOffset != null)
            {
                return (double)gpuVoltageOffset;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - GetOverclockGPUVoltageOffset");
            Crashes.TrackError(ex);
        }

        return 0.0;
    }

    public double GetOverclockGPUFrequencyOffset()
    {
        if (!_initialized)
        {
            return 0.0;
        }

        try
        {
            var gpuFrequencyOffset = _gpuInterop.GetOverclockGPUFrequencyOffset();
            if (gpuFrequencyOffset != null)
            {
                return (double)gpuFrequencyOffset;
            }
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

    public bool SetOverclockTemperatureLimit(double newGPUTemperatureLimit)
    {
        if (!_initialized)
        {
            return false;
        }

        try
        {
            var result = _gpuInterop.SetOverclockTemperatureLimit(newGPUTemperatureLimit);
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - SetOverclockTemperatureLimit");
            Crashes.TrackError(ex);
        }

        return false;
    }

    public bool SetOverclockGPUVoltageOffset(double newGPUVoltageOffset)
    {
        if (!_initialized)
        {
            return false;
        }

        try
        {
            var result = _gpuInterop.SetOverclockGPUVoltageOffset(newGPUVoltageOffset);
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - SetOverclockGPUVoltageOffset");
            Crashes.TrackError(ex);
        }

        return false;
    }

    public bool SetOverclockGPUFrequencyOffset(double newGPUFrequencyOffset)
    {
        if (!_initialized)
        {
            return false;
        }

        try
        {
            var result = _gpuInterop.SetOverclockGPUVoltageOffset(newGPUFrequencyOffset);
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
            PowerProperties? result = _gpuInterop.GetPowerProperties();
            if (result != null)
            {
                return result;
            }
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
            PowerLimitsCombination? result = _gpuInterop.GetPowerLimits();
            if (result != null)
            {
                return result;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - GetPowerLimits");
            Crashes.TrackError(ex);
        }

        return default;
    }

    /// <summary>
    /// DON'T CALL THIS METHOD MANUALLY.
    /// </summary>
    public void Dispose() => _gpuInterop.Dispose();
}
