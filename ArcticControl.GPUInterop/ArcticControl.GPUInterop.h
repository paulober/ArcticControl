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
		UInt16^ TemperatureInC;
		TempSensorType SensorType;
	};

	public ref class PowerProperties
	{
	public:
		Boolean^ CanControl;
		Int32^ DefaultLimit;
		Int32^ MinLimit;
		Int32^ MaxLimit;
	};
	
	/// <summary>
	/// The power controller (Punit) will throttle the operating 
	/// frequency if the power averaged over a 
	/// window (typically seconds) exceeds this limit.
	/// </summary>
	public ref class SustainedPowerLimit
	{
	public:
		/// <summary>
		/// Indicates if the limit is enabled (true) or ignored (false)
		/// </summary>
		Boolean^ Enabled;
		/// <summary>
		/// Power limit in milliwatts
		/// </summary>
		Int32^ Power;
		/// <summary>
		/// Power averaging window (Tau) in milliseconds
		/// </summary>
		Int32^ Interval;
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
		/// <summary>
		/// Indicates if the limit is enabled (true) or ignored (false)
		/// </summary>
		Boolean^ Enabled;
		/// <summary>
		/// Power limit in milliwatts
		/// </summary>
		Int32^ Power;
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
		/// <summary>
		/// Power limit in milliwatts for 
		/// the AC power source.
		/// </summary>
		Int32^ PowerAC;
		/// <summary>
		/// Power limit in milliwatts for the DC 
		/// power source. On input, this is ignored 
		/// if the product does not have a battery. 
		/// On output, this will be -1 if the 
		/// product does not have a battery.
		/// </summary>
		Int32^ PowerDC;
	};

	public ref class PowerLimitsCombination
	{
	public:
		SustainedPowerLimit^ SustainedPowerLimit;
		BurstPowerLimit^ BurstPowerLimit;
		PeakPowerLimit^ PeakPowerLimit;
	};

	public ref class GPUInterop
	{
	private:
		uint32_t* adapterCount;
		ctl_device_adapter_handle_t* hDevices;
		uint32_t* fansCount;
		ctl_fan_handle_t* hFans;
		ctl_api_handle_t* hAPIHandle;
		uint32_t* pwrCount;
		ctl_pwr_handle_t* hPwrHandle;

		/// <summary>
		/// Get control api handle if adapter is connected.
		/// </summary>
		/// <returns>Api init succeed</returns>
		Boolean initApi();

	public:
		!GPUInterop();
		~GPUInterop() { this->!GPUInterop(); };

		// Fügen Sie hier Ihre Methoden für diese Klasse ein
		Boolean InitCtlApi();
		String^ GetAdapterName();
		void SetFansToDefaultMode();
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
		Boolean SetOverclockWaiver();

		Double^ GetOverclockTemperatureLimit();
		Boolean SetOverclockTemperatureLimit(Double^ newTempLimit);
		Double^ GetOverclockPowerLimit();
		Boolean SetOverclockPowerLimit(Double^ newPowerLimit);

		// GPU
		Double^ GetOverclockGPUVoltageOffset();
		Boolean SetOverclockGPUVoltageOffset(Double^ newGPUVoltageOffset);
		Double^ GetOverclockGPUFrequencyOffset();
		Boolean SetOverclockGPUFrequencyOffset(Double^ newGPUFrequencyOffset);

		// VRAM (useless for Arc GPUs)
		Double^ GetOverclockVRAMVoltageOffset();
		Boolean SetOverclockVRAMVoltageOffset(Double^ newVRAMVoltageOffset);
		Double^ GetOverclockVRAMFrequencyOffset();
		Boolean SetOverclockVRAMFrequencyOffset(Double^ newVRAMFrequencyOffset);

		// Power
		Boolean InitPowerDomains();
		PowerProperties^ GetPowerProperties();
		PowerLimitsCombination^ GetPowerLimits();

		// just for test
		String^ GetMyName();
		Boolean TestApi();
	};
}
