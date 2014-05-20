using System;
using System.Threading;
using System.Threading.Tasks;
using Debug = System.Diagnostics.Debug;

namespace Solar.Car.Console
{
	public class Program
	{
		public static void Main(string[] args)
		{
			// Set resource prefix
			Config.Resource_Prefix = "";
			// Enable debugging
			Debug.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(System.Console.Out));
			Debug.Listeners.Add(new System.Diagnostics.TextWriterTraceListener("debug.log"));
			Debug.WriteLine("PROGRAM: Hello World!");
			// Setup SolarCar environment
			if (Environment.OSVersion.Platform.HasFlag(PlatformID.MacOSX) ||
			    Environment.OSVersion.Platform.HasFlag(PlatformID.Unix))
			{
				Config.Platform = Config.PlatformID.Unix;
			}
			else
			{
				Config.Platform = Config.PlatformID.Win32;
			}
			// Load configuration from file
			Config.LoadConfig(System.IO.File.ReadAllText(@"Config.json"));

			RunApp();

			// Console.WriteLine("Press any key to continue...");
			// Console.WriteLine();
			// Console.ReadKey();
		}

		public static void RunApp()
		{
			// Inject platform-specific logic
			try
			{
				using (var ts = new CancellationTokenSource())
				using (var ds = new JsonDataSource())
				{
					Task solarcar_loop = Solar.Program.RunProgram(ts.Token, ds, new CommManager(), new HttpGui());
					// System.Diagnostics.Process.Start(Config.HTTPSERVER_CAR_URL);
					System.Console.ReadKey();
					ts.Cancel();
					solarcar_loop.Wait();
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine("PROGRAM: EXCEPTION: " + e.ToString());
			}
			finally
			{
				Debug.WriteLine("PROGRAM: Run finished");
			}
		}

#region Misc Process Utilities

		/// <summary>
		/// Shutdown the computer, requires shutdown executable.
		/// </summary>
		public static void Shutdown()
		{
			if (Config.Platform == Config.PlatformID.Unix)
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
			if (Config.Platform == Config.PlatformID.Win32)
			{
				var startInfo = new System.Diagnostics.ProcessStartInfo("SolarCar.exe") { Verb = "runas" };
				System.Diagnostics.Process.Start(startInfo);
				Environment.Exit(0);
			}
		}

#endregion
	}
}
