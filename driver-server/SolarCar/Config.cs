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
		static PlatformID _pid = Environment.OSVersion.Platform;
		static JObject config = LoadConfig();

		static JObject LoadConfig()
		{
			return JObject.Parse(System.IO.File.ReadAllText(@"Config.json"));
		}

		/// CANUSB safety limit for reading
		public static int CANUSB_READ_BUFFER_LIMIT = (int)config["CANUSB_READ_BUFFER_LIMIT"];
		/// CANUSB safety limits for writing
		public static int CANUSB_WRITE_BUFFER_LIMIT = (int)config["CANUSB_WRITE_BUFFER_LIMIT"];
		/// CANUSB port name
		public static string CANUSB_SERIAL_DEV = _pid.HasFlag(PlatformID.Unix) || _pid.HasFlag(PlatformID.MacOSX) ?
			(string)config["CANUSB_SERIAL_DEV_MAC"] : (string)config["CANUSB_SERIAL_DEV_WINDOWS"];
		/// CANUSB interval between write attempts
		public static int CANUSB_TX_INTERVAL_MS = (int)config["CANUSB_TX_INTERVAL_MS"];
		/// CANUSB interval between read attempts
		public static int CANUSB_RX_INTERVAL_MS = (int)config["CANUSB_RX_INTERVAL_MS"];
		/// Car's HTTP Server listening prefix 
		public static string HTTPSERVER_CAR_PREFIX = (string)config["HTTPSERVER_CAR_PREFIX"];
		/// Laptop's HTTP Server listening prefix
		public static string HTTPSERVER_LAPTOP_PREFIX = (string)config["HTTPSERVER_LAPTOP_PREFIX"];
		/// HTTP Server timeout for receive attempts. The Car shuts off the motor when this happens.
		public static int HTTPSERVER_TIMEOUT_MS = (int)config["HTTPSERVER_TIMEOUT_MS"];
		/// Car HTTP server's directory. This folder should be compiled into the Assembly.
		public static string HTTPSERVER_GUI_SUBDIR = (string)config["HTTPSERVER_GUI_SUBDIR"];
		/// Database location
		public static string SQLITE_DB_FILE = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + "/solarcar.sqlite3";
		public static string SQLITE_CONNECTION_CLASS_AQN = typeof(Mono.Data.Sqlite.SqliteConnection).AssemblyQualifiedName;
		/// Car DNS name and HTTP port
		public static string HTTPSERVER_CAR_URL = (string)config["HTTPSERVER_CAR_URL"];
		/// Laptop's DNS name, HTTP port, and URL of telemetry
		public static string HTTPSERVER_LAPTOP_URL = (string)config["HTTPSERVER_LAPTOP_URL"];
		/// extra, unused DNS name
		public static string HTTPSERVER_EXTRA_URL = (string)config["HTTPSERVER_EXTRA_URL"];
	}
}
