using System;

namespace SolarCar {
	class MotorController {
		readonly Hardware hardware = null;

		public MotorController(Hardware hw) {
			this.hardware = hw;
		}

		void ChooseVelocity() {
			bool is_braking = this.hardware.BrakePedel;

			float accel = this.hardware.AccelAmount;
			float regen = this.hardware.RegenAmount;

			float motor_velocity = 0;
			float motor_current = 0;

			// If User Driving is enabled, and if Precharging is complete.
			if (hardware.UserDrive && hardware.Precharge == 2) {
				if (is_braking) {
					this.hardware.BrakeLights = true;
				} else {
					this.hardware.BrakeLights = false;
					if (regen > 0) {
						// regen MUST be between 0 and 1
						motor_current = regen;
					} else if (accel > 0) {
						motor_velocity = Config.MAX_VELOCITY;
						// accel MUST be between 0 and 1
						motor_current = accel;
					}
				}
			}

			this.hardware.SetMotor(motor_velocity, motor_current);
		}

		public void RunLoop() {
			while (this.hardware != null) {
				this.ChooseVelocity();
				System.Threading.Thread.Sleep(1); // 1ms
			}
		}
	}
}

