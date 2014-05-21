using System;
using System.Threading;
using System.Threading.Tasks;
using Debug = System.Diagnostics.Debug;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Solar.Laptop
{
	class Program
	{
		public static void Main(string[] args)
		{
			// Set resource prefix
			Config.Resource_Prefix = "";
			// Enable debugging
			Debug.Listeners.Add(new System.Diagnostics.TextWriterTraceListener("debug.log"));
			Debug.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(System.Console.Out));
			Debug.WriteLine("PROGRAM:\tHello World!");
			// Setup SolarCar environment
			Config.Platform = 
				(Environment.OSVersion.Platform.HasFlag(PlatformID.MacOSX) || Environment.OSVersion.Platform.HasFlag(PlatformID.Unix)) ? 
				Config.PlatformID.Unix : 
				Config.PlatformID.Win32;
			// Load configuration from file
			Config.LoadConfig(System.IO.File.ReadAllText(@"Config.json"));

			try
			{
				using (var ts = new CancellationTokenSource())
				using (var laptop = new HttpServerManager())
				{
					Task laptop_loop = laptop.BusinessLoop(ts.Token);
					System.Console.ReadKey();
					ts.Cancel();
					laptop_loop.Wait();
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine("PROGRAM: EXCEPTION: " + e.ToString());
			}
		}
	}
}
