using System;
using System.Linq;
using System.Threading.Tasks;
using CancellationToken = System.Threading.CancellationToken;
using WebClient = System.Net.WebClient;
using Stream = System.IO.Stream;
using JsonConvert = Newtonsoft.Json.JsonConvert;
using Debug = System.Diagnostics.Debug;
using System.Collections.Concurrent;

namespace Solar
{
	/// <summary>
	/// Database for telemetry, trip events, and commands.
	/// </summary>
	public class Database: IDataServiceLayer
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

		public IDataSource DataSource{ get; set; }

		public int CountStatus()
		{
			ConcurrentQueue<Status> statuses = this.DataSource.GetConnection();
			return statuses.Count();
		}

		public void PushStatus(Solar.Status data)
		{
			if (data.Timestamp == 0)
				data.Timestamp = UnixTimeNow();
			ConcurrentQueue<Status> statuses = this.DataSource.GetConnection();
			statuses.Enqueue(data);
		}

		public Status GetFirstStatus()
		{
			ConcurrentQueue<Status> statuses = this.DataSource.GetConnection();
			Status stat = null;
			statuses.TryPeek(out stat);
			return stat;
		}

		public bool DeleteFirstStatus()
		{
			ConcurrentQueue<Status> statuses = this.DataSource.GetConnection();
			Status stat = null;
			return statuses.TryDequeue(out stat);
		}

		async void SaveCarTelemetry(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				this.DataSource.Save();
				await Task.Delay(Config.DB_SAVE_INTERVAL_MS); // 10s
			}
		}

		public async Task ConsumeCarTelemetry(CancellationToken token)
		{
			this.SaveCarTelemetry(token);
			while (!token.IsCancellationRequested)
			{
				bool success = false;
				if (this.CountStatus() > 0)
				{
					Debug.WriteLine("DB Consumer attempting row");
					Status status = await Task.Run(() => this.GetFirstStatus());
					string json = await JsonConvert.SerializeObjectAsync(status);
					byte[] encoded = System.Text.Encoding.Default.GetBytes(json);

					// upload with POST
					Debug.WriteLine("DB Consumer uploading: " + status.Timestamp);
					using (WebClient myWebClient = new WebClient())
					{
						try
						{
							// default timeout = 100s

							byte[] response = await myWebClient.UploadDataTaskAsync(Config.HTTPSERVER_LAPTOP_URL, encoded);
							if (response.Length > 1)
							{
								// TODO check response status
								this.DeleteFirstStatus();
								success = true;
								Debug.WriteLine("DB Consumer uploaded: " + System.Text.Encoding.Default.GetString(response));
							}
						}
						catch (System.Net.WebException e)
						{
							Debug.WriteLine("DB Consumer WebException: " + e.Message);
						}
					}
				}
				else
				{
					Debug.WriteLine("DB Consumer count: " + this.CountStatus());
				}

				if (!token.IsCancellationRequested)
				{
					if (!success)
					{
						Debug.WriteLine("DB Consumer: Delaying...");
						await Task.Delay(950); // extra 950ms if didn't upload
					}

					await Task.Delay(50); // 50ms
				}
			}
		}
#if UNDEFINED
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
		bool debug = false;
		IDataSource _data_source;

		public IDataSource DataSource
		{
		get { return this._data_source; }
		set
		{
		this._data_source = value;
		// don't use "Data Source=:memory:"
		using (DbConnection conn = this.DataSource.GetConnection())
		using (DataContext db = new DataContext(conn))
		{
		if (debug)
			db.Log = Console.Out;
		Debug.WriteLine(db.Connection.ConnectionString);
		db.ExecuteCommand("CREATE TABLE IF NOT EXISTS " + _table + _create);
		db.SubmitChanges();
	}
	Debug.WriteLine("DB connected: " + Car.Config.SQLITE_DB_FILE);
}
}

/// <summary>
/// Initializes a new instance of the CarDatabase class, creates Status table.
/// </summary>
public Database()
{
}

/// <summary>
/// Counts the number of Status rows.
/// </summary>
/// <returns>Number of rows.</returns>
public int CountStatus()
{
	using (IDbConnection conn = this.DataSource.GetConnection())
	using (DataContext db = new DataContext(conn))
	{
		if (debug)
			db.Log = Console.Out;
		Table<Solar.Status> statuses = db.GetTable<Solar.Status>();
		return statuses.Count();
	}
}

