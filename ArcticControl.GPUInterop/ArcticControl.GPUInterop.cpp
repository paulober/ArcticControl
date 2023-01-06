#include "pch.h"

#include <stdio.h>
#include <stdlib.h>
#include <conio.h>
#include <vector>
#include <string>

#include "ArcticControl.GPUInterop.h"

std::string DecodeRetCode(ctl_result_t Res);
#define WriteLine System::Diagnostics::Debug::WriteLine

String^ ArcticControlGPUInterop::GPUInterop::GetMyName()
{
    // throw gcnew System::NotImplementedException();
    // TODO: hier return-Anweisung eingeben
    return "Hi there in CLR. I'm the real slim shady!";
}

Boolean ArcticControlGPUInterop::GPUInterop::initApi()
{
    if (hDevices != nullptr && adapterCount != nullptr && hAPIHandle != nullptr)
    {
        return true;
    }

    ctl_result_t result = CTL_RESULT_SUCCESS;
    hDevices = nullptr;
    ctl_init_args_t ctlInitArgs{};
    hAPIHandle = (ctl_api_handle_t*)malloc(sizeof(ctl_api_handle_t));
    
    adapterCount = (uint32_t*)malloc(sizeof(*adapterCount));
    *adapterCount = 0;

    _CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF);

    ctlInitArgs.AppVersion = CTL_MAKE_VERSION(CTL_IMPL_MAJOR_VERSION, CTL_IMPL_MINOR_VERSION);
    ctlInitArgs.flags = CTL_INIT_FLAG_USE_LEVEL_ZERO;
    ctlInitArgs.Size = sizeof(ctlInitArgs);
    ctlInitArgs.Version = 0;
    ZeroMemory(&ctlInitArgs.ApplicationUID, sizeof(ctl_application_id_t));

    result = ctlInit(&ctlInitArgs, hAPIHandle);

    if (CTL_RESULT_SUCCESS == result)
    {
        // enumerate all devices to check if some are available
        result = ctlEnumerateDevices(*hAPIHandle, adapterCount, hDevices);

        if (CTL_RESULT_SUCCESS == result)
        {
            hDevices = (ctl_device_adapter_handle_t*)malloc(sizeof(ctl_device_adapter_handle_t) * (*adapterCount));

            if (hDevices != NULL)
            {
                result = ctlEnumerateDevices(*hAPIHandle, adapterCount, hDevices);

                if (CTL_RESULT_SUCCESS == result)
                {
                    fansCount = (uint32_t*)malloc(sizeof(*fansCount));
                    *fansCount = 0;

                    result = ctlEnumFans(hDevices[0], fansCount, hFans);

                    if (CTL_RESULT_SUCCESS == result)
                    {
                        hFans = (ctl_fan_handle_t*)malloc(sizeof(ctl_fan_handle_t) * (*fansCount));

                        if (hFans != NULL)
                        {
                            result = ctlEnumFans(hDevices[0], fansCount, hFans);

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

Boolean ArcticControlGPUInterop::GPUInterop::TestApi() 
{
    if (initApi())
    {
        this->!GPUInterop();

        return true;
    }

    return false;
}

Boolean ArcticControlGPUInterop::GPUInterop::InitCtlApi()
{
    if (hAPIHandle != nullptr)
    {
        goto Exit;
    }

    return initApi();

Exit:
    return false;
}

String^ ArcticControlGPUInterop::GPUInterop::GetAdapterName()
{
    if (hAPIHandle == nullptr && (*adapterCount) < 1)
    {
        goto Exit;
    }

    ctl_result_t result;
    ctl_device_adapter_properties_t adapterProps{ 0 };
    adapterProps.Size = sizeof(ctl_device_adapter_properties_t);
    adapterProps.pDeviceID = malloc(sizeof(LUID));
    adapterProps.device_id_size = sizeof(LUID);

    if (adapterProps.pDeviceID == NULL)
    {
        goto Exit;
    }

    result = ctlGetDeviceProperties(hDevices[0], &adapterProps);

    if (result == CTL_RESULT_SUCCESS) 
    {
        String^ str = gcnew String(adapterProps.name);
        return str;
    }

Exit:
    return String::Empty;
}

void ArcticControlGPUInterop::GPUInterop::SetFansToDefaultMode()
{
    if (hAPIHandle == nullptr && (*adapterCount) < 1)
    {
        return;
    }

    ctl_result_t result;
    for (uint32_t i = 0; i < *fansCount; i++)
    {
        result = ctlFanSetDefaultMode(hFans[i]);

        if (result != CTL_RESULT_SUCCESS)
        {
            WriteLine("GPUInterop: Error setting fans to default mode!");
            return;
        }
    }
}


array<ArcticControlGPUInterop::TempSensor^>^ ArcticControlGPUInterop::GPUInterop::GetTemperatures()
{
    if (hAPIHandle == nullptr && (*adapterCount) < 1)
    {
        return gcnew array<TempSensor^>{};
    }

    ctl_result_t result;
    uint32_t sensorsCount = 0;
    ctl_temp_handle_t* hTempSensors;

    result = ctlEnumTemperatureSensors(hDevices[0], &sensorsCount, nullptr);

    if ((result != CTL_RESULT_SUCCESS) || sensorsCount == 0)
    {
        WriteLine("\nTemperature component not supported. Error: %s" + (gcnew String(DecodeRetCode(result).c_str())));
        return gcnew array<TempSensor^>{};
    }
    else
    {
        WriteLine("\nNumber of Temperature Handles [%u]", sensorsCount);
    }

    if (CTL_RESULT_SUCCESS == result)
    {
        hTempSensors = new ctl_temp_handle_t[sensorsCount];

        if (hTempSensors != NULL)
        {
            result = ctlEnumTemperatureSensors(hDevices[0], &sensorsCount, hTempSensors);

            if (CTL_RESULT_SUCCESS == result)
            {
                List<TempSensor^>^ gcTempSensors 
                    = gcnew List<TempSensor^>();

                for (uint32_t i = 0; i < sensorsCount; i++)
                {
                    TempSensor^ ts = gcnew TempSensor;

                    WriteLine("\n\nFor Temperature Handle [%u]" + i.ToString());
                    WriteLine("\n[Temperature] Get Temperature properties:");

                    ctl_temp_properties_t tempProps{ 0 };
                    tempProps.Size = sizeof(ctl_temp_properties_t);
                    result = ctlTemperatureGetProperties(hTempSensors[i], &tempProps);

                    if (result != CTL_RESULT_SUCCESS)
                    {
                        WriteLine("\nError: %s"+ gcnew String(DecodeRetCode(result).c_str()) +" from Temperature get properties.");
                    }
                    else
                    {
                        WriteLine("[Temperature] Max temp [%u]" + ((uint32_t)tempProps.maxTemperature).ToString());
                        WriteLine("[Temperature] Sensor type [%s]" + ((tempProps.type == CTL_TEMP_SENSORS_GLOBAL) ? "Global" :
                            (tempProps.type == CTL_TEMP_SENSORS_GPU) ? "Gpu" :
                            (tempProps.type == CTL_TEMP_SENSORS_MEMORY) ? "Memory" :
                            "Unknown"));

                        ts->SensorType = ((tempProps.type == CTL_TEMP_SENSORS_GLOBAL) ? TempSensorType::Global :
                            (tempProps.type == CTL_TEMP_SENSORS_GPU) ? TempSensorType::GPU :
                            (tempProps.type == CTL_TEMP_SENSORS_MEMORY) ? TempSensorType::VRAM :
                            TempSensorType::Unknown);
                    }

                    WriteLine("[Temperature] Get Temperature state:");

                    double temperature = 0;
                    result = ctlTemperatureGetState(hTempSensors[i], &temperature);

                    if (result != CTL_RESULT_SUCCESS)
                    {
                        WriteLine("\nError: %s"+ gcnew String(DecodeRetCode(result).c_str()) +"  from Temperature get state.");
                    }
                    else
                    {
                        WriteLine("[Temperature] Current Temperature [%f"+ temperature.ToString() + "] C degrees \n \n");
                        ts->TemperatureInC = gcnew System::UInt16(temperature);
                    }

                    gcTempSensors->Add(ts);
                }

                return gcTempSensors->ToArray();
            }
        }
    }

    return gcnew array<TempSensor^>{};
}

// !! DANGEROUS !!
Boolean ArcticControlGPUInterop::GPUInterop::SetOverclockWaiver()
{
    if (hAPIHandle == nullptr && (*adapterCount) < 1)
    {
        goto Exit;
    }

    ctl_result_t result;
    result = ctlOverclockWaiverSet(hDevices[0]);

    if (CTL_RESULT_SUCCESS == result)
    {
        return true;
    }

Exit:
    return false;
}

Double^ ArcticControlGPUInterop::GPUInterop::GetOverclockTemperatureLimit()
{
    if (hAPIHandle == nullptr && (*adapterCount) < 1)
    {
        goto Exit;
    }

    ctl_result_t result;
    double tempLimit = 0.0;
    result = ctlOverclockTemperatureLimitGet(hDevices[0], &tempLimit);

    if (CTL_RESULT_SUCCESS == result)
    {
        return tempLimit;
    }

Exit:
    return gcnew Double(0.0);;
}

Boolean ArcticControlGPUInterop::GPUInterop::SetOverclockTemperatureLimit(Double^ newTempLimit)
{
    if (hAPIHandle == nullptr && (*adapterCount) < 1)
    {
        goto Exit;
    }

    ctl_result_t result;
    result = ctlOverclockTemperatureLimitSet(hDevices[0], *newTempLimit);

    if (CTL_RESULT_SUCCESS == result)
    {
        return true;
    }

Exit:
    return false;
}

Double^ ArcticControlGPUInterop::GPUInterop::GetOverclockPowerLimit()
{
    if (hAPIHandle == nullptr && (*adapterCount) < 1)
    {
        goto Exit;
    }

    ctl_result_t result;
    double sustainedPowerLimit = 0.0;
    result = ctlOverclockPowerLimitGet(hDevices[0], &sustainedPowerLimit);

    if (CTL_RESULT_SUCCESS == result)
    {
        // mW in W -> /1000
        return sustainedPowerLimit/1000;
    }

Exit:
    return gcnew Double(0.0);
}

Boolean ArcticControlGPUInterop::GPUInterop::SetOverclockPowerLimit(Double^ newPowerLimit)
{
    if (hAPIHandle == nullptr && (*adapterCount) < 1)
    {
        goto Exit;
    }

    ctl_result_t result;
    result = ctlOverclockPowerLimitSet(hDevices[0], *newPowerLimit);

    WriteLine(
        "[GPUInterop]: GPU PowerLimit overclock to " + newPowerLimit->ToString()
        + " with Status: " + gcnew String(DecodeRetCode(result).c_str()));
    if (CTL_RESULT_SUCCESS == result)
    {
        return true;
    }

Exit:
    return false;
}

Double^ ArcticControlGPUInterop::GPUInterop::GetOverclockGPUVoltageOffset()
{
    if (hAPIHandle == nullptr && (*adapterCount) < 1)
    {
        goto Exit;
    }

    ctl_result_t result;
    double voltageOffset;
    result = ctlOverclockGpuVoltageOffsetGet(hDevices[0], &voltageOffset);

    if (CTL_RESULT_SUCCESS == result)
    {
        return voltageOffset;
    }

Exit:
    return gcnew Double(0.0);
}

Boolean ArcticControlGPUInterop::GPUInterop::SetOverclockGPUVoltageOffset(Double^ newGPUVoltageOffset)
{
    if (hAPIHandle == nullptr && (*adapterCount) < 1)
    {
        goto Exit;
    }

    ctl_result_t result;
    result = ctlOverclockGpuVoltageOffsetSet(hDevices[0], *newGPUVoltageOffset);

    if (CTL_RESULT_SUCCESS == result)
    {
        return true;
    }

Exit:
    return false;
}

Double^ ArcticControlGPUInterop::GPUInterop::GetOverclockGPUFrequencyOffset()
{
    if (hAPIHandle == nullptr && (*adapterCount) < 1)
    {
        goto Exit;
    }

    ctl_result_t result;
    double frequencyOffset;
    result = ctlOverclockGpuFrequencyOffsetGet(hDevices[0], &frequencyOffset);

    if (CTL_RESULT_SUCCESS == result)
    {
        return frequencyOffset;
    }

Exit:
    return gcnew Double(0.0);
}

Boolean ArcticControlGPUInterop::GPUInterop::SetOverclockGPUFrequencyOffset(Double^ newGPUFrequencyOffset)
{
    if (hAPIHandle == nullptr && (*adapterCount) < 1)
    {
        goto Exit;
    }

    ctl_result_t result;
    result = ctlOverclockGpuFrequencyOffsetSet(hDevices[0], *newGPUFrequencyOffset);

    WriteLine(
        "[GPUInterop]: GPU Frequency offset: " + newGPUFrequencyOffset->ToString()
        + " with Status: " + gcnew String(DecodeRetCode(result).c_str()));
    if (CTL_RESULT_SUCCESS == result)
    {
        return true;
    }

Exit:
    return false;
}

Double^ ArcticControlGPUInterop::GPUInterop::GetOverclockVRAMVoltageOffset()
{
    if (hAPIHandle == nullptr && (*adapterCount) < 1)
    {
        goto Exit;
    }

    ctl_result_t result;
    double voltageOffset;
    result = ctlOverclockVramVoltageOffsetGet(hDevices[0], &voltageOffset);

    if (CTL_RESULT_SUCCESS == result)
    {
        return voltageOffset;
    }

Exit:
    return gcnew Double(0.0);
}

Boolean ArcticControlGPUInterop::GPUInterop::SetOverclockVRAMVoltageOffset(Double^ newVRAMVoltageOffset)
{
    if (hAPIHandle == nullptr && (*adapterCount) < 1)
    {
        goto Exit;
    }

    ctl_result_t result;
    result = ctlOverclockVramVoltageOffsetSet(hDevices[0], *newVRAMVoltageOffset);

    if (CTL_RESULT_SUCCESS == result)
    {
        return true;
    }

Exit:
    return false;
}

Double^ ArcticControlGPUInterop::GPUInterop::GetOverclockVRAMFrequencyOffset()
{
    if (hAPIHandle == nullptr && (*adapterCount) < 1)
    {
        goto Exit;
    }

    ctl_result_t result;
    double frequencyOffset;
    result = ctlOverclockVramFrequencyOffsetGet(hDevices[0], &frequencyOffset);

    if (CTL_RESULT_SUCCESS == result)
    {
        return frequencyOffset;
    }

Exit:
    return gcnew Double(0.0);
}

Boolean ArcticControlGPUInterop::GPUInterop::SetOverclockVRAMFrequencyOffset(Double^ newVRAMFrequencyOffset)
{
    if (hAPIHandle == nullptr && (*adapterCount) < 1)
    {
        goto Exit;
    }

    ctl_result_t result;
    result = ctlOverclockVramFrequencyOffsetSet(hDevices[0], *newVRAMFrequencyOffset);

    if (CTL_RESULT_SUCCESS == result)
    {
        return true;
    }

Exit:
    return false;
}

Boolean ArcticControlGPUInterop::GPUInterop::InitPowerDomains()
{
    if (hAPIHandle == nullptr || (*adapterCount) < 1)
    {
        goto Exit;
    }

    ctl_result_t result;
    // TODO: maybe free memory before if handle != nullptr so this could cause memory leaks
    hPwrHandle = nullptr;
    pwrCount = (uint32_t*)malloc(sizeof(*pwrCount));
    *pwrCount = 0;
    result = ctlEnumPowerDomains(hDevices[0], pwrCount, nullptr);
    if ((result != CTL_RESULT_SUCCESS) || pwrCount == 0)
    {
        WriteLine("Power component not supported. Error: " + gcnew String(DecodeRetCode(result).c_str()));
        goto Exit;
    }
    else
    {
        WriteLine("[GPUInterop]: Number of Power Handles " + gcnew UInt32(*pwrCount));
    }

    hPwrHandle = new ctl_pwr_handle_t[*pwrCount];

    result = ctlEnumPowerDomains(hDevices[0], pwrCount, hPwrHandle);

    if (result != CTL_RESULT_SUCCESS)
    {
        WriteLine("Error: " + gcnew String(DecodeRetCode(result).c_str()) + " for Power handle.");

        // cleanup mess
        delete[] hPwrHandle;
        hPwrHandle = nullptr;
        return false;
    }
    else 
    {
        return true;
    }

Exit:
    return false;
}

ArcticControlGPUInterop::PowerProperties^ ArcticControlGPUInterop::GPUInterop::GetPowerProperties()
{
    if (hAPIHandle == nullptr || (*adapterCount) < 1 || (*pwrCount) < 1 || hPwrHandle == nullptr)
    {
        goto Exit;
    }

    ctl_result_t result;
    ctl_power_properties_t powerProperties{ 0 };
    powerProperties.Size = sizeof(ctl_power_properties_t);
    result = ctlPowerGetProperties(hPwrHandle[0], &powerProperties);

    if (result == CTL_RESULT_SUCCESS)
    {
        PowerProperties^ pwrProperties = gcnew PowerProperties();
        pwrProperties->CanControl = powerProperties.canControl;
        pwrProperties->DefaultLimit = powerProperties.defaultLimit;
        pwrProperties->MinLimit = powerProperties.minLimit;
        pwrProperties->MaxLimit = powerProperties.maxLimit;

        return pwrProperties;
    }

Exit:
    return nullptr;
}

ArcticControlGPUInterop::PowerLimitsCombination^ ArcticControlGPUInterop::GPUInterop::GetPowerLimits()
{
    if (hAPIHandle == nullptr || (*adapterCount) < 1 || (*pwrCount) < 1 || hPwrHandle == nullptr)
    {
        goto Exit;
    }

    ctl_result_t result;
    ctl_power_limits_t powerLimits{ 0 };
    powerLimits.Size = sizeof(ctl_power_limits_t);
    result = ctlPowerGetLimits(hPwrHandle[0], &powerLimits);

    if (result == CTL_RESULT_SUCCESS)
    {
        // allocate result in GC and return

        SustainedPowerLimit^ sutainedPwrLimit = gcnew SustainedPowerLimit();
        sutainedPwrLimit->Enabled = powerLimits.sustainedPowerLimit.enabled;
        sutainedPwrLimit->Power = powerLimits.sustainedPowerLimit.power;
        sutainedPwrLimit->Interval = powerLimits.sustainedPowerLimit.interval;

        BurstPowerLimit^ burstPwrLimit = gcnew BurstPowerLimit();
        burstPwrLimit->Enabled = powerLimits.burstPowerLimit.enabled;
        burstPwrLimit->Power = powerLimits.burstPowerLimit.power;

        PeakPowerLimit^ peakPwrLimit = gcnew PeakPowerLimit();
        peakPwrLimit->PowerAC = powerLimits.peakPowerLimits.powerAC;
        peakPwrLimit->PowerDC = powerLimits.peakPowerLimits.powerDC;

        // combine all
        PowerLimitsCombination^ pwrLimitsCombi = gcnew PowerLimitsCombination();
        pwrLimitsCombi->SustainedPowerLimit = sutainedPwrLimit;
        pwrLimitsCombi->BurstPowerLimit = burstPwrLimit;
        pwrLimitsCombi->PeakPowerLimit = peakPwrLimit;

        return pwrLimitsCombi;
    }

Exit:
    // TODO: maybe throw error because nullptr can cause problems here with GC and so
    return nullptr;
}

ArcticControlGPUInterop::GPUInterop::!GPUInterop()
{
    if (hAPIHandle != nullptr)
    {
        ctlClose(*hAPIHandle);
        free(hAPIHandle);
        hAPIHandle = nullptr;
    }

    CTL_FREE_MEM(hDevices);
    CTL_FREE_MEM(adapterCount);
    CTL_FREE_MEM(hFans);
    CTL_FREE_MEM(fansCount);
    CTL_FREE_MEM(hPwrHandle);
    CTL_FREE_MEM(pwrCount);
}

// Decoding the return code for the most common error codes.
std::string DecodeRetCode(ctl_result_t Res)
{
    switch (Res)
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
