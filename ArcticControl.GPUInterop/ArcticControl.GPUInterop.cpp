#include "pch.h"

#include <stdlib.h>
#include <conio.h>
#include <vector>
#include <string>

// available from Visual C++ 2008 => (if _MSC_VER > 1499)
#include <msclr/marshal.h>
using namespace msclr::interop;

#include "ArcticControl.GPUInterop.h"

std::string decode_ret_code(ctl_result_t res);
#define WRITE_LINE System::Diagnostics::Debug::WriteLine

String^ ArcticControlGPUInterop::GPUInterop::GetMyName()
{
    // throw gcnew System::NotImplementedException();
    // TODO: hier return-Anweisung eingeben
    return "Hi there in CLR. I'm the real slim shady!";
}

bool ArcticControlGPUInterop::GPUInterop::init_api()
{
    if (h_devices_ != nullptr || adapter_count_ != nullptr || h_api_handle_ != nullptr)
    {
        return true;
    }

    h_devices_ = nullptr;
    ctl_init_args_t ctl_init_args{};
    h_api_handle_ = static_cast<ctl_api_handle_t*>(malloc(sizeof(ctl_api_handle_t)));
    
    adapter_count_ = static_cast<uint32_t*>(malloc(sizeof*adapter_count_));
    *adapter_count_ = 0;

    _CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF);

    ctl_init_args.AppVersion = CTL_MAKE_VERSION(CTL_IMPL_MAJOR_VERSION, CTL_IMPL_MINOR_VERSION);
    ctl_init_args.flags = CTL_INIT_FLAG_USE_LEVEL_ZERO;
    ctl_init_args.Size = sizeof(ctl_init_args);
    ctl_init_args.Version = 0;
    ZeroMemory(&ctl_init_args.ApplicationUID, sizeof(ctl_application_id_t));

    if (ctl_result_t result = ctlInit(&ctl_init_args, h_api_handle_); CTL_RESULT_SUCCESS == result)
    {
        // enumerate all devices to check if some are available
        result = ctlEnumerateDevices(*h_api_handle_, adapter_count_, h_devices_);

        if (CTL_RESULT_SUCCESS == result)
        {
            h_devices_ = static_cast<ctl_device_adapter_handle_t*>(malloc(sizeof(ctl_device_adapter_handle_t) * *adapter_count_));

            if (h_devices_ != nullptr)
            {
                result = ctlEnumerateDevices(*h_api_handle_, adapter_count_, h_devices_);

                if (CTL_RESULT_SUCCESS == result)
                {
                    // check for compatibility (only Arc A-Series dGPUs)
                    ctl_device_adapter_properties_t dev_adapter_props{0};
                    dev_adapter_props.Size = sizeof(ctl_device_adapter_properties_t);
                    dev_adapter_props.pDeviceID = malloc(sizeof(LUID));
                    dev_adapter_props.device_id_size = sizeof(LUID);
                    dev_adapter_props.Version = 1;

                    if (dev_adapter_props.pDeviceID == nullptr)
                    {
                        return false;
                    }

                    for (uint32_t idx = 0; idx < *adapter_count_; idx++)
                    {
                        if (h_devices_[idx] != nullptr)
                        {
                            result = ctlGetDeviceProperties(h_devices_[idx], &dev_adapter_props);

                            if (result == CTL_RESULT_ERROR_UNSUPPORTED_VERSION)
                            {
                                WRITE_LINE("[GPUInterop]: init_api - ctlGetDeviceProperties version " +
                                           "mismatch - reducing version to 0 and retrying");
                                dev_adapter_props.Version = 0;
                                result = ctlGetDeviceProperties(h_devices_[idx], &dev_adapter_props);
                            }

                            if (result != CTL_RESULT_SUCCESS)
                            {
                                WRITE_LINE("[GPUInterop]: init_api - ctlGetDeviceProperties result: "
                                    + gcnew String(decode_ret_code(result).c_str()));
                                continue;
                            }

                            if (dev_adapter_props.device_type != CTL_DEVICE_TYPE_GRAPHICS)
                            {
                                WRITE_LINE("[GPUInterop]: init_api - Not a graphics device");
                                continue;
                            }

                            // if device vendor is not Intel
                            if (0x8086 != dev_adapter_props.pci_vendor_id)
                                continue;

                            if (array<uint32_t>::IndexOf(supported_device_ids_, dev_adapter_props.pci_device_id) > -1)
                            {
                                WRITE_LINE("[GPUInterop]: init_api - found supported device");
                                
                                // device is supported -> set it as selected
                                selected_device_ = idx;
                                break;
                            }
                        }
                    }
                    if (dev_adapter_props.pDeviceID != nullptr)
                    {
                        CTL_FREE_MEM(dev_adapter_props.pDeviceID);
                    }

                    // return if not supported device was found
                    if (selected_device_==-1)
                    {
                        WRITE_LINE("[GPUInterop]: init_api - no supported device found");

                        // free unmanaged resources as they wouldn't be needed anymore
                        CTL_FREE_MEM(h_devices_);
                        CTL_FREE_MEM(adapter_count_);

                        throw gcnew PlatformNotSupportedException;
                    }
                    // TODO: maybe remove all h_devices_ handles which are not needed
                    // end check for compatibility

                    // TODO: maybe move this into separate method
                    // get fan handles for later
                    fans_count_ = static_cast<uint32_t*>(malloc(sizeof(*fans_count_)));
                    *fans_count_ = 0;

                    result = ctlEnumFans(h_devices_[selected_device_], fans_count_, h_fans_);

                    if (CTL_RESULT_SUCCESS == result)
                    {
                        h_fans_ = static_cast<ctl_fan_handle_t*>(malloc(sizeof(ctl_fan_handle_t) * (*fans_count_)));

                        if (h_fans_ != nullptr)
                        {
                            result = ctlEnumFans(h_devices_[selected_device_], fans_count_, h_fans_);

                            if (CTL_RESULT_SUCCESS == result)
                            {
                                return true;
                            }
                        }
                    }                    
                }
            }            
        }
    }

    return false;
}

bool ArcticControlGPUInterop::GPUInterop::TestApi() 
{
    if (init_api())
    {
        this->!GPUInterop();

        return true;
    }

    return false;
}

bool ArcticControlGPUInterop::GPUInterop::InitCtlApi()
{
    // avoid multiple init
    // be aware that this could also be the case when
    // init fails with PlatformNotSupportedException
    // so this stops people from hot-swapping their GPUs if
    // its needed to support this :))
    if (h_api_handle_ != nullptr)
    {
        return false;
    }

    return init_api();
}

