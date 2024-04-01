using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;
using ArcticControl.IntelWebAPI.Contracts.Services;
using ArcticControl.IntelWebAPI.Models;
using Microsoft.Extensions.Logging;

namespace ArcticControl.IntelWebAPI.Services;
public partial class WebArcDriversService : IWebArcDriversService
{
    // TODO: use logger in this class

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebArcDriversService> _logger;

    private readonly List<WebArcDriver> _webArcDrivers = new();

    public WebArcDriversService(IHttpClientFactory httpClientFactory, ILogger<WebArcDriversService> logger)
    {
        (_httpClientFactory, _logger) = (httpClientFactory, logger);
    }


    /// <summary>
    /// Preload data of driver by scaning its detail webpage.
    /// </summary>
    /// <param name="webPage">The html page as string.</param>
    /// <returns>Tuple of driverId and driver date.</returns>
    /// <exception cref="Exception">When conversion of retrieved values does not work like expected. Maybe the website changed.</exception>
    private Tuple<DateTime, string, Uri, string, string, Uri, string> PreloadWebDriver(string webPage)
    {
        // date long complicated version
        /*var bannerActionsRegex = new Regex("<span class=\"dc-page-banner-actions-action__value\".*?>(.*?)<\\/span>");
        var bannerActions = bannerActionsRegex.Matches(webPage);
        if (bannerActions.Count != 2)
        {
            throw new Exception($"PreloadWebDriver - bannerActions count {bannerActions.Count} is not equal to expected 2");
        }

        var matchOne = bannerActions[0].Groups[1].Value;
        var matchTwo = bannerActions[1].Groups[1].Value;

        // is entry/software product id by intel not driver id
        long driverId;
        DateTime driverDate;

        if (matchTwo.Contains('/'))
        {
            driverId = long.Parse(matchOne);
            // expected format mm/dd/yyyy
            var dateParts = matchTwo.Split('/');
            driverDate = new DateTime(int.Parse(dateParts[2]), int.Parse(dateParts[0]), int.Parse(dateParts[1]));
        } 
        else
        {
            driverId = long.Parse(matchTwo);
            // expected format mm/dd/yyyy
            var dateParts = matchOne.Split('/');
            driverDate = new DateTime(int.Parse(dateParts[2]), int.Parse(dateParts[0]), int.Parse(dateParts[1]));
        }*/

        // get date //
        var dateRegex = new Regex("<meta name=\"lastModifieddate\" content=\"(?<date>.*?)\"\\/>");

        var dateMatch = dateRegex.Match(webPage);
        if (!dateMatch.Success || !dateMatch.Groups.ContainsKey("date"))
        {
            throw new Exception("PreloadWebDriver - date found");
        }
        // expected format mm/dd/yyyy hh:mm:ss
        var dateParts = dateMatch.Groups["date"].Value.Split(" ")[0].Split('/');
        var driverDate = new DateTime(int.Parse(dateParts[2]), int.Parse(dateParts[0]), int.Parse(dateParts[1]));

        // get version //

        // complicated one
        //var versionRegex = new Regex("(?:<p class=\"dc-page-short-description__text\">(?:(?:.*?\\r?\\n?)*?(?<version>\\d+\\.\\d+?\\.\\d+\\.\\d+)(?:.*?\\r?\\n?)*?)<\\/p>)");

        // meta tag based
        string? version;
        try
        {
            var versionRegex = VersionRegex();

            var versionMatch = versionRegex.Match(webPage);
            if (!versionMatch.Success || versionMatch.Groups.Count != 3 || versionMatch.Groups["version"] == null || !versionMatch.Groups["version"].Success)
            {
                Console.WriteLine(versionMatch.Groups);
                throw new Exception("PreloadWebDriver - no version new found");
            }
            version = versionMatch.Groups["version"].Value;
        }
        catch (Exception)
        {
            var versionRegex = VersionRegexOld();

            var versionMatch = versionRegex.Match(webPage);
            if (!versionMatch.Success || versionMatch.Groups["version"] == null || !versionMatch.Groups["version"].Success)
            {
                throw new Exception("PreloadWebDriver - no version old found");
            }
            version = versionMatch.Groups["version"].Value;
        }

        // get download uri //

        // complicated one
        //var downloadRegex = new Regex("<button.+?data-wap_ref=.+?download-button\".+?available-download-button__cta\".+?data-modal-id=\"2\".+?data-href=\"(.*?)\">");

        // meta tag based - does not exist anymore
        /*var downloadRegex = DownloadRegex();

        var downloadMatch = downloadRegex.Match(webPage) ?? throw new Exception("PreloadWebDriver - download uri not found");
        var downloadUri = new Uri(downloadMatch.Groups[1].Value, UriKind.Absolute);*/

        // check if exe or zip download comes first
        var downloadsOrderRegex = DownloadsOrderRegex();

        var downloadsOrder = downloadsOrderRegex.Matches(webPage);
        if (downloadsOrder == null || downloadsOrder.Count < 1)
        {
            throw new Exception("PreloadWebDriver - no downloads order found");
        }
        var downloadIndex = downloadsOrder.Last().Groups["downloadlink"].Value.Contains(".exe") ? downloadsOrder.Count - 1 : 0;
        var downloadUri = new Uri(downloadsOrder[downloadIndex].Groups["downloadlink"].Value, UriKind.Absolute);

        var sizeRegex = SizeRegex();

        var sizeMatches = sizeRegex.Matches(webPage);
        if (sizeMatches == null || sizeMatches.Count < 1)
        {
            throw new Exception("PreloadWebDriver - could not load download sizes");
        }
        var driverSize = sizeMatches[downloadIndex].Groups["size"].Value;

        var sha256Regex = SHA256Regex();
        var sha1Regex = SHA1Regex();

        var sha256Matches = sha256Regex.Matches(webPage);
        var sha1Matches = sha1Regex.Matches(webPage);
        var isSha256 = sha256Matches != null && sha256Matches.Count >= 1;
        if (!isSha256 && (sha1Matches == null || sha1Matches.Count < 1))
        {
            throw new Exception("PreloadWebDriver - could not load sha1 or sha256 hashes");
        }
        var sha1Hash = (isSha256 ? sha256Matches[downloadIndex] : sha1Matches[downloadIndex]).Groups[isSha256 ? "sha256" : "sha1"].Value;

        var releaseNotesRegex = ReleaseNotesRegex();

        var releaseNotesMatch = releaseNotesRegex.Match(webPage);
        if (releaseNotesMatch == null || !releaseNotesMatch.Groups.ContainsKey("url"))
        {
            throw new Exception("PreloadWebDriver - could not find release-notes");
        }
        var releaseNotes = new Uri(releaseNotesMatch.Groups["url"].Value);

        // get description html from intel website
        var startIdx = webPage.IndexOf("<div data-bundle-id=\"downloadcenter.components.content.dcPageDetailedDescription\" class=\"dc-page-detailed-description\">");
        const string endMarker = "\n</div>\n</div>";
        var endIdx = webPage.IndexOf(endMarker, startIdx);

        var driverDesc = webPage.Substring(startIdx, endIdx - startIdx + endMarker.Length);

        return Tuple.Create(driverDate, version, downloadUri, driverSize, sha1Hash, releaseNotes, driverDesc);
    }

