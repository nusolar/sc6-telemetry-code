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

		public void PushStatus(Solar.Status data)
		{
			if (data.Timestamp == 0)
				data.Timestamp = UnixTimeNow();
			ConcurrentQueue<Status> statuses = this.DataSource.GetConnection();
			statuses.Enqueue(data);
		}

		public void Archive()
		{
			this.DataSource.Archive();
		}
		/*public int CountStatus()
		{
			ConcurrentQueue<Status> statuses = this.DataSource.GetConnection();
			return statuses.Count();
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

		public async Task ConsumeCarTelemetry(CancellationToken token)
		{
			//this.SaveCarTelemetry(token);
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
		}*/
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
		/*static ConcurrentQueue<Status> LoadJson(string path)
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
		}*/
		ConcurrentQueue<Status> data;

		public JsonDataSource()
		{
			data = new ConcurrentQueue<Status>();
		}

		public ConcurrentQueue<Status> GetConnection()
		{
			return data;
		}

		public void Archive()
		{
			using (var stream = System.IO.File.CreateText(Config.Resource_Prefix + string.Format(Config.DB_JSON_CAR_FILES, Database.UnixTimeNow())))
			{
				ConcurrentQueue<Status> cache_data = this.data;
				this.data = new ConcurrentQueue<Status>();
				JsonDb jdb = new JsonDb { count = cache_data.Count, data = cache_data.ToArray() };
				stream.WriteAsync(
					Newtonsoft.Json.JsonConvert.SerializeObject(jdb).Replace("},{", "},\n{")
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
					this.Archive();
				}
				disposed = true;
			}
		}

#endregion
	}
}
