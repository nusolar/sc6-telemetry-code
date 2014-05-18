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
		Solar.DriverInput DriverInput = new Solar.DriverInput { gear = Solar.Gear.Run, sigs = Solar.Signals.None };
		bool drive_input_new = false;

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
			this.drive_input_new = true;
			Debug.WriteLine("CARMANAGER: input Gear={0}, Signals={1}", gear, sigs);
		}

		/// <summary>
		/// Extract CAN packet data, and update the Model.
		/// This function should apply a low-pass-filter to noisy values.
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
					var current_pkt = new Can.Addr.bms1.pack_volt_curr(p.Data); // t=?
					this.status.PackCurrent = current_pkt.pack_current; 
					this.status.PackVoltage = current_pkt.pack_voltage;
					break;
				case Can.Addr.dc.pedals._id:
					var pedals_pkt = new Can.Addr.dc.pedals(p.Data); // t=50ms
					this.status.AccelPedal = pedals_pkt.accel_pedal;
					this.status.RegenPedal = pedals_pkt.regen_pedal;
					this.status.BrakePedal = ((Int16)pedals_pkt.brake_pedal > 0);
					break;
				case Can.Addr.ws20.motor_bus._id:
					var motor_bus_pkt = new Can.Addr.ws20.motor_bus(p.Data); // t=200ms
					this.status.MotorVoltage = motor_bus_pkt.busVoltage;
					this.status.MotorCurrent = motor_bus_pkt.busCurrent;
					break;
				case Can.Addr.ws20.motor_velocity._id:
					var motor_vel_pkt = new Can.Addr.ws20.motor_velocity(p.Data); // t=200ms
					// RPM; velocity;
					this.status.MotorRpm = motor_vel_pkt.motorVelocity;
					this.status.MotorVelocity = motor_vel_pkt.vehicleVelocity;
					break;
				case Can.Addr.ws20.odom_bus_ah._id:
					var odometer_pkt = new Can.Addr.ws20.odom_bus_ah(p.Data); // t=1s
					// Distance travelled; Amp-Hours drawn
					this.status.MotorOdometer = odometer_pkt.odom;
					this.status.MotorAmpHours = odometer_pkt.dcBusAmpHours;
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
							Debug.WriteLine("CARMANAGER: writing packet");
							if (this.drive_input_new)
							{
								this.SendCanPackets(canusb);
								this.drive_input_new = false;
							}
							await Task.Delay(Config.CANUSB_TX_INTERVAL_MS);
						}
					});
					Task read_loop = Task.Run(async () =>
					{
						while (!token.IsCancellationRequested)
						{
							Debug.WriteLine("CARMANAGER: checking packets");
							canusb.CheckPackets();
							await Task.Delay(Config.CANUSB_RX_INTERVAL_MS);
						}
					});

					await send_loop;
					await read_loop;
				}
				catch (System.IO.IOException ex)
				{
					Debug.WriteLine("CARMANAGER IO Exception: " + ex.Message);
				}
				finally
				{
					canusb.Close();
				}
				await Task.Delay(5000); // wait 1s before reopening SerialPort
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
				await Task.Delay(Config.DB_ADD_INTERVAL_MS); // 1s
			}
		}

		/// <summary>
		/// Maintain the Model of the Car, by reading and writing to the CAN bus.
		/// Accepts a Task CancellationToken.
		/// </summary>
		/// <param name="obj">The CancellationToken.</param>
		public async Task BusinessLoop(CancellationToken token)
		{

			Task can_loop = this.CanLoop(token);
			Task make_telemetry_loop = this.ProduceCarTelemetry(token);
			Task send_telemetry_loop = this.DataLayer.ConsumeCarTelemetry(token);
			await can_loop;
			await make_telemetry_loop;
			await send_telemetry_loop;
		}

#endregion
	}
}

