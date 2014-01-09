using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace SolarCar {
	class IJsonReporter<TReport>: AsyncSerialPort where TReport: new() {
		public TReport Report = new TReport();

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

		/// <summary>
		/// Sends a JSON value to the device.
		/// </summary>
		/// <param name="command">Command.</param>
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
}

