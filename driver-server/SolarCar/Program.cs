using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Debug = System.Diagnostics.Debug;

namespace Solar.Car
{
	/// <summary>
	/// Centralized tasks for running the solar car.
	/// </summary>
	public class Program
	{
		/// <summary>
		/// Run the solar car's HttpGui, command Driver Controls, gather telemetry.
		/// </summary>
		public static async Task RunCar()
		{
			// DataAggregator collects all data
			CarFrontend car = new CarFrontend();
			// UIs run on separate threads.
			HttpGui web = new HttpGui(car);
			// Telemetry caching
			Database db = new Database();

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
#else
				await web_loop;
#endif
				tokenSource.Cancel();
				Debug.WriteLine("PROGRAM: Aborted");
				await web_loop;
				await can_loop;
				await prod_loop;
				await cons_loop;
			}
		}
	}
}
