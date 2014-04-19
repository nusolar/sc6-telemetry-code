
namespace SolarCar
{
	/// <summary>
	/// Configuration settings.
	/// </summary>
	public static class Config
	{
#if FALSE
		public const int NUM_BATTERIES = 32;
		// BPS configuration
		public const float MAX_VOLTAGE = 4.3f;
		public const float MIN_VOLTAGE = 2.75f;
		public const float MAX_BATT_CURRENT_DISCHARGING = 72.8f;
		// max charging current should be 36.4
		public const float MAX_BATT_CURRENT_CHARGING = -32;
		public const float MAX_ARRAY_CURRENT = 10;
		public const float MIN_ARRAY_CURRENT = -1;
		public const float MAX_TEMP = 45;
		public const float MIN_TEMP = 0;
		// BPS charge/discharge limits:
		public const float MIN_DISCHARGE_VOLTAGE = MIN_VOLTAGE + 0.01f;
		public const float MAX_CHARGE_VOLTAGE = MAX_VOLTAGE - 0.01f;
		// Motor controls configuration
		public const float ACCEL_THRESH = 0.05f;
		public const float REGEN_THRESH = 0.1f;
		public const float MAX_VELOCITY = 101;
#endif
		/// CANUSB safety limits
		public const int CANUSB_READ_BUFFER_LIMIT = 1020;
		public const int CANUSB_WRITE_BUFFER_LIMIT = 500;
		/// CANUSB port name
		public const string CANUSB_DEV_FILE = "/dev/tty.usbserial-LWR8N2L2";
		public const int CANUSB_TX_INTERVAL_MS = 50;
		public const int CANUSB_RX_INTERVAL_MS = 50;
		/// HTTP Server paths
		public const string HTTPSERVER_CAR_PREFIX = "http://+:8080/";
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
