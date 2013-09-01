using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SolarCar {
	class Driver {
		readonly Hardware hardware = null;
		public bool DrivingMode = false;

		public Driver(Hardware hw) {
			this.hardware = hw;
		}

		void ChooseVelocity() {
			bool is_braking = this.hardware.BrakePedel;

			float accel = this.hardware.AccelAmount;
			float regen = this.hardware.RegenAmount;
			bool accel_en = accel > Config.ACCEL_THRESH;
			bool regen_en = regen > Config.REGEN_THRESH;

			float motor_velocity = 0;
			float motor_current = 0;

			if (DrivingMode) {
				if (is_braking || regen_en) {
					if (regen_en) {
						// regen MUST be between 0 and 1
						motor_current = regen;
					}
				} else if (accel_en) {
					motor_velocity = Config.MAX_VELOCITY;
					// accel MUST be between 0 and 1
					motor_current = accel;
				}
			}

			this.hardware.SetMotor(motor_velocity, motor_current);
		}

		public void RunLoop() {
			while (this.hardware != null) {
				this.ChooseVelocity();
				System.Threading.Thread.Sleep(1);
			}
		}
	}

	class MainClass {
		void ReadBps() {
			// runs I/O on main thread.
			Hardware hw = new Hardware();

		}

		public static void Main(string[] args) {
			Console.WriteLine("Hello World!");
			CarData.Test();
			ISerial<BatteryReport>.Test();
			ISerial<BatteryReport>.Test();
			ISerial<BatteryReport>.Test();

//			Console.WriteLine("Press any key to continue...");
//			Console.WriteLine();
//			Console.ReadKey();
		}
	}
}
