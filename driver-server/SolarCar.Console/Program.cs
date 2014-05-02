using System;
using Debug = System.Diagnostics.Debug;
using System.Threading.Tasks;

namespace Solar.Car.Console
{
	class Program
	{
		/// <summary>
		/// Shutdown the computer, requires shutdown executable.
		/// </summary>
		public static void Shutdown()
		{
			if (Environment.OSVersion.Platform.HasFlag(PlatformID.MacOSX) ||
			    Environment.OSVersion.Platform.HasFlag(PlatformID.Unix))
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
			if (Environment.OSVersion.Platform.HasFlag(PlatformID.Win32NT))
			{
				var startInfo = new System.Diagnostics.ProcessStartInfo("SolarCar.exe") { Verb = "runas" };
				System.Diagnostics.Process.Start(startInfo);
				Environment.Exit(0);
			}
		}

		public static void Main(string[] args)
		{
			Debug.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(System.Console.Out));
			Debug.WriteLine("PROGRAM: Hello World!");
			Solar.Car.Program.RunCar().Wait();
			Debug.WriteLine("PROGRAM: Run finished");
			
			// Console.WriteLine("Press any key to continue...");
			// Console.WriteLine();
			// Console.ReadKey();
		}
	}
}
