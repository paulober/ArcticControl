namespace ArcticControl.Models;

public struct PowerProperties
{
    public bool CanControl;
    public int DefaultLimit;
    public int MinLimit;
    public int MaxLimit;
}

public struct SustainedPowerLimit
{
    public bool Enabled;
    public int Power;
    public int Interval;
}

public struct BurstPowerLimit
{
    public bool Enabled;
    public int Power;
}

public struct PeakPowerLimit
{
    public int PowerAC;
    public int PowerDC;
}

public struct PowerLimitsCombination
{
    public SustainedPowerLimit SustainedPowerLimit;
    public BurstPowerLimit BurstPowerLimit;
    public PeakPowerLimit PeakPowerLimit;
}

public struct FanProperties
{
    public bool CanControl;
    public uint SupportedModes;
    public uint SupportedUnits;
    public int MaxRPM;
    public int MaxPoints;
}

public class FrequencyProperties
{ 
    public bool CanControl;
    public double HardwareMin;
    public double HardwareMax;
};

public enum FrequencyCap : uint
{
    AveragePowerCap = 1,
    BurstPowerCap = 2,
    CurrentLimit = 4,
    ThermalLimit = 8,
    PsuAlert = 16,
    SoftwareRange = 32,
    HardwareRange = 64,
    Max = 0x80000000,
    Unknown = 999
};

public class FrequencyState
{
    public double CurrentVoltage;
    public double RequestedFrequency;
    public double TDPFrequency;
    public double EfficientFrequency;
    public double ActualFrequency;
    public FrequencyCap Cap;
};

public class PCIeProperties
{
    public bool IsReBarSupported;
    public bool IsReBarEnabled;
    public int Lanes;
    public int Gen;
};
