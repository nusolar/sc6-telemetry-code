
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
		// Bootup files
		public const string SAMPLE_BATTERY_REPORT = "/Users/alex/GitHub/sc6-telemetry-code/sample_bms.json";
		public const string SAMPLE_MOTOR_REPORT = "/Users/alex/GitHub/sc6-telemetry-code/sample_motor.json";
		public const string SAMPLE_INPUT_REPORT = "/Users/alex/GitHub/sc6-telemetry-code/sample_input.json";
		public const string SAMPLE_OUTPUT_REPORT = "/Users/alex/GitHub/sc6-telemetry-code/sample_output.json";
		// Tx CAN LOOP
		public const int LOOP_INTERVAL_MS = 1000;
		// Platform-specific things
		public const string CANUSB_DEV_FILE = "/dev/tty.CANUSB";
		public const string HTTPSERVER_PREFIX = "http://+:8080/";
	}
}
