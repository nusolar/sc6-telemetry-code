using System;
using System.Timers;

namespace SolarCar {
	class SignalController {
		readonly Hardware hardware = null;
		bool haz_blink = true;
		Timer haz_timer = null;

		public SignalController(Hardware hw) {
			this.hardware = hw;
			this.haz_timer = new Timer(800);
			this.haz_timer.Elapsed += (object sender, ElapsedEventArgs e) => (this.haz_blink = !this.haz_blink);
		}

		void SetSignals() {
			this.hardware.Horn = this.hardware.UserHorn;
			this.hardware.HeadLights = this.hardware.UserHeadlights;

			// start/disable Hazard lights
			if (this.hardware.UserSignal == Hardware.Signals.Hazards) {
				if (!this.haz_timer.Enabled) {
					this.haz_blink = true;
					this.haz_timer.Start();
				}
			} else {
				this.haz_timer.Stop();
				this.haz_blink = true;
			}

			// set Turn Signal values
			switch (this.hardware.UserSignal) {
				case Hardware.Signals.Left:
					this.hardware.LeftSignal = true;
					this.hardware.RightSignal = false;
				case Hardware.Signals.Right:
					this.hardware.LeftSignal = false;
					this.hardware.RightSignal = true;
				case Hardware.Signals.Hazards:
					this.hardware.LeftSignal = this.haz_blink;
					this.hardware.RightSignal = this.haz_blink;
				default:
					break;
			}
		}

		public void RunLoop() {
			while (this.hardware != null) {
				this.SetSignals();
				System.Threading.Thread.Sleep(Config.LOOP_INTERVAL); // 1ms
			}
		}
	}
}

