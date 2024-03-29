#pragma once

#include "igcl_api.h"

using namespace System;
using namespace System::Collections::Generic;

namespace ArcticControlGPUInterop {
	public enum class TempSensorType : UInt16
	{
		Unknown = 0,
		Global = 1,
		GPU = 2,
		VRAM = 3
	};

	public ref class TempSensor
	{
	public:
		UInt16 TemperatureInC;
		TempSensorType SensorType;
	};

	public ref class PowerProperties
	{
	public:
		static PowerProperties^ create(const ctl_power_properties_t* power_properties)
		{
			auto gc_power_props = gcnew PowerProperties;

			gc_power_props->CanControl = power_properties->canControl;
			gc_power_props->DefaultLimit = power_properties->defaultLimit;
			gc_power_props->MinLimit = power_properties->minLimit;
			gc_power_props->MaxLimit = power_properties->maxLimit;

			return gc_power_props;
		}
		
		bool CanControl;
		Int32 DefaultLimit;
		Int32 MinLimit;
		Int32 MaxLimit;
	};
	
	/// <summary>
	/// The power controller (Punit) will throttle the operating 
	/// frequency if the power averaged over a 
	/// window (typically seconds) exceeds this limit.
	/// </summary>
	public ref class SustainedPowerLimit
	{
	public:
		static SustainedPowerLimit^ create(const ctl_power_sustained_limit_t* sustained_limit)
		{
			auto gc_sustained_limit = gcnew SustainedPowerLimit;

			gc_sustained_limit->Enabled = sustained_limit->enabled;
			gc_sustained_limit->Power = sustained_limit->power;
			gc_sustained_limit->Interval = sustained_limit->interval;

			return gc_sustained_limit;
		}
		
		/// <summary>
		/// Indicates if the limit is enabled (true) or ignored (false)
		/// </summary>
		bool Enabled;
		/// <summary>
		/// Power limit in milliwatts
		/// </summary>
		Int32 Power;
		/// <summary>
		/// Power averaging window (Tau) in milliseconds
		/// </summary>
		Int32 Interval;
	};

	/// <summary>
	/// <para>Burst power limit.</para>
	/// The power controller(Punit) will throttle 
	/// the operating frequency of the device if 
	/// the power averaged over a few milliseconds 
	/// exceeds a limit known as PL2. Typically 
	/// PL2 > PL1 so that it permits the 
	/// frequency to burst higher for short 
	/// periods than would be 
	/// otherwise permitted by PL1.
	/// </summary>
	public ref class BurstPowerLimit
	{
	public:
		static BurstPowerLimit^ create(const ctl_power_burst_limit_t* burst_limit)
		{
			auto gc_burst_limit = gcnew BurstPowerLimit;

			gc_burst_limit->Enabled = burst_limit->enabled;
			gc_burst_limit->Power = burst_limit->power;

			return gc_burst_limit;
		}
		
		/// <summary>
		/// Indicates if the limit is enabled (true) or ignored (false)
		/// </summary>
		bool Enabled;
		/// <summary>
		/// Power limit in milliwatts
		/// </summary>
		Int32 Power;
	};

	/// <summary>
	/// <para>Peak power limit.</para>
	///
	///	<para>The power controller(Punit) will 
	/// preemptively throttle the operating 
	/// frequency of the device when the 
	/// instantaneous power exceeds this 
	/// limit.The limit is known as PL4. 
	/// It expresses the maximum power 
	/// that can be drawn from the power supply.</para>
	///
	///	<para>If this power limit is removed or 
	/// set too high, the power supply will 
	/// generate an interrupt when it detects 
	/// an overcurrent conditionand the power 
	/// controller will throttle the device 
	/// frequencies down to min.It is thus 
	/// better to tune the PL4 value in 
	/// order to avoid such excursions.</para>
	/// </summary>
	public ref class PeakPowerLimit
	{
	public:
		static PeakPowerLimit^ create(const ctl_power_peak_limit_t* power_peak_limit)
		{
			auto gc_peak_power_limit = gcnew PeakPowerLimit;
			
			gc_peak_power_limit->PowerAC = power_peak_limit->powerAC;
			gc_peak_power_limit->PowerDC = power_peak_limit->powerDC;

			return gc_peak_power_limit;
		}
		
		/// <summary>
		/// Power limit in milli-watts for 
		/// the AC power source.
		/// </summary>
		Int32 PowerAC;
		/// <summary>
		/// Power limit in milli-watts for the DC 
		/// power source. On input, this is ignored 
		/// if the product does not have a battery. 
		/// On output, this will be -1 if the 
		/// product does not have a battery.
		/// </summary>
		Int32 PowerDC;
	};

