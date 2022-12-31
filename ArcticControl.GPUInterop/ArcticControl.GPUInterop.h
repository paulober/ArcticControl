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

	public ref class GPUInterop
	{
	private:
		uint32_t* adapterCount;
		ctl_device_adapter_handle_t* hDevices;
		uint32_t* fansCount;
		ctl_fan_handle_t* hFans;
		ctl_api_handle_t* hAPIHandle;

		/// <summary>
		/// Get control api handle if adapter is connected.
		/// </summary>
		/// <returns>Api init succeed</returns>
		bool^ initApi();

	public:
		!GPUInterop();
		~GPUInterop() { this->!GPUInterop(); };

		// Fügen Sie hier Ihre Methoden für diese Klasse ein
		Boolean^ InitCtlApi();
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
		Boolean^ SetOverclockWaiver();

		Double^ GetOverclockTemperatureLimit();
		Boolean^ SetOverclockTemperatureLimit(Double^ newTempLimit);
		Double^ GetOverclockPowerLimit();
		Boolean^ SetOverclockPowerLimit(Double^ newPowerLimit);

		// GPU
		Double^ GetOverclockGPUVoltageOffset();
		Boolean^ SetOverclockGPUVoltageOffset(Double^ newGPUVoltageOffset);
		Double^ GetOverclockGPUFrequencyOffset();
		Boolean^ SetOverclockGPUFrequencyOffset(Double^ newGPUFrequencyOffset);

		// VRAM
		Double^ GetOverclockVRAMVoltageOffset();
		Boolean^ SetOverclockVRAMVoltageOffset(Double^ newVRAMVoltageOffset);
		Double^ GetOverclockVRAMFrequencyOffset();
		Boolean^ SetOverclockVRAMFrequencyOffset(Double^ newVRAMFrequencyOffset);

		// just for test
		String^ GetMyName();
		Boolean^ TestApi();
	};
}
