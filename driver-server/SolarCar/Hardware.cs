using System;

namespace SolarCar {
	/// <summary>
	/// All Hardware. Instantiating this class activate DataReceived events for all hardware.
	/// </summary>
	class Hardware {
		readonly BPS bps = new BPS();
		readonly CanUsb can_bus = new CanUsb("/dev/tty.CANUSB");
		readonly PedalIn pedals = new PedalIn("/dev/tty.PEDALS");
		readonly DigitalOut brake_lights = new DigitalOut("/dev/tty.BRAKEL");
		readonly DigitalOut right_signal = new DigitalOut("/dev/tty.RIGHTL");
		readonly DigitalOut left_signal = new DigitalOut("/dev/tty.LEFTL");
		readonly DigitalOut headlights = new DigitalOut("/dev/tty.HEADL");
		readonly DigitalOut horn = new DigitalOut("/dev/tty.HORN");

		public Hardware() {
		}
		#region BPS HW control
		/// <summary>
		/// String representing Health of batteries.
		/// </summary>
		public string Health() {
			for (int i=0; i<Config.NUM_BATTERIES; i++) {
				if (bps.Report.battery_voltages[i] > Config.MAX_VOLTAGE) {
					return "OVER_VOLT_" + i.ToString();
				} else if (bps.Report.battery_voltages[i] < Config.MIN_VOLTAGE) {
					return "UNDER_VOLT_" + i.ToString();
				} else if (bps.Report.battery_temperatures[i] > Config.MAX_TEMP) {
					return "OVER_TEMP_" + i.ToString();
				} else if (bps.Report.battery_temperatures[i] < Config.MIN_TEMP) {
					return "UNDER_TEMP_" + i.ToString();
				}
			}

			if (bps.Report.battery_current > Config.MAX_BATT_CURRENT_DISCHARGING) {
				return "OVER_CURRENT_DISCHARGING_0";
			} else if (bps.Report.battery_current < Config.MAX_BATT_CURRENT_CHARGING) {
				return "OVER_CURRENT_CHARGING_0";
			} else if (bps.Report.array_current > Config.MAX_ARRAY_CURRENT) {
				return "OVER_CURRENT_1";
			} else if (bps.Report.array_current < Config.MIN_ARRAY_CURRENT) {
				return "UNDER_CURRENT_1";
			}

			return "NONE";
		}

		public bool CanDischarge() {
			for (int i=0; i<Config.NUM_BATTERIES; i++) {
				if (bps.Report.battery_voltages[i] < Config.MIN_DISCHARGE_VOLTAGE) {
					return false;
				}
			}
			return true;
		}

		public bool CanCharge() {
			for (int i=0; i<Config.NUM_BATTERIES; i++) {
				if (bps.Report.battery_voltages[i] > Config.MAX_CHARGE_VOLTAGE) {
					return false;
				}
			}
			return true;
		}

		public int Mode { 
			get { return this.bps.Report.mode; }
			set { this.bps.SetMode(value); }
		}

		public int Precharge {
			get { return this.bps.Report.precharge; }
		}
		#endregion
		/**
		 * Wraps communication with MotorController Hardware, and the Pedals
		 */
		#region Motor HW control
		public void SetMotor(float motor_velocity, float motor_current) {
			can_bus.SendDriveCmd(motor_velocity, motor_current);
		}

		public float GetMotorVelocity { get { return this.can_bus.motor_report.motor_velocity; } }

		/// <summary>
		/// Gets a value indicating whether the brake pedel is pressed.
		/// </summary>
		/// <value><c>true</c> if brake pedel is pressed; otherwise, <c>false</c>.</value>
		public bool BrakePedel { get { return pedals.Report.brake_pedal; } }

		/// <summary>
		/// Gets the acceleration pedel's value.
		/// </summary>
		/// <value>The accel pedel, ranges 0-1.</value>
		public float AccelAmount {
			get {
				float accel = (pedals.Report.accel_pedal + 0) / 1023;
				accel = (accel > 1) ? 1 : ((accel < 0) ? 0 : accel);
				return (accel > Config.ACCEL_THRESH) ? accel : 0;
			}
		}

		/// <summary>
		/// Gets the regen amount.
		/// </summary>
		/// <value>Regen amount, ranges 0-1.</value>
		public float RegenAmount {
			get {
				float regen = (pedals.Report.regen_pedal + 0) / 1023;
				regen = (regen > 1) ? 1 : ((regen < 0) ? 0 : regen);
				return (regen > Config.REGEN_THRESH) ? regen : 0;
			}
		}
		#endregion
		/**
		 * Wraps communication with Signal Hardware.
		 */
		#region Driver signal HW controls
		/// <summary>
		/// Assignable value representing the headlights.
		/// </summary>
		/// <value><c>true</c> if headlights; otherwise, <c>false</c>.</value>
		public bool HeadLights {
			get { return this.headlights.Value;}
			set { this.headlights.Set(value);}
		}

		/// <summary>
		/// Assignable value representing the brake lights.
		/// </summary>
		/// <value><c>true</c> if brake lights; otherwise, <c>false</c>.</value>
		public bool BrakeLights {
			get { return this.brake_lights.Value;}
			set { this.brake_lights.Set(value);}
		}

		/// <summary>
		/// Assignable value representing the right turn signal.
		/// </summary>
		/// <value><c>true</c> if right signal; otherwise, <c>false</c>.</value>
		public bool RightSignal {
			get { return this.right_signal.Value;}
			set { this.right_signal.Set(value);}
		}

		/// <summary>
		/// Assignable value representing the left turn signal.
		/// </summary>
		/// <value><c>true</c> if left signal; otherwise, <c>false</c>.</value>
		public bool LeftSignal {
			get { return this.left_signal.Value;}
			set { this.left_signal.Set(value);}
		}

		/// <summary>
		/// Assignable value representing the horn.
		/// </summary>
		/// <value><c>true</c> if horn; otherwise, <c>false</c>.</value>
		public bool Horn {
			get { return this.horn.Value;}
			set { this.horn.Set(value);}
		}
		#endregion
		#region User commands
		public enum Signals {
			Off = 0,
			Left,
			Right,
			Hazards
		}

		public bool UserDrive = false;
		public bool UserHorn = false;
		public Signals UserSignal = Signals.Off;
		public bool UserHeadlights = false;
		#endregion
		public CarReport Report {
			get {
				CarReport rep = new CarReport();
				rep.Battery = this.bps.Report;

				// TODO get array values

				// TODO more motor values
				rep.motor_velocity = this.can_bus.motor_report.motor_velocity;
				rep.motor_rpm = this.can_bus.motor_report.motor_rpm;

				rep.accel_pedal = this.AccelAmount;
				rep.regen_pedel = this.RegenAmount;
				rep.brake_pedal = this.BrakePedel;

				rep.Brakelights = this.BrakeLights;
				// using User values as actual.
				rep.Horn = this.UserHorn;
				rep.Headlights = this.UserHeadlights;
				rep.Signal = (int)this.UserSignal;
				return rep;
			}
		}
	}
}

