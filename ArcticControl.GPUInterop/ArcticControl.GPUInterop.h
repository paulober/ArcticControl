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
