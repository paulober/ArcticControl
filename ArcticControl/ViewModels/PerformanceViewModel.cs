using System.Diagnostics;
using System.Globalization;
using System.Management;
using ArcticControl.Contracts.Services;
using ArcticControl.Contracts.ViewModels;
using ArcticControl.Core.Helpers;
using ArcticControl.Helpers;
using ArcticControl.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace ArcticControl.ViewModels;

internal enum SliderValueDefaults: ushort
{
    GpuVoltageOffset = 1,
    GpuPowerLimit = 2,
    GpuTemperatureLimit = 3,
    GpuFrequencyOffset = 4,
    FanSpeed = 5,
    FrequencyMinimum = 6,
    FrequencyMaximum = 7,
    VoltageLock = 8,
    FrequencyLock = 9
}

internal static class SliderDefaultValues
{
    internal const ushort GpuFrequencyOffset = 0;
    internal const ushort GpuVoltageOffset = 0;
    internal const ushort GpuPowerLimit = 190;
    internal const ushort GpuTemperatureLimit = 90;
    internal const ushort FanSpeed = 20;
    internal const double FrequencyMinimum = 0.0;
    internal const double FrequencyMaximum = 0.0;
    internal const double VoltageLock = 0.0;
    internal const double FrequencyLock = 0.0;
}

public class PerformanceViewModel : ObservableRecipient, INavigationAware
{
    /// <summary>
    /// INOP at the moment cause no NotifyCollectionChangedAction available for indicating changes in an element.
    /// Alernatives always cause flickering when they reload the items.
    /// </summary>
    public AdvancedObservableCollection<PerformanceValueDataObject> PerformanceValues
    {
        get; init;
    } = new();

    /// <summary>
    /// does not actually contain only slider values
    /// </summary>
    private readonly Dictionary<SliderValueDefaults, double> _currentActiveSliderValues = new()
    {
        { SliderValueDefaults.GpuPowerLimit, Convert.ToDouble(SliderDefaultValues.GpuPowerLimit) },
        { SliderValueDefaults.GpuTemperatureLimit, Convert.ToDouble(SliderDefaultValues.GpuTemperatureLimit) },
        { SliderValueDefaults.GpuVoltageOffset, Convert.ToDouble(SliderDefaultValues.GpuVoltageOffset) },
        { SliderValueDefaults.GpuFrequencyOffset, Convert.ToDouble(SliderDefaultValues.GpuFrequencyOffset) },
        { SliderValueDefaults.FanSpeed, Convert.ToDouble(SliderDefaultValues.FanSpeed) },
        { SliderValueDefaults.FrequencyMinimum, Convert.ToDouble(SliderDefaultValues.FrequencyMinimum) },
        { SliderValueDefaults.FrequencyMaximum, Convert.ToDouble(SliderDefaultValues.FrequencyMaximum) },
        { SliderValueDefaults.VoltageLock, Convert.ToDouble(SliderDefaultValues.VoltageLock) },
        { SliderValueDefaults.FrequencyLock, Convert.ToDouble(SliderDefaultValues.FrequencyLock) }
    };

    public bool WaiverSigned = false;
    
    private bool _loadFanSpeedSlider = false;

    public bool LoadFanSpeedSlider
    {
        get => _loadFanSpeedSlider;
        set => SetProperty(ref _loadFanSpeedSlider, value);
    }

    private double _frequencyMinimum;
    public double FrequencyMinimum
    {
        get => _frequencyMinimum;
        set => SetProperty(ref _frequencyMinimum, value);
    }
    
    private double _frequencyMaximum;
    public double FrequencyMaximum
    {
        get => _frequencyMaximum;
        set => SetProperty(ref _frequencyMaximum, value);
    }
    
    private double _frequencyLock;
    public double FrequencyLock
    {
        get => _frequencyLock;
        set => SetProperty(ref _frequencyLock, value);
    }
    
    private double _voltageLock;
    public double VoltageLock
    {
        get => _voltageLock;
        set => SetProperty(ref _voltageLock, value);
    }

