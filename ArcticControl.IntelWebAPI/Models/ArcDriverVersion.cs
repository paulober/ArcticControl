using System.ComponentModel.DataAnnotations;

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
    public uint WddmVersion
    {
        get; set;
    }

    /// <summary>
    /// Unused Field
    /// </summary>
    public ushort UnusedField
    {
        get; set; 
    }

    /// <summary>
    /// Combind with Part2 results in a 7 digit Build Number.
    /// </summary>
    [MaxLength(3)]
    public string BuildNumberPart1
    {
        get; set;
    }

    /// <summary>
    /// Combind with Part1 results in a 7 digit Build Number.
    /// </summary>
    [MaxLength(4)]
    public string BuildNumberPart2
    {
        get; set;
    }

    public LocalArcDriverState LocalState
    {
        get; set;
    } = LocalArcDriverState.Old;

    public bool IsLatest { get; set; } = false;

    public ArcDriverVersion(string version)
    {
        var splitted = version.Split('.');
        WddmVersion = uint.Parse(splitted[0]);
        UnusedField = ushort.Parse(splitted[1]);
        BuildNumberPart1 = splitted[2];
        BuildNumberPart2 = splitted[3];
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


    // overloaded equals comparisson
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
        throw new NotImplementedException();
    }

    public bool Equals(ArcDriverVersion? other) 
    {
        if (other != null)
        {
            return other.WddmVersion == WddmVersion
                && other.UnusedField == UnusedField
                && other.BuildNumberPart1 == BuildNumberPart1
                && other.BuildNumberPart2 == BuildNumberPart2;
        }

        return false;
    }
}
