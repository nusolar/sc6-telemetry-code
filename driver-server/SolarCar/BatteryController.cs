using System;

namespace SolarCar {
	class Mode {
		public bool CanDischarge = false, CanCharge = false, Drive = false;
		public Mode ChargeConj = null;
		public Mode DischargeConj = null;
		public Mode DriveConj = null;
		public int ID = -1;
	}

	class BatteryController {
		readonly Hardware hardware = null;
		readonly Mode Off, Discharging, Drive, EmptyCharging, Charging, DriveCharging;
		volatile Mode Current;

		public BatteryController(Hardware hw) {
			this.hardware = hw;

			this.Off = new Mode { ID = 0, CanDischarge = false, Drive = false, CanCharge = false };
			this.Discharging = new Mode { ID = 1, CanDischarge = true, Drive = false, CanCharge = false };
			this.Drive = new Mode { ID = 2, CanDischarge = true, Drive = true, CanCharge = false };
			this.EmptyCharging = new Mode { ID = 4, CanDischarge = false, Drive = false, CanCharge = true };
			this.Charging = new Mode { ID = 5, CanDischarge = true, Drive = false, CanCharge = true };
			this.DriveCharging = new Mode { ID = 6, CanDischarge = true, Drive = true, CanCharge = true };

			this.Off.ChargeConj = this.EmptyCharging;
			this.Off.DischargeConj = this.Discharging;
			this.Off.DriveConj = null;

			this.Discharging.ChargeConj = this.Charging;
			this.Discharging.DischargeConj = this.Off;
			this.Discharging.DriveConj = this.Drive;

			this.Drive.ChargeConj = this.DriveCharging;
			this.Drive.DischargeConj = this.Off;
			this.Drive.DriveConj = this.Discharging;

			this.EmptyCharging.ChargeConj = this.Off;
			this.EmptyCharging.DischargeConj = this.Charging;
			this.EmptyCharging.DriveConj = null;

			this.Charging.ChargeConj = this.Discharging;
			this.Charging.DischargeConj = this.EmptyCharging;
			this.Charging.DriveConj = this.DriveCharging;

			this.DriveCharging.ChargeConj = this.Drive;
			this.DriveCharging.DischargeConj = this.EmptyCharging;
			this.DriveCharging.DriveConj = this.Charging;

			this.Current = this.Off;
		}

		/// <summary>
		/// Check the battery health, and adjust Protection Mode.
		/// </summary>
		void CheckHealth() {
			bool health = hardware.Health() == "NONE";
			bool can_discharge = hardware.CanDischarge();
			bool can_charge = hardware.CanCharge();
			bool is_drive = hardware.UserDrive;

			if (!health) {
				// kill everything, if batteries unhealthy
				this.Current = this.Off;
			} else {
				if (can_discharge != this.Current.CanDischarge) {
					// FIRST kill discharge if needed.
					this.Current = this.Current.DischargeConj;
				} else if (can_charge != this.Current.CanCharge) {
					// THEN toggle charging, if needed.
					this.Current = this.Current.ChargeConj;
				} else if (is_drive != this.Current.Drive) {
					// FINALLY after discharge & charge are correct, drive.
					if (this.Current.DriveConj != null) {
						this.Current = this.Current.DriveConj;
					} else {
						Console.WriteLine("Warning: Cannot activate Drive! Probably because cannot discharge.");
					}
				}
			}

			// Send the Mode to the BPS board.
			this.hardware.Mode = this.Current.ID;
#if DEBUG
			Console.WriteLine("Mode: " + this.Current.ID.ToString());
#endif
		}

		/// <summary>
		/// BPS run loop. Sleeps 1ms per cycle.
		/// </summary>
		public void RunLoop() {
			while (this.hardware != null) {
				this.CheckHealth();
				System.Threading.Thread.Sleep(Config.LOOP_INTERVAL); // 1ms
			}
		}
	}
}

