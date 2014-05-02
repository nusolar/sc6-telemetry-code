using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Debug = System.Diagnostics.Debug;

namespace Solar.Car
{
	class CarFrontend
	{
#region Hardware settings

		Car.Status status = new Car.Status();
		Car.DriverInput DriverInput = new Car.DriverInput();

#endregion

		public CarFrontend()
		{
		}

		public Car.Status Status { get { return (Car.Status)status.Clone(); } }

		public void HandleUserInput(Car.Gear gear, Car.Signals sigs)
		{
			this.DriverInput.gear = gear;
			this.DriverInput.sigs = sigs;

			Debug.WriteLine("CARFRONT: input Gear={0}, Signals={1}", gear, sigs);
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

		public void SendCanPackets(CanUsb canusb)
		{
			// Write a CAN packet
			Can.Addr.os.user_cmds p = new Can.Addr.os.user_cmds(0);
			p.gearFlags = (UInt16)(this.DriverInput.gear);
			p.signalFlags = (UInt16)this.DriverInput.sigs;
			canusb.TransmitPacket(p);
		}

		async Task SendCanLoop(CancellationToken token, CanUsb canusb)
		{
			while (!token.IsCancellationRequested)
			{
				Debug.WriteLine("CARFRONT: writing packet");

				this.SendCanPackets(canusb);
				await Task.Delay(Config.CANUSB_TX_INTERVAL_MS);
			}
		}

		async Task ReadCanLoop(CancellationToken token, CanUsb canusb)
		{
			while (!token.IsCancellationRequested)
			{
				Debug.WriteLine("CARFRONT: checking packets");

				canusb.CheckPackets();
				await Task.Delay(Config.CANUSB_RX_INTERVAL_MS);
			}
		}

		public async Task CanLoop(object obj)
		{
			CancellationToken token = (CancellationToken)obj;

			while (!token.IsCancellationRequested)
			{
				// CanUsb communicates with car's CAN bus.
				using (CanUsb canusb = new CanUsb(Config.CANUSB_SERIAL_DEV))
				{
					// attach handler for CAN Read Event
					canusb.handlers += this.ProcessCanPacket;

					try
					{
						canusb.Open();

						using (var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token))
						{
							var child_token = tokenSource.Token;
							Task send_loop = this.SendCanLoop(child_token, canusb);
							Task read_loop = this.ReadCanLoop(child_token, canusb);
							await send_loop;
							await read_loop;
						}
					}
					catch (System.IO.InternalBufferOverflowException ex)
					{
						Debug.WriteLine("CARFRONT Buffer Exception: " + ex.Message);
					}
					catch (System.IO.IOException ex)
					{
						Debug.WriteLine("CARFRONT IO Exception: " + ex.Message);
					}
					finally
					{
						canusb.Close();
					}
				}
				await Task.Delay(1000); // wait 1s before reopening SerialPort
			}
		}
	}
}

