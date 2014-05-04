using System;
using System.Threading;
using System.Threading.Tasks;
using Debug = System.Diagnostics.Debug;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Solar.Laptop
{
	class JsonDataSource: IDataSource
	{
		class JsonDb
		{
			public int count { get; set; }

			public Status[] data { get; set; }
		}

		static ConcurrentQueue<Status> LoadJson()
		{
			try
			{
				using (var stream = System.IO.File.OpenText(Solar.Car.Config.DB_JSON_LAPTOP_FILE))
				{
					JsonDb obj = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonDb>(stream.ReadToEnd());
					stream.Close();
					obj.count = (obj.count == obj.data.Length) ? obj.count : obj.data.Length;
					return new ConcurrentQueue<Status>(obj.data);
				}
			}
			catch (System.NullReferenceException)
			{
				Debug.WriteLine("DB Loading: Unrecognizable JsonDb");
			}
			catch (System.IO.FileNotFoundException)
			{
				Debug.WriteLine("DB Loading: FNF: " + Solar.Car.Config.DB_JSON_LAPTOP_FILE);
			}
			return new ConcurrentQueue<Status>();
		}

		ConcurrentQueue<Status> data = LoadJson();

		public ConcurrentQueue<Status> GetConnection()
		{
			return data;
		}

		public void Save()
		{
			using (var stream = System.IO.File.CreateText(Solar.Car.Config.DB_JSON_LAPTOP_FILE))
			{
				JsonDb jdb = new JsonDb { count = this.data.Count, data = this.data.ToArray() };
				stream.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(jdb)).Wait();
				stream.Close();
				Debug.WriteLine("DB Save: Written to file");
			}
		}

#region IDisposable

		bool disposed = false;

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (this.data != null)
				{
					this.Save();
				}
				disposed = true;
			}
		}

#endregion
	}

	class Program
	{
		public static async Task RunLaptop(IDataSource dataSource, IBusinessLayer car, IAppLayer web = null)
		{
			IDataServiceLayer db = new Database();
			db.DataSource = dataSource;
			if (car != null)
				car.DataLayer = db;
			if (web != null)
				web.Manager = car;

			using (var tokenSource = new CancellationTokenSource())
			{
				List<Task> tasks = new List<Task>();
				if (web != null)
					tasks.Add(web.AppLayerLoop(tokenSource.Token));
				if (car != null)
					tasks.Add(car.BusinessLoop(tokenSource.Token));
				//tasks.Add(db.ConsumeCarTelemetry(tokenSource.Token));
#if DEBUG
				tasks.Add(Task.Run(() => Console.ReadKey(), tokenSource.Token));
#endif

				await Task.WhenAny(tasks.ToArray());
				tokenSource.Cancel();
				Debug.WriteLine("PROGRAM: Aborted");
				await Task.WhenAll(tasks.ToArray());
			}
		}

		public static void Main(string[] args)
		{
			// Enable debugging
			Debug.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(System.Console.Out));
			Debug.WriteLine("PROGRAM: Hello World!");
			// Setup SolarCar environment
			if (Environment.OSVersion.Platform.HasFlag(PlatformID.MacOSX) ||
			    Environment.OSVersion.Platform.HasFlag(PlatformID.Unix))
			{
				Solar.Car.Config.Platform = Solar.Car.Config.PlatformID.Unix;
			}
			else
			{
				Solar.Car.Config.Platform = Solar.Car.Config.PlatformID.Win32;
			}
			// Load configuration from file
			Solar.Car.Config.LoadConfig(() => System.IO.File.ReadAllText(@"Config.json"));

			using (var ds = new JsonDataSource())
				RunLaptop(ds, new HttpServerManager(), null).Wait();
		}
	}
}
