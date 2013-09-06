using System;
using System.Collections.Generic;

namespace SolarCar {
	/// <summary>
	/// Data aggregate, from BPS hardware.
	/// </summary>
	class BatteryReport {
		public float battery_current = 0;
		public float array_current = 0;
		// current counts
		public float battery_cc = 0;
		public float array_cc = 0;
		// energy counts
		public float battery_Ec = 0;
		public float array_Ec = 0;
		// Lists range 0-31
		public List<float> battery_voltages = null;
		public List<float> battery_temperatures = null;
		// current modes
		public int mode = 0;
		public int precharge = 0;
		// misc fields
		public int disabled_module = 0;
		public int last_error = 0;
		public int uptime = 0;
		public string message = "";
	}

	class ArrayReport {
		public int red_Uin = 0;
		public int red_Iin = 0;
		public int red_Uout = 0;
	}

	/// <summary>
	/// Data aggregate, from Motor hardware.
	/// </summary>
	class MotorReport {
		public float motor_rpm = 0;
		public float motor_velocity = 0;
	}

	class PedalReport {
		public int accel_pedal = 0;
		public int regen_pedal = 0;
		public bool brake_pedal = false;
	}

	class DigitalOutReport {
		public bool output = false;
		public string name = "";
	}

	/// <summary>
	/// Data aggregate, from SolarCar.Hardware for HttpServer
	/// </summary>
	class CarReport {
		public BatteryReport Battery = null;
		public ArrayReport Array = null;
		public float motor_rpm = 0f, motor_velocity = 0f;
		// Pedal status
		public bool brake_pedal = false;
		public float accel_pedal = 0;
		public float regen_pedel = 0;
		// Signal status
		public bool Horn, Headlights, Brakelights;
		public int Signal;
	}
}
