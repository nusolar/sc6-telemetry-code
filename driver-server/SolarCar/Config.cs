
namespace SolarCar {
	static class Config {
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
		public const float MIN_DISCHARGE_VOLTAGE = 2.76f;
		public const float MAX_CHARGE_VOLTAGE = 4.29f;
		// Motor controls configuration
		public const float ACCEL_THRESH = 0.05f;
		public const float REGEN_THRESH = 0.1f;
		public const float MAX_VELOCITY = 101;
	}
}