    public async Task<bool> PreloadWebArcDriverDataAsync()
    {
        _webArcDrivers.Clear();

        using var client = _httpClientFactory.CreateClient("Intel");

        try
        {
            //using var response = await client.GetAsync("/content/www/de/de/download/726609/intel-arc-graphics-windows-dch-driver.html");
            using var response = await client.GetAsync("/content/www/us/en/download/785597/intel-arc-iris-xe-graphics-windows.html");
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();

#if DEBUG
            var sw = Stopwatch.StartNew();
#endif
            var result = PreloadWebDriver(responseBody);
#if DEBUG
            sw.Stop();
            Debug.WriteLine("Collected first intel driver data in " + sw.ElapsedMilliseconds.ToString() + " ms");
#endif
            var splitedSize = result.Item4.Split(" ");
            _webArcDrivers.Add(new WebArcDriver(
                result.Item2,
                result.Item1,
                result.Item3,
                // format the en-US number to its representation in the current culture
                double.Parse(splitedSize[0], new CultureInfo("en-US")).ToString("N", CultureInfo.CurrentCulture) + " " + splitedSize[1],
                result.Item5,
                result.Item6,
                result.Item7
            ));

            // get other driver detail page urls - does skip link marked as selected
            var otherDriverUrlsRegex = new Regex("<select.*?id=\"version-driver-select\".*?>.|\\r|\\n*?<option value=\"(?<url>\\/.*?\\.html)\">");
            var matches = otherDriverUrlsRegex.Matches(responseBody) ?? throw new Exception("PreloadWebDriver - Cannot match other driver links!");
            foreach (var match in matches)
            {
                if (match != null && match?.GetType() == typeof(Match))
                {
                    var m = (Match)match;

                    // > 5 random number to avoid empty urls
                    if (m.Groups.ContainsKey("url") && m.Groups["url"].Value.Length > 5)
                    {
                        using var driverDetailResp = await client.GetAsync(m.Groups["url"].Value);
                        driverDetailResp.EnsureSuccessStatusCode();
                        var respBody = await driverDetailResp.Content.ReadAsStringAsync();
                        var driverDetails = PreloadWebDriver(respBody);

                        if (driverDetails != null)
                        {
                            _webArcDrivers.Add(new WebArcDriver(
                                driverDetails.Item2,
                                driverDetails.Item1,
                                driverDetails.Item3,
                                driverDetails.Item4,
                                driverDetails.Item5,
                                driverDetails.Item6,
                                driverDetails.Item7
                            ));
                        }
                    }
                }
            }

        }
        catch (Exception e)
        {
            Debug.WriteLine("\nException Cought!");
            Debug.WriteLine("Message: " + e.Message);
            await Task.CompletedTask;
            return false;
        }


        await Task.CompletedTask;
        return true;
    }

