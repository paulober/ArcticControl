using System.Diagnostics;
using System.Security.Principal;

namespace ArcticControl.Helpers;

public static class UACChecker
{
    public static bool IsAdministrator()
    {
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[UACChecker]: Error: " + ex.Message);
        }

        return false;
    }
}