String^ ArcticControlGPUInterop::GPUInterop::GetAdapterName()
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1 || selected_device_ < 0)
    {
        return String::Empty;
    }

    ctl_device_adapter_properties_t adapter_props{ 0 };
    adapter_props.Size = sizeof(ctl_device_adapter_properties_t);
    adapter_props.pDeviceID = malloc(sizeof(LUID));
    adapter_props.device_id_size = sizeof(LUID);

    if (adapter_props.pDeviceID != nullptr)
    {
        if (const ctl_result_t result = ctlGetDeviceProperties(h_devices_[selected_device_], &adapter_props);
            result == CTL_RESULT_SUCCESS)
        {
            auto str = gcnew String(adapter_props.name);
            return str;
        }
    }

    return String::Empty;
}

array<ArcticControlGPUInterop::TempSensor^>^ ArcticControlGPUInterop::GPUInterop::GetTemperatures()
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1 || selected_device_ < 0)
    {
        return gcnew array<TempSensor^>{};
    }

    uint32_t sensors_count = 0;

    ctl_result_t result = ctlEnumTemperatureSensors(h_devices_[selected_device_], &sensors_count, nullptr);

    if ((result != CTL_RESULT_SUCCESS) || sensors_count == 0)
    {
        WRITE_LINE("\nTemperature component not supported. Error: %s" + (gcnew String(decode_ret_code(result).c_str())));
        return gcnew array<TempSensor^>{};
    }
    
    WRITE_LINE("\nNumber of Temperature Handles [%u]", sensors_count);

    if (CTL_RESULT_SUCCESS == result)
    {
        if (const auto h_temp_sensors = new ctl_temp_handle_t[sensors_count];
            h_temp_sensors != nullptr)
        {
            result = ctlEnumTemperatureSensors(h_devices_[selected_device_], &sensors_count, h_temp_sensors);

            if (CTL_RESULT_SUCCESS == result)
            {
                auto gc_temp_sensors 
                    = gcnew List<TempSensor^>();

                for (uint32_t i = 0; i < sensors_count; i++)
                {
                    auto ts = gcnew TempSensor;

                    WRITE_LINE("\n\nFor Temperature Handle [%u]" + i.ToString());
                    WRITE_LINE("\n[Temperature] Get Temperature properties:");

                    ctl_temp_properties_t tempProps{ 0 };
                    tempProps.Size = sizeof(ctl_temp_properties_t);
                    result = ctlTemperatureGetProperties(h_temp_sensors[i], &tempProps);

                    if (result != CTL_RESULT_SUCCESS)
                    {
                        WRITE_LINE("\nError: %s"+ gcnew String(decode_ret_code(result).c_str()) +" from Temperature get properties.");
                    }
                    else
                    {
                        WRITE_LINE("[Temperature] Max temp [%u]" + static_cast<uint32_t>(tempProps.maxTemperature).ToString());
                        WRITE_LINE("[Temperature] Sensor type [%s]" + ((tempProps.type == CTL_TEMP_SENSORS_GLOBAL) ? "Global" :
                            (tempProps.type == CTL_TEMP_SENSORS_GPU) ? "Gpu" :
                            (tempProps.type == CTL_TEMP_SENSORS_MEMORY) ? "Memory" :
                            "Unknown"));

                        ts->SensorType = ((tempProps.type == CTL_TEMP_SENSORS_GLOBAL) ? TempSensorType::Global :
                            (tempProps.type == CTL_TEMP_SENSORS_GPU) ? TempSensorType::GPU :
                            (tempProps.type == CTL_TEMP_SENSORS_MEMORY) ? TempSensorType::VRAM :
                            TempSensorType::Unknown);
                    }

                    WRITE_LINE("[Temperature] Get Temperature state:");

                    double temperature = 0;
                    result = ctlTemperatureGetState(h_temp_sensors[i], &temperature);

                    if (result != CTL_RESULT_SUCCESS)
                    {
                        WRITE_LINE("\nError: %s"+ gcnew String(decode_ret_code(result).c_str()) +"  from Temperature get state.");
                    }
                    else
                    {
                        WRITE_LINE("[Temperature] Current Temperature [%f"+ temperature.ToString() + "] C degrees \n \n");
                        ts->TemperatureInC = static_cast<UInt16>(temperature);
                    }

                    gc_temp_sensors->Add(ts);
                }

                return gc_temp_sensors->ToArray();
            }
        }
    }

    return gcnew array<TempSensor^>{};
}

// !! DANGEROUS !!
bool ArcticControlGPUInterop::GPUInterop::SetOverclockWaiver()
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1 || selected_device_ < 0)
    {
        return false;
    }

    if (const ctl_result_t result = ctlOverclockWaiverSet(h_devices_[selected_device_]);
        CTL_RESULT_SUCCESS == result)
    {
        return true;
    }

    return false;
}

double ArcticControlGPUInterop::GPUInterop::GetOverclockTemperatureLimit()
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1 || selected_device_ < 0)
    {
        return 0.0;
    }
    else
    {
        double temp_limit = 0.0;

        if (const ctl_result_t result = ctlOverclockTemperatureLimitGet(h_devices_[selected_device_], &temp_limit);
            CTL_RESULT_SUCCESS == result)
        {
            return temp_limit;
        }
    }

    return 0.0;
}

bool ArcticControlGPUInterop::GPUInterop::SetOverclockTemperatureLimit(const double new_temp_limit)
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1 || selected_device_ < 0)
    {
        return false;
    }
    else
    {
        if (const ctl_result_t result = ctlOverclockTemperatureLimitSet(h_devices_[selected_device_], new_temp_limit);
            CTL_RESULT_SUCCESS == result)
        {
            return true;
        }
    }

    return false;
}

double ArcticControlGPUInterop::GPUInterop::GetOverclockPowerLimit()
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1 || selected_device_ < 0)
    {
        return 0.0;
    }
    
    double sustained_power_limit;

    if (const ctl_result_t result = ctlOverclockPowerLimitGet(h_devices_[selected_device_], &sustained_power_limit); CTL_RESULT_SUCCESS == result)
    {
        // mW in W -> /1000
        return sustained_power_limit / 1000;
    }

    return 0.0;
}

bool ArcticControlGPUInterop::GPUInterop::SetOverclockPowerLimit(double new_power_limit)
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1 || selected_device_ < 0)
    {
        return false;
    }
    
    const ctl_result_t result = ctlOverclockPowerLimitSet(h_devices_[selected_device_], new_power_limit);

    WRITE_LINE(
        "[GPUInterop]: GPU PowerLimit overclock to " + new_power_limit.ToString()
        + " with Status: " + gcnew String(decode_ret_code(result).c_str()));
    if (CTL_RESULT_SUCCESS == result)
    {
        return true;
    }

    return false;
}

