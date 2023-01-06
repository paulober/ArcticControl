using System.Diagnostics;
using System.Management;
using ArcticControl.Contracts.Services;
using ArcticControl.Contracts.ViewModels;
using ArcticControl.Core.Helpers;
using ArcticControl.Core.Models;
using ArcticControl.Helpers;
using ArcticControlGPUInterop;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel;

namespace ArcticControl.ViewModels;

internal enum SliderValueDefaults: ushort
{
    GPUVoltageOffset = 1,
    GPUPowerLimit = 2,
    GPUTemperatureLimit = 3,
    GPUFrequencyOffset = 4,
    FanSpeed = 5
}

internal static class SliderDefaultValues
{
    internal const ushort GPUFrequencyOffset = 0;
    internal const ushort GPUVoltageOffset = 0;
    internal const ushort GPUPowerLimit = 190;
    internal const ushort GPUTemperatureLimit = 90;
    internal const ushort FanSpeed = 20;
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

    private readonly Dictionary<SliderValueDefaults, double> CurrentActiveSliderValues = new()
    {
        { SliderValueDefaults.GPUPowerLimit, Convert.ToDouble(SliderDefaultValues.GPUPowerLimit) },
        { SliderValueDefaults.GPUTemperatureLimit, Convert.ToDouble(SliderDefaultValues.GPUTemperatureLimit) },
        { SliderValueDefaults.GPUVoltageOffset, Convert.ToDouble(SliderDefaultValues.GPUVoltageOffset) },
        { SliderValueDefaults.GPUFrequencyOffset, Convert.ToDouble(SliderDefaultValues.GPUFrequencyOffset) },
        { SliderValueDefaults.FanSpeed, Convert.ToDouble(SliderDefaultValues.FanSpeed) },
    };

    public bool WaiverSigned = false;

    private double _gpuFrequencyOffsetSliderValue = Convert.ToDouble(SliderDefaultValues.GPUFrequencyOffset);

    public double GPUFrequencyOffsetSliderValue
    {
        get => _gpuFrequencyOffsetSliderValue;
        set => SetProperty(ref _gpuFrequencyOffsetSliderValue, value);
    }

    private double _gpuVoltageOffsetSliderValue = Convert.ToDouble(SliderDefaultValues.GPUVoltageOffset);

    public double GPUVoltageOffsetSliderValue
    {
        get => _gpuVoltageOffsetSliderValue;
        set => SetProperty(ref _gpuVoltageOffsetSliderValue, value); 
    }

    private double _gpuPowerMaxLimit = 228.0;
    public double GPUPowerMaxLimit
    {
        get => _gpuPowerMaxLimit;
        set => SetProperty(ref _gpuPowerMaxLimit, value);
    }

    private double _gpuPowerLimitSliderValue = Convert.ToDouble(SliderDefaultValues.GPUPowerLimit);

    public double GPUPowerLimitSliderValue
    {
        get => _gpuPowerLimitSliderValue;
        set => SetProperty(ref _gpuPowerLimitSliderValue, value);
    }

    private double _gpuTemperatureLimitSliderValue = Convert.ToDouble(SliderDefaultValues.GPUTemperatureLimit);

    public double GPUTemperatureLimitSliderValue
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

    #region ValueDataObject properties to be able to update the values
    private PerformanceValueDataObject _cpuUtilizationObj 
        = new() { Title = "CPU Utilization", Value = "0.0", Unit = "%" };
    public PerformanceValueDataObject CPUUtilizationObj
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

    private PerformanceValueDataObject _gpuVolatageObj
        = new() { Title = "GPU Volatage", Value = "650", Unit = "mV" };
    public PerformanceValueDataObject GPUVolatageObj
    {
        get => _gpuVolatageObj;
        set => SetProperty(ref _gpuVolatageObj, value);
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

    private DispatcherQueue? _dpq;
    private CancellationTokenSource? _tickTimerTaskCancelationTokenSource;
    private Task? _tickTimerTask;
    //private readonly GPUInterop _gpuInterop;
    private readonly IIntelGraphicsControlService _igcs;
    private readonly ILocalSettingsService _localSettingsService;

    // framecounter, UI Binded Property, format, 
    private readonly List<Tuple<PerformanceSource, string>> _performanceCounters = new()
    {
        // handles property access via reflections
        // "Processor", "% Processor Time", "_Total"
        Tuple.Create(new PerformanceSource(new PerformanceSourceArgs
        { 
            Type = PerformanceSourceType.PerformanceCounter, 
            PerformanceCounterArgs = new string[] {"Processor Information", "% Processor Utility", "_Total"}, 
            Format = "0.0"
        }), nameof(CPUUtilizationObj)),
        Tuple.Create(new PerformanceSource(new PerformanceSourceArgs()
        {
            Type = PerformanceSourceType.ValueOffsetCallback,
            // PerformanceCounterArgs = new string[] {"Memory", "Available KBytes"},
            Format = "0.0",
            ValueOffsetCallback = (float value) =>
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
                        return Math.Round(percent, 0).ToString();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception on getting value offset for CPUUtilizationObj: " + ex.Message);
                }
                return "N/A";
            }
        }), nameof(MemoryUtilizationObj))/*,
        Tuple.Create(new PerformanceSource(new PerformanceSourceArgs()
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
            // ItemsSource refresh does not work because no available option for event
            /*foreach (var item in ViewModel.PerformanceValues)
            {
                switch (item.Title)
                {
                    case "CPU Utilization":
                        item.Value = _performanceCounter.NextValue().ToString("0.0");
                        break;
                    default:
                        item.Value = "0.0";
                        break;
                }
            }*/

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

