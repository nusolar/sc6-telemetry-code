using System;
using System.Threading;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace SolarCar {
	class MainClass {
		static void RunCar() {
			// runs hardware I/O on main thread.
			Hardware hw = new Hardware();

			// HW controllers run on separate threads.
			BatteryController bps = new BatteryController(hw);
			MotorController drv = new MotorController(hw);
			SignalController sig = new SignalController(hw);

			// UIs run on separate threads.
			UserCommands commands = new UserCommands(hw);
			HttpServer web = new HttpServer(commands);

			// do RunLoops in separate threads:
			Thread.Sleep(1); // 1ms
			Thread bps_loop = new Thread(new ThreadStart(bps.RunLoop));
			Thread drv_loop = new Thread(new ThreadStart(drv.RunLoop));
			Thread sig_loop = new Thread(new ThreadStart(sig.RunLoop));
			Thread web_loop = new Thread(new ThreadStart(web.RunLoop));
			bps_loop.Start();
			drv_loop.Start();
			sig_loop.Start();
			web_loop.Start();
			bps_loop.Join();
			drv_loop.Join();
			sig_loop.Join();
			web_loop.Join();
		}

		[DllImport("test.dylib")]
		public static extern void printer_s(
			//[MarshalAs(UnmanagedType.LPStr)]
			string s);

		[StructLayout(LayoutKind.Sequential)]
		public class message {
			public int i;
			[MarshalAs(UnmanagedType.LPStr)]
			public string str;
			public int j;
		}

		[DllImport("test.dylib")]
		public static extern void printer_m(
			[Out]
			message m);

		[DllImport("test.dylib", CharSet = CharSet.Unicode)]
		public static extern int ac_compare(
			[MarshalAs(UnmanagedType.LPArray)]
			Int32[] p);

		public static void Main(string[] args) {
			Console.WriteLine("Hello World!");
			CarData.Test();
			IJsonReporter<BatteryReport>.Test();
			IJsonReporter<BatteryReport>.Test();
			IJsonReporter<BatteryReport>.Test();

			// RunCar();

			// Testing/Debugging code follows:
			printer_s("42 foo");
			message m = new message { str = "barz", j = 666, i = 14 };
			printer_m(m);
			Console.WriteLine("i: " + m.i.ToString());

			HidDevice hid = new HidDevice(1452, 601, "DJHC9MZ0VADNYGE0");
			if (HidDevice.Initialized)
				Console.WriteLine("Inited!");

			if (hid.Open) {
				Console.WriteLine("device found");
				byte[] buffer = new byte[256];
				int n_copied = hid.Read(buffer);
				Console.WriteLine("copied: " + n_copied.ToString());
				if (n_copied > 0) {
					for (int i=0; i<n_copied; i++) {
						Console.WriteLine("b" + i.ToString() + ": " + buffer[i].ToString());
					}
				}
			} else {
				Console.WriteLine("device not found! nullptr returned");
			}

			// Console.WriteLine("Press any key to continue...");
			// Console.WriteLine();
			// Console.ReadKey();
		}
	}
}
