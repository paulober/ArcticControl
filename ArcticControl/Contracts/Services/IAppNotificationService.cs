using System.Collections.Specialized;

namespace ArcticControl.Contracts.Services;

public interface IAppNotificationService
{
    void Initialize();

    bool Show(string payload);

    bool ShowMessage(string message);

    bool ShowWithActionAndProgressBar(string payload, string action, string status, string title, double pbValue, string valueStringOverride);

    NameValueCollection ParseArguments(string arguments);

    void Unregister();
}
