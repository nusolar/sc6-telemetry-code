using System;
using System.Threading;

namespace SolarCar {
	class MainClass {
		void RunCar() {
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

		public static void Main(string[] args) {
			Console.WriteLine("Hello World!");
			CarData.Test();
			IJsonReporter<BatteryReport>.Test();
			IJsonReporter<BatteryReport>.Test();
			IJsonReporter<BatteryReport>.Test();

//			Console.WriteLine("Press any key to continue...");
//			Console.WriteLine();
//			Console.ReadKey();
		}
	}
}
