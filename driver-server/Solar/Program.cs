using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Debug = System.Diagnostics.Debug;
using System.Collections.Generic;

namespace Solar
{
	/// <summary>
	/// Centralized tasks for running the solar car.
	/// </summary>
	public class Program
	{
		/// <summary>
		/// Run the solar car's HttpGui, command Driver Controls, gather telemetry.
		/// </summary>
		public static async Task RunProgram(CancellationToken parentToken,
		                                    IDataSource dataSource,
		                                    IBusinessLayer car,
		                                    IAppLayer gui = null)
		{
			// Telemetry caching
			IDataServiceLayer db = new Database();
			db.DataSource = dataSource;
			// DataAggregator collects all data
			if (car != null)
				car.DataLayer = db;
			// UIs run on separate threads.
			if (gui != null)
				gui.Manager = car;
				
			// spawn cancellable CAN and GUI communication threads:
			using (var innerSource = new CancellationTokenSource())
			{
				var childSource = CancellationTokenSource.CreateLinkedTokenSource(parentToken, innerSource.Token);

				List<Task> tasks = new List<Task>();
				if (gui != null)
					tasks.Add(gui.AppLayerLoop(childSource.Token));
				if (car != null)
					tasks.Add(car.BusinessLoop(childSource.Token));

				await Task.WhenAny(tasks.ToArray());
				innerSource.Cancel();
				Debug.WriteLine("PROGRAM: Aborted");
				await Task.WhenAll(tasks.ToArray());
			}
		}
	}
}
