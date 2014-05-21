using System;
using System.Windows.Forms;
using Debug = System.Diagnostics.Debug;

namespace Solar.Car.WinForms
{
	public class Program
	{
		public static void Main(string[] args)
		{
			// Set resource prefix
			Config.Resource_Prefix = "";
			// Enable debugging
			Debug.Listeners.Add(new System.Diagnostics.TextWriterTraceListener("debug.log"));
			Debug.WriteLine("PROGRAM:\tHello World!");
			// Setup SolarCar environment
			Config.Platform = 
				(Environment.OSVersion.Platform.HasFlag(PlatformID.MacOSX) || Environment.OSVersion.Platform.HasFlag(PlatformID.Unix)) ? 
				Config.PlatformID.Unix : 
				Config.PlatformID.Win32;
			// Load configuration from file
			Config.LoadConfig(System.IO.File.ReadAllText(@"Config.json"));

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
	}
}
