using System.Collections.ObjectModel;
using ArcticControl.Contracts.Services;
using ArcticControl.Contracts.ViewModels;
using ArcticControl.Helpers;
using ArcticControl.IntelWebAPI.Contracts.Services;
using ArcticControl.IntelWebAPI.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArcticControl.ViewModels;

public class DriversViewModel : ObservableRecipient, INavigationAware
{
    private readonly IWebArcDriversService _webArcDriversService;
    private WebArcDriver? _newSelected;
    private WebArcDriver? _oldSelected;

    public WebArcDriver? NewSelected
    {
        get => _newSelected;
        set
        {
            // deselect driver in old drivers list (like a connection between the two)
            if (_oldSelected != null)
            {
                _oldSelected = null;
                OnPropertyChanged(nameof(OldSelected));
                // does not trigger OnChanged for OldSelected
                //SetProperty(ref _oldSelected, null);
            }

            SetProperty(ref _newSelected, value);
            OnPropertyChanged(nameof(Selected));
        }
    }

    public WebArcDriver? OldSelected
    {
        get => _oldSelected;
        set
        {
            if (_newSelected != null)
            {
                _newSelected = null;
                OnPropertyChanged(nameof(NewSelected));
                // does not trigger OnChanged for OldSelected
                //SetProperty(ref _newSelected, null);
            }

            SetProperty(ref _oldSelected, value);
            OnPropertyChanged(nameof(Selected));
        }
    }

    public WebArcDriver? Selected =>
            // should only be one selected so no priority based problem here
            NewSelected ?? OldSelected;

    public ObservableCollection<WebArcDriver> NewWebArcDrivers { get; private set; } = new ObservableCollection<WebArcDriver>();
    public ObservableCollection<WebArcDriver> OldWebArcDrivers { get; private set; } = new ObservableCollection<WebArcDriver>();

    public DriversViewModel(IWebArcDriversService webArcDriversService)
    {
        _webArcDriversService= webArcDriversService;
    }

    public async void OnNavigatedTo(object parameter)
    {
        // do not clear to shorten loading times
        // only on refesh button clicked
        // WebArcDrivers.Clear();

        // drivers are sorted from new (Index 0) to oldest (Index ^1)
        var data = await _webArcDriversService.GetWebArcDriverDataAsync();
        // have to sort because current start webpage for driver details starts with 3802 and not the newest
        data = data.OrderByDescending(wac => wac.DriverVersion.GetBuildNumber());
        var arcDriverVersion = InstalledDriverHelper.GetArcDriverVersion();

        // flag gets set when all new drivers including the installed one has been added
        var newDriversAdded = false;
        foreach (var item in data)
        {
            if (newDriversAdded)
            {
                OldWebArcDrivers.Add(item);
            } 
            else
            {
                bool driverVersionEqToInstalled = item.DriverVersion.GetFullVersion() == arcDriverVersion;
                if (NewWebArcDrivers.Count == 0 
                    || driverVersionEqToInstalled 
                    || (arcDriverVersion == string.Empty && NewWebArcDrivers.Count < 2))
                {
                    // overwrite state for coloring and UI representation configuration of this driver
                    item.DriverVersion.LocalState =
                    driverVersionEqToInstalled
                    ? LocalArcDriverState.Current
                    : LocalArcDriverState.New;
                    NewWebArcDrivers.Add(item);

                    if (
                        driverVersionEqToInstalled
                        // load latest 2 drivers if intel arc driver is not installed
                        || (arcDriverVersion == string.Empty && NewWebArcDrivers.Count == 2))
                    {
                        newDriversAdded = true;
                    } 
                    else // would be equal to NewWebArcDrivers.Count == 1 && OldWebArcDrivers.Count == 0
                    {
                        // have to be latest as if no driver is installed Count
                        // would be != 2 and if installed then it would not
                        // be driverVersionInstalled but in got into this if, so it's newest
                        item.DriverVersion.IsLatest = true;
                    }
                }
                // already newest driver added to NewDrivers but item is not the installed one so move to OldDrivers
                else if (arcDriverVersion != string.Empty)
                {
                    // make it visible that they are newer than the installed one
                    item.DriverVersion.LocalState = LocalArcDriverState.New;
                    OldWebArcDrivers.Add(item);
                }
            }
        }
    }

    public void OnNavigatedFrom()
    {
    }

    public void EnsureItemSelected()
    {
        NewSelected ??= NewWebArcDrivers.Count > 0 ? NewWebArcDrivers.First() : null;
    }
}
