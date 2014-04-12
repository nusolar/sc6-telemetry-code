using System;
using System.Collections.Generic;

namespace SolarCar
{
	namespace Car
	{
		/// Taken from Tritium BMS Manual
		enum Precharge
		{
			Error = 0,
			Idle = 1,
			Enable = 5,
			Measure = 2,
			Precharge = 3,
			Run = 4
		}

		[Flags]
		enum Gear
		{
			None = 0,
			Idle = 1,
			Drive = 2,
			Reverse = 4
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
			/// <summary>
			/// The Ah consumed from the pack. 0=Full, counts to user-set max capacity.
			/// </summary>
			public Single PackSOC;
			public Single PackSOCPerc;
			public Precharge BMSPrecharge;
			/// <summary>
			/// The highest cell voltage in mV.
			/// </summary>
			public UInt16 MaxVoltage;
			public UInt16 MinVoltage;
			/// <summary>
			/// The highest cell temp in decidegrees Celcius.
			/// </summary>
			public UInt16 MaxTemp;
			public UInt16 MinTemp;
			/// <summary>
			/// The pack voltage in mV.
			/// </summary>
			public Int32 PackVoltage;
			/// <summary>
			/// The pack current in mA.
			/// </summary>
			public Int32 PackCurrent;
			public UInt32 BMSExtendedStatusFlags;
		}
	}
	class UserInput
	{
		public Car.Gear gear = Car.Gear.None;
		public Car.Signals sigs = Car.Signals.None;
	}

	class DataAggregator
	{
		public delegate void TxCanDelegate(Can.Packet p);

		public event TxCanDelegate tx_cans;

		Car.Status status = new Car.Status();

		public Car.Status Status { get { return this.status; } }

		public DataAggregator()
		{
		}

		public void HandleUserInput(UserInput input)
		{
			this.status.RequestedGear = input.gear;
			this.status.RequestedSignals = input.sigs;
#if DEBUG
			Console.WriteLine("Data: input Gear={0}, Signals={1}", input.gear, input.sigs);
#endif
		}

		public void ProcessCanPacket(Can.Packet p)
		{
			switch (p.ID)
			{
				case Can.Addr.bms1.pack_soc._id:
					var pack_soc_pkt = new Can.Addr.bms1.pack_soc();
					this.status.PackSOC = pack_soc_pkt.soc_Ah;
					this.status.PackSOCPerc	= pack_soc_pkt.soc_percentage;
					break;
				case Can.Addr.bms1.precharge._id:
					var precharge_pkt = new Can.Addr.bms1.precharge();
					this.status.BMSPrecharge	= (Car.Precharge)precharge_pkt.precharge_state;
					break;
				case Can.Addr.bms1.max_min_temps._id:
					break;
				case Can.Addr.bms1.max_min_volts._id:
					break;
				case Can.Addr.bms1.pack_volt_curr._id:
					var current_pkt = new Can.Addr.bms1.pack_volt_curr(p.Data);
					this.status.PackCurrent = current_pkt.pack_current;
					this.status.PackVoltage = current_pkt.pack_voltage;
					break;
				case Can.Addr.dc.pedals._id:
					var pedals_pkt = new Can.Addr.dc.pedals(p.Data);
					this.status.AccelPedal = pedals_pkt.accel_pedal;
					this.status.RegenPedal = pedals_pkt.regen_pedal;
					this.status.BrakePedal = ((Int16)pedals_pkt.brake_pedal > 0);
					break;
				case Can.Addr.ws20.motor_bus._id:
					var motor_bus_pkt = new Can.Addr.ws20.motor_bus(p.Data);
					this.status.MotorVoltage = motor_bus_pkt.busVoltage;
					this.status.MotorCurrent = motor_bus_pkt.busCurrent;
					break;
				case Can.Addr.ws20.motor_velocity._id:
					var motor_vel_pkt = new Can.Addr.ws20.motor_velocity(p.Data);
					// RPM; velocity;
					this.status.MotorRpm = motor_vel_pkt.motorVelocity;
					this.status.MotorVelocity = motor_vel_pkt.vehicleVelocity;
					break;
			}
		}

		void TxCanPacket()
		{
			Can.Addr.os.user_cmds p = new Can.Addr.os.user_cmds(0);
			p.gearFlags = (UInt16)this.status.RequestedGear;
			p.signalFlags = (UInt16)this.status.RequestedSignals;

			this.tx_cans(p);
		}

		public void TxCanLoop()
		{
			while (this.tx_cans != null)
			{
				this.TxCanPacket();
				System.Threading.Thread.Sleep(Config.CAN_TX_INTERVAL_MS);
			}
#if DEBUG
			Console.WriteLine("DATA Warning: TxCanLoop ended!");
#endif
		}
	}
}