    private double _gpuFrequencyOffsetSliderValue = Convert.ToDouble(SliderDefaultValues.GpuFrequencyOffset);

    public double GpuFrequencyOffsetSliderValue
    {
        get => _gpuFrequencyOffsetSliderValue;
        set => SetProperty(ref _gpuFrequencyOffsetSliderValue, value);
    }
    
    private double _gpuVoltageOffsetSliderValue = Convert.ToDouble(SliderDefaultValues.GpuVoltageOffset);

    public double GpuVoltageOffsetSliderValue
    {
        get => _gpuVoltageOffsetSliderValue;
        set => SetProperty(ref _gpuVoltageOffsetSliderValue, value); 
    }

    private double _gpuPowerMaxLimit = 228.0;
    public double GpuPowerMaxLimit
    {
        get => _gpuPowerMaxLimit;
        private set => SetProperty(ref _gpuPowerMaxLimit, value);
    }

    private double _gpuPowerLimitSliderValue = Convert.ToDouble(SliderDefaultValues.GpuPowerLimit);

    public double GpuPowerLimitSliderValue
    {
        get => _gpuPowerLimitSliderValue;
        set => SetProperty(ref _gpuPowerLimitSliderValue, value);
    }

    private double _gpuTemperatureLimitSliderValue = Convert.ToDouble(SliderDefaultValues.GpuTemperatureLimit);

    public double GpuTemperatureLimitSliderValue
    {
        get => _gpuTemperatureLimitSliderValue;
        set => SetProperty(ref _gpuTemperatureLimitSliderValue, value);
    }

    private uint _fanSpeedSliderValue = Convert.ToUInt32(SliderDefaultValues.FanSpeed);

    public uint FanSpeedSliderValue
    {
        get => _fanSpeedSliderValue;
        set => SetProperty(ref _fanSpeedSliderValue, value);
    }

    private bool _fanSpeedFixed = false;

    public bool FanSpeedFixed
    {
        get => _fanSpeedFixed;
        set => SetProperty(ref _fanSpeedFixed, value);
    }

    private bool _powerLimitSliderEnabled = true;

    public bool PowerLimitSliderEnabled
    {
        get => _powerLimitSliderEnabled;
        set => SetProperty(ref _powerLimitSliderEnabled, value);
    }
    
    private bool _isTelemetryOverlayToggleBtnChecked = false;

    public bool IsTelemetryOverlayToggleBtnChecked
    {
        get => _isTelemetryOverlayToggleBtnChecked;
        set => SetProperty(ref _isTelemetryOverlayToggleBtnChecked, value);
    }

    #region ValueDataObject properties to be able to update the values
    private PerformanceValueDataObject _cpuUtilizationObj 
        = new() { Title = "CPU Utilization", Value = "0.0", Unit = "%" };
    public PerformanceValueDataObject CpuUtilizationObj
    {
        get => _cpuUtilizationObj;
        set => SetProperty(ref _cpuUtilizationObj, value);
    }

    private PerformanceValueDataObject _memoryUtilizationObj
        = new() { Title = "Memory Utilization", Value = "0.0", Unit = "%" };
    public PerformanceValueDataObject MemoryUtilizationObj
    {
        get => _memoryUtilizationObj;
        set => SetProperty(ref _memoryUtilizationObj, value);
    }

    private PerformanceValueDataObject _gpuVoltageObj
        = new() { Title = "GPU Voltage", Value = "0.0", Unit = "mV" };

    public PerformanceValueDataObject GPUVoltageObj
    {
        get => _gpuVoltageObj;
        set => SetProperty(ref _gpuVoltageObj, value);
    }
    #endregion

    public PerformanceViewModel(
        ILocalSettingsService localSettingsService,
        IIntelGraphicsControlService intelGraphicsControlService)
    {
        _localSettingsService = localSettingsService;
        // _gpuInterop = new GPUInterop();
        _igcs = intelGraphicsControlService;
    }

