using System;
using System.IO.Ports;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SolarCar {
	/// <summary>
	/// Wrapper to send to and receive packets from NUSolar Serial devices.
	/// </summary>
	class SyncSerialPort {
		readonly object port_lock = new Object();
		readonly SerialPort port = new SerialPort();
		readonly object buffer_lock = new Object();
		string buffer = "";

		public delegate void LineReceivedDelegate(string line);

		public event LineReceivedDelegate LineReceived;

		/// <summary>
		/// Initializes a new instance of the SolarCar.ISerial class.
		/// </summary>
		/// <param name="name">Serial Port's name</param>
		protected SyncSerialPort(string name) {
			port.PortName = name;
			port.BaudRate = 115200;
			port.ReadTimeout = 50; // 50 ms
			port.WriteTimeout = 50; // 50 ms
			port.DataReceived += new SerialDataReceivedEventHandler(this.ReadData);
			port.Open();
		}

		/// <summary>
		/// Releases the SerialPort and performs other cleanup operations before the SolarCar.ISerial is
		/// reclaimed by garbage collection.
		/// </summary>
		~SyncSerialPort() {
			port.Close();
		}

		protected string NewLine {
			get { return port.NewLine; }
			set { port.NewLine = value; }
		}

		/// <summary>
		/// Acquire the SerialPort, read data into buffer. DELEGATE to SerialPort.DataReceived event,
		/// invoked on a separate thread by SerialPort.
		/// </summary>
		/// <param name="sender">Sender, a SerialPort.</param>
		/// <param name="e">Event Args</param>
		void ReadData(object sender, SerialDataReceivedEventArgs e) {
			if (e.EventType != System.IO.Ports.SerialData.Chars) {
				return;
			}
			if ((SerialPort)sender != this.port) {
				Console.WriteLine("ReadData only supports reading its own port!");
			}

			// acquire lock on CAN bus, read upto 21 bytes.
			string temp_buffer = null;
			try {
				lock (port_lock) {
					temp_buffer = this.port.ReadExisting();
				}
			} catch (TimeoutException) {
				Console.WriteLine("CANBUS: read timed out. SerialPort may be busy.");
				return;
			}

			// Add data to this.buffer, check for newlines.
			string new_line = null;
			lock (buffer_lock) {
				this.buffer += temp_buffer;
				if (this.buffer.Contains(this.NewLine)) {
					int newline_index = this.buffer.IndexOf(this.NewLine);
					// copy the first line in the buffer.
					new_line = this.buffer.Substring(0, newline_index + 1);
					// remove copied data from buffer.
					this.buffer = this.buffer.Substring(newline_index + 1);
				}
			}

			// Handle newline, if exists.
			if (new_line != null) {
				this.LineReceived(new_line);
			}
		}

		/// <summary>
		/// Acquire the SerialPort, and write a line.
		/// </summary>
		/// <param name="line">Line.</param>
		public void SyncWriteLine(string line) {
			try {
				lock (port_lock) {
					port.WriteLine(line);
				}
			} catch (TimeoutException) {
				Console.WriteLine("CANBUS: write timed out. SerialPort may be busy.");
			}
		}
	}

	class IJsonReporter<TReport>: SyncSerialPort {
		public TReport Report;

		public IJsonReporter(string name): base(name) {
			this.LineReceived += new LineReceivedDelegate(this.HandleLine);
		}

		/// <summary>
		/// reads line as JSON, updates this.Report. DELEGATE to LineReceived event
		/// </summary>
		/// <returns>The packet.</returns>
		void HandleLine(string line) {
			TReport report = default(TReport);
			try {
				// Deserialization takes ~100 microseconds per BatteryReport, amortized
				report = JsonConvert.DeserializeObject<TReport>(line);
			} catch (JsonReaderException) {
				Console.WriteLine("Bad JSON line: " + line);
			}
			// TODO fix the Report memory leak.
			this.Report = report;
		}

		/**
		 * sends a JSON value to the device, over serial port.
		 */
		protected void SendValue(string command) {
			JObject obj = new JObject();
			obj["command"] = command;
			this.SyncWriteLine(obj.ToString());
		}

		protected void SendValue(int command) {
			JObject obj = new JObject();
			obj["command"] = command;
			this.SyncWriteLine(obj.ToString());
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

	class BPS: IJsonReporter<BatteryReport> {
		public BPS(): base("/dev/tty.BMS-1234") {
		}

		public void SetMode(int mode) {
			this.SendValue(mode);
		}
	}

	class DigitalOut: IJsonReporter<DigitalOutReport> {
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

		public bool Value { get { return this.Report.output; } }
	}

	class PedalIn: IJsonReporter<PedalReport> {
		public PedalIn(string name): base(name) {
		}

		public int AccelPedal { get { return this.Report.accel_pedal; } }

		public bool BrakePedal { get { return this.Report.brake_pedal; } }
	}
}

