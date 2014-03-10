
namespace SolarCar {
	class BPS: IJsonReporter<BatteryReport> {
		public BPS(): base("/dev/tty.BMS-1234") {
			this.Report = Json.CreateFromJsonFile<BatteryReport>(Config.SAMPLE_BATTERY_REPORT);
		}

		public void SetMode(int mode) {
			this.SendValue(mode);
		}
	}

	/// <summary>
	/// A USB replacement for BPS:IJsonReporter. TODO: Finish, and replace DigitalOut and PedalIn too.
	/// </summary>
	class BPSUsb: HidDevice {
		public BatteryReport Report = new BatteryReport();

		public BPSUsb(): base(0x04D8, 0xAAAA, "NUSolarBPS0") {
		}

		public void Poll() {
			byte[] data = new byte[65]; // 64 data, +1byte in case an unexpected Report Number is sent/prefixed

			if (this.Read(data) > 0) {
				switch (data[0]) {
					case 0x10:
						{
							// handle voltages
						}
					default:
						break;
				}
			}
		}

		public void SetMode(int mode) {
			this.Write(new byte[] { 0x00, (byte)mode });
		}
	}

	class DigitalOut: IJsonReporter<DigitalOutReport> {
		/// <summary>
		/// Initializes a <see cref="SolarCar.DigitalOut"/>, in the OFF state.
		/// </summary>
		/// <param name="name">Port name</param>
		public DigitalOut(string name): base(name) {
			this.Report = Json.CreateFromJsonFile<DigitalOutReport>(Config.SAMPLE_OUTPUT_REPORT);
			this.TurnOff();
		}

		public void TurnOn() {
			this.SendValue(1);
		}

		public void TurnOff() {
			this.SendValue(0);
		}

		public void Set(bool State) {
			if (State) {
				this.TurnOn();
			} else {
				this.TurnOff();
			}
		}

		public bool Value { get { return this.Report.output; } }
	}

	class PedalIn: IJsonReporter<PedalReport> {
		public PedalIn(string name): base(name) {
			this.Report = Json.CreateFromJsonFile<PedalReport>(Config.SAMPLE_INPUT_REPORT);
		}

		public int AccelPedal { get { return this.Report.accel_pedal; } }

		public bool BrakePedal { get { return this.Report.brake_pedal; } }
	}
}
