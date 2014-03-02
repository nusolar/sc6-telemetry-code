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
			public Gear RequestedGear;
			public Signals RequestedSignals;
			// Pedals
			public UInt16 AccelPedal;
			public UInt16 RegenPedal;
			public bool BrakePedal;
			// motor
			public float MotorRpm, MotorVelocity;
			public float MotorVoltage, MotorCurrent;
			// BPS
			public Mode BPSMode;
			public UInt64 BatteryHealth;
			public UInt16 BatteryCurrent;
			public UInt16 ArrayCurrent;
			// Lists range 0-31
			public List<UInt16> BatteryVoltages = null;
			public List<UInt16> BatteryTemps = null;
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
			CanHandler ch = new CanHandler(0, Can.Addr.ws20.motor_velocity._id, this.HandleCanPacket);
			canusb.handlers.Add(ch);
		}

		public Car.Status Status { get { return this.status; } }

		public void HandleUserInput(UserInput input)
		{
			this.status.RequestedMode = input.mode;
			this.status.RequestedGear = input.gear;
			this.status.RequestedSignals = input.sigs;

			#if DEBUG
			Console.WriteLine("Set Mode={0}, Gear={1}, Signals={2}", input.mode, input.gear, input.sigs);
			#endif
		}

		public void HandleCanPacket(Can.Packet p)
		{
			switch (p.ID)
			{
				case Can.Addr.bps_tx.bps_status._id:
					Can.Addr.bps_tx.bps_status bps_status_pkt = new Can.Addr.bps_tx.bps_status(p.Data);
					this.status.BPSMode = (Car.Mode)bps_status_pkt.mode;
					break;
				case Can.Addr.bps_tx.current._id:
					Can.Addr.bps_tx.current current_pkt = new Can.Addr.bps_tx.current(p.Data);
					this.status.BatteryCurrent = current_pkt.battery;
					this.status.ArrayCurrent = current_pkt.array;
					break;
				case Can.Addr.bps_tx.voltage_temp._id:
					Can.Addr.bps_tx.voltage_temp vt_pkt = new Can.Addr.bps_tx.voltage_temp(p.Data);
					this.status.BatteryVoltages[vt_pkt.module] = vt_pkt.voltage;
					this.status.BatteryTemps[vt_pkt.temp] = vt_pkt.temp;
					break;
				case Can.Addr.dc.pedals._id:
					Can.Addr.dc.pedals pedals_pkt = new Can.Addr.dc.pedals(p.Data);
					this.status.AccelPedal = pedals_pkt.accel_pedal;
					this.status.RegenPedal = pedals_pkt.regen_pedal;
					this.status.BrakePedal = ((Int16)pedals_pkt.brake_pedal > 0);
					break;
				case Can.Addr.ws20.motor_bus._id:
					Can.Addr.ws20.motor_bus motor_bus_pkt = new Can.Addr.ws20.motor_bus(p.Data);
					this.status.MotorVoltage = motor_bus_pkt.busVoltage;
					this.status.MotorCurrent = motor_bus_pkt.busCurrent;
					break;
				case Can.Addr.ws20.motor_velocity._id:
					Can.Addr.ws20.motor_velocity motor_vel_pkt = new Can.Addr.ws20.motor_velocity(p.Data);
					// RPM; velocity;
					this.status.MotorRpm = motor_vel_pkt.motorVelocity;
					this.status.MotorVelocity = motor_vel_pkt.vehicleVelocity;
					break;
			}
		}

		void TxCanPacket()
		{
			Can.Addr.os.user_cmds p = new Can.Addr.os.user_cmds(0);
			p.power = (UInt16)this.status.RequestedMode;
			// power :1
			// drive :1
			// array :1

			p.gearFlags = (UInt16)this.status.RequestedGear;
			// gear/drive :1
			// reverse :1

			p.signalFlags = (UInt16)this.status.RequestedSignals;
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