double ArcticControlGPUInterop::GPUInterop::GetOverclockGPUVoltageOffset()
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1 || selected_device_ < 0)
    {
        return 0.0;
    }
    
    double voltage_offset;

    if (const ctl_result_t result = ctlOverclockGpuVoltageOffsetGet(h_devices_[selected_device_], &voltage_offset);
        CTL_RESULT_SUCCESS == result)
    {
        return voltage_offset;
    }

    return 0.0;
}

bool ArcticControlGPUInterop::GPUInterop::SetOverclockGPUVoltageOffset(const double new_gpu_voltage_offset)
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1)
    {
        return false;
    }
    
    if (const ctl_result_t result = ctlOverclockGpuVoltageOffsetSet(h_devices_[selected_device_], new_gpu_voltage_offset);
        CTL_RESULT_SUCCESS == result)
    {
        return true;
    }

    return false;
}

double ArcticControlGPUInterop::GPUInterop::GetOverclockGPUFrequencyOffset()
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1)
    {
        return 0.0;
    }
    
    double frequency_offset;

    if (const ctl_result_t result = ctlOverclockGpuFrequencyOffsetGet(h_devices_[selected_device_], &frequency_offset);
        CTL_RESULT_SUCCESS == result)
    {
        return frequency_offset;
    }

    return 0.0;
}

bool ArcticControlGPUInterop::GPUInterop::SetOverclockGPUFrequencyOffset(double new_gpu_frequency_offset)
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1 || selected_device_ < 0)
    {
        return false;
    }
    
    const ctl_result_t result = ctlOverclockGpuFrequencyOffsetSet(h_devices_[selected_device_], new_gpu_frequency_offset);

    WRITE_LINE(
        "[GPUInterop]: GPU Frequency offset: " + new_gpu_frequency_offset.ToString()
        + " with Status: " + gcnew String(decode_ret_code(result).c_str()));
    if (CTL_RESULT_SUCCESS == result)
    {
        return true;
    }

    return false;
}

double ArcticControlGPUInterop::GPUInterop::GetOverclockVRAMVoltageOffset()
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1 || selected_device_ < 0)
    {
        return 0.0;
    }
    
    double voltage_offset;

    if (const ctl_result_t result = ctlOverclockVramVoltageOffsetGet(h_devices_[selected_device_], &voltage_offset);
        CTL_RESULT_SUCCESS == result)
    {
        return voltage_offset;
    }

    return 0.0;
}

bool ArcticControlGPUInterop::GPUInterop::SetOverclockVRAMVoltageOffset(const double new_vram_voltage_offset)
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1 || selected_device_ < 0)
    {
        return false;
    }
    
    if (const ctl_result_t result = ctlOverclockVramVoltageOffsetSet(h_devices_[selected_device_], new_vram_voltage_offset);
        CTL_RESULT_SUCCESS == result)
    {
        return true;
    }

    return false;
}

double ArcticControlGPUInterop::GPUInterop::GetOverclockVRAMFrequencyOffset()
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1 || selected_device_ < 0)
    {
        return 0.0;
    }
    
    double frequency_offset;

    if (const ctl_result_t result = ctlOverclockVramFrequencyOffsetGet(h_devices_[selected_device_], &frequency_offset);
        CTL_RESULT_SUCCESS == result)
    {
        return frequency_offset;
    }

    return 0.0;
}

bool ArcticControlGPUInterop::GPUInterop::SetOverclockVRAMFrequencyOffset(const double new_vram_frequency_offset)
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1 || selected_device_ < 0)
    {
        return false;
    }
    
    if (const ctl_result_t result = ctlOverclockVramFrequencyOffsetSet(h_devices_[selected_device_], new_vram_frequency_offset);
        CTL_RESULT_SUCCESS == result)
    {
        return true;
    }

    return false;
}

System::Tuple<double, double>^ ArcticControlGPUInterop::GPUInterop::GetOverclockGPULock()
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1 || selected_device_ < 0)
    {
        return nullptr;
    }

    ctl_oc_vf_pair_t get_oc_vf_pair;
    get_oc_vf_pair.Size = sizeof(ctl_oc_vf_pair_t);

    if (const ctl_result_t result = ctlOverclockGpuLockGet(h_devices_[selected_device_], &get_oc_vf_pair);
        result == CTL_RESULT_SUCCESS)
    {
        return Tuple::Create(get_oc_vf_pair.Voltage, get_oc_vf_pair.Frequency);
    }

    return nullptr;
}

bool ArcticControlGPUInterop::GPUInterop::SetOverclockGPULock(const double voltage, const double frequency)
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1 || selected_device_ < 0)
    {
        return nullptr;
    }

    ctl_oc_vf_pair_t set_oc_vf_pair;
    set_oc_vf_pair.Size = sizeof(ctl_oc_vf_pair_t);
    set_oc_vf_pair.Voltage = voltage;
    set_oc_vf_pair.Frequency = frequency;

    // ReSharper disable once CppSomeObjectMembersMightNotBeInitialized
    const ctl_result_t result = ctlOverclockGpuLockSet(h_devices_[selected_device_], set_oc_vf_pair);

    return result == CTL_RESULT_SUCCESS;
}

bool ArcticControlGPUInterop::GPUInterop::InitPowerDomains()
{
    if (h_api_handle_ == nullptr
        || *adapter_count_ < 1
        || selected_device_ < 0
        || pwr_count_ != nullptr
        || h_pwr_handle_ != nullptr)
    {
        return false;
    }

    pwr_count_ = static_cast<uint32_t*>(malloc(sizeof*pwr_count_));
    *pwr_count_ = 0;

    ctl_result_t result = ctlEnumPowerDomains(h_devices_[selected_device_], pwr_count_, nullptr);
    if (result != CTL_RESULT_SUCCESS || pwr_count_ == nullptr)
    {
        WRITE_LINE("Power component not supported. Error: " + gcnew String(decode_ret_code(result).c_str()));
        return false;
    }
    WRITE_LINE("[GPUInterop]: Number of Power Handles " + gcnew UInt32(*pwr_count_));

    h_pwr_handle_ = new ctl_pwr_handle_t[*pwr_count_];

    result = ctlEnumPowerDomains(h_devices_[selected_device_], pwr_count_, h_pwr_handle_);

    if (result == CTL_RESULT_SUCCESS)
    {
        return true;
    }
    
    WRITE_LINE("Error: " + gcnew String(decode_ret_code(result).c_str()) + " for Power handle.");

    // cleanup the mess
    //delete[] hPwrHandle;
    //hPwrHandle = nullptr;
    CTL_FREE_MEM(h_pwr_handle_);
    
    return false;
}