    public bool IsNoArcDriverInstalled() => _igcs.IsDummy();

    private DispatcherQueue? _dpq;
    private CancellationTokenSource? _tickTimerTaskCancellationTokenSource;
    private Task? _tickTimerTask;
    // ReSharper disable once IdentifierTypo
    private readonly IIntelGraphicsControlService _igcs;
    private readonly ILocalSettingsService _localSettingsService;

    // frame-counter, UI bound Property, format, 
    private readonly List<Tuple<PerformanceSource, string>> _performanceCounters = new()
    {
        // handles property access via reflections
        // "Processor", "% Processor Time", "_Total"
        Tuple.Create(new PerformanceSource(new PerformanceSourceArgs
        { 
            Type = PerformanceSourceType.PerformanceCounter, 
            PerformanceCounterArgs = new string[] {"Processor Information", "% Processor Utility", "_Total"}, 
            Format = "0.0"
        }, deferPerfCounterSetup: true), nameof(CpuUtilizationObj)),
        Tuple.Create(new PerformanceSource(new PerformanceSourceArgs()
        {
            Type = PerformanceSourceType.ValueOffsetCallback,
            // PerformanceCounterArgs = new string[] {"Memory", "Available KBytes"},
            Format = "0.0",
            ValueOffsetCallback = (float value, object? arcNativeArgs) =>
            {
                try
                {
                    var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");

                    var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new {
                        FreePhysicalMemory = double.Parse(mo["FreePhysicalMemory"].ToString() ?? "0.0"),
                        TotalVisibleMemorySize = double.Parse(mo["TotalVisibleMemorySize"].ToString() ?? "0.0")
                    }).FirstOrDefault();

                    if (memoryValues != null)
                    {
                        var percent = ((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100;
                        return Math.Round(percent, 0).ToString(CultureInfo.CurrentCulture);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception on getting value offset for CPUUtilizationObj: " + ex.Message);
                }
                return "N/A";
            }
        }), nameof(MemoryUtilizationObj))/*,
        Tuple.Create(new PerformanceSource(new PerformanceSourceArgs
        {
            Type = PerformanceSourceType.ValueOffsetCallback,
            Format = "0.0",
            ArcNativeArgs = App.GetService<IIntelGraphicsControlService>(),
            ValueOffsetCallback = (float value, object? arcNativeArgs) =>
            {
                if (arcNativeArgs is IIntelGraphicsControlService igcs)
                {
                    
                    var powerProperties = igcs.GetPowerProperties();
                }
                
                return "N/A";
            }
        }), nameof(GPUVoltageObj))*/
        /*Tuple.Create(new PerformanceSource(new PerformanceSourceArgs()
        {
            Type = PerformanceSourceType.PerformanceCounter,
            PerformanceCounterArgs = new string[] {"Processor", "% Processor Time", "_Total"},
            Format = "0.0"
        }), nameof(GPUVolatageObj))*/
    };

    private async Task DoTick()
    {
        _ = _dpq?.TryEnqueue(() =>
        {
            // property access inside loop equal to for example:
            // CPUUtilizationObj.Value = _performanceCounter.NextValue().ToString("0.0");
            foreach (var counter in _performanceCounters)
            {
                var prop = this.GetType().GetProperty(counter.Item2);
                var pvdo = (PerformanceValueDataObject?)(prop?.GetValue(this, null));
                //prop?.SetValue(this, ...);
                if (pvdo != null)
                {
                    pvdo.Value = counter.Item1.NextValue();
                }
            }
        });
        await Task.CompletedTask;
    }

    public void StartBackgroundTickTimer(DispatcherQueue dispatcherQueue)
    {
        _dpq = dispatcherQueue;

        _tickTimerTaskCancellationTokenSource = new CancellationTokenSource();
        var ct = _tickTimerTaskCancellationTokenSource.Token;
        _tickTimerTask = Task.Run(async () =>
        {
            // Were we already canceled?
            ct.ThrowIfCancellationRequested();

            var tickTimer = new PeriodicTimer(TimeSpan.FromSeconds(0.5));
            // TODO: don't know if checking for IsCancellationRequested in while condition is good when using an async condition parallel
            while (await tickTimer.WaitForNextTickAsync(ct) || ct.IsCancellationRequested)
            {
                if (ct.IsCancellationRequested)
                {
                    // Clean up here
                    // ...
                    // then
                    ct.ThrowIfCancellationRequested();
                } else { await DoTick(); Thread.Sleep(500); }
            }
        }, ct);
    }

    public bool IsTickTimerStarted() => _tickTimerTask != null;

    // TODO: maybe switch to Task vs void
    public async void OnNavigatedTo(object parameter)
    {
        // ItemsSource placeholders setup
        /*
        PerformanceValues.Add(new PerformanceValueDataObject { Title = "CPU Utilization", Value = "9.1", Unit = "%" });
        PerformanceValues.Add(new PerformanceValueDataObject { Title = "Memory Utilization", Value = "17.8", Unit = "%" });
        PerformanceValues.Add(new PerformanceValueDataObject { Title = "GPU Volatage", Value = "650", Unit = "mV" });
        PerformanceValues.Add(new PerformanceValueDataObject { Title = "GPU Clock", Value = "850", Unit = "MHz" });
        PerformanceValues.Add(new PerformanceValueDataObject { Title = "GPU Temperature", Value = "56", Unit = "°C" });
        PerformanceValues.Add(new PerformanceValueDataObject { Title = "CPU Power", Value = "44", Unit = "W" });
        PerformanceValues.Add(new PerformanceValueDataObject { Title = "GPU Utilization", Value = "19.93", Unit = "%" });
        PerformanceValues.Add(new PerformanceValueDataObject { Title = "VRAM Clock", Value = "1093", Unit = "MHz" });
        PerformanceValues.Add(new PerformanceValueDataObject { Title = "VRAM Effective Frequency", Value = "830", Unit = "MHz" });
        PerformanceValues.Add(new PerformanceValueDataObject { Title = "VRAM Temperature", Value = "72", Unit = "°C" });
        PerformanceValues.Add(new PerformanceValueDataObject { Title = "Fan Speed", Value = "110", Unit = "RPM" });
        PerformanceValues.Add(new PerformanceValueDataObject { Title = "Render Utilization", Value = "69", Unit = "%" });
        */

        if (_igcs.IsInitialized() && !_igcs.AreFrequencyDomainsInitialized())
        {
            _igcs.InitFrequencyDomains();
        }
        
        GetOverclockingValues();
        
        /*
        if (_igcs.IsInitialized())
        {
            // do api testing
        }*/

        var settingsGpuPowerMaxLimit = await _localSettingsService
            .ReadSettingAsync<double>(LocalSettingsKeys.GPUPowerMaxLimit);
        GpuPowerMaxLimit = settingsGpuPowerMaxLimit < 228.0 ? 228.0 : settingsGpuPowerMaxLimit;
    }

    private void GetOverclockingValues(bool skipTempLimit = false)
    {
        if (!_igcs.IsInitialized())
        {
            return;
        }

        var powerLimit = _igcs.GetOverclockPowerLimit();
        var tempLimit = skipTempLimit 
            ? GpuTemperatureLimitSliderValue 
            : _igcs.GetOverclockTemperatureLimit();
        var gpuVoltageOffset = _igcs.GetOverclockGpuVoltageOffset();
        var gpuFrequencyOffset = _igcs.GetOverclockGpuFrequencyOffset();
        var minMaxFrequency = _igcs.GetMinMaxFrequency();
        var ocVfLockPair = _igcs.GetOverclockGpuLock();

        // unlimited power limit
        if (powerLimit == 0.0)
        {
            PowerLimitSliderEnabled = false;
        }
        else
        {
            PowerLimitSliderEnabled = true;
            GpuPowerLimitSliderValue = powerLimit;
        }
        
        GpuTemperatureLimitSliderValue = tempLimit;
        GpuVoltageOffsetSliderValue = gpuVoltageOffset;
        GpuFrequencyOffsetSliderValue = gpuFrequencyOffset;

        _currentActiveSliderValues[SliderValueDefaults.GpuPowerLimit] = powerLimit;
        _currentActiveSliderValues[SliderValueDefaults.GpuTemperatureLimit] = tempLimit;
        _currentActiveSliderValues[SliderValueDefaults.GpuVoltageOffset] = gpuVoltageOffset;
        _currentActiveSliderValues[SliderValueDefaults.GpuFrequencyOffset] = gpuFrequencyOffset;
        
        // frequency stuff
        // TODO: maybe stop returning nullable values replace with defaults - but would remove placeholders
        if (minMaxFrequency != null)
        {
            FrequencyMinimum = minMaxFrequency.Item1;
            FrequencyMaximum = minMaxFrequency.Item2;
            
            _currentActiveSliderValues[SliderValueDefaults.FrequencyMinimum] = minMaxFrequency.Item1;
            _currentActiveSliderValues[SliderValueDefaults.FrequencyMaximum] = minMaxFrequency.Item2;
        }
        
        // gpu lock stuff
        if (ocVfLockPair != null)
        {
            VoltageLock = ocVfLockPair.Item1;
            FrequencyLock = ocVfLockPair.Item2;
            
            _currentActiveSliderValues[SliderValueDefaults.VoltageLock] = ocVfLockPair.Item1;
            _currentActiveSliderValues[SliderValueDefaults.FrequencyLock] = ocVfLockPair.Item2;
        }

        // TODO: Fan stuff
        // if (FanSpeedFixed)...
    }

    public void ResetToDefaultSliderValues()
    {
        GpuPowerLimitSliderValue = Convert.ToDouble(SliderDefaultValues.GpuPowerLimit);
        GpuTemperatureLimitSliderValue = Convert.ToDouble(SliderDefaultValues.GpuTemperatureLimit);
        GpuVoltageOffsetSliderValue = Convert.ToDouble(SliderDefaultValues.GpuVoltageOffset);
        GpuFrequencyOffsetSliderValue = Convert.ToDouble(SliderDefaultValues.GpuFrequencyOffset);
        var resultInitPowerDomains = _igcs.InitPowerDomains();
        if (resultInitPowerDomains)
        {
            var props = _igcs.GetPowerProperties();
            if (props is { DefaultLimit: > 0 })
            {
                GpuPowerLimitSliderValue = props.Value.DefaultLimit;
            }
        }
        
        // frequency stuff
        if (_igcs.AreFrequencyDomainsInitialized())
        {
            var frequencyProperties = _igcs.GetFrequencyProperties();
            if (frequencyProperties != null)
            {
                // TODO: store in db for this device
                Debug.WriteLine("Freqmin: " + frequencyProperties.HardwareMin);
                FrequencyMinimum = frequencyProperties.HardwareMin;
                // TODO: change default maximum
                FrequencyMaximum = 2400.0;
            }
        }
        else
        {
            FrequencyMinimum = Convert.ToDouble(SliderDefaultValues.FrequencyMinimum);
            FrequencyMaximum = Convert.ToDouble(SliderDefaultValues.FrequencyMaximum);
        }
        
        // voltage stuff
        VoltageLock = Convert.ToDouble(SliderDefaultValues.VoltageLock);
        FrequencyLock = Convert.ToDouble(SliderDefaultValues.FrequencyLock);
    }

    public void RevertSliderChanges(object? sender, RoutedEventArgs? e)
    {
        GpuPowerLimitSliderValue = _currentActiveSliderValues[SliderValueDefaults.GpuPowerLimit];
        GpuTemperatureLimitSliderValue = _currentActiveSliderValues[SliderValueDefaults.GpuTemperatureLimit];
        GpuVoltageOffsetSliderValue = _currentActiveSliderValues[SliderValueDefaults.GpuVoltageOffset];
        GpuFrequencyOffsetSliderValue = _currentActiveSliderValues[SliderValueDefaults.GpuFrequencyOffset];
        
        // frequency stuff
        FrequencyMinimum = _currentActiveSliderValues[SliderValueDefaults.FrequencyMinimum];
        FrequencyMaximum = _currentActiveSliderValues[SliderValueDefaults.FrequencyMaximum];
        
        // voltage stuff
        VoltageLock = _currentActiveSliderValues[SliderValueDefaults.VoltageLock];
        FrequencyLock = _currentActiveSliderValues[SliderValueDefaults.FrequencyLock];
    }

    public bool ApplyChanges()
    {
        var doSkipTempLimit = false;

        if (GpuPowerLimitSliderValue != _currentActiveSliderValues[SliderValueDefaults.GpuPowerLimit])
        {
            // *1000 to convert W in mW
            _igcs.SetOverclockPowerLimit(GpuPowerLimitSliderValue*1000.0);
        }
        if (GpuTemperatureLimitSliderValue != _currentActiveSliderValues[SliderValueDefaults.GpuTemperatureLimit])
        {
            if (!WaiverSigned)
            {
                return false;
            }
            var result = _igcs.SetOverclockTemperatureLimit(GpuTemperatureLimitSliderValue);
            if (result)
            {
                // skip temp limit check because the gpu needs time to refresh values so it would return old tempLimit
                doSkipTempLimit = true;
            }
        }

        if (FrequencyMinimum != _currentActiveSliderValues[SliderValueDefaults.FrequencyMinimum] 
            || FrequencyMaximum != _currentActiveSliderValues[SliderValueDefaults.FrequencyMaximum])
        {
            if (!WaiverSigned)
            {
                return false;
            }
            _igcs.SetMinMaxFrequency(FrequencyMinimum, FrequencyMaximum);
        }

        if (VoltageLock != _currentActiveSliderValues[SliderValueDefaults.VoltageLock]
            || FrequencyLock != _currentActiveSliderValues[SliderValueDefaults.FrequencyLock])
        {
            if (!WaiverSigned)
            {
                return false;
            }

            _igcs.SetOverclockGpuLock(VoltageLock, FrequencyLock);
        }
        
        if (GpuVoltageOffsetSliderValue != _currentActiveSliderValues[SliderValueDefaults.GpuVoltageOffset])
        {
            if (!WaiverSigned)
            {
                return false;
            }
            _igcs.SetOverclockGpuVoltageOffset(GpuVoltageOffsetSliderValue);
        }
        if (GpuFrequencyOffsetSliderValue != _currentActiveSliderValues[SliderValueDefaults.GpuFrequencyOffset])
        {
            if (!WaiverSigned)
            {
                return false;
            }
            _igcs.SetOverclockGpuFrequencyOffset(GpuFrequencyOffsetSliderValue);
        }

        // check if gpu driver accepted values or adjust some and also reset CurrentValues variable
        GetOverclockingValues(skipTempLimit: doSkipTempLimit);

        return true;
    }

    public void SetOverclockWaiver()
    {
        _igcs.SetOverclockWaiver();
    }

    public Task TelemetryOverlayEnabledChanged()
    {
        // do telemetry overlay stuff here
        
        return Task.CompletedTask;
    }

    public async void OnNavigatedFrom()
    {
        if (_tickTimerTask == null || _tickTimerTaskCancellationTokenSource == null)
        {
            return;
        }

        try
        {
            _tickTimerTaskCancellationTokenSource?.Cancel();
            if (_tickTimerTask != null && _tickTimerTask?.Status != TaskStatus.WaitingForActivation)
            {
                await _tickTimerTask!;
            }
        }
        catch (OperationCanceledException ex)
        {
            Debug.WriteLine("[PerformanceViewModel]: TickTimer - " +
                            $"{nameof(OperationCanceledException)} thrown with message: {ex.Message}");
        }
        finally
        {
            _tickTimerTaskCancellationTokenSource?.Dispose();
        }
    }
}
