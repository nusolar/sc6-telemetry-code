using System;
using Mono.Data.Sqlite;

namespace SolarCar {
	class CarReport {
		public BatteryReport b = null;
		public ArrayReport r = null;
	}

	class CarData {
		public void commit(BatteryReport report) {
		}

		public void commit(ArrayReport report) {
		}

		public void commit(DigitalOutReport report) {
		}

		/**
		 * critical health of the batteries.
		 */
		public bool Health() {
			return false;
		}

		public bool BatteriesCanCharge() {
			return false;
		}

		public bool BatteriesCanDischarge() {
			return false;
		}

		public bool ArrayAvailable() {
			return false;
		}

		public static void Test() {
			using (SqliteConnection con = new SqliteConnection("Data Source=:memory:")) {
				con.Open();
				using (SqliteCommand cmd = new SqliteCommand("SELECT SQLITE_VERSION()", con)) {
					Console.WriteLine(cmd.ExecuteScalar().ToString());
				}
			}
		}
	}
}
