using System.Collections.ObjectModel;
using ArcticControl.Contracts.ViewModels;
using ArcticControl.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArcticControl.ViewModels;

public class PerformanceViewModel : ObservableRecipient, INavigationAware
{
    public ObservableCollection<PerformanceValueDataObject> PerformanceValues { get; } 
        = new ObservableCollection<PerformanceValueDataObject>();

    public PerformanceViewModel()
    {
    }

    public void OnNavigatedTo(object parameter)
    {

        PerformanceValues.Clear();

        PerformanceValues.Add(new PerformanceValueDataObject { Title = "CPU Utilization", Value = "9.1", Unit = "%" });
        PerformanceValues.Add(new PerformanceValueDataObject { Title="Memory Utilization", Value="17.8", Unit="%" });
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
    }
    public void OnNavigatedFrom()
    {
    
    }
}
