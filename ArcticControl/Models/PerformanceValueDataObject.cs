using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArcticControl.Models;
public class PerformanceValueDataObject : ObservableObject
{
    private string _title;
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private string _value;
    public string Value 
    { 
        get => _value;
        set => SetProperty(ref _value, value); 
    }

    private string _unit;
    public string Unit
    {
        get => _unit;
        set => SetProperty(ref _unit, value);
    }

    public PerformanceValueDataObject(string title = "", string value = "", string unit = "")
    {
        _title = title;
        _value = value;
        _unit = unit;
    }

    public static PerformanceValueDataObject Copy(PerformanceValueDataObject source) 
        => new() { Title = source.Title, Unit = source.Unit, Value = source.Value };

    public static PerformanceValueDataObject CopyWithNewValue(PerformanceValueDataObject source, string newValue) 
        => new() { Title = source.Title, Unit = source.Unit, Value = newValue };
}