ArcticControlGPUInterop::PowerProperties^ ArcticControlGPUInterop::GPUInterop::GetPowerProperties()
{
    if (h_api_handle_ == nullptr
        || *adapter_count_ < 1
        || selected_device_ < 0
        || *pwr_count_ < 1
        || h_pwr_handle_ == nullptr)
    {
        return nullptr;
    }

    ctl_power_properties_t power_properties{0};
    power_properties.Size = sizeof(ctl_power_properties_t);

    if (const ctl_result_t result = ctlPowerGetProperties(h_pwr_handle_[0], &power_properties);
        result == CTL_RESULT_SUCCESS)
    {
        PowerProperties^ pwr_properties = PowerProperties::create(&power_properties);

        return pwr_properties;
    }

    return nullptr;
}

ArcticControlGPUInterop::PowerLimitsCombination^ ArcticControlGPUInterop::GPUInterop::GetPowerLimits()
{
    if (h_api_handle_ == nullptr 
        || *adapter_count_ < 1 
        || *pwr_count_ < 1 
        || h_pwr_handle_ == nullptr)
    {
        return nullptr;
    }

    ctl_power_limits_t power_limits{ 0 };
    power_limits.Size = sizeof(ctl_power_limits_t);

    if (const ctl_result_t result = ctlPowerGetLimits(h_pwr_handle_[0], &power_limits); result == CTL_RESULT_SUCCESS)
    {
        // allocate result in GC and return

        SustainedPowerLimit^ sustained_pwr_limit = SustainedPowerLimit::create(&power_limits.sustainedPowerLimit);

        BurstPowerLimit^ burst_pwr_limit = BurstPowerLimit::create(&power_limits.burstPowerLimit);

        PeakPowerLimit^ peak_pwr_limit = PeakPowerLimit::create(&power_limits.peakPowerLimits);

        // combine all
        auto pwr_limits_combo = gcnew PowerLimitsCombination;
        pwr_limits_combo->SustainedPowerLimit = sustained_pwr_limit;
        pwr_limits_combo->BurstPowerLimit = burst_pwr_limit;
        pwr_limits_combo->PeakPowerLimit = peak_pwr_limit;

        return pwr_limits_combo;
    }

    return nullptr;
}

bool ArcticControlGPUInterop::GPUInterop::InitFansHandles()
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1 || selected_device_ < 0 || selected_device_ < 0)
    {
        return false;
    }

    if (fans_count_ != nullptr)
    {
        free(fans_count_);
        fans_count_ = nullptr;
    }

    fans_count_ = static_cast<uint32_t*>(malloc(sizeof(*fans_count_)));
    *fans_count_ = 0;

    if (ctl_result_t result = ctlEnumFans(h_devices_[selected_device_], fans_count_, h_fans_); CTL_RESULT_SUCCESS == result)
    {
        h_fans_ = static_cast<ctl_fan_handle_t*>(malloc(sizeof(ctl_fan_handle_t) * (*fans_count_)));

        if (h_fans_ != nullptr)
        {
            result = ctlEnumFans(h_devices_[selected_device_], fans_count_, h_fans_);

            if (CTL_RESULT_SUCCESS == result)
            {
                return true;
            }
        }
    }

    return false;
}

ArcticControlGPUInterop::FanProperties^ ArcticControlGPUInterop::GPUInterop::GetFanProperties()
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1 || selected_device_ < 0 || h_fans_ == nullptr || *fans_count_ < 1)
    {
        return nullptr;
    }

    ctl_fan_properties_t fan_properties{};
    fan_properties.Size = sizeof(ctl_fan_properties_t);
    const ctl_result_t result = ctlFanGetProperties(h_fans_[0], &fan_properties);

    if (result == CTL_RESULT_SUCCESS)
    {
        return FanProperties::create(&fan_properties);
    }
    
    WRITE_LINE(
"[GPUInterop]: GetFanProperties failed with result: " + gcnew String(decode_ret_code(result).c_str()));

    return nullptr;
}

ArcticControlGPUInterop::FanConfig^ ArcticControlGPUInterop::GPUInterop::GetFanConfig()
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1 || selected_device_ < 0 || h_fans_ == nullptr || *fans_count_ < 1)
    {
        return nullptr;
    }

    ctl_fan_config_t fan_config{};
    fan_config.Size = sizeof(ctl_fan_config_t);

    if (const ctl_result_t result = ctlFanGetConfig(h_fans_[0], &fan_config); result == CTL_RESULT_SUCCESS)
    {
        return FanConfig::create(&fan_config);
    }

    return nullptr;
}

bool ArcticControlGPUInterop::GPUInterop::SetFansToDefaultMode()
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1 || selected_device_ < 0)
    {
        return false;
    }

    for (uint32_t i = 0; i < *fans_count_; i++)
    {
        const ctl_result_t result = ctlFanSetDefaultMode(h_fans_[i]);

        if (result == CTL_RESULT_SUCCESS)
        {
            return true;
        }
        
        WRITE_LINE(
            "[GPUInterop]: Error setting fans to default mode! "+ 
            "Fans count : " + (*fans_count_).ToString() +
            "Result: " + gcnew String(decode_ret_code(result).c_str()));
    }

    return false;
}

UInt32 ArcticControlGPUInterop::GPUInterop::GetGamingFlipMode(String^ application)
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1 || selected_device_ < 0)
    {
        return CTL_GAMING_FLIP_MODE_FLAG_MAX;
    }

    ctl_3d_feature_getset_t get_3d_property{0};
    get_3d_property.Size = sizeof(ctl_3d_feature_getset_t);

    get_3d_property.FeatureType = CTL_3D_FEATURE_GAMING_FLIP_MODES;
    get_3d_property.bSet = false;
    get_3d_property.CustomValueSize = 0;
    get_3d_property.pCustomValue = nullptr;
    // application specify
    if (application != nullptr)
    {
        marshal_context^ context = gcnew marshal_context();
        const char* app_name = context->marshal_as<const char*>(application);
        get_3d_property.ApplicationName = const_cast<char*>(app_name);
        get_3d_property.ApplicationNameLength = static_cast<int8_t>(strlen(app_name));
        delete context;
    }
    get_3d_property.ValueType = CTL_PROPERTY_VALUE_TYPE_ENUM;
    get_3d_property.Version = 0;

    if (h_devices_ != nullptr)
    {
        if (const ctl_result_t result = ctlGetSet3DFeature(h_devices_[selected_device_], &get_3d_property);
            result == CTL_RESULT_SUCCESS)
        {
            WRITE_LINE("[GPUInterop]: EnableType: " + get_3d_property.Value.EnumType.EnableType.ToString());
            const auto gfm = static_cast<ctl_gaming_flip_mode_flag_t>(get_3d_property.Value.EnumType.EnableType);
            return gfm;
        }
        else
        {
            WRITE_LINE("[GPUInterop]: error reading global gaming flip mode! Result: "
                + gcnew String(decode_ret_code(result).c_str()));
        }
    }

    return CTL_GAMING_FLIP_MODE_FLAG_MAX;
}

