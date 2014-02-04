using System;
using Mono.Data.Sqlite;

namespace SolarCar
{
	/// <summary>
	/// Database for telemetry, trip events, and commands.
	/// </summary>
	class CarData
	{
		public void commit(Car.Status report)
		{
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
