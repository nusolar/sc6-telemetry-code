using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Debug = System.Diagnostics.Debug;

namespace Solar.Car
{
	/// <summary>
	/// The Business-Layer connecting the GUI/Application-Layer to the Car's
	/// Hardware-Service-Layer (the CAN bus) and Data-Service-Layer (the Telemetry DB).
	/// Solar.Car.Comm sends DriverInput to the Car, and receives telemetry data.
	/// </summary>
	public class CommManager: IBusinessLayer
	{
#region Model

		Solar.Status status = new Solar.Status();
		Solar.DriverInput DriverInput = new Solar.DriverInput();

#endregion

		/// <summary>
		/// Injected by App.
		/// </summary>
		public IDataServiceLayer DataLayer { get; set; }

		/// <summary>
		/// Gets a Copy of Model's status.
		/// </summary>
		public Solar.Status Status { get { return (Solar.Status)status.DeepClone(); } }

		/// <summary>
		/// Accept data from Application Layer/UI, update Model's status.
		/// </summary>
		public void HandleUserInput(Solar.Gear gear, Solar.Signals sigs)
		{
			this.DriverInput.gear = gear;
			this.DriverInput.sigs = sigs;

			Debug.WriteLine("CARFRONT: input Gear={0}, Signals={1}", gear, sigs);
		}

		/// <summary>
		/// Extract CAN packet data, update Model's status.
		/// </summary>
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
					this.status.BMSPrecharge	= (Solar.Precharge)precharge_pkt.precharge_state;
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

		/// <summary>
		/// Write Model data to passed CanUsb as CAN packet
		/// </summary>
		public void SendCanPackets(CanUsbWrapper canusb)
		{
			Can.Addr.os.user_cmds p = new Can.Addr.os.user_cmds(0);
			p.gearFlags = (UInt16)(this.DriverInput.gear);
			p.signalFlags = (UInt16)this.DriverInput.sigs;
			canusb.TransmitPacket(p);
		}

#region Tasks

		async Task CanLoop(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				// CanUsb communicates with car's CAN bus.
				CanUsbWrapper canusb = new CanUsbWrapper();
				try
				{
					// attach handler for CAN Read Event
					canusb.handlers += this.ProcessCanPacket;
					canusb.Open();

					Task send_loop = Task.Run(async () =>
					{
						while (!token.IsCancellationRequested)
						{
							Debug.WriteLine("CARFRONT: writing packet");
							this.SendCanPackets(canusb);
							await Task.Delay(Config.CANUSB_TX_INTERVAL_MS);
						}
					});
					Task read_loop = Task.Run(async () =>
					{
						while (!token.IsCancellationRequested)
						{
							Debug.WriteLine("CARFRONT: checking packets");
							canusb.CheckPackets();
							await Task.Delay(Config.CANUSB_RX_INTERVAL_MS);
						}
					});

					await send_loop;
					await read_loop;
				}
				catch (System.IO.IOException ex)
				{
					Debug.WriteLine("CARFRONT IO Exception: " + ex.Message);
				}
				finally
				{
					canusb.Close();
				}
				await Task.Delay(1000); // wait 1s before reopening SerialPort
			}
		}

		async Task ProduceCarTelemetry(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				try
				{
					Debug.WriteLine("DB Producer: pushing");
					await Task.Run(() => DataLayer.PushStatus(this.Status));
				}
				catch (Exception e)
				{
					Debug.WriteLine("DB Producer: EXCEPTION: " + e.Message);
				}
				await Task.Delay(1000); // 1s
			}
		}

		/// <summary>
		/// Maintain the Model of the Car, by reading and writing to the CAN bus.
		/// Accepts a Task CancellationToken.
		/// </summary>
		/// <param name="obj">The CancellationToken.</param>
		public async Task BusinessLoop(CancellationToken token)
		{
			using (var childTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token))
			{
				Task can_loop = this.CanLoop(childTokenSource.Token);
				Task prod_loop = this.ProduceCarTelemetry(childTokenSource.Token);
				await can_loop;
				await prod_loop;
			}
		}

#endregion
	}
}

