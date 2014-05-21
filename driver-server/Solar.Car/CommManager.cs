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

#endregion

		/// <summary>
		/// Injected by App.
		/// </summary>
		public IDataServiceLayer DataLayer { get; set; }

		public IAppLayer AppLayer { get; set; }

		/// <summary>
		/// Gets a Copy of Model's status.
		/// </summary>
		public Solar.Status Status { get { return (Solar.Status)status.DeepClone(); } }

		public CommManager()
		{
			this.DataLayer = new Database();
			this.AppLayer = new HttpGui();
			this.AppLayer.Manager = this;
		}

		/// <summary>
		/// Accept data from Application Layer/UI, update Model's status.
		/// </summary>
		public void HandleUserInput(Solar.Gear gear, Solar.Signals sigs)
		{
			this.DriverInput.gear = gear;
			this.DriverInput.sigs = sigs;
			Task.Run(this.SendCanPacket);
			Debug.WriteLine("MANAGER:\tinput Gear={0}, Signals={1}", gear, sigs);
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
		public Action SendCanPacket;

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

					this.SendCanPacket = () =>
					{
						if (!token.IsCancellationRequested && canusb.IsOpen)
						{
							Debug.WriteLine("MANAGER:\tCan: writing packet");
							Can.Addr.os.user_cmds p = new Can.Addr.os.user_cmds
							{
								gearFlags = (UInt16)this.DriverInput.gear,
								signalFlags = (UInt16)this.DriverInput.sigs
							};
							canusb.TransmitPacket(p);
						}
					};

					while (!token.IsCancellationRequested)
					{
						Debug.WriteLine("MANAGER:\tCanUsb: checking packets");
						canusb.PollPackets();
						await Task.Delay(Config.CANUSB_RX_INTERVAL_MS);
					}
				}
				catch (System.IO.IOException e)
				{
					Debug.WriteLine("MANAGER:\tCanUsb: IO Exception: " + e.ToString());
				}
				catch (Exception e)
				{
					Debug.WriteLine("MANAGER:\tCanUsb: EXCEPTION: " + e.ToString());
				}
				finally
				{
					canusb.Close();
				}
				await Task.Delay(Config.CANUSB_DISCONNECT_RETRY_MS, token); // wait 5s before reopening SerialPort
			}
		}

		async Task ProduceCarTelemetry(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				try
				{
					Debug.WriteLine("MANAGER:\tDatabase: pushing");
					await Task.Run(() => DataLayer.PushStatus(this.Status));
				}
				catch (Exception e)
				{
					Debug.WriteLine("MANAGER:\tDatabase: EXCEPTION: " + e.Message);
				}
				await Task.Delay(Config.DB_ADD_INTERVAL_MS, token); // 0.25s
			}
		}

		async Task DropboxCarTelemetry(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				try
				{
					// archive every 10s
					await this.DataLayer.PushToDropbox();
				}
				catch (Exception e)
				{
					Debug.WriteLine("MANAGER:\tArchiving: EXCEPTION: " + e.ToString());
				}
				await Task.Delay(Config.DB_SAVE_INTERVAL_MS, token);
			}
		}

		/// <summary>
		/// Maintain the Model of the Car, by reading and writing to the CAN bus.
		/// Run the solar car's HttpGui, command Driver Controls, gather telemetry.
		/// Accepts a Task CancellationToken.
		/// </summary>
		/// <param name="obj">The CancellationToken.</param>
		public async Task BusinessLoop(CancellationToken parentToken)
		{
			using (var innerSource = new CancellationTokenSource())
			{
				var childSource = CancellationTokenSource.CreateLinkedTokenSource(parentToken, innerSource.Token);
				List<Task> tasks = new List<Task>();

				// collect data from CanUsb
				tasks.Add(this.CanLoop(childSource.Token));
				// Telemetry caching
				tasks.Add(this.ProduceCarTelemetry(childSource.Token));
				// Telemetry transmission
				tasks.Add(this.DropboxCarTelemetry(childSource.Token));
				// UIs run on separate thread.
				tasks.Add(this.AppLayer.AppLayerLoop(childSource.Token));

				await Task.WhenAny(tasks.ToArray());
				innerSource.Cancel();
				Debug.WriteLine("MANAGER:\tBusinessLoop: Aborted");
				await Task.WhenAll(tasks.ToArray());
			}
		}

#endregion

#region IDisposable

		bool disposed = false;

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					if (this.DataLayer != null)
					{
						this.DataLayer.Dispose();
					}
				}
				disposed = true;
			}
		}

#endregion
	}
}

