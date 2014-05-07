using System;
using System.Threading.Tasks;
using Debug = System.Diagnostics.Debug;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Solar.Car.Console
{
	/// <summary>
	/// Maintains a ConcurrentQueue in memory, saves to JSON at shutdown.
	/// WARNING: Can leak memory!
	/// </summary>
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
				using (var stream = System.IO.File.OpenText(Config.DB_JSON_CAR_FILE))
				{
					JsonDb obj = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonDb>(stream.ReadToEnd());
					stream.Close();
					obj.count = (obj.count == obj.data.Length) ? obj.count : obj.data.Length;
					var r = from datum in obj.data
					        orderby datum.Timestamp
					        select datum;

					return new ConcurrentQueue<Status>(r.ToList());
				}
			}
			catch (System.NullReferenceException)
			{
				Debug.WriteLine("DB Loading: Unrecognizable JsonDb");
			}
			catch (System.IO.FileNotFoundException)
			{
				Debug.WriteLine("DB Loading: FNF: " + Config.DB_JSON_CAR_FILE);
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
			using (var stream = System.IO.File.CreateText(Config.DB_JSON_CAR_FILE))
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
#region Misc Process Utilities

		/// <summary>
		/// Shutdown the computer, requires shutdown executable.
		/// </summary>
		public static void Shutdown()
		{
			if (Solar.Car.Config.Platform == Solar.Car.Config.PlatformID.Unix)
			{
				System.Diagnostics.Process.Start("shutdown", "-h now");
			}
			else
			{
				System.Diagnostics.Process.Start("shutdown", "-s -t 0");
			}
		}

		/// <summary>
		/// WINDOWS ONLY - Restarts as admin.
		/// </summary>
		public static void RestartAsAdmin()
		{
			if (Solar.Car.Config.Platform == Solar.Car.Config.PlatformID.Win32)
			{
				var startInfo = new System.Diagnostics.ProcessStartInfo("SolarCar.exe") { Verb = "runas" };
				System.Diagnostics.Process.Start(startInfo);
				Environment.Exit(0);
			}
		}

#endregion

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
			Solar.Car.Config.LoadConfig(System.IO.File.ReadAllText(@"Config.json"));

			// Inject platform-specific logic
			try
			{
				using (var ds = new JsonDataSource())
				{
					Solar.Program.RunProgram(ds, new CommManager(), new HttpGui()).Wait();
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine("PROGRAM: EXCEPTION: " + e.ToString());
			}

			Debug.WriteLine("PROGRAM: Run finished");
			
			// Console.WriteLine("Press any key to continue...");
			// Console.WriteLine();
			// Console.ReadKey();
		}
	}
}
