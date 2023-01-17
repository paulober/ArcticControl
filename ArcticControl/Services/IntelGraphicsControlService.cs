using System.Diagnostics;
using ArcticControl.Contracts.Services;
using ArcticControl.Models;
using ArcticControlGPUInterop;
using Microsoft.AppCenter.Crashes;

// types from native dll
using ManagedFanProperties = ArcticControl.Models.FanProperties;
using ManagedFrequencyProperties = ArcticControl.Models.FrequencyProperties;
using ManagedFrequencyCap = ArcticControl.Models.FrequencyCap;
using ManagedFrequencyState = ArcticControl.Models.FrequencyState;
using ManagedPCIeProperties = ArcticControl.Models.PCIeProperties;
using ManagedSustainedPowerLimit = ArcticControl.Models.SustainedPowerLimit;
using ManagedBurstPowerLimit = ArcticControl.Models.BurstPowerLimit;
using ManagedPeakPowerLimit = ArcticControl.Models.PeakPowerLimit;
using ManagedPowerLimitsCombination = ArcticControl.Models.PowerLimitsCombination;
using ManagedPowerProperties = ArcticControl.Models.PowerProperties;

namespace ArcticControl.Services;
public class IntelGraphicsControlService: IIntelGraphicsControlService
{
    private readonly bool _initialized = false;
    private bool _frequencyDomainsInitialized = false;
    private readonly GPUInterop? _gpuInterop;

