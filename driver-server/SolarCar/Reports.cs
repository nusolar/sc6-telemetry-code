using System;
using System.Collections.Generic;

namespace SolarCar {
	class MotorReport {
		public float motor_rpm = 0;
		public float motor_velocity = 0;
	}

	class ArrayReport {
	}

	/// <summary>
	/// Data aggregate, holding a BPS report
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
		public int mode = 0;
		// misc fields
		public int disabled_module = 0;
		public string message = "";
		public int uptime = 0;
	}

	class DigitalOutReport {
		public bool output = false;
		public string name = "";
	}

	class PedalReport {
		public int accel_pedal = 0;
		public bool brake_pedal = false;
	}
}