bool ArcticControlGPUInterop::GPUInterop::SetGamingFlipMode(UInt32 flip_mode, String^ application)
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1 || selected_device_ < 0)
    {
        return false;
    }

    ctl_3d_feature_getset_t set_3d_property{ 0 };
    set_3d_property.Size = sizeof(ctl_3d_feature_getset_t);

    set_3d_property.FeatureType = CTL_3D_FEATURE_GAMING_FLIP_MODES;
    set_3d_property.bSet = true;
    set_3d_property.CustomValueSize = 0;
    set_3d_property.pCustomValue = nullptr;

    // application specify
    if (application != nullptr)
    {
        marshal_context^ context = gcnew marshal_context();
        const char* app_name = context->marshal_as<const char*>(application);
        set_3d_property.ApplicationName = const_cast<char*>(app_name);
        set_3d_property.ApplicationNameLength = static_cast<int8_t>(strlen(app_name));
        delete context;
    }
    
    set_3d_property.ValueType = CTL_PROPERTY_VALUE_TYPE_ENUM;
    set_3d_property.Value.EnumType.EnableType = static_cast<ctl_gaming_flip_mode_flag_t>(flip_mode);
    set_3d_property.Version = 0;

    if (h_devices_ != nullptr)
    {
        WRITE_LINE("[GPUInterop]: trying to set CTL_3D_FEATURE_GAMING_FLIP_MODES to "
            + set_3d_property.Value.EnumType.EnableType);
        
        if (const ctl_result_t result = ctlGetSet3DFeature(h_devices_[selected_device_], &set_3d_property);
            result == CTL_RESULT_SUCCESS)
        {
            return true;
        }

        WRITE_LINE("[GPUInterop]: failed to set 3d feature of type CTL_3D_FEATURE_GAMING_FLIP_MODES.");
    }

    return false;
}

UInt32 ArcticControlGPUInterop::GPUInterop::GetAnisotropicFilteringMode(String^ application)
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1 || selected_device_ < 0)
    {
        return CTL_3D_ANISOTROPIC_TYPES_MAX;
    }

    ctl_3d_feature_getset_t get_3d_property{0};
    get_3d_property.Size = sizeof(ctl_3d_feature_getset_t);

    get_3d_property.FeatureType = CTL_3D_FEATURE_ANISOTROPIC;
    get_3d_property.bSet = false;
    get_3d_property.CustomValueSize = 0;
    get_3d_property.pCustomValue = nullptr;
    // application specify
    if (application != nullptr)
    {
        marshal_context^ context = gcnew marshal_context();
        const char* app_name = context->marshal_as<const char*>(application);
        get_3d_property.ApplicationName = const_cast<char*>(app_name);
        get_3d_property.ApplicationNameLength = static_cast<int8_t>(strlen(app_name));
        delete context;
    }
    get_3d_property.ValueType = CTL_PROPERTY_VALUE_TYPE_ENUM;
    get_3d_property.Version = 0;

    if (h_devices_ != nullptr)
    {
        if (const ctl_result_t result = ctlGetSet3DFeature(h_devices_[selected_device_], &get_3d_property);
            result == CTL_RESULT_SUCCESS)
        {
            WRITE_LINE("[GPUInterop]: GetAnisotropicFilteringMode - EnableType: "
                + get_3d_property.Value.EnumType.EnableType.ToString());
            const auto afm = static_cast<ctl_3d_anisotropic_types_t>(get_3d_property.Value.EnumType.EnableType);
            return afm;
        }
        else
        {
            WRITE_LINE("[GPUInterop]: error reading anisotropic filtering mode! Result: "
                + gcnew String(decode_ret_code(result).c_str()));
            // TODO: !IMPORTANT! change this in the future as it's a very bad solution to ctlGetSet3DFeature
            // crashing when set to AppChoice in Arc Control
            //return AnisotropicFilteringMode::AppChoice;
        }
    }
    
    return CTL_3D_ANISOTROPIC_TYPES_MAX;
}

bool ArcticControlGPUInterop::GPUInterop::SetAnisotropicFilteringMode(UInt32 anisotropic_mode, String^ application)
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1 || selected_device_ < 0)
    {
        return false;
    }

    ctl_3d_feature_getset_t set_3d_property{ 0 };
    set_3d_property.Size = sizeof(ctl_3d_feature_getset_t);

    set_3d_property.FeatureType = CTL_3D_FEATURE_ANISOTROPIC;
    set_3d_property.bSet = true;
    set_3d_property.CustomValueSize = 0;
    set_3d_property.pCustomValue = nullptr;
    // application specify
    if (application != nullptr)
    {
        marshal_context^ context = gcnew marshal_context();
        const char* app_name = context->marshal_as<const char*>(application);
        set_3d_property.ApplicationName = const_cast<char*>(app_name);
        set_3d_property.ApplicationNameLength = static_cast<int8_t>(strlen(app_name));
        delete context;
    }
    set_3d_property.ValueType = CTL_PROPERTY_VALUE_TYPE_ENUM;
    set_3d_property.Value.EnumType.EnableType = static_cast<ctl_3d_anisotropic_types_t>(anisotropic_mode);
    set_3d_property.Version = 0;

    if (h_devices_ != nullptr)
    {
        WRITE_LINE("[GPUInterop]: trying to set CTL_3D_FEATURE_ANISOTROPIC to "
            + set_3d_property.Value.EnumType.EnableType);
        
        if (const ctl_result_t result = ctlGetSet3DFeature(h_devices_[selected_device_], &set_3d_property);
            result == CTL_RESULT_SUCCESS)
        {
            return true;
        }

        WRITE_LINE("[GPUInterop]: failed to set 3d feature of type CTL_3D_FEATURE_ANISOTROPIC.");
    }

    return false;
}