    public IntelGraphicsControlService()
    {
        try
        {
            _gpuInterop = new GPUInterop();

            _initialized = _gpuInterop.InitCtlApi();
        }
        catch (Exception)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: error setting up service " +
                            "as platform is not supported");
        }
    }

    public bool IsDummy() => false;

    public bool IsInitialized() => _initialized;

    public bool SetOverclockWaiver()
    {
        if (!_initialized || _gpuInterop == null)
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
        if (!_initialized || _gpuInterop == null)
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
        if (!_initialized || _gpuInterop == null)
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
        if (!_initialized || _gpuInterop == null)
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
        if (!_initialized || _gpuInterop == null)
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
        if (!_initialized || _gpuInterop == null)
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
        if (!_initialized || _gpuInterop == null)
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
        if (!_initialized || _gpuInterop == null)
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
        if (!_initialized || _gpuInterop == null)
        {
            return false;
        }

        try
        {
            var result = _gpuInterop.SetOverclockGPUFrequencyOffset(newGpuFrequencyOffset);
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - SetOverclockGPUFrequencyOffset");
            Crashes.TrackError(ex);
        }

        return false;
    }

    public Tuple<double, double>? GetOverclockGpuLock()
    {
        if (!_initialized || _gpuInterop == null)
        {
            return null;
        }
        
        try
        {
            var ocVfPair = _gpuInterop.GetOverclockGPULock();
            return ocVfPair;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - GetOverclockGpuLock");
            Crashes.TrackError(ex);
        }

        return null;
    }

    public bool SetOverclockGpuLock(double voltage, double frequency)
    {
        if (!_initialized || _gpuInterop == null)
        {
            return false;
        }
        
        try
        {
            var result = _gpuInterop.SetOverclockGPULock(voltage, frequency);
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - SetOverclockGpuLock");
            Crashes.TrackError(ex);
        }
        
        return false;
    }

    public bool InitPowerDomains()
    {
        if (!_initialized || _gpuInterop == null)
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

    public ManagedPowerProperties? GetPowerProperties()
    {
        if (!_initialized || _gpuInterop == null)
        {
            return null;
        }

        try
        {
            var result = _gpuInterop.GetPowerProperties();

            if (result != null)
            {
                return new ManagedPowerProperties
                {
                    CanControl = result.CanControl,
                    DefaultLimit = result.DefaultLimit,
                    MinLimit = result.MinLimit,
                    MaxLimit = result.MaxLimit
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - GetPowerProperties");
            Crashes.TrackError(ex);
        }

        return default;
    }

    public ManagedPowerLimitsCombination? GetPowerLimits()
    {
        if (!_initialized || _gpuInterop == null)
        {
            return null;
        }

        try
        {
            var result = _gpuInterop.GetPowerLimits();

            if (result != null)
            {
                return new ManagedPowerLimitsCombination
                {
                    SustainedPowerLimit = new ManagedSustainedPowerLimit
                    {
                        Enabled = result.SustainedPowerLimit.Enabled,
                        Power = result.SustainedPowerLimit.Power,
                        Interval = result.SustainedPowerLimit.Interval
                    },
                    BurstPowerLimit = new ManagedBurstPowerLimit
                    {
                        Enabled = result.BurstPowerLimit.Enabled,
                        Power = result.BurstPowerLimit.Power
                    },
                    PeakPowerLimit = new ManagedPeakPowerLimit
                    {
                        PowerAC = result.PeakPowerLimit.PowerAC,
                        PowerDC = result.PeakPowerLimit.PowerDC
                    }
                };
            }
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
        if (!_initialized || _gpuInterop == null)
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

    public ManagedFanProperties? GetFanProperties()
    {
        if (!_initialized || _gpuInterop == null)
        {
            return default;
        }

        try
        {
            var fanProps = _gpuInterop.GetFanProperties();

            if (fanProps != null)
            {
                return new ManagedFanProperties
                {
                    CanControl = fanProps.CanControl,
                    SupportedModes = fanProps.SupportedModes,
                    SupportedUnits = fanProps.SupportedUnits,
                    MaxRPM = fanProps.MaxRPM,
                    MaxPoints = fanProps.MaxPoints
                };
            }
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
        if (!_initialized || _gpuInterop == null)
        {
            return GamingFlipMode.Unknown;
        }

        try
        {
            var gamingFlipMode = (GamingFlipMode)_gpuInterop.GetGamingFlipMode(app);
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
        if (!_initialized || _gpuInterop == null)
        {
            return false;
        }

        try
        {
            var result = _gpuInterop.SetGamingFlipMode((uint)gamingFlipMode, app);
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
        if (!_initialized || _gpuInterop == null)
        {
            return AnisotropicFilteringMode.Unknown;
        }

        try
        {
            var anisotropicMode = (AnisotropicFilteringMode)_gpuInterop.GetAnisotropicFilteringMode(app);
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
        if (!_initialized || _gpuInterop == null)
        {
            return false;
        }

        try
        {
            var result = _gpuInterop.SetAnisotropicFilteringMode((uint)anisotropicMode, app);
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
        if (!_initialized || _gpuInterop == null)
        {
            return CmaaMode.Unknown;
        }

        try
        {
            var cmaaMode = (CmaaMode)_gpuInterop.GetCmaaMode(app);
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
        if (!_initialized || _gpuInterop == null)
        {
            return false;
        }

        try
        {
            var result = _gpuInterop.SetCmaaMode((uint)cmaaMode, app);
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
        if (!_initialized || _gpuInterop == null)
        {
            return false;
        }
        
        try
        {
            var state = _gpuInterop.IsSharpeningFilterActive(app);
            return state;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - IsSharpeningFilterActive");
            // TODO: check each tracked exception in this file if really needed to track
            Crashes.TrackError(ex);
        }
        
        return false;
    }

    public bool SetSharpeningFilter(bool on, string? app = null)
    {
        if (!_initialized || _gpuInterop == null)
        {
            return false;
        }
        
        try
        {
            var result = _gpuInterop.SetSharpeningFilter(on, app);
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - SetSharpeningFilter");
            Crashes.TrackError(ex);
        }
        
        return false;
    }

    public bool InitFrequencyDomains()
    {
        if (!_initialized || _gpuInterop == null)
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
    
    public ManagedFrequencyProperties? GetFrequencyProperties()
    {
        if (!_initialized || _gpuInterop == null)
        {
            return null;
        }
        
        try
        {
            var freqProps = _gpuInterop.GetFrequencyProperties();
            if (freqProps != null)
            {
                return new ManagedFrequencyProperties
                {
                    CanControl = freqProps.CanControl,
                    HardwareMin = freqProps.HardwareMin,
                    HardwareMax = freqProps.HardwareMax
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[IntelGraphicsControlService]: Error - GetFrequencyProperties");
            Crashes.TrackError(ex);
        }

        return null;
    }
    
    public ManagedFrequencyState? GetFrequencyState()
    {
        if (!_initialized || _gpuInterop == null)
        {
            return null;
        }
        
        try
        {
            var freqState = _gpuInterop.GetFrequencyState();

            if (freqState != null)
            {
                return new ManagedFrequencyState
                {
                    CurrentVoltage = freqState.CurrentVoltage,
                    RequestedFrequency = freqState.RequestedFrequency,
                    TDPFrequency = freqState.TDPFrequency,
                    EfficientFrequency = freqState.EfficientFrequency,
                    ActualFrequency = freqState.ActualFrequency,
                    Cap = (ManagedFrequencyCap)freqState.Cap
                };
            }
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
        if (!_initialized || _gpuInterop == null)
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
        if (!_initialized || _gpuInterop == null)
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

    public ManagedPCIeProperties? GetPCIeProperties()
    {
        if (!_initialized || _gpuInterop == null)
        {
            return null;
        }
        
        try
        {
            var props = _gpuInterop.GetPCIeProperties();

            if (props != null)
            {
                return new ManagedPCIeProperties
                {
                    IsReBarSupported = props.IsReBarSupported,
                    IsReBarEnabled = props.IsReBarEnabled,
                    Lanes = props.Lanes,
                    Gen = props.Gen
                };
            }
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
    public void Dispose() => _gpuInterop?.Dispose();
}
