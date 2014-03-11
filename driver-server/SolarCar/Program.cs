using System;
using System.Threading;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace SolarCar
{
	class Program
	{
		static void RunCar()
		{
			// CanUsb talks to CAN bus.
			CanUsb canusb = new CanUsb(Config.CANUSB_DEV_FILE);
			// DataAggregator collects all data
			DataAggregator data = new DataAggregator();

			// link up data
			canusb.handlers += data.ProcessCanPacket;
			data.tx_cans += canusb.TransmitPacket;

			// UIs run on separate threads.
			HttpServer web = new HttpServer(data);

			// do RunLoops in separate threads:
			Thread.Sleep(1); // 1ms
			Thread web_loop = new Thread(new ThreadStart(web.RunLoop));
			Thread txcan_loop = new Thread(new ThreadStart(data.TxCanLoop));
			web_loop.Start();
			txcan_loop.Start();

			System.Diagnostics.Process.Start(@"http://localhost:8080/index.html");

			web_loop.Join();
			txcan_loop.Join();
		}

		public static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			CarData.Test();

			RunCar();

			// Console.WriteLine("Press any key to continue...");
			// Console.WriteLine();
			// Console.ReadKey();
		}
	}
}
