namespace ArcticControl.Core.Helpers;
public class LocalSettingsKeys
{
    // bases / category root key
    private const string BASE_PRIVACYPOLICY = "PrivacyPolicy:";
    private const string BASE_PERFORMANCE = "Performance:";

    public const string PrivacyPolicyConsented = $"{BASE_PRIVACYPOLICY}Consented";
    public const string RequestedTheme = "AppBackgroundRequestedTheme";
    public const string GPUPowerMaxLimit = $"{BASE_PERFORMANCE}GPUPowerMaxLimit";
}
