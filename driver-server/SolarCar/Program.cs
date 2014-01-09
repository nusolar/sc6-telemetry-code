using System;
using System.Threading;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace SolarCar {
	class MainClass {
		static void RunCar() {
			// UIs run on separate threads.
			DataAggregator data = new DataAggregator();
			HttpServer web = new HttpServer(data);

			// do RunLoops in separate threads:
			Thread.Sleep(1); // 1ms
			Thread web_loop = new Thread(new ThreadStart(web.RunLoop));
			web_loop.Start();
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