	public ref class PowerLimitsCombination
	{
	public:		
		SustainedPowerLimit^ SustainedPowerLimit;
		BurstPowerLimit^ BurstPowerLimit;
		PeakPowerLimit^ PeakPowerLimit;
	};

	public ref class FanProperties
	{
	public:
		static FanProperties^ create(const ctl_fan_properties_t* fan_properties)
		{
			auto gc_fan_props = gcnew FanProperties;

			gc_fan_props->CanControl = fan_properties->canControl;
			gc_fan_props->MaxPoints = fan_properties->maxPoints;
			gc_fan_props->MaxRPM = fan_properties->maxRPM;
			gc_fan_props->SupportedModes = fan_properties->supportedModes;
			gc_fan_props->SupportedUnits = fan_properties->supportedUnits;

			return gc_fan_props;
		}
		
		/// <summary>
		/// Indicates if software can control the fan speed assuming the user has permissions
		/// </summary>
		bool CanControl;
		/// <summary>
		/// Bitfield of supported fan configuration modes (1<<ctl_fan_speed_mode_t)
		/// </summary>
		UInt32 SupportedModes;
		/// <summary>
		/// Bitfield of supported fan speed units (1<<ctl_fan_speed_units_t)
		/// </summary>
		UInt32 SupportedUnits;
		/// <summary>
		/// The maximum RPM of the fan. A value of -1 means that this property is unknown.
		/// </summary>
		Int32 MaxRPM;
		/// <summary>
		/// The maximum number of points in the fan temp/speed table. 
		/// A value of -1 means that this fan doesn’t support providing a temp/speed table.
		/// </summary>
		Int32 MaxPoints;
	};

	public enum class FanSpeedMode : UInt16
	{
		/// <summary>
		/// The fan speed is operating using the hardware default settings.
		/// </summary>
		Default,
		/// <summary>
		/// The fan speed is currently set to a fixed value.
		/// </summary>
		Fixed,
		/// <summary>
		/// The fan speed is currently controlled dynamically by hardware based on a temp/speed table.
		/// </summary>
		Table,
		Max
	};

	public enum class FanSpeedUnits : UInt16
	{
		/// <summary>
		/// The fan speed is in units of revolutions per minute (rpm)
		/// </summary>
		Rpm,
		/// <summary>
		/// The fan speed is a percentage of the maximum speed of the fan.
		/// </summary>
		Percent,
		Max
	};

	public ref class FanSpeed
	{
	public:
		static FanSpeed^ create(const ctl_fan_speed_t* fan_speed)
		{
			auto gc_fan_speed = gcnew FanSpeed;
			gc_fan_speed->Speed = fan_speed->speed;
			switch (fan_speed->units)
			{
			case CTL_FAN_SPEED_UNITS_RPM:
				gc_fan_speed->Units = FanSpeedUnits::Rpm;
				break;
			case CTL_FAN_SPEED_UNITS_PERCENT:
				gc_fan_speed->Units = FanSpeedUnits::Percent;
				break;
			case CTL_FAN_SPEED_UNITS_MAX:
				gc_fan_speed->Units = FanSpeedUnits::Max;
				break;
			default:
				break;
			}

			return gc_fan_speed;
		}
		
		/// <summary>
		/// The speed of the fan. On output, a value of -1 indicates that there is no fixed fan speed setting.
		/// </summary>
		Int32 Speed;
		/// <summary>
		/// The units that the fan speed is expressed in. On output, if fan speed is -1 then units should be ignored.
		/// </summary>
		FanSpeedUnits Units;
	};

	public ref class FanTempSpeed
	{		
	public:
		static FanTempSpeed^ create(const ctl_fan_temp_speed_t* fan_temp_speed)
		{
			auto gc_fan_temp_speed = gcnew FanTempSpeed;

			gc_fan_temp_speed->Temperature = fan_temp_speed->temperature;
			gc_fan_temp_speed->Speed = FanSpeed::create(&fan_temp_speed->speed);

			return gc_fan_temp_speed;
		}
		
		UInt32 Temperature;
		FanSpeed^ Speed;
	};

