using System;
using System.Collections.Generic;

namespace SolarCar {
	sealed class CarStatus {
		public enum Mode {
			Off = 0,
			Discharging = 1,
			Drive = 3,
			EmptyCharging = 4,
			Charging = 5,
			DriveCharging = 7
		}

		public enum Signals {
			Off = 0,
			Left,
			Right,
			Hazards
		}
		// Pedals
		public UInt16 AccelPedal;
		public UInt16 RegenPedal;
		public bool Braking;
		public bool Reverse;
		// motor
		public float MotorRpm, MotorVelocity;
		public float MotorVoltage, MotorCurrent;
		// lights
		public Signals RequestedSignals;
		public bool Headlights;
		public bool Horn;
		// BPS modes
		public Mode RequestedMode;
		public Mode BPSMode;
		public UInt64 BatteryHealth;
		// Lists range 0-31
		public List<float> BatteryVoltages = null;
		public List<float> BatteryTemps = null;
	}

	sealed class UserInput {
		public bool power = false, drive = false, reverse = false;
		public CarStatus.Signals sigs = CarStatus.Signals.Off;
		public bool heads = false, horn = false;
	}

	sealed class DataAggregator {
		CarStatus status = new CarStatus();

		public DataAggregator() {
		}

		public UserInput input;

		void TxCanPacket() {
			// power :1
			// gear/drive :1
			// reverse :1

			// signals :2
			// head :1
			// horn :1
		}

		void HandleCanPacket(CanPacket p) {
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

