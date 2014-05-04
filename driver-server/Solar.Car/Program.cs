using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Debug = System.Diagnostics.Debug;
using System.Collections.Generic;

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
		public static async Task RunCar(IDataSource dataSource, IBusinessLayer car, IAppLayer web = null)
		{
			// Telemetry caching
			IDataServiceLayer db = new Database();
			db.DataSource = dataSource;
			// DataAggregator collects all data
			if (car != null)
				car.DataLayer = db;
			// UIs run on separate threads.
			if (web != null)
				web.Manager = car;
				
			// spawn cancellable CAN and GUI communication threads:
			using (var tokenSource = new CancellationTokenSource())
			{
				List<Task> tasks = new List<Task>();
				if (web != null)
					tasks.Add(web.AppLayerLoop(tokenSource.Token));
				if (car != null)
					tasks.Add(car.BusinessLoop(tokenSource.Token));
				tasks.Add(db.ConsumeCarTelemetry(tokenSource.Token));
#if DEBUG
				tasks.Add(Task.Run(() => Console.ReadKey(), tokenSource.Token));
#endif

				await Task.WhenAny(tasks.ToArray());
				tokenSource.Cancel();
				Debug.WriteLine("PROGRAM: Aborted");
				await Task.WhenAll(tasks.ToArray());
			}
		}
	}
}