	public ref class FanSpeedTable
	{
	public:
		static FanSpeedTable^ create(const ctl_fan_speed_table_t* fan_speed_table)
		{
			auto gc_fan_speed_table = gcnew FanSpeedTable;
			
			gc_fan_speed_table->NumPoints = fan_speed_table->numPoints;
			gc_fan_speed_table->table = gcnew List<FanTempSpeed^>();
			for (const ctl_fan_temp_speed_t fan_temp_speed : fan_speed_table->table)
			{            
				gc_fan_speed_table->table->Add(FanTempSpeed::create(&fan_temp_speed));
			}

			return gc_fan_speed_table;
		}

		/// <summary>
		/// The number of valid points in the fan speed table.
		/// 0 means that there is no fan speed table configured. 
		/// -1 means that a fan speed table is not supported by the hardware.
		/// </summary>
		Int32 NumPoints;
		/// <summary>
		/// Array of temperature/fan speed pairs. The table is ordered based 
		/// on temperature from lowest to highest.
		/// </summary>
		List<FanTempSpeed^>^ table;
	};

	public ref class FanConfig
	{
	public:
		static FanConfig^ create(const ctl_fan_config_t* fan_config)
		{
			auto gc_fan_config = gcnew FanConfig;
			
			switch (fan_config->mode)
			{
			case CTL_FAN_SPEED_MODE_DEFAULT:
				gc_fan_config->Mode = FanSpeedMode::Default;
				break;
			case CTL_FAN_SPEED_MODE_FIXED:
				gc_fan_config->Mode = FanSpeedMode::Fixed;
				break;
			case CTL_FAN_SPEED_MODE_TABLE:
				gc_fan_config->Mode = FanSpeedMode::Table;
				break;
			case CTL_FAN_SPEED_MODE_MAX:
				gc_fan_config->Mode = FanSpeedMode::Max;
				break;
			default:
				break;
			}

			gc_fan_config->SpeedFixed = FanSpeed::create(&fan_config->speedFixed);
			gc_fan_config->SpeedTable = FanSpeedTable::create(&fan_config->speedTable);

			return gc_fan_config;
		}
		
		/// <summary>
		/// The fan speed mode (fixed, temp-speed table)
		/// </summary>
		FanSpeedMode^ Mode;
		/// <summary>
		/// The current fixed fan speed setting
		/// </summary>
		FanSpeed^ SpeedFixed;
		/// <summary>
		/// A table containing temperature/speed pairs
		/// </summary>
		FanSpeedTable^ SpeedTable;
	};

	public enum class ThreeDFeature : UInt16
	{
		FramePacing,
		EnduranceGaming,
		FrameLimit,
		Anisotropic,
		Cmaa,
		TextureFilteringQuality,
		AdaptiveTessellation,
		SharpeningFilter,
		Msaa,
		GamingFlipMode,
		AppProfiles,
		AppProfileDetails,
		EmulatedTyped64BitAtomics,
		Max
	};

	public enum class PropValueType : UInt16
	{
		Bool,
		Float,
		Int32,
		Uint32,
		Enum,
		Custom,
		Max
	};

	/*
	public union PropInfo
	{
		
	};
	
	public ref class ThreeDFeatureDetails
	{
	public:
		ThreeDFeature^ FeatureType;
		PropValueType^ ValueType;
		
	};*/

	public ref class PCIeProperties
	{
	public:
		bool IsReBarSupported;
		bool IsReBarEnabled;
		System::Int32 Lanes;
		System::Int32 Gen;
	};

	public ref class FrequencyProperties
	{
	public:
		static FrequencyProperties^ create(const ctl_freq_properties_t* freq_props)
		{
			auto gc_freq_props = gcnew FrequencyProperties;
			gc_freq_props->CanControl = freq_props->canControl;
			gc_freq_props->HardwareMin = freq_props->min;
			gc_freq_props->HardwareMax = freq_props->min;

			return gc_freq_props;
		}
		
		bool CanControl;
		/// <summary>
		/// The maximum non-overclock hardware clock frequency in units of MHz.
		/// </summary>
		double HardwareMin;
		/// <summary>
		/// The minimum hardware clock frequency in units of MHz.
		/// </summary>
		double HardwareMax;
	};

	public enum class FrequencyCap : UInt32
	{
		AveragePowerCap = 1,
		BurstPowerCap = 2,
		CurrentLimit = 4,
		ThermalLimit = 8,
		PsuAlert = 16,
		SoftwareRange = 32,
		HardwareRange = 64,
		Max = 0x80000000,
		Unknown = 999
	};

