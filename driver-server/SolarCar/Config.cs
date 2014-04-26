using System;

namespace SolarCar
{
	/// <summary>
	/// Configuration settings.
	/// </summary>
	public static class Config
	{
		public static PlatformID _pid = Environment.OSVersion.Platform;
		/// CANUSB safety limits
		public const int CANUSB_READ_BUFFER_LIMIT = 1020;
		public const int CANUSB_WRITE_BUFFER_LIMIT = 500;
		/// CANUSB port name
		public static string CANUSB_SERIAL_DEV = _pid.HasFlag(PlatformID.Unix) || _pid.HasFlag(PlatformID.MacOSX) ?
			"/dev/tty.usbserial-LWR8N2L2" : "COM0";
		public const int CANUSB_TX_INTERVAL_MS = 50;
		public const int CANUSB_RX_INTERVAL_MS = 50;
		/// HTTP Server paths
		public const string HTTPSERVER_CAR_PREFIX = "http://+:8080/";
		public const string HTTPSERVER_LAPTOP_PREFIX = "http://+:8081/";
		public const int HTTPSERVER_TIMEOUT_MS = 1000;
		public const string HTTPSERVER_GUI_SUBDIR = "driver-gui";
		/// Database location
		public static string SQLITE_DB_FILE = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + "/solarcar.sqlite3";
		public static string SQLITE_CONNECTION_CLASS_AQN = typeof(Mono.Data.Sqlite.SqliteConnection).AssemblyQualifiedName;
#if DEBUG
		/// Car and Laptop DNS name
		public const string HTTPSERVER_CAR_URL = "http://localhost:8080/";
		public const string HTTPSERVER_LAPTOP_URL = "http://localhost:8081/telemetry";
		public const string HTTPSERVER_EXTRA_URL = "localhost";
#else
		/// Car and Laptop DNS name
		public const string HTTPSERVER_CAR_URL = "http://nusolar-car.no-ip.org:8080/";
		public const string HTTPSERVER_LAPTOP_URL = "http://nusolar-server.no-ip.org:8081/telemetry";
		public const string HTTPSERVER_EXTRA_URL = "nusolar.no-ip.biz";
#endif
	}
}