    public async Task<IEnumerable<WebArcDriver>> GetWebArcDriverDataAsync()
    {
        if (_webArcDrivers.Count == 0)
        {
            if (!await PreloadWebArcDriverDataAsync())
            {
                return new List<WebArcDriver>();
            }
        }

        return _webArcDrivers;
    }

    [GeneratedRegex("<meta name=\"DownloadVersion\" content=\"(?<version>\\d+\\.\\d+\\.\\d+\\.\\d+_\\d+\\.\\d+)( [A-Za-z]+ [A-Za-z]+)?\"\\/>")]
    private static partial Regex VersionRegex();
    [GeneratedRegex("<meta name=\"DownloadVersion\" content=\"(?<version>\\d+\\.\\d+\\.\\d+\\.\\d+)( [A-Za-z]+ [A-Za-z]+)?\"\\/>")]
    private static partial Regex VersionRegexOld();
    [GeneratedRegex("<meta name=\"RecommendedDownloadUrl\" content=\"(?<url>.*?)\"\\/>")]
    private static partial Regex DownloadRegex();
    [GeneratedRegex("available-download-button__cta.*? data-href=\"(?<downloadlink>.*?(\\.zip|\\.exe))\"")]
    private static partial Regex DownloadsOrderRegex();
    [GeneratedRegex("Size: (?<size>[,. 0-9]+ (?:KB|MB|GB))(?:\\s|\\S|\\n)*?")]
    private static partial Regex SizeRegex();
    [GeneratedRegex("SHA1: (?<sha1>[A-Z. 0-9]+)(?:\\s|\\n)*?")]
    private static partial Regex SHA1Regex();
    [GeneratedRegex("SHA256: (?<sha256>[A-Z. 0-9]+)(?:\\s|\\n)*?")]
    private static partial Regex SHA256Regex();
    [GeneratedRegex("dc-page-documentation-list__item--fixed.*? href=\"(?<url>.*?\\.pdf)\"")]
    private static partial Regex ReleaseNotesRegex();
}
