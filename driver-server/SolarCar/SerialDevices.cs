using System;
using System.IO.Ports;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SolarCar {
	/// <summary>
	/// Wrapper to send to and receive packets from NUSolar Serial devices.
	/// </summary>
	abstract class ISerial<TReport> {
		readonly SerialPort port = new SerialPort();
		public TReport Report;
		string buffer = "";

		/// <summary>
		/// Initializes a new instance of the SolarCar.ISerial class.
		/// </summary>
		/// <param name="name">Serial Port's name</param>
		protected ISerial(string name) {
			port.PortName = name;
			port.BaudRate = 115200;
			port.DataReceived += new SerialDataReceivedEventHandler(this.ReadData);
			port.Open();
		}

		/// <summary>
		/// Releases the SerialPort and performs other cleanup operations before the SolarCar.ISerial is
		/// reclaimed by garbage collection.
		/// </summary>
		~ISerial() {
			port.Close();
		}

		/// <summary>
		/// Read the serial port's data into buffer. DELEGATE.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		void ReadData(object sender, SerialDataReceivedEventArgs e) {
			SerialPort sender_port = (SerialPort)sender;
			this.buffer += sender_port.ReadExisting();
			if (this.buffer.Contains("\n")) {
				this.Report = this.GetPacket();
			}
		}

		/**
		 * reads this controller's buffer, and returns a data packet. NULL if no packet.
		 */
		TReport GetPacket() {
			TReport report = default(TReport);
			try {
				// Deserialization takes ~100 microseconds per BatteryReport, amortized
				report = JsonConvert.DeserializeObject<TReport>(this.buffer);
			} catch (JsonReaderException) {
				Console.WriteLine("Bad JSON line: " + this.buffer);
			}
			return report;
		}

		/**
		 * sends a JSON value to the device, over serial port.
		 */
		protected void SendValue(string command) {
			JObject obj = new JObject();
			obj["command"] = command;
			port.WriteLine(obj.ToString());
		}

		protected void SendValue(int command) {
			JObject obj = new JObject();
			obj["command"] = command;
			port.WriteLine(obj.ToString());
		}

		/**
		 * Tests BMS json template.
		 */
		public static void Test() {
			using (System.IO.StreamReader file = System.IO.File.OpenText("/Users/alex/GitHub/sc6-telemetry-code/sample_bms.json")) {
				String lines = file.ReadToEnd();
				System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
				BatteryReport[] dict = new BatteryReport[1000];
				sw.Start();
				for (int i = 0; i < 1000; i++) {
					dict[i] = JsonConvert.DeserializeObject<BatteryReport>(lines);
				}
				sw.Stop();
				Console.WriteLine("time for 1000 packets: " + (sw.ElapsedMilliseconds).ToString() + " ms");
				Console.WriteLine(dict[0].uptime);
			}

		}
	}

	class BPS: ISerial<BatteryReport> {
		public BPS(): base("/dev/tty.BMS-1234") {
		}

		public void SetMode(int mode) {
			this.SendValue(mode);
		}
	}

	class DigitalOut: ISerial<DigitalOutReport> {
		/// <summary>
		/// Initializes a <see cref="SolarCar.DigitalOut"/>, in the OFF state.
		/// </summary>
		/// <param name="name">Port name</param>
		public DigitalOut(string name): base(name) {
			this.TurnOff();
		}

		public void TurnOn() {
			this.SendValue("on");
		}

		public void TurnOff() {
			this.SendValue("off");
		}

		public void Set(bool State) {
			if (State) {
				this.TurnOn();
			} else {
				this.TurnOff();
			}
		}

		public bool Status { get { return this.Report.output; } }
	}

	class PedalIn: ISerial<PedalReport> {
		public PedalIn(string name): base(name) {
		}

		public int AccelPedal { get { return this.Report.accel_pedal; } }

		public bool BrakePedal { get { return this.Report.brake_pedal; } }
	}
}

