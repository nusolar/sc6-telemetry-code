
namespace SolarCar
{
	static class Config
	{
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
		// Timers
#if DEBUG
		public const int CAN_TX_INTERVAL_MS = 50;
		public const int HTTP_TIMEOUT_MS = 1200;
#else
		public const int CAN_TX_INTERVAL_MS = 100;
		public const int HTTP_TIMEOUT_MS = 100;
#endif
		// Platform-specific things
		public const string CANUSB_DEV_FILE = "/dev/tty.usbserial-LWR8N2L2";
		public const string HTTPSERVER_PREFIX = "http://+:8080/";
		public const string GUI_SUBDIR = "driver-gui";
	}
}
