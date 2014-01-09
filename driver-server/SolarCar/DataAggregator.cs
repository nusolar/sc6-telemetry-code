using System;
using System.Collections.Generic;

namespace SolarCar {
	namespace Car {
		enum Mode {
			Off = 0,
			Discharging = 1,
			Drive = 3,
			EmptyCharging = 4,
			Charging = 5,
			DriveCharging = 7
		}

		[Flags]
		enum Gears {
			None = 0,
			Drive = 1,
			Reverse = 2
		}

		[Flags]
		enum Signals {
			None = 0,
			Left = 1,
			Right = 2,
			Headlights = 4
		}

		class Status {
			// Driver inputs
			public Mode RequestedMode;
			public Gears Gear;
			public Signals RequestedLights;
			public bool Horn;
			// Pedals
			public UInt16 AccelPedal;
			public UInt16 RegenPedal;
			public bool Braking;
			// motor
			public float MotorRpm, MotorVelocity;
			public float MotorVoltage, MotorCurrent;
			// BPS
			public Mode BPSMode;
			public UInt64 BatteryHealth;
			public float BatteryCurrent;
			public float ArrayCurrent;
			// Lists range 0-31
			public List<float> BatteryVoltages = null;
			public List<float> BatteryTemps = null;
		}
	}
	class UserInput {
		public bool power = false, array = false;
		public bool drive = false, reverse = false;
		public Car.Signals sigs = Car.Signals.None;
		public bool horn = false;
	}

	class DataAggregator {
		public Car.Status status = new Car.Status();
		public UserInput input = null;

		public DataAggregator() {
		}

		public void TxCanPacket() {
			// power :1
			// array :1

			// gear/drive :1
			// reverse :1

			// signals :3
			// horn :1
		}

		public void HandleCanPacket(CanPacket p) {
			switch (p.ID) {
				case CanAddr.Pedals.Status:
					// accel, regen, brake pedal
					break;
				case CanAddr.WS20.motor_bus:
					// voltage; current;
					break;
				case CanAddr.WS20.motor_velocity:
					// RPM; velocity;
					break;
			}
		}
	}
}

