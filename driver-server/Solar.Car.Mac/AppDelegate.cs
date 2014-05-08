using System;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using Debug = System.Diagnostics.Debug;
using System.Threading;
using System.Threading.Tasks;

namespace Solar.Car.Mac
{
	public partial class AppDelegate : NSApplicationDelegate
	{
		MainWindowController mainWindowController;
		CancellationTokenSource solarcar_cancel = new CancellationTokenSource();
		Task solarcar_loop = null;

		public AppDelegate()
		{
			// Set resource prefix
			Config.Resource_Prefix = NSBundle.MainBundle.ResourcePath + @"/../MonoBundle/";
			// Enable debugging
			Debug.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(Config.Resource_Prefix + "debug.log"));
			// Setup SolarCar environment
			Config.Platform = Config.PlatformID.Unix;
			// Load configuration from file
			Config.LoadConfig(System.IO.File.ReadAllText(Config.Resource_Prefix + "Config.json"));
		}

		public override void FinishedLaunching(NSObject notification)
		{
			solarcar_loop = this.SolarCarMain();
			mainWindowController = new MainWindowController();
			mainWindowController.Window.MakeKeyAndOrderFront(this);
		}

		public async Task SolarCarMain()
		{
			try
			{
				using (var ds = new JsonDataSource(Config.Resource_Prefix + Config.DB_JSON_CAR_FILE))
				{
					await Solar.Program.RunProgram(this.solarcar_cancel.Token, ds, new CommManager(), new HttpGui());
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine("PROGRAM: EXCEPTION: " + e.ToString());
			}
			finally
			{
				this.mainWindowController.Close();
			}
		}

		public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender)
		{
			return true;
		}

		public void SolarCarStop(NSObject sender)
		{
			this.solarcar_cancel.Cancel();
		}

		public override NSApplicationTerminateReply ApplicationShouldTerminate(NSApplication sender)
		{
			this.solarcar_loop.Wait();
			return NSApplicationTerminateReply.Now;
		}
	}
}

