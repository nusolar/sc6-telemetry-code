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

		[StructLayout(LayoutKind.Sequential)]
		public class message {
			public int i;
			[MarshalAs(UnmanagedType.LPStr)]
			public string str;
			public int j;
		}

		public static void Main(string[] args) {
			Console.WriteLine("Hello World!");
			CarData.Test();

			// RunCar();


			// Console.WriteLine("Press any key to continue...");
			// Console.WriteLine();
			// Console.ReadKey();
		}
	}
}
