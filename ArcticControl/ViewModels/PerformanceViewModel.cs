using System.Diagnostics;
using System.Management;
using ArcticControl.Contracts.ViewModels;
using ArcticControl.Core.Models;
using ArcticControl.Helpers;
using ArcticControlGPUInterop;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;

namespace ArcticControl.ViewModels;

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

    public PerformanceViewModel()
    {
    }

    private DispatcherQueue? _dpq;
    private CancellationTokenSource? _tickTimerTaskCancelationTokenSource;
    private Task? _tickTimerTask;

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
                        FreePhysicalMemory = double.Parse(mo["FreePhysicalMemory"].ToString()),
                        TotalVisibleMemorySize = double.Parse(mo["TotalVisibleMemorySize"].ToString())
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
        }), nameof(MemoryUtilizationObj)),
        Tuple.Create(new PerformanceSource(new PerformanceSourceArgs()
        {
            Type = PerformanceSourceType.PerformanceCounter,
            PerformanceCounterArgs = new string[] {"Processor", "% Processor Time", "_Total"},
            Format = "0.0"
        }), nameof(GPUVolatageObj))
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

    public void OnNavigatedTo(object parameter)
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
        using GPUInterop gpuInterop = new();
        //Debug.WriteLine("gpuInterop.TestApi(): " + gpuInterop.TestApi().ToString());
        var result = (bool)gpuInterop.InitCtlApi();
        if (result)
        {
            Debug.WriteLine("GPUInterop: " + gpuInterop.GetAdapterName());
            var res = (double)gpuInterop.GetOverclockVRAMVoltageOffset();
            var res1 = (double)gpuInterop.GetOverclockVRAMFrequencyOffset();
            Debug.WriteLine(res);
        }
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