	public ref class FrequencyState
	{
	public:
		static FrequencyState^ create(const ctl_freq_state_t* freq_state)
		{
			auto gc_freq_state = gcnew FrequencyState;

			gc_freq_state->CurrentVoltage = freq_state->currentVoltage;
			gc_freq_state->RequestedFrequency = freq_state->request;
			gc_freq_state->TDPFrequency = freq_state->tdp;
			gc_freq_state->EfficientFrequency = freq_state->efficient;
			gc_freq_state->ActualFrequency = freq_state->actual;

			gc_freq_state->Cap = static_cast<FrequencyCap>(freq_state->throttleReasons);

			if (gc_freq_state->Cap == FrequencyCap::Max)
			{
				gc_freq_state->Cap = FrequencyCap::Unknown;
			}
			
			return gc_freq_state;
		}
		
		double CurrentVoltage;
		double RequestedFrequency;
		double TDPFrequency;
		double EfficientFrequency;
		double ActualFrequency;
		FrequencyCap Cap;
	};

	public enum class Units : UInt32
	{
		// Type is Frequency with units in MHz.
		FrequencyMhz = 0,
		// Type is Frequency with units in GT/s (gigatransfers per second).
		OperationsGts = 1,
		// Type is Frequency with units in MT/s (megatransfers per second).
		OperationsMts = 2,
		// Type is Voltage with units in Volts.
		VoltageVolts = 3,
		// Type is Power with units in Watts.
		PowerWatts = 4,
		// Type is Temperature with units in Celsius.
		TemperatureCelsius = 5,
		// Type is Energy with units in Joules.
		EnergyJoules = 6,
		// Type is Time with units in Seconds.
		TimeSeconds = 7,
		// Type is Memory with units in Bytes.
		MemoryBytes = 8,
		// Type is Angular Speed with units in Revolutions per Minute.
		AngularSpeedRpm = 9,
		// Type of units unknown.
		Unknown = 0x4800FFFF,
		Max
	};

	public enum class DataType : UInt32
	{
		// The data type is 8 bit signed integer.
		Int8 = 0,
		// The data type is 8 bit unsigned integer.
		Uint8 = 1,
		// The data type is 16 bit signed integer.
		Int16 = 2,
		// The data type is 16 bit unsigned integer.
		Uint16 = 3,
		// The data type is 32 bit signed integer.
		Int32 = 4,
		// The data type is 32 bit unsigned integer.
		Uint32 = 5,
		// The data type is 64 bit signed integer.
		Int64 = 6,
		// The data type is 64 bit unsigned integer.
		Uint64 = 7,
		// The data type is 32 bit floating point.
		Float = 8,
		// The data type is 64 bit floating point.
		Double = 9,
		// The data type is an array of 8 bit unsigned integers.
		StringAscii = 10,
		// The data type is an array of 16 bit unsigned integers.
		StringUtf16 = 11,
		// The data type is an array of 32 bit unsigned integers.
		StringUtf132 = 12,
		// The data type is unknown.
		Unknown = 0x4800FFFF,
		Max
	};
	
	public ref class OcTelemetryItem
	{
	public:
		bool BSupported;
		Units Units;
		DataType Type;
		System::Object Value;
	};

	public ref class PowerTelemetry
	{
	public:
		OcTelemetryItem^ TimeStamp;
		OcTelemetryItem^ GpuEnergyCounter;
		OcTelemetryItem^ GpuVoltage;
		OcTelemetryItem^ GpuCurrentClockFrequency;
		OcTelemetryItem^ GpuCurrentTemperature;
		OcTelemetryItem^ GlobalActivityCounter;
		OcTelemetryItem^ RenderComputeActivityCounter;
		OcTelemetryItem^ MediaActivityCounter;
		bool GpuPowerLimit;
		bool GpuTemperatureLimited;
		bool GpuCurrentLimited;
		bool GpuVoltageLimited;
		bool GpuUtilizationLimited;
		OcTelemetryItem^ VramEnergyCounter;
		OcTelemetryItem^ VramVoltage;
		OcTelemetryItem^ VramCurrentClockFrequency;
		OcTelemetryItem^ VramCurrentEffectiveFrequency;
		OcTelemetryItem^ VramReadBandwidthCounter;
		
	};

