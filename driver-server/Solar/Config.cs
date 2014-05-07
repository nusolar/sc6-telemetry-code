using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Solar.Car
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
		static JObject config = JObject.Parse(System.IO.File.ReadAllText(@"Config.json"));

		public static void LoadConfig(String loader)
		{
			// return JObject.Parse(System.IO.File.ReadAllText(@"Config.json"));
			config = JObject.Parse(loader);
		}

		/// CANUSB safety limit for reading
		public static int CANUSB_READ_BUFFER_LIMIT { get { return (int)config["CANUSB_READ_BUFFER_LIMIT"]; } }
		/// CANUSB safety limits for writing
		public static int CANUSB_WRITE_BUFFER_LIMIT { get { return (int)config["CANUSB_WRITE_BUFFER_LIMIT"]; } }
		/// CANUSB port name
		public static string CANUSB_SERIAL_DEV { get { return ((Platform == PlatformID.Unix) || (Platform == PlatformID.Android)) ?
			(string)config["CANUSB_SERIAL_DEV_MAC"] : (string)config["CANUSB_SERIAL_DEV_WINDOWS"];}}
		/// CANUSB interval between write attempts
		public static int CANUSB_TX_INTERVAL_MS { get { return (int)config["CANUSB_TX_INTERVAL_MS"]; } }
		/// CANUSB interval between read attempts
		public static int CANUSB_RX_INTERVAL_MS { get { return (int)config["CANUSB_RX_INTERVAL_MS"]; } }
		/// Car's HTTP Server listening prefix
		public static string HTTPSERVER_CAR_PREFIX { get { return (string)config["HTTPSERVER_CAR_PREFIX"]; } }
		/// Laptop's HTTP Server listening prefix
		public static string HTTPSERVER_LAPTOP_PREFIX { get { return (string)config["HTTPSERVER_LAPTOP_PREFIX"]; } }
		/// HTTP Server timeout for receive attempts. The Car shuts off the motor when this happens.
		public static int HTTPSERVER_TIMEOUT_MS { get { return (int)config["HTTPSERVER_TIMEOUT_MS"]; } }
		/// Car HTTP server's directory. This folder should be compiled into the Assembly.
		public static string HTTPSERVER_GUI_SUBDIR { get { return (string)config["HTTPSERVER_GUI_SUBDIR"]; } }
		/// Database location
		public static string DB_JSON_CAR_FILE { get { return System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + (string)config["DB_JSON_CAR_FILE"];}}
		public static string DB_JSON_LAPTOP_FILE { get { return System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + (string)config["DB_JSON_LAPTOP_FILE"];}}
		/// <summary>
		/// Time between Writeouts to JSON file.
		/// </summary>
		public static int DB_SAVE_INTERVAL_MS { get { return (int)config["DB_SAVE_INTERVAL_MS"]; } }
		public static string SQLITE_DB_FILE { get { return System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + "/solarcar.sqlite3";}}
		/// <summary>
		/// typeof(Mono.Data.Sqlite.SqliteConnection).AssemblyQualifiedName
		/// ==
		/// "Mono.Data.Sqlite.SqliteConnection, Mono.Data.Sqlite, Version=4.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756"
		/// </summary>
		public static string SQLITE_CONNECTION_CLASS_AQN = "Mono.Data.Sqlite.SqliteConnection, Mono.Data.Sqlite, Version=4.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756";
		/// Car DNS name and HTTP port
		public static string HTTPSERVER_CAR_URL { get { return (string)config["HTTPSERVER_CAR_URL"]; } }
		/// Laptop's DNS name, HTTP port, and URL of telemetry
		public static string HTTPSERVER_LAPTOP_URL { get { return (string)config["HTTPSERVER_LAPTOP_URL"]; } }
		/// extra, unused DNS name
		public static string HTTPSERVER_EXTRA_URL { get { return (string)config["HTTPSERVER_EXTRA_URL"]; } }
	}
}
