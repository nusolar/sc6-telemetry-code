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
		Mode Current = null;

		public BatteryController(Hardware hw) {
			this.hardware = hw;

			this.Off = new Mode { ID = 0, CanDischarge = false, Drive = false, CanCharge = false,
				ChargeConj = this.EmptyCharging,
				DischargeConj = this.Discharging,
				DriveConj = null
			};
			this.Discharging = new Mode { ID = 1, CanDischarge = true, Drive = false, CanCharge = false,
				ChargeConj = this.Charging,
				DischargeConj = this.Off,
				DriveConj = this.Drive
			};
			this.Drive = new Mode { ID = 2, CanDischarge = true, Drive = true, CanCharge = false,
				ChargeConj = this.DriveCharging,
				DischargeConj = this.Off,
				DriveConj = this.Discharging
			};
			this.EmptyCharging = new Mode { ID = 4, CanDischarge = false, Drive = false, CanCharge = true,
				ChargeConj = this.Off,
				DischargeConj = this.Charging,
				DriveConj = null
			};
			this.Charging = new Mode { ID = 5, CanDischarge = true, Drive = false, CanCharge = true,
				ChargeConj = this.Discharging,
				DischargeConj = this.EmptyCharging,
				DriveConj = this.DriveCharging
			};
			this.DriveCharging = new Mode { ID = 6, CanDischarge = true, Drive = true, CanCharge = true,
				ChargeConj = this.Drive,
				DischargeConj = this.EmptyCharging,
				DriveConj = this.Charging
			};

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
					// FIRST: kill discharge if needed.
					this.Current = this.Current.DischargeConj;
				} else if (can_charge != this.Current.CanCharge) {
					this.Current = this.Current.ChargeConj;
				} else if (is_drive != this.Current.Drive) {
					if (this.Drive.DriveConj != null) {
						this.Current = this.Current.DriveConj;
					} else {
						Console.WriteLine("Warning: Cannot activate Drive! Probably because cannot discharge.");
					}
				}
			}

			// Send the Mode to the BPS board.
			this.hardware.Mode = this.Current.ID;
		}

		/// <summary>
		/// BPS run loop. Sleeps 1ms per cycle.
		/// </summary>
		public void RunLoop() {
			while (this.hardware != null) {
				this.CheckHealth();
				System.Threading.Thread.Sleep(1); // 1ms
			}
		}
	}
}

