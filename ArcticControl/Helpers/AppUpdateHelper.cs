using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Microsoft.AppCenter.Crashes;
using Windows.Foundation;
using Windows.Management.Deployment;
using Windows.Storage;

namespace ArcticControl.Helpers;
internal class AppUpdateHelper
{
    public static async Task<int> UpdateApplicationAsync(string inputPackageUri)
    {
        int returnValue = 0;

        // download update
        var httpClientFactory = App.GetService<IHttpClientFactory>();
        using var client = httpClientFactory.CreateClient("AppRepository");
        var localFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(inputPackageUri, CreationCollisionOption.ReplaceExisting);
        using (var msixStream = await client.GetStreamAsync(inputPackageUri))
        {
            // write
            using var fsStream = await localFile.OpenStreamForWriteAsync();
            await msixStream.CopyToAsync(fsStream);
        }

        var packageUri = new Uri(localFile.Path);
        var packageManager = new PackageManager();
        IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress>? deploymentOperation = null;
        try
        {
            deploymentOperation = packageManager.AddPackageAsync(
                packageUri,
                null,
                DeploymentOptions.ForceApplicationShutdown);
        }
        catch(Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Crashes.TrackError(ex);
            returnValue = 1;
        }

        uint res = RelaunchHelper.RegisterApplicationRestart(null, RelaunchHelper.RestartFlags.NONE);
        
        if (deploymentOperation != null)
        {
            var opCompletedEvent = new ManualResetEvent(false);
            
            deploymentOperation.Completed = (depProgress, status) =>
            {
                opCompletedEvent.Set();
            };
            opCompletedEvent.WaitOne();

            if (deploymentOperation.Status == AsyncStatus.Error)
            {
                var deploymentResult = deploymentOperation.GetResults();
                Debug.WriteLine("Error code: {0}", deploymentOperation.ErrorCode);
                Debug.WriteLine("Error text: {0}", deploymentResult.ErrorText);
                returnValue = 1;
            }
            else if (deploymentOperation.Status == AsyncStatus.Completed)
            {
                Debug.WriteLine("Installation succeeded");
            }
            else
            {
                returnValue = 1;
                Debug.WriteLine("Installation status unknown");
            }
        }

        return returnValue;
    }

    public static async Task<string> GetVersionDataFromServerAsync()
    {
        try
        {
            var httpClientFactory = App.GetService<IHttpClientFactory>();
            using var client = httpClientFactory.CreateClient("AppRepository");
            var current = await client.GetStringAsync("Version.txt");
            return current;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Crashes.TrackError(ex);
            return "0.0.0.0";
        }
    }

    public static async Task<int> UpdateNowHelperAsync(string version)
    {
        return await UpdateApplicationAsync($"ArcticControl_{version}_x64.msix");
    }

    public static async Task<string> CheckForUpdates()
    {
        var serverVersionData = await GetVersionDataFromServerAsync();

        if (serverVersionData == string.Empty)
        {
            return serverVersionData;
        }

        try
        {
            var newVersion = new Version(serverVersionData);
            var package = Package.Current;
            var packageVersion = package.Id.Version;
            var installedVersion = new Version(string.Format("{0}.{1}.{2}.{3}", packageVersion.Major, packageVersion.Minor,
                packageVersion.Build, packageVersion.Revision));

            if (newVersion.CompareTo(installedVersion) > 0)
            {
                return serverVersionData;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }

        return string.Empty;
    }
}
