using System;
using System.Collections.Generic;

namespace SolarCar {
	namespace Car {
		[Flags]
		enum Mode {
			Off = 0,
			Discharging = 1,
			Drive = 2 | 1,
			EmptyCharging = 4,
			Charging = 4 | 1,
			DriveCharging = 4 | 2 | 1
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
			Headlights = 4,
			Horn = 8
		}

		class Status {
			// Driver inputs
			public Mode RequestedMode;
			public Gears Gear;
			public Signals RequestedSignals;
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
		public Car.Mode mode = Car.Mode.Off;
		public Car.Gears gears = Car.Gears.None;
		public Car.Signals sigs = Car.Signals.None;
	}

	class DataAggregator {
		public Car.Status status = new Car.Status();

		public DataAggregator() {
		}

		public void HandleUserInput(UserInput input) {
			this.status.RequestedMode = input.mode;
			this.status.Gear = input.gears;
			this.status.RequestedSignals = input.sigs;
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