        _tickTimerTaskCancelationTokenSource = new();
        var ct = _tickTimerTaskCancelationTokenSource.Token;
        _tickTimerTask = Task.Run(async () =>
        {
            // Were we already canceled?
            ct.ThrowIfCancellationRequested();

            var tickTimer = new PeriodicTimer(TimeSpan.FromSeconds(0.5));
            // TODO: don't know if checking for IsCancellationRequested in while condition is good when using an async condition paralell
            while (await tickTimer.WaitForNextTickAsync() || ct.IsCancellationRequested)
            {
                if (ct.IsCancellationRequested)
                {
                    // Clean up here
                    // ...
                    // then
                    ct.ThrowIfCancellationRequested();
                } else { await DoTick(); }
            }
        }, ct);
    }

    // TODO: maybe switch to Task vs void
    public async void OnNavigatedTo(object parameter)
    {
        // ItemsSource placeholders setup
        /*
        PerformanceValues.Clear();

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

        // will call !GPUInterop finalizer automaticly after current scope ends
        /*using GPUInterop gpuInterop = new();
        //Debug.WriteLine("gpuInterop.TestApi(): " + gpuInterop.TestApi().ToString());
        var result = (bool)gpuInterop.InitCtlApi();
        if (result)
        {
            Debug.WriteLine("GPUInterop: " + gpuInterop.GetAdapterName());
            var res = (double)gpuInterop.GetOverclockVRAMVoltageOffset();
            var res1 = (double)gpuInterop.GetOverclockVRAMVoltageOffset();
            Debug.WriteLine(res);
        }*/

        //var result = (bool)_gpuInterop.InitCtlApi();
        if (_igcs.IsInitialized())
        {
            GetOverclockingValues();
            // TODO: move somewhere
            // GPUPowerTest

            var result = _igcs.InitPowerDomains();
            if (result)
            {
                Debug.WriteLine("PowerInit: Res true");
                var powerProps = _igcs.GetPowerProperties();
                if (powerProps != null)
                {
                    Debug.WriteLine(
                        $"PowerProps: CanControl-{powerProps.CanControl} ; DefaultLimit-{powerProps.DefaultLimit} ;" +
                        $" MinLimit-{powerProps.MinLimit} ; MaxLimit-{powerProps.MaxLimit}");
                }
                var powerLimits = _igcs.GetPowerLimits();
                if (powerLimits != null)
                {
                    Debug.WriteLine($"PowerLimits: " + Environment.NewLine +
                        $"SustainedPowerLimits: Enabled-{powerLimits.SustainedPowerLimit.Enabled} ; Power-{powerLimits.SustainedPowerLimit.Power} ; Interval-{powerLimits.SustainedPowerLimit.Interval}" + Environment.NewLine +
                        $"BurstPowerLimit: Enabled-{powerLimits.BurstPowerLimit.Enabled} ; Power-{powerLimits.BurstPowerLimit.Power}" + Environment.NewLine +
                        $"PeakPowerLimit: PowerDC-{powerLimits.PeakPowerLimit.PowerDC} ; PowerAC-{powerLimits.PeakPowerLimit.PowerAC}");
                }
            }
        }

        var settingsGPUPowerMaxLimit = await _localSettingsService.ReadSettingAsync<double>(LocalSettingsKeys.GPUPowerMaxLimit);
        GPUPowerMaxLimit = settingsGPUPowerMaxLimit < 228.0 ? 228.0 : settingsGPUPowerMaxLimit;
    }

    private void GetOverclockingValues(bool skipTempLimit = false)
    {
        var powerLimit = _igcs.GetOverclockPowerLimit();
        var tempLimit = skipTempLimit 
            ? GPUTemperatureLimitSliderValue 
            : _igcs.GetOverclockTemperatureLimit();
        var gpuVoltageOffset = _igcs.GetOverclockGPUVoltageOffset();
        var gpuFrequencyOffset = _igcs.GetOverclockGPUFrequencyOffset();

        GPUPowerLimitSliderValue = powerLimit;
        GPUTemperatureLimitSliderValue = tempLimit;
        GPUVoltageOffsetSliderValue = gpuVoltageOffset;
        GPUFrequencyOffsetSliderValue = gpuFrequencyOffset;

        CurrentActiveSliderValues[SliderValueDefaults.GPUPowerLimit] = powerLimit;
        CurrentActiveSliderValues[SliderValueDefaults.GPUTemperatureLimit] = tempLimit;
        CurrentActiveSliderValues[SliderValueDefaults.GPUVoltageOffset] = gpuVoltageOffset;
        CurrentActiveSliderValues[SliderValueDefaults.GPUFrequencyOffset] = gpuFrequencyOffset;

        // TODO: Fan stuff
        // if (FanSpeedFixed)...
    }

    public void ResetToDefaultSliderValues()
    {
        GPUPowerLimitSliderValue = Convert.ToDouble(SliderDefaultValues.GPUPowerLimit);
        GPUTemperatureLimitSliderValue = Convert.ToDouble(SliderDefaultValues.GPUTemperatureLimit);
        GPUVoltageOffsetSliderValue = Convert.ToDouble(SliderDefaultValues.GPUVoltageOffset);
        GPUFrequencyOffsetSliderValue = Convert.ToDouble(SliderDefaultValues.GPUFrequencyOffset);
    }

    public void RevertSliderChanges(object? sender, RoutedEventArgs? e)
    {
        GPUPowerLimitSliderValue = CurrentActiveSliderValues[SliderValueDefaults.GPUPowerLimit];
        GPUTemperatureLimitSliderValue = CurrentActiveSliderValues[SliderValueDefaults.GPUTemperatureLimit];
        GPUVoltageOffsetSliderValue = CurrentActiveSliderValues[SliderValueDefaults.GPUVoltageOffset];
        GPUFrequencyOffsetSliderValue = CurrentActiveSliderValues[SliderValueDefaults.GPUFrequencyOffset];
    }

    public bool ApplyChanges()
    {
        var doSkipTempLimit = false;

        if (GPUPowerLimitSliderValue != CurrentActiveSliderValues[SliderValueDefaults.GPUPowerLimit])
        {
            // *1000 to convert W in mW
            _igcs.SetOverclockPowerLimit(GPUPowerLimitSliderValue*1000.0);
        }
        if (GPUTemperatureLimitSliderValue != CurrentActiveSliderValues[SliderValueDefaults.GPUTemperatureLimit])
        {
            if (!WaiverSigned)
            {
                return false;
            }
            var result = _igcs.SetOverclockTemperatureLimit(GPUTemperatureLimitSliderValue);
            if (result)
            {
                // skip temp limit check because the gpu needs time to refresh values so it would return old tempLimit
                doSkipTempLimit = true;
            }
        }
        if (GPUVoltageOffsetSliderValue != CurrentActiveSliderValues[SliderValueDefaults.GPUVoltageOffset])
        {
            if (!WaiverSigned)
            {
                return false;
            }
            _igcs.SetOverclockGPUVoltageOffset(GPUVoltageOffsetSliderValue);
        }
        if (GPUFrequencyOffsetSliderValue != CurrentActiveSliderValues[SliderValueDefaults.GPUFrequencyOffset])
        {
            if (!WaiverSigned)
            {
                return false;
            }
            _igcs.SetOverclockGPUFrequencyOffset(GPUFrequencyOffsetSliderValue);
        }

        // check if gpu driver acepted values or adjust some and also reset CurrentValues variable
        GetOverclockingValues(skipTempLimit: doSkipTempLimit);

        return true;
    }

    public void SetOverclockWaiver()
    {
        _igcs.SetOverclockWaiver();
    }

    public async void OnNavigatedFrom()
    {
        if (_tickTimerTask != null && _tickTimerTaskCancelationTokenSource != null)
        {
            _tickTimerTaskCancelationTokenSource?.Cancel();

            try
            {
                await _tickTimerTask;
            }
            catch (OperationCanceledException ex)
            {
                Debug.WriteLine($"{nameof(OperationCanceledException)} thrown with message: {ex.Message}");
            }
            finally
            {
                _tickTimerTaskCancelationTokenSource?.Dispose();
            }
        }
    }
}