UInt32 ArcticControlGPUInterop::GPUInterop::GetCmaaMode(String^ application)
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1 || selected_device_ < 0)
    {
        return CTL_3D_CMAA_TYPES_MAX;
    }

    ctl_3d_feature_getset_t get_3d_property{0};
    get_3d_property.Size = sizeof(ctl_3d_feature_getset_t);

    get_3d_property.FeatureType = CTL_3D_FEATURE_CMAA;
    get_3d_property.bSet = false;
    get_3d_property.CustomValueSize = 0;
    get_3d_property.pCustomValue = nullptr;
    // application specify
    if (application != nullptr)
    {
        marshal_context^ context = gcnew marshal_context();
        const char* app_name = context->marshal_as<const char*>(application);
        get_3d_property.ApplicationName = const_cast<char*>(app_name);
        get_3d_property.ApplicationNameLength = static_cast<int8_t>(strlen(app_name));
        delete context;
    }
    get_3d_property.ValueType = CTL_PROPERTY_VALUE_TYPE_ENUM;
    get_3d_property.Version = 0;

    if (h_devices_ != nullptr)
    {
        if (const ctl_result_t result = ctlGetSet3DFeature(h_devices_[selected_device_], &get_3d_property);
            result == CTL_RESULT_SUCCESS)
        {
            WRITE_LINE("[GPUInterop]: EnableType: " + get_3d_property.Value.EnumType.EnableType.ToString());
            const auto cm = static_cast<ctl_3d_cmaa_types_t>(get_3d_property.Value.EnumType.EnableType);
            return cm;
        }
        else
        {
            WRITE_LINE("[GPUInterop]: error reading cmaa mode! Result: "
                + gcnew String(decode_ret_code(result).c_str()));
        }
    }
    
    return CTL_3D_CMAA_TYPES_MAX;
}

bool ArcticControlGPUInterop::GPUInterop::SetCmaaMode(UInt32 cmaa_mode, String^ application)
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1 || selected_device_ < 0)
    {
        return false;
    }

    ctl_3d_feature_getset_t set_3d_property{ 0 };
    set_3d_property.Size = sizeof(ctl_3d_feature_getset_t);

    set_3d_property.FeatureType = CTL_3D_FEATURE_CMAA;
    set_3d_property.bSet = true;
    set_3d_property.CustomValueSize = 0;
    set_3d_property.pCustomValue = nullptr;
    // application specify
    if (application != nullptr)
    {
        marshal_context^ context = gcnew marshal_context();
        const char* app_name = context->marshal_as<const char*>(application);
        set_3d_property.ApplicationName = const_cast<char*>(app_name);
        set_3d_property.ApplicationNameLength = static_cast<int8_t>(strlen(app_name));
        delete context;
    }
    set_3d_property.ValueType = CTL_PROPERTY_VALUE_TYPE_ENUM;
    set_3d_property.Value.EnumType.EnableType = static_cast<ctl_3d_anisotropic_types_t>(cmaa_mode);
    set_3d_property.Version = 0;

    if (h_devices_ != nullptr)
    {
        WRITE_LINE("[GPUInterop]: trying to set CTL_3D_FEATURE_CMAA to "
            + set_3d_property.Value.EnumType.EnableType);
        
        if (const ctl_result_t result = ctlGetSet3DFeature(h_devices_[selected_device_], &set_3d_property);
            result == CTL_RESULT_SUCCESS)
        {
            return true;
        }

        WRITE_LINE("[GPUInterop]: failed to set 3d feature of type CTL_3D_FEATURE_CMAA.");
    }

    return false;
}

bool ArcticControlGPUInterop::GPUInterop::IsSharpeningFilterActive(String^ application)
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1 || selected_device_ < 0)
    {
        // TODO: search for a better exception type
        // forbidden exception types:
        // https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/using-standard-exception-types
        throw gcnew PlatformNotSupportedException;
    }

    ctl_3d_feature_getset_t get_3d_property{0};
    get_3d_property.Size = sizeof(ctl_3d_feature_getset_t);

    get_3d_property.FeatureType = CTL_3D_FEATURE_SHARPENING_FILTER;
    get_3d_property.bSet = false;
    get_3d_property.CustomValueSize = 0;
    get_3d_property.pCustomValue = nullptr;
    // application specify
    if (application != nullptr)
    {
        marshal_context^ context = gcnew marshal_context();
        const char* app_name = context->marshal_as<const char*>(application);
        get_3d_property.ApplicationName = const_cast<char*>(app_name);
        get_3d_property.ApplicationNameLength = static_cast<int8_t>(strlen(app_name));
        delete context;
    }
    get_3d_property.ValueType = CTL_PROPERTY_VALUE_TYPE_ENUM;
    get_3d_property.Version = 0;

    if (h_devices_ != nullptr)
    {
        if (const ctl_result_t result = ctlGetSet3DFeature(h_devices_[selected_device_], &get_3d_property);
            result == CTL_RESULT_SUCCESS)
        {
            WRITE_LINE("[GPUInterop]: EnableType: " + get_3d_property.Value.EnumType.EnableType.ToString());
            const auto sf = static_cast<ctl_3d_sharpening_filter_types_t>(get_3d_property.Value.EnumType.EnableType);
            return sf == CTL_3D_SHARPENING_FILTER_TYPES_TURN_ON;
        }
        else
        {
            WRITE_LINE("[GPUInterop]: error reading sharpening filter state! Result: "
                + gcnew String(decode_ret_code(result).c_str()));
        }
    }

    throw gcnew PlatformNotSupportedException;
}

bool ArcticControlGPUInterop::GPUInterop::SetSharpeningFilter(const bool on, String^ application)
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1 || selected_device_ < 0)
    {
        throw gcnew PlatformNotSupportedException;
    }

    ctl_3d_feature_getset_t set_3d_property{ 0 };
    set_3d_property.Size = sizeof(ctl_3d_feature_getset_t);

    set_3d_property.FeatureType = CTL_3D_FEATURE_SHARPENING_FILTER;
    set_3d_property.bSet = true;
    set_3d_property.CustomValueSize = 0;
    set_3d_property.pCustomValue = nullptr;
    // application specify
    if (application != nullptr)
    {
        marshal_context^ context = gcnew marshal_context();
        const char* app_name = context->marshal_as<const char*>(application);
        set_3d_property.ApplicationName = const_cast<char*>(app_name);
        set_3d_property.ApplicationNameLength = static_cast<int8_t>(strlen(app_name));
        delete context;
    }
    set_3d_property.ValueType = CTL_PROPERTY_VALUE_TYPE_ENUM;
    set_3d_property.Value.EnumType.EnableType
    = on ? CTL_3D_SHARPENING_FILTER_TYPES_TURN_ON : CTL_3D_SHARPENING_FILTER_TYPES_TURN_OFF;
    set_3d_property.Version = 0;

    if (h_devices_ != nullptr)
    {
        WRITE_LINE("[GPUInterop]: trying to set CTL_3D_FEATURE_SHARPENING_FILTER to "
            + set_3d_property.Value.EnumType.EnableType);
        
        if (const ctl_result_t result = ctlGetSet3DFeature(h_devices_[selected_device_], &set_3d_property);
            result == CTL_RESULT_SUCCESS)
        {
            return true;
        }

        WRITE_LINE("[GPUInterop]: failed to set 3d feature of type CTL_3D_FEATURE_CMAA.");
    }

    throw gcnew PlatformNotSupportedException;
}

