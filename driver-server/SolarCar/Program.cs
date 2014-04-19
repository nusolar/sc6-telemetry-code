using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace SolarCar
{
	class Program
	{
		static void RunCar()
		{
			// DataAggregator collects all data
			CarFrontend car = new CarFrontend();
			// UIs run on separate threads.
			HttpServer web = new HttpServer(car);
			// Telemetry caching
			CarDatabase db = new CarDatabase();

			Thread.Sleep(1); // 1ms

			// spawn cancellable CAN and GUI communication threads:
			using (var tokenSource = new CancellationTokenSource())
			{
				var token = tokenSource.Token;
				Task web_loop = web.ReceiveLoop(token);
				Task can_loop = car.CanLoop(token);
				Task prod_loop = db.ProduceCarTelemetry(token, car);
				Task cons_loop = db.ConsumeCarTelemetry(token);

				// open GUI
				System.Diagnostics.Process.Start(@"http://localhost:8080/index.html");

#if DEBUG
				Console.ReadKey();
				tokenSource.Cancel();
				Console.WriteLine("PROGRAM: Aborted");
				web_loop.Wait();
				can_loop.Wait();
				prod_loop.Wait();
				cons_loop.Wait();
#else
				web_loop.Wait();
				tokenSource.Cancel();
				can_loop.Wait();
				prod_loop.Wait();
				cons_loop.Wait();
#endif
			}
		}

		public static void Main(string[] args)
		{
			Console.WriteLine("PROGRAM: Hello World!");

			RunCar();
			// Thread.Sleep(1000); // delay for canusb destruction

			Console.WriteLine("PROGRAM: Run finished");

			// Console.WriteLine("Press any key to continue...");
			// Console.WriteLine();
			// Console.ReadKey();
		}
	}
}
