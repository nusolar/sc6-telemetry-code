using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Solar
{
	/// <summary>
	/// Configuration settings.
	/// </summary>
	public static class Config
	{
		public enum PlatformID
		{
			Unix,
			Win32,
			Android,
			IOS,
			WinPhone
		}

		public static PlatformID Platform = PlatformID.Unix;
		// public static Type CanUsbHardwareType = typeof(CanUsbHardware);
		static JObject config = null;

		public static void LoadConfig(String loader)
		{
			// return JObject.Parse(System.IO.File.ReadAllText(@"Config.json"));
			config = JObject.Parse(loader);
		}

		public static string Resource_Prefix = null;

		/// CANUSB safety limit for reading
		public static int CANUSB_READ_BUFFER_LIMIT { get { return (int)config["CANUSB_READ_BUFFER_LIMIT"]; } }

		/// CANUSB safety limits for writing
		public static int CANUSB_WRITE_BUFFER_LIMIT { get { return (int)config["CANUSB_WRITE_BUFFER_LIMIT"]; } }

		/// CANUSB port name
		public static string CANUSB_SERIAL_DEV { get { return ((Platform == PlatformID.Unix) || (Platform == PlatformID.Android)) ?
					(string)config["CANUSB_SERIAL_DEV_UNIX"] : (string)config["CANUSB_SERIAL_DEV_WINDOWS"]; } }

		/// CANUSB interval between read attempts
		public static int CANUSB_RX_INTERVAL_MS { get { return (int)config["CANUSB_RX_INTERVAL_MS"]; } }

		/// CANUSB interval between connection attempts
		public static int CANUSB_DISCONNECT_RETRY_MS { get { return (int)config["CANUSB_DISCONNECT_RETRY_MS"]; } }

		/// Car Database location
		public static string DB_DROPBOX_FILES { get { return (string)config["DB_DROPBOX_FILES"]; } }

		/// Car Database location
		public static string DB_CAR_FILE { get { return (string)config["DB_CAR_FILE"]; } }

		/// Laptop Database location
		public static string DB_LAPTOP_FILE { get { return (string)config["DB_LAPTOP_FILE"]; } }

		/// Time between additions to DB
		public static int DB_ADD_INTERVAL_MS { get { return (int)config["DB_ADD_INTERVAL_MS"]; } }

		/// Time between Writeouts of DB to disk.
		public static int DB_SAVE_INTERVAL_MS { get { return (int)config["DB_SAVE_INTERVAL_MS"]; } }

		/// Car's HTTP Server listening prefix
		public static string HTTPSERVER_CAR_PREFIX { get { return (string)config["HTTPSERVER_CAR_PREFIX"]; } }

		/// Laptop's HTTP Server listening prefix
		public static string HTTPSERVER_LAPTOP_PREFIX { get { return (string)config["HTTPSERVER_LAPTOP_PREFIX"]; } }

		/// HTTP Server timeout for receive attempts. The Car shuts off the motor when this happens.
		public static int HTTPSERVER_TIMEOUT_MS { get { return (int)config["HTTPSERVER_TIMEOUT_MS"]; } }

		/// Car HTTP server's directory. This folder should be compiled into the Assembly.
		public static string HTTPSERVER_GUI_SUBDIR { get { return (string)config["HTTPSERVER_GUI_SUBDIR"]; } }

		/// Car DNS name and HTTP port
		public static string HTTPSERVER_CAR_URL { get { return (string)config["HTTPSERVER_CAR_URL"]; } }

		/// Laptop's DNS name, HTTP port, and URL of telemetry
		public static string HTTPSERVER_LAPTOP_URL { get { return (string)config["HTTPSERVER_LAPTOP_URL"]; } }

		/// extra, unused DNS name
		public static string HTTPSERVER_EXTRA_URL { get { return (string)config["HTTPSERVER_EXTRA_URL"]; } }
	}
}