bool ArcticControlGPUInterop::GPUInterop::InitFrequencyDomains()
{
    if (h_api_handle_ == nullptr
        || *adapter_count_ < 1
        || selected_device_ < 0

        // already initialized
        || selected_freq_ >= 0
        || h_freq_handle_ != nullptr)
    {
        return false;
    }

    uint32_t freq_handle_count = 0;

    if (ctl_result_t result = ctlEnumFrequencyDomains(h_devices_[selected_device_], &freq_handle_count, nullptr);
        result == CTL_RESULT_SUCCESS && freq_handle_count > 0)
    {
        h_freq_handle_ = new ctl_freq_handle_t[freq_handle_count];
        ctlEnumFrequencyDomains(h_devices_[selected_device_], &freq_handle_count, h_freq_handle_);

        for (uint32_t i = 0; i < freq_handle_count; i++)
        {
            ctl_freq_properties_t freq_properties{0};
            freq_properties.Size = sizeof(ctl_freq_properties_t);
            result = ctlFrequencyGetProperties(h_freq_handle_[i], &freq_properties);

            if (result == CTL_RESULT_SUCCESS && freq_properties.type == CTL_FREQ_DOMAIN_GPU)
            {
                WRITE_LINE("[GPUInterop]: found freq handle");
                selected_freq_ = i;
                return true;
            }
        }
    }

    return false;
}

ArcticControlGPUInterop::FrequencyProperties^ ArcticControlGPUInterop::GPUInterop::GetFrequencyProperties()
{
    if (h_api_handle_ == nullptr
        || *adapter_count_ < 1
        || selected_device_ < 0
        || selected_freq_ < 0
        || h_freq_handle_ == nullptr)
    {
        return nullptr;
    }

    ctl_freq_properties_t freq_properties{0};
    freq_properties.Size = sizeof(ctl_freq_properties_t);

    if (const ctl_result_t result = ctlFrequencyGetProperties(h_freq_handle_[selected_freq_], &freq_properties);
        result == CTL_RESULT_SUCCESS)
    {
        return FrequencyProperties::create(&freq_properties);
    }

    return nullptr;
}

ArcticControlGPUInterop::FrequencyState^ ArcticControlGPUInterop::GPUInterop::GetFrequencyState()
{
    if (h_api_handle_ == nullptr
        || *adapter_count_ < 1
        || selected_device_ < 0
        || selected_freq_ < 0
        || h_freq_handle_ == nullptr)
    {
        return nullptr;
    }

    ctl_freq_state_t freq_state{0};
    freq_state.Size = sizeof(ctl_freq_state_t);

    if (const ctl_result_t result = ctlFrequencyGetState(h_freq_handle_[selected_freq_], &freq_state);
        result == CTL_RESULT_SUCCESS)
    {
        return FrequencyState::create(&freq_state);
    }

    return nullptr;
}

System::Tuple<double, double>^ ArcticControlGPUInterop::GPUInterop::GetMinMaximumFrequency()
{
    if (h_api_handle_ == nullptr
        || *adapter_count_ < 1
        || selected_device_ < 0
        || selected_freq_ < 0
        || h_freq_handle_ == nullptr)
    {
        return nullptr;
    }

    ctl_freq_range_t freq_range{0};
    freq_range.Size = sizeof(ctl_freq_range_t);
    const ctl_result_t result = ctlFrequencyGetRange(h_freq_handle_[selected_freq_], &freq_range);

    if (result == CTL_RESULT_SUCCESS)
    {
        WRITE_LINE("[GPUInterop]: GetMinMaximumFrequency");
        return Tuple::Create(freq_range.min, freq_range.max);
    }
    else
    {
        WRITE_LINE("[GPUInterop]: GetMinMaximumFrequency failed with result: "
            + gcnew String(decode_ret_code(result).c_str()));
    }

    return nullptr;
}

bool ArcticControlGPUInterop::GPUInterop::SetMinMaximumFrequency(const double min_freq, const double max_freq)
{
    if (h_api_handle_ == nullptr
        || *adapter_count_ < 1
        || selected_device_ < 0
        || selected_freq_ < 0
        || h_freq_handle_ == nullptr)
    {
        return false;
    }

    ctl_freq_range_t freq_range{0};
    freq_range.min = min_freq;
    freq_range.max = max_freq;
    freq_range.Size = sizeof(freq_range);

    if (const ctl_result_t result = ctlFrequencySetRange(h_freq_handle_[selected_freq_], &freq_range);
        result == CTL_RESULT_SUCCESS)
    {
        WRITE_LINE("[GPUInterop]: SetMinMaximumFrequency");
        return true;
    }
    else
    {
        WRITE_LINE("[GPUInterop]: SetMinMaximumFrequency failed with result: " 
            + gcnew String(decode_ret_code(result).c_str()));
    }

    return false;
}

ArcticControlGPUInterop::PCIeProperties^ ArcticControlGPUInterop::GPUInterop::GetPCIeProperties()
{
    if (h_api_handle_ == nullptr || *adapter_count_ < 1 || selected_device_ < 0)
    {
        return nullptr;
    }

    ctl_pci_properties_t pci_properties{0};
    pci_properties.Size = sizeof(ctl_pci_properties_t);

    if (const ctl_result_t result = ctlPciGetProperties(h_devices_[selected_device_], &pci_properties);
        result == CTL_RESULT_SUCCESS)
    {
        // TODO: for some reason _supported is false and enabled true on my system
        // pci_properties.resizable_bar_supported && pci_properties.resizable_bar_enabled
        auto pcie_props = gcnew PCIeProperties;
        pcie_props->IsReBarSupported = pci_properties.resizable_bar_supported;
        pcie_props->IsReBarEnabled = pci_properties.resizable_bar_enabled;
        pcie_props->Lanes = pci_properties.maxSpeed.width;
        pcie_props->Gen = pci_properties.maxSpeed.gen;
        return pcie_props;
    }

    return  nullptr;
}

