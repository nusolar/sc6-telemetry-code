using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Solar.Car.Mac
{
	public partial class MainWindowController : MonoMac.AppKit.NSWindowController
	{
		MainWindowDelegate _delegate = new MainWindowDelegate();

#region Constructors

		// Called when created from unmanaged code
		public MainWindowController(IntPtr handle) : base(handle)
		{
			Initialize();
		}
		// Called when created directly from a XIB file
		[Export("initWithCoder:")]
		public MainWindowController(NSCoder coder) : base(coder)
		{
			Initialize();
		}
		// Call to load from the XIB/NIB file
		public MainWindowController() : base("MainWindow")
		{
			Initialize();
		}
		// Shared initialization code
		void Initialize()
		{
			this.Window.Delegate = _delegate;
		}

#endregion

		//strongly typed window accessor
		public new MainWindow Window
		{
			get
			{
				return (MainWindow)base.Window;
			}
		}

		public override void WindowDidLoad()
		{
			base.WindowDidLoad();
			this.webView.MainFrame.LoadRequest(new NSUrlRequest(new NSUrl(Config.HTTPSERVER_CAR_URL)));
		}
	}

	public class MainWindowDelegate : NSWindowDelegate
	{
		public override bool WindowShouldClose(NSObject sender)
		{
			((AppDelegate)NSApplication.SharedApplication.Delegate).SolarCarStop(sender);
			return true;
		}
	}
}

