using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace ArcticControl.IntelWebAPI.Models;

public enum LocalArcDriverState : ushort
{
    // reversed order to save some memory
    Old,
    New, 
    Current,
}

/// <summary>
/// Specs from https://www.intel.de/content/www/de/de/support/articles/000005654/graphics.html.
/// </summary>
public class ArcDriverVersion : IEquatable<ArcDriverVersion>
{
    /// <summary>
    /// Operating System (WDDM version)
    /// </summary>
    private int WddmVersion
    {
        get;
    }

    /// <summary>
    /// Unused Field
    /// </summary>
    private short UnusedField
    {
        get;
    }

    /// <summary>
    /// Combind with Part2 results in a 7 digit Build Number.
    /// </summary>
    [MaxLength(3)]
    private string BuildNumberPart1
    {
        get;
    }

    /// <summary>
    /// Combind with Part1 results in a 7 digit Build Number.
    /// </summary>
    [MaxLength(4)]
    private string BuildNumberPart2
    {
        get;
    }

    [MaxLength(4)]
    private string BuildNumberAdditionalPart
    {
        get;
    }

    /// <summary>
    /// Just a state container for element grouping in the UI.
    /// </summary>
    public LocalArcDriverState LocalState
    {
        get; set;
    } = LocalArcDriverState.Old;

    /// <summary>
    /// Just a state for UI markup.
    /// </summary>
    public bool IsLatest { get; set; } = false;

    public ArcDriverVersion(string version)
    {
        // https://www.intel.com/content/www/us/en/support/articles/000005654/graphics.html

        var split = version.Split('.');
        WddmVersion = int.Parse(split[0]);
        UnusedField = short.Parse(split[1]);

        switch (split.Length)
        {

            case 4:
                BuildNumberPart1 = split[2];
                BuildNumberPart2 = split[3];
                BuildNumberAdditionalPart = "0";
                break;
            case 5:
                BuildNumberPart1 = split[2];
                BuildNumberPart2 = split[3].Split("_")[0];
                BuildNumberAdditionalPart = split[4];
                break;
            default:
                BuildNumberPart1 = "0";
                BuildNumberPart2 = "0";
                BuildNumberAdditionalPart = "0";
                Debug.WriteLine("Invalid Arc Driver Version!");
                break;
        }
    }

    /// <summary>
    /// 7 digit Build Number.
    /// </summary>
    /// <returns></returns>
    public uint GetBuildNumber() => uint.Parse(BuildNumberPart1 + BuildNumberPart2);
    /// <summary>
    /// Get BuildNumber for UI display.
    /// </summary>
    public string BuildNumber => "Build: " + string.Join('.', BuildNumberPart1, BuildNumberPart2);
    public string GetFullVersion() => string.Join('.', WddmVersion.ToString(), UnusedField.ToString(), BuildNumberPart1, BuildNumberPart2);
    public string GetAdditionalBuildNumber() => BuildNumberAdditionalPart;
    
    /// <summary>
    /// String representation of driver version with Intel prefix.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => "Intel® Graphics Driver " + GetFullVersion() + (BuildNumberAdditionalPart != "0" ? "_" + BuildNumberAdditionalPart : "");

    // overloaded operators
    // TODO: need to update if WDDM (Windows Display Driver Model) gets an update so the WddmVersion will change
    public static bool operator <(ArcDriverVersion left, ArcDriverVersion right)
    {
        return left.GetBuildNumber() < right.GetBuildNumber();
    }

    public static bool operator >(ArcDriverVersion left, ArcDriverVersion right)
    {
        return left.GetBuildNumber() > right.GetBuildNumber();
    }


    // overloaded equals comparison
    public override bool Equals(object? obj)
    {
        if (obj is ArcDriverVersion adv)
        {
            return Equals(adv);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(WddmVersion, UnusedField, BuildNumberPart1, BuildNumberPart2);
    }

    public bool Equals(ArcDriverVersion? other) 
    {
        if (other != null)
        {
            return other.WddmVersion == WddmVersion
                && other.UnusedField == UnusedField
                && other.BuildNumberPart1 == BuildNumberPart1
                && other.BuildNumberPart2 == BuildNumberPart2
                && other.BuildNumberAdditionalPart == BuildNumberAdditionalPart;
        }

        return false;
    }
}
