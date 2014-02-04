using System;
using System.Collections.Generic;

namespace SolarCar
{
	namespace Car
	{
		[Flags]
		enum Mode
		{
			Off = 0,
			Discharging = 1,
			Drive = 2 | 1,
			EmptyCharging = 4,
			Charging = 4 | 1,
			DriveCharging = 4 | 2 | 1
		}

		[Flags]
		enum Gear
		{
			None = 0,
			Drive = 1,
			Reverse = 2
		}

		[Flags]
		enum Signals
		{
			None = 0,
			Left = 1,
			Right = 2,
			Headlights = 4,
			Horn = 8
		}

		class Status
		{
			// Driver inputs
			public Mode RequestedMode;
			public Gear Gear;
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
	class UserInput
	{
		public Car.Mode mode = Car.Mode.Off;
		public Car.Gear gear = Car.Gear.None;
		public Car.Signals sigs = Car.Signals.None;
	}

	class DataAggregator
	{
		Car.Status status = new Car.Status();
		CanUsb canusb = new CanUsb(Config.CANUSB_DEV_FILE);

		public DataAggregator()
		{
			CanHandler ch = new CanHandler(0, Can.Addr.ws20.motor_bus, this.HandleCanPacket);
			canusb.handlers.Add(ch);

		}

		public Car.Status Status { get { return this.status; } }

		public void HandleUserInput(UserInput input)
		{
			this.status.RequestedMode = input.mode;
			this.status.Gear = input.gear;
			this.status.RequestedSignals = input.sigs;

			#if DEBUG
			Console.WriteLine("Set Mode={0}, Gear={1}, Signals={2}", input.mode, input.gear, input.sigs);
			#endif
		}

		public void HandleCanPacket(Can.Packet p)
		{
			switch (p.ID)
			{
				case Can.Addr.pedals.Status:
					// accel, regen, brake pedal
					break;
				case Can.Addr.ws20.motor_bus:
					// voltage; current;
					break;
				case Can.Addr.ws20.motor_velocity:
					// RPM; velocity;
					break;
			}
		}

		void TxCanPacket()
		{
			Can.Packet p = new Can.Packet(0, 8, 0);
			p.frame.uint16x4.uint16_0 = (UInt16)this.status.RequestedMode;
			// power :1
			// drive :1
			// array :1

			p.frame.uint16x4.uint16_1 = (UInt16)this.status.Gear;
			// gear/drive :1
			// reverse :1

			p.frame.uint16x4.uint16_2 = (UInt16)this.status.RequestedSignals;
			// signals :3
			// horn :1

			this.canusb.TransmitPacket(p);
		}

		public void TxCanLoop()
		{
			while (canusb != null)
			{
				this.TxCanPacket();
				System.Threading.Thread.Sleep(Config.LOOP_INTERVAL_MS);
			}
		}
	}
}

