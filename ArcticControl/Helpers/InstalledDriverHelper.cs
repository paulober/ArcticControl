using System.Diagnostics;
using System.Management;
using Microsoft.AppCenter.Crashes;

namespace ArcticControl.Helpers;

internal static class InstalledDriverHelper
{
    public static bool IsIntelGraphicsDriverInstalled()
    {
        try
        {
            /*//SELECT PathName FROM Win32_SystemDriver
            //WHERE State = 'Running' AND ServiceType = 'Kernel Driver' AND Caption LIKE '%intel%'
            SelectQuery query = new("Win32_SystemDriver");
            query.Condition = "State = 'Running' AND ServiceType = 'Kernel Driver' AND Caption LIKE '%intel%'";
            query.SelectedProperties.Add("PathName");
            ManagementObjectSearcher searcher = new(query);
            var drivers = searcher.Get();
            if (drivers.Count > 0)
            {
                foreach (var driver in drivers)
                {
                    var pathName = driver.GetPropertyValue("PathName");
                    if (pathName is string pn)
                    {
                        if (
                            pn.Contains("igdkmd64.sys")
                            || pn.Contains("igdkmdn64.sys")
                            || pn.Contains("igdkmdnd64.sys")
                        )
                        {
                            return true;
                        }
                    }
                }
            }
            else
            {
                Debug.WriteLine("Could not find any installed Intel Graphics Driver");
            }*/
            //maybe does not work on store tester pcs return Path.Exists("C:\\Windows\\System32\\ControlLib.dll");

            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            foreach (ManagementBaseObject mo in searcher.Get())
            {
                try
                {
                    PropertyData description = mo.Properties["Description"];
                    if (description != null)
                    {
                        string desc = (string)description.Value;
                        if (desc.Contains("Intel(R) Arc(TM)"))
                        {
                            return Path.Exists("C:\\Windows\\System32\\ControlLib.dll");
                        }
                    }
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
        } catch (Exception ex)
        {
            Debug.WriteLine("Error searching for Intel driver: " + ex);
            Crashes.TrackError(ex);
        }

        return false;
    }

    /// <summary>
    /// Returns the version of the Intel Graphics driver. If no Intel GPU is installed it returns empty.
    /// </summary>
    /// <returns></returns>
    public static string GetArcDriverVersion()
    {
        try
        {
            /*SelectQuery query = new("Win32_PnPSignedDriver");
            query.Condition = "DeviceClass = 'DISPLAY' AND DriverProviderName LIKE '%Intel%'";
            query.SelectedProperties.Add("DriverVersion");
            ManagementObjectSearcher searcher = new(query);
            var drivers = searcher.Get();
            if (drivers.Count > 0)
            {
                foreach (var driver in drivers)
                {
                    var driverVersion = driver.GetPropertyValue("DriverVersion");
                    if (driverVersion is string dv)
                    {
                        return dv;
                    }
                }
            }*/

            // maybe faster than searching all PnPSignedDrivers
            SelectQuery query = new("Win32_VideoController")
            {
                Condition = "Caption LIKE '%Intel(R) Arc(TM)%' OR Name LIKE '%Intel(R) Arc(TM)%'"
            };
            query.SelectedProperties.Add("DriverVersion");
            ManagementObjectSearcher searcher = new(query);
            var drivers = searcher.Get();
            if (drivers.Count > 0)
            {
                foreach (var driver in drivers)
                {
                    var driverVersion = driver.GetPropertyValue("DriverVersion");
                    if (driverVersion is string dv)
                    {
                        return dv;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error searching for signed Intel driver: " + ex.ToString());
        }

        return string.Empty;
    }
}
