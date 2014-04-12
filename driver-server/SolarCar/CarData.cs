using System;
using Mono.Data.Sqlite;
using System.Collections.Generic;
using Thread = System.Threading.Thread;

namespace SolarCar
{
	/// <summary>
	/// Database for telemetry, trip events, and commands.
	/// </summary>
	class CarData
	{
		static DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0);

		static public long UnixTimeNow()
		{
			TimeSpan timeSpan = (DateTime.UtcNow - epoch);
			return (long)timeSpan.TotalSeconds;
		}

		DataAggregator data;
		List<Car.Status> database;

		public void TelemetryLoop()
		{
			while (this.database != null)
			{
				this.database.Add(data.Status);
				Thread.Sleep(1000);
			}
		}

		public static void Test()
		{
			using (SqliteConnection con = new SqliteConnection("Data Source=:memory:"))
			{
				con.Open();
				using (SqliteCommand cmd = new SqliteCommand("SELECT SQLITE_VERSION()", con))
				{
					Console.WriteLine(cmd.ExecuteScalar().ToString());
				}
			}
		}
	}
}