ArcticControlGPUInterop::GPUInterop::!GPUInterop()
{
    if (h_api_handle_ != nullptr)
    {
        ctlClose(*h_api_handle_);
        CTL_FREE_MEM(h_api_handle_);
    }

    CTL_FREE_MEM(h_devices_);
    CTL_FREE_MEM(adapter_count_);
    CTL_FREE_MEM(h_fans_);
    CTL_FREE_MEM(fans_count_);
    CTL_FREE_MEM(h_pwr_handle_);
    CTL_FREE_MEM(pwr_count_);
    CTL_FREE_MEM(h_freq_handle_);
}

// Decoding the return code for the most common error codes.
std::string decode_ret_code(const ctl_result_t res)
{
    switch (res)
    {
    case CTL_RESULT_SUCCESS:
    {
        return std::string("[CTL_RESULT_SUCCESS]");
    }
    case CTL_RESULT_ERROR_CORE_OVERCLOCK_NOT_SUPPORTED:
    {
        return std::string("[CTL_RESULT_ERROR_CORE_OVERCLOCK_NOT_SUPPORTED]");
    }
    case CTL_RESULT_ERROR_CORE_OVERCLOCK_VOLTAGE_OUTSIDE_RANGE:
    {
        return std::string("[CTL_RESULT_ERROR_CORE_OVERCLOCK_VOLTAGE_OUTSIDE_RANGE]");
    }
    case CTL_RESULT_ERROR_CORE_OVERCLOCK_FREQUENCY_OUTSIDE_RANGE:
    {
        return std::string("[CTL_RESULT_ERROR_CORE_OVERCLOCK_FREQUENCY_OUTSIDE_RANGE]");
    }
    case CTL_RESULT_ERROR_CORE_OVERCLOCK_POWER_OUTSIDE_RANGE:
    {
        return std::string("[CTL_RESULT_ERROR_CORE_OVERCLOCK_POWER_OUTSIDE_RANGE]");
    }
    case CTL_RESULT_ERROR_CORE_OVERCLOCK_TEMPERATURE_OUTSIDE_RANGE:
    {
        return std::string("[CTL_RESULT_ERROR_CORE_OVERCLOCK_TEMPERATURE_OUTSIDE_RANGE]");
    }
    case CTL_RESULT_ERROR_GENERIC_START:
    {
        return std::string("[CTL_RESULT_ERROR_GENERIC_START]");
    }
    case CTL_RESULT_ERROR_CORE_OVERCLOCK_RESET_REQUIRED:
    {
        return std::string("[CTL_RESULT_ERROR_CORE_OVERCLOCK_RESET_REQUIRED]");
    }
    case CTL_RESULT_ERROR_CORE_OVERCLOCK_IN_VOLTAGE_LOCKED_MODE:
    {
        return std::string("[CTL_RESULT_ERROR_CORE_OVERCLOCK_IN_VOLTAGE_LOCKED_MODE");
    }
    case CTL_RESULT_ERROR_CORE_OVERCLOCK_WAIVER_NOT_SET:
    {
        return std::string("[CTL_RESULT_ERROR_CORE_OVERCLOCK_WAIVER_NOT_SET]");
    }
    case CTL_RESULT_ERROR_NOT_INITIALIZED:
    {
        return std::string("[CTL_RESULT_ERROR_NOT_INITIALIZED]");
    }
    case CTL_RESULT_ERROR_ALREADY_INITIALIZED:
    {
        return std::string("[CTL_RESULT_ERROR_ALREADY_INITIALIZED]");
    }
    case CTL_RESULT_ERROR_DEVICE_LOST:
    {
        return std::string("[CTL_RESULT_ERROR_DEVICE_LOST]");
    }
    case CTL_RESULT_ERROR_INSUFFICIENT_PERMISSIONS:
    {
        return std::string("[CTL_RESULT_ERROR_INSUFFICIENT_PERMISSIONS]");
    }
    case CTL_RESULT_ERROR_NOT_AVAILABLE:
    {
        return std::string("[CTL_RESULT_ERROR_NOT_AVAILABLE]");
    }
    case CTL_RESULT_ERROR_UNINITIALIZED:
    {
        return std::string("[CTL_RESULT_ERROR_UNINITIALIZED]");
    }
    case CTL_RESULT_ERROR_UNSUPPORTED_VERSION:
    {
        return std::string("[CTL_RESULT_ERROR_UNSUPPORTED_VERSION]");
    }
    case CTL_RESULT_ERROR_UNSUPPORTED_FEATURE:
    {
        return std::string("[CTL_RESULT_ERROR_UNSUPPORTED_FEATURE]");
    }
    case CTL_RESULT_ERROR_INVALID_ARGUMENT:
    {
        return std::string("[CTL_RESULT_ERROR_INVALID_ARGUMENT]");
    }
    case CTL_RESULT_ERROR_INVALID_NULL_HANDLE:
    {
        return std::string("[CTL_RESULT_ERROR_INVALID_NULL_HANDLE]");
    }
    case CTL_RESULT_ERROR_INVALID_NULL_POINTER:
    {
        return std::string("[CTL_RESULT_ERROR_INVALID_NULL_POINTER]");
    }
    case CTL_RESULT_ERROR_INVALID_SIZE:
    {
        return std::string("[CTL_RESULT_ERROR_INVALID_SIZE]");
    }
    case CTL_RESULT_ERROR_UNSUPPORTED_SIZE:
    {
        return std::string("[CTL_RESULT_ERROR_UNSUPPORTED_SIZE]");
    }
    case CTL_RESULT_ERROR_NOT_IMPLEMENTED:
    {
        return std::string("[CTL_RESULT_ERROR_NOT_IMPLEMENTED]");
    }
    case CTL_RESULT_ERROR_ZE_LOADER:
    {
        return std::string("[CTL_RESULT_ERROR_ZE_LOADER]");
    }
    case CTL_RESULT_ERROR_INVALID_OPERATION_TYPE:
    {
        return std::string("[CTL_RESULT_ERROR_INVALID_OPERATION_TYPE]");
    }
    case CTL_RESULT_ERROR_UNKNOWN:
    {
        return std::string("[CTL_RESULT_ERROR_UNKNOWN]");
    }
    default:
        return std::string("[Unknown Error]");
    }
}
