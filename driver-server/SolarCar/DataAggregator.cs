using System;
using System.Collections.Generic;

namespace SolarCar {
	class CarStatus {
		public enum Mode {
			Off = 0,
			Discharging,
			Drive,
			EmptyCharging,
			Charging,
			DriveCharging
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

	public class DataAggregator {
		CarStatus status = new CarStatus();

		public DataAggregator() {
		}

		void SetUserInput(CarStatus.Mode mode, CarStatus.Signals signals, bool headlights, bool horn) {
			status.RequestedMode = mode;
			status.RequestedSignals = signals;
			status.Headlights = headlights;
			status.Horn = horn;
		}

		void HandlePedals(CanPacket p) {
			switch (p.ID) {
				case CanAddr.Pedals.Status:
					// accel, regen, brake pedal
					break;
			}
		}

		void HandleMotorController(CanPacket p) {
			if (p.ID == CanAddr.WS20.motor_bus) {
				// voltage; current;
			} else if (p.ID == CanAddr.WS20.motor_velocity) {
				// RPM; velocity;
			}
		}
	}
}

