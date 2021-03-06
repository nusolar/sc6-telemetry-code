﻿using System;
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
			Debug.WriteLine("PROGRAM:\tHello World!");
			// Setup SolarCar environment
			Config.Platform = 
				(Environment.OSVersion.Platform.HasFlag(PlatformID.MacOSX) || Environment.OSVersion.Platform.HasFlag(PlatformID.Unix)) ? 
				Config.PlatformID.Unix : 
				Config.PlatformID.Win32;
			// Load configuration from file
			Config.LoadConfig(System.IO.File.ReadAllText(@"Config.json"));

			RunApp();
		}

		public static void RunApp()
		{
			// Inject platform-specific logic
			try
			{
				using (var ts = new CancellationTokenSource())
				using (var solarcar = new CommManager())
				{
					Task solarcar_loop = solarcar.BusinessLoop(ts.Token);
					// System.Diagnostics.Process.Start(Config.HTTPSERVER_CAR_URL);
					System.Console.WriteLine("Press any key to halt...");
					System.Console.ReadKey();
					ts.Cancel();
					solarcar_loop.Wait();
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine("PROGRAM:\tEXCEPTION: " + e.ToString());
			}
			finally
			{
				Debug.WriteLine("PROGRAM:\tRun finished");
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
