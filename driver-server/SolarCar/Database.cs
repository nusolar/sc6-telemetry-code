using System;
using Mono.Data.Sqlite;
using System.Data.Linq;
using System.Linq;
using IDataRecord = System.Data.IDataRecord;
using System.Threading.Tasks;
using CancellationToken = System.Threading.CancellationToken;
using WebClient = System.Net.WebClient;
using Stream = System.IO.Stream;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace SolarCar
{
	/// <summary>
	/// Database for telemetry, trip events, and commands.
	/// </summary>
	public class Database
	{
		static DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0);

		/// <summary>
		/// Returns the current Unix/Epoch time, useful for timestamping.
		/// </summary>
		/// <returns>The current time.</returns>
		static public long UnixTimeNow()
		{
			TimeSpan timeSpan = (DateTime.UtcNow - epoch);
			return (long)timeSpan.TotalSeconds;
		}

		const string _table = "Status";
		const string _create = "(Timestamp int, Gear int, Signals int, " +
		                       "AccelPedal int, RegenPedal int, BrakePedal int, " +
		                       "MotorRpm real, MotorVelocity real, MotorVoltage real, MotorCurrent real, " +
		                       "PackSOC real, PackSOCPerc real, BMSPrecharge int, " +
		                       "MaxVoltage int, MinVoltage int, MaxTemp int, MinTemp int, " +
		                       "PackVoltage int, PackCurrent int, BMSExtendedStatusFlags int)";
		//static string _insert = _create.Replace(" int", "").Replace(" real", "");
		//static string _values = "(@Timestamp, @Gear, @Signals, " +
		//                        "@AccelPedal, @RegenPedal, @BrakePedal, " +
		//                        "@MotorRpm, @MotorVelocity, @MotorVoltage, @MotorCurrent, " +
		//                        "@PackSOC, @PackSOCPerc, @BMSPrecharge, " +
		//                        "@MaxVoltage, @MinVoltage, @MaxTemp, @MinTemp, " +
		//                        "@PackVoltage, @PackCurrent, @BMSExtendedStatusFlags)";
		static string _connection_string = "Data Source=" + Config.SQLITE_DB_FILE +
		                                   ";DbLinqProvider=Sqlite;DbLinqConnectionType=" + Config.SQLITE_CONNECTION_CLASS_AQN + ";";

		/// <summary>
		/// Initializes a new instance of the CarDatabase class, creates Status table.
		/// </summary>
		public Database()
		{
			if (!System.IO.File.Exists(Config.SQLITE_DB_FILE))
				SqliteConnection.CreateFile(Config.SQLITE_DB_FILE);
			// don't use "Data Source=:memory:"
			using (DataContext db = new DataContext(_connection_string))
			{
#if DEBUG
				db.Log = Console.Out;
#endif
				db.ExecuteCommand("CREATE TABLE IF NOT EXISTS " + _table + _create);
				db.SubmitChanges();
			}
#if DEBUG
			Console.WriteLine("DB connected: " + Config.SQLITE_DB_FILE);
#endif
		}

		/// <summary>
		/// Counts the number of Status rows.
		/// </summary>
		/// <returns>Number of rows.</returns>
		public int CountStatus()
		{
			using (DataContext db = new DataContext(_connection_string + "Read Only=True;"))
			{
#if DEBUG
				db.Log = Console.Out;
#endif
				Table<Car.Status> statuses = db.GetTable<Car.Status>();
				return statuses.Count();
			}
		}

		/// <summary>
		/// Pushs a Status row.
		/// </summary>
		/// <param name="data">The Status.</param>
		public void PushStatus(Car.Status data)
		{
			data.Timestamp = UnixTimeNow();
			using (DataContext db = new DataContext(_connection_string))
			{
#if DEBUG
				db.Log = Console.Out;
#endif
				Table<Car.Status> statuses = db.GetTable<Car.Status>();
				statuses.InsertOnSubmit(data);
				Console.WriteLine("DB submitting: " + statuses.ToString());

				db.SubmitChanges();
			}
//			using (SqliteConnection con = new SqliteConnection("Data Source=" + Config.SQLITE_DB_FILE + ";Version=3;"))
//			{
//				con.Open();
//
//				using (SqliteCommand cmd = new SqliteCommand("INSERT INTO " + _table + _insert + " VALUES " + _values, con))
//				{
//					cmd.Parameters.Add(new SqliteParameter("@Timestamp", data.Timestamp));
//					cmd.Parameters.Add(new SqliteParameter("@Gear", data.Gear));
//					cmd.Parameters.Add(new SqliteParameter("@Signals", data.Signals));
//
//					cmd.Parameters.Add(new SqliteParameter("@AccelPedal", data.AccelPedal));
//					cmd.Parameters.Add(new SqliteParameter("@RegenPedal", data.RegenPedal));
//					cmd.Parameters.Add(new SqliteParameter("@BrakePedal", data.BrakePedal));
//
//					cmd.Parameters.Add(new SqliteParameter("@MotorRpm", data.MotorRpm));
//					cmd.Parameters.Add(new SqliteParameter("@MotorVelocity", data.MotorVelocity));
//					cmd.Parameters.Add(new SqliteParameter("@MotorVoltage", data.MotorVoltage));
//					cmd.Parameters.Add(new SqliteParameter("@MotorCurrent", data.MotorCurrent));
//
//					cmd.Parameters.Add(new SqliteParameter("@PackSOC", data.PackSOC));
//					cmd.Parameters.Add(new SqliteParameter("@PackSOCPerc", data.PackSOCPerc));
//					cmd.Parameters.Add(new SqliteParameter("@BMSPrecharge", data.BMSPrecharge));
//
//					cmd.Parameters.Add(new SqliteParameter("@MaxVoltage", data.MaxVoltage));
//					cmd.Parameters.Add(new SqliteParameter("@MinVoltage", data.MinVoltage));
//					cmd.Parameters.Add(new SqliteParameter("@MaxTemp", data.MaxTemp));
//					cmd.Parameters.Add(new SqliteParameter("@MinTemp", data.MinTemp));
//
//					cmd.Parameters.Add(new SqliteParameter("@PackVoltage", data.PackVoltage));
//					cmd.Parameters.Add(new SqliteParameter("@PackCurrent", data.PackCurrent));
//					cmd.Parameters.Add(new SqliteParameter("@BMSExtendedStatusFlags", data.BMSExtendedStatusFlags));
//
//					object num_rows = cmd.ExecuteNonQuery();
//					Console.WriteLine("DB: " + num_rows.ToString());
//				}
//
//				using (SqliteCommand cmd = new SqliteCommand("SELECT SQLITE_VERSION()", con))
//				{
//					Console.WriteLine("DB: " + cmd.ExecuteScalar().ToString());
//				}
//
//				con.Close();
//			}
		}

		/// <summary>
		/// Returns the first Status row.
		/// </summary>
		/// <returns>The row.</returns>
		public Car.Status GetFirstRow()
		{
			using (DataContext db = new DataContext(_connection_string))
			{
#if DEBUG
				db.Log = Console.Out;
#endif
				Table<Car.Status> statuses = db.GetTable<Car.Status>();

				IQueryable<Car.Status> rows = 
					from status in statuses
					orderby status.Timestamp ascending
					select status;

				return rows.First();
			}
		}

		public void DeleteFirstRow()
		{
			using (DataContext db = new DataContext(_connection_string))
			{
#if DEBUG
				db.Log = Console.Out;
#endif
				Table<Car.Status> statuses = db.GetTable<Car.Status>();

				IQueryable<Car.Status> rows = 
					from status in statuses
					orderby status.Timestamp ascending
					select status;

				statuses.DeleteOnSubmit(rows.First());
				db.SubmitChanges();
			}
		}

		internal async Task ProduceCarTelemetry(object obj, CarFrontend car)
		{
			CancellationToken token = (CancellationToken)obj;
			while (!token.IsCancellationRequested)
			{
#if DEBUG
				Console.WriteLine("DB: pushing");
#endif
				try
				{
					await Task.Run(() => this.PushStatus(car.Status));
				}
				catch (SqliteException e)
				{
					Console.WriteLine("DB EXCEPTION: " + e.Message);
				}

				await Task.Delay(1000); // 1s
			}
		}

		internal async Task ConsumeCarTelemetry(object obj)
		{
			CancellationToken token = (CancellationToken)obj;
			while (!token.IsCancellationRequested)
			{
				if (this.CountStatus() > 0)
				{
					Console.WriteLine("DB popping row");
					Car.Status status = await Task.Run(() => this.GetFirstRow());
					string json = await JsonConvert.SerializeObjectAsync(status);
					byte[] encoded = System.Text.Encoding.Default.GetBytes(json);

					// upload with POST
					Console.WriteLine("DB uploading");
					using (WebClient myWebClient = new WebClient())
					{
						try
						{
							// default timeout = 100s
							byte[] response = await myWebClient.UploadDataTaskAsync(Config.HTTPSERVER_LAPTOP_URL, encoded);
							if (response.Length > 1)
							{
								// TODO check response status
								this.DeleteFirstRow();
							}
#if DEBUG
							Console.WriteLine("DB uploaded: " + System.Text.Encoding.Default.GetString(response));
#endif
						}
						catch (System.Net.WebException e)
						{
							Console.WriteLine("DB WebException: " + e.Message);
						}
					}
				}
#if DEBUG
				else
				{
					Console.WriteLine("DB count: " + this.CountStatus());
				}
#endif
				await Task.Delay(100); // 100ms
			}
		}
	}
}
