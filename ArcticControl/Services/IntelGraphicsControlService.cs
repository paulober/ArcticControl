using System.Diagnostics;
using ArcticControl.Contracts.Services;
using ArcticControlGPUInterop;
using Microsoft.AppCenter.Crashes;

namespace ArcticControl.Services;
public class IntelGraphicsControlService: IIntelGraphicsControlService
{
    private readonly bool _initialized;
    private bool _frequencyDomainsInitialized = false;
    private readonly GPUInterop _gpuInterop;

    public IntelGraphicsControlService()
    {
        _gpuInterop = new GPUInterop();

        try
        {
            _initialized = _gpuInterop.InitCtlApi();
        }
        catch (PlatformNotSupportedException)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: error setting up service " +
                            "as platform is not supported");
        }
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

    public GamingFlipMode GetGamingFlipMode(string? app = null)
    {
        if (!_initialized)
        {
            return GamingFlipMode.Unknown;
        }

        try
        {
            var gamingFlipMode = _gpuInterop.GetGamingFlipMode(app);
            return gamingFlipMode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - GetGamingFlipMode");
            Crashes.TrackError(ex);
        }

        return GamingFlipMode.Unknown;
    }

    public bool SetGamingFlipMode(GamingFlipMode gamingFlipMode, string? app = null)
    {
        if (!_initialized)
        {
            return false;
        }

        try
        {
            var result = _gpuInterop.SetGamingFlipMode(gamingFlipMode, app);
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - SetGamingFlipMode");
            Crashes.TrackError(ex);
        }
        
        return false;
    }
    
    public AnisotropicFilteringMode GetAnisotropicFilteringMode(string? app = null)
    {
        if (!_initialized)
        {
            return AnisotropicFilteringMode.Unknown;
        }

        try
        {
            var anisotropicMode = _gpuInterop.GetAnisotropicFilteringMode(app);
            return anisotropicMode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - GetAnisotropicFilteringMode");
            Crashes.TrackError(ex);
        }

        return AnisotropicFilteringMode.Unknown;
    }

    public bool SetAnisotropicFilteringMode(AnisotropicFilteringMode anisotropicMode, string? app = null)
    {
        if (!_initialized)
        {
            return false;
        }

        try
        {
            var result = _gpuInterop.SetAnisotropicFilteringMode(anisotropicMode, app);
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - SetAnisotropicFilteringMode");
            Crashes.TrackError(ex);
        }
        
        return false;
    }
    
    public CmaaMode GetCmaaMode(string? app = null)
    {
        if (!_initialized)
        {
            return CmaaMode.Unknown;
        }

        try
        {
            var cmaaMode = _gpuInterop.GetCmaaMode(app);
            return cmaaMode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - GetCmaaMode");
            Crashes.TrackError(ex);
        }

        return CmaaMode.Unknown;
    }

    public bool SetCmaaMode(CmaaMode cmaaMode, string? app = null)
    {
        if (!_initialized)
        {
            return false;
        }

        try
        {
            var result = _gpuInterop.SetCmaaMode(cmaaMode, app);
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - SetCmaaMode");
            Crashes.TrackError(ex);
        }
        
        return false;
    }

    public bool IsSharpeningFilterActive(string? app = null)
    {
        if (!_initialized)
        {
            return false;
        }
        
        try
        {
            var state = _gpuInterop.IsSharpeningFilterActive(app);
            return state;
        }
        catch (Exception)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - IsSharpeningFilterActive");
            // TODO: check each tracked exception in this file if really needed to track
            //Crashes.TrackError(ex);
        }
        
        return false;
    }

    public bool SetSharpeningFilter(bool on, string? app = null)
    {
        if (!_initialized)
        {
            return false;
        }
        
        try
        {
            var result = _gpuInterop.SetSharpeningFilter(on, app);
            return result;
        }
        catch (Exception)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - SetSharpeningFilter");
            //Crashes.TrackError(ex);
        }
        
        return false;
    }

    public bool InitFrequencyDomains()
    {
        if (!_initialized)
        {
            return false;
        }

        try
        {
            var result = _gpuInterop.InitFrequencyDomains();

            _frequencyDomainsInitialized = result;
            
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - InitFrequencyDomains");
            Crashes.TrackError(ex);
        }

        return false;
    }

    public bool AreFrequencyDomainsInitialized() => _frequencyDomainsInitialized;
    
    public FrequencyProperties? GetFrequencyProperties()
    {
        if (!_initialized)
        {
            return null;
        }
        
        try
        {
            var freqProps = _gpuInterop.GetFrequencyProperties();
            return freqProps;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - GetFrequencyProperties");
            Crashes.TrackError(ex);
        }

        return null;
    }
    
    public FrequencyState? GetFrequencyState()
    {
        if (!_initialized)
        {
            return null;
        }
        
        try
        {
            var freqState = _gpuInterop.GetFrequencyState();
            return freqState;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - GetFrequencyState");
            Crashes.TrackError(ex);
        }

        return null;
    }
    
    public Tuple<double, double>? GetMinMaxFrequency()
    {
        if (!_initialized)
        {
            return null;
        }
        
        try
        {
            var range = _gpuInterop.GetMinMaximumFrequency();
            return range;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - GetFrequencyState");
            Crashes.TrackError(ex);
        }

        return null;
    }

    public bool SetMinMaxFrequency(double minFreq, double maxFreq)
    {
        if (!_initialized)
        {
            return false;
        }
        
        try
        {
            var result = _gpuInterop.SetMinMaximumFrequency(minFreq, maxFreq);
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - SetMinMaxFrequency");
            Crashes.TrackError(ex);
        }

        return false;
    }

    public PCIeProperties? GetPCIeProperties()
    {
        if (!_initialized)
        {
            return null;
        }
        
        try
        {
            var props = _gpuInterop.GetPCIeProperties();

            return props;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - GetPCIeProperties: " + ex.Message);
        }

        return null;
    }

    /// <summary>
    /// DON'T CALL THIS METHOD MANUALLY.
    /// </summary>
    public void Dispose() => _gpuInterop.Dispose();
}