	public ref class GPUInterop
	{
		array<uint32_t>^ supported_device_ids_;

		uint32_t* adapter_count_;
		ctl_device_adapter_handle_t* h_devices_;
		int selected_device_ = -1;
		List<String^>^ device_names_;

		// fans
		uint32_t* fans_count_;
		ctl_fan_handle_t* h_fans_;

		// api
		ctl_api_handle_t* h_api_handle_;

		// power
		uint32_t* pwr_count_;
		ctl_pwr_handle_t* h_pwr_handle_;

		// frequency
		int selected_freq_ = -1;
		ctl_freq_handle_t* h_freq_handle_;
		
		/// <summary>
		/// Get control api handle if adapter is connected.
		/// </summary>
		/// <returns>Api init succeed</returns>
		bool init_api();

	public:
		GPUInterop() {			
			// https://dgpu-docs.intel.com/devices/hardware-table.html
			supported_device_ids_ = gcnew array<UInt32>(11) {
				0x5690, // <-- Arc A77M
				0x56A0, // <-- Arc A770
				0x5691, // <-- Arc A730M
				0x56A1, // <-- Arc A750
				0x5692, // <-- Arc A550M
				0x56A5, // <-- Arc A380
				0x5693, // <-- Arc A370M
				0x5694, // <-- Arc A350M
				0x56A6, // <-- Arc A310
			};

			device_names_ = gcnew List<System::String^>;
		};

		!GPUInterop();
		~GPUInterop() { this->!GPUInterop(); };

		// add your methods for this class here
		bool InitCtlApi();
		String^ GetAdapterName();
		List<String^>^ GetDeviceAdapterNames()
		{
			return device_names_;
		}
		void ChangeSelectedDevice(int idx)
		{
			selected_device_ = idx;
		}
		array<TempSensor^>^ GetTemperatures();
		/// <summary>
		/// <para>Very dangerous!! - Read description</para>
		/// <para>Overclock Waiver - Warranty Waiver.</para>
		///
		/// <para>- Most of the overclock functions will return an error if the waiver is not set. 
		///	  This is because most overclock settings will increase the electric / thermal stress 
		///   on the part and thus reduce its lifetime.</para>
		///
		///	<para>- By setting the waiver, the user is indicate that they are accepting a reduction 
		/// in the lifetime of the part.</para>
		///
		///	<para>- It is the responsibility of overclock applications to notify each user at least 
		/// once with a popup of the dangersand requiring acceptance.</para>
		///
		///	<para>- Only once the user has accepted should this function be called by the application.</para>
		///
		///	<para>- It is acceptable for the application to cache the user choiceand call this function 
		/// on future executions without issuing the popup.</para>
		/// 
		/// <para>Excerpt from the documentation provided by Intel. - 31.12.2022</para>
		/// </summary>
		/// <returns>True if the action as executed with a SUCESS result.</returns>
		bool SetOverclockWaiver();

		double GetOverclockTemperatureLimit();
		bool SetOverclockTemperatureLimit(double new_temp_limit);
		double GetOverclockPowerLimit();
		bool SetOverclockPowerLimit(double new_power_limit);

		// GPU
		double GetOverclockGPUVoltageOffset();
		bool SetOverclockGPUVoltageOffset(double new_gpu_voltage_offset);
		double GetOverclockGPUFrequencyOffset();
		bool SetOverclockGPUFrequencyOffset(double new_gpu_frequency_offset);

		// VRAM (useless for Arc GPUs)
		double GetOverclockVRAMVoltageOffset();
		bool SetOverclockVRAMVoltageOffset(double new_vram_voltage_offset);
		double GetOverclockVRAMFrequencyOffset();
		bool SetOverclockVRAMFrequencyOffset(double new_vram_frequency_offset);

		// gpu lock
		System::Tuple<double, double>^ GetOverclockGPULock();
		bool SetOverclockGPULock(double voltage, double frequency);

		// overclock power telemetry
		
		// Power
		bool InitPowerDomains();
		PowerProperties^ GetPowerProperties();
		PowerLimitsCombination^ GetPowerLimits();

		// Fans
		bool InitFansHandles();
		FanProperties^ GetFanProperties();
		FanConfig^ GetFanConfig();
		bool SetFansToDefaultMode();
		System::Int32 GetFanSpeed();
		bool SetFansToFixedSpeed(System::Int32 speed);

		// Games (3d) - provide null to application if want to get/set global settings
		UInt32 GetGamingFlipMode(String^ application);
		bool SetGamingFlipMode(UInt32 flip_mode, String^ application);
		UInt32 GetAnisotropicFilteringMode(String^ application);
		bool SetAnisotropicFilteringMode(UInt32 anisotropic_mode, String^ application);
		UInt32 GetCmaaMode(String^ application);
		bool SetCmaaMode(UInt32 cmaa_mode, String^ application);

		bool IsSharpeningFilterActive(String^ application);
		bool SetSharpeningFilter(bool on, String^ application);

		// Frequency
		bool InitFrequencyDomains();
		FrequencyProperties^ GetFrequencyProperties();
		FrequencyState^ GetFrequencyState();
		System::Tuple<double, double>^ GetMinMaximumFrequency();
		bool SetMinMaximumFrequency(double min_freq, double max_freq);

		// system status/info
		PCIeProperties^ GetPCIeProperties();

		// just for test
		bool TestApi();
	};
}
