using System.Globalization;

namespace ArcticControl.IntelWebAPI.Models;
public class WebArcDriver
{
    public ArcDriverVersion DriverVersion
    {
        get; set;
    }

    public DateTime DriverDate
    {
        get; set; 
    }

    public Uri DownloadUri
    {
        get; set;
    }

    public string Size
    {
        get; set;
    }

    /// <summary>
    /// Either a SHA1 or SHA256 hash.
    /// </summary>
    public string SHA
    {
        get; set;
    }

    public Uri ReleaseNotesUri
    {
        get; set;
    }

    // HTML from intel website
    public string ReleaseDescription
    {
        get; set;
    }

    public WebArcDriver(string version, DateTime driverDate, Uri downloadUri, string size, string sha, Uri releaseNotesUri, string releaseDescription = "")
    {
        DriverVersion = new ArcDriverVersion(version);
        DriverDate = driverDate;
        DownloadUri = downloadUri;
        Size = size;
        SHA = sha;
        ReleaseNotesUri = releaseNotesUri;
        ReleaseDescription = releaseDescription;
    }

    public string FriendlyDate => $"{DriverDate.Date.ToString(CultureInfo.CurrentUICulture).Split(" ")[0]}";
    public override string ToString() => $"Driver-Version: {DriverVersion}";
    public bool IsSHA256 => SHA.Length > 41;
}
