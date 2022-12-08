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

    public string SHA1
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

    public WebArcDriver(string version, DateTime driverDate, Uri downloadUri, string size, string sha1, Uri releaseNotesUri, string releaseDescription = "")
    {
        DriverVersion = new ArcDriverVersion(version);
        DriverDate = driverDate;
        DownloadUri = downloadUri;
        Size = size;
        SHA1 = sha1;
        ReleaseNotesUri = releaseNotesUri;
        ReleaseDescription = releaseDescription;
    }

    public string FriendlyDate => $"{DriverDate.Date.ToString(CultureInfo.CurrentUICulture).Split(" ")[0]}";
    public override string ToString() => $"Driver-Version: {DriverVersion}";
}