/// <summary>
/// Pushs a Status row.
/// </summary>
/// <param name="data">The Status.</param>
public void PushStatus(Solar.Status data)
{
	data.Timestamp = UnixTimeNow();
	using (IDbConnection conn = this.DataSource.GetConnection())
	using (DataContext db = new DataContext(conn))
	{
		if (debug)
			db.Log = Console.Out;

		Table<Solar.Status> statuses = db.GetTable<Solar.Status>();
		statuses.InsertOnSubmit(data);

		Debug.WriteLine("DB submitting: " + statuses.ToString());
		db.SubmitChanges();
	}
	//			using (SqliteConnection con = new SqliteConnection("Data Source=" + Car.Config.SQLITE_DB_FILE + ";Version=3;"))
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
	//					Debug.WriteLine("DB: " + num_rows.ToString());
	//				}
	//
	//				using (SqliteCommand cmd = new SqliteCommand("SELECT SQLITE_VERSION()", con))
	//				{
	//					Debug.WriteLine("DB: " + cmd.ExecuteScalar().ToString());
	//				}
	//
	//				con.Close();
	//			}
}

/// <summary>
/// Returns the first Status row.
/// </summary>
/// <returns>The row.</returns>
public Solar.Status GetFirstStatus()
{
	using (IDbConnection conn = this.DataSource.GetConnection())
	using (DataContext db = new DataContext(conn))
	{
		if (debug)
			db.Log = Console.Out;

		Table<Solar.Status> statuses = db.GetTable<Solar.Status>();

		IQueryable<Solar.Status> rows =
			from status in statuses
			orderby status.Timestamp ascending
			select status;

		return rows.First();
	}
}

public void DeleteFirstStatus()
{
	using (IDbConnection conn = this.DataSource.GetConnection())
	using (DataContext db = new DataContext(conn))
	{
		if (debug)
			db.Log = Console.Out;

		Table<Solar.Status> statuses = db.GetTable<Solar.Status>();

		IQueryable<Solar.Status> rows =
			from status in statuses
			orderby status.Timestamp ascending
			select status;

		statuses.DeleteOnSubmit(rows.First());
		db.SubmitChanges();
	}
}
#endif
	}

	/// <summary>
	/// Maintains a ConcurrentQueue in memory, saves to JSON at shutdown.
	/// WARNING: Can leak memory!
	/// </summary>
	public class JsonDataSource: IDataSource
	{
		class JsonDb
		{
			public int count { get; set; }

			public Status[] data { get; set; }
		}

		static ConcurrentQueue<Status> LoadJson(string path)
		{
			try
			{
				using (var stream = System.IO.File.OpenText(path))
				{
					JsonDb obj = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonDb>(stream.ReadToEnd());
					stream.Close();
					obj.count = (obj.count == obj.data.Length) ? obj.count : obj.data.Length;
					var r = from datum in obj.data
					        orderby datum.Timestamp
					        select datum;

					return new ConcurrentQueue<Status>(r.ToList());
				}
			}
			catch (System.NullReferenceException)
			{
				Debug.WriteLine("DB Loading: Unrecognizable JsonDb");
			}
			catch (System.IO.FileNotFoundException)
			{
				Debug.WriteLine("DB Loading: FNF: " + Config.DB_JSON_CAR_FILE);
			}
			return new ConcurrentQueue<Status>();
		}

		string DataPath { get; set; }

		ConcurrentQueue<Status> data;

		public JsonDataSource(string DataPath)
		{
			this.DataPath = DataPath;
			data = LoadJson(this.DataPath);
		}

		public ConcurrentQueue<Status> GetConnection()
		{
			return data;
		}

		public void Save()
		{
			using (var stream = System.IO.File.CreateText(this.DataPath))
			{
				JsonDb jdb = new JsonDb { count = this.data.Count, data = this.data.ToArray() };
				stream.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(jdb).Replace("},{", "},\n{")
				).Wait();
				stream.Close();
				Debug.WriteLine("DB Save: Written to file");
			}
		}

#region IDisposable

		bool disposed = false;

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (this.data != null)
				{
					this.Save();
				}
				disposed = true;
			}
		}

#endregion
	}
}
