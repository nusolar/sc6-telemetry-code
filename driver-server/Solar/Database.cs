using System;
using System.Linq;
using System.Threading.Tasks;
using CancellationToken = System.Threading.CancellationToken;
using WebClient = System.Net.WebClient;
using Stream = System.IO.Stream;
using JsonConvert = Newtonsoft.Json.JsonConvert;
using Debug = System.Diagnostics.Debug;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Solar
{
	public class JsonDb
	{
		public int count { get; set; }

		public List<string> headers { get; set; }

		public List<List<object>> data { get; set; }
	}

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
		static public double UnixTimeNow()
		{
			TimeSpan timeSpan = (DateTime.UtcNow - epoch);
			return timeSpan.TotalSeconds;
		}

		public IDataSource DataSource { get; set; }

		public object data_lock = new object();

		List<object> to_list(object obj)
		{
			List<object> list = new List<object>();
			foreach (var prop in obj.GetType().GetProperties())
			{
				list.Add(prop.GetValue(obj));
			}
			return list;
		}

		public void PushStatus(Solar.Status data)
		{
			if (data.Timestamp == 0)
				data.Timestamp = UnixTimeNow();
			List<object> serialized_data = this.to_list(data);
			lock (data_lock)
			{
				JsonDb db = this.DataSource.GetConnection();
				db.data.Add(serialized_data);
				++db.count;
				Debug.WriteLine("DB:\t\tPushStatus: success");
			}
		}

		public async Task PushToDropbox()
		{
			string archive_path = string.Format(Config.DB_DROPBOX_FILES, Database.UnixTimeNow());
			JsonDb archive_obj = null;
			lock (data_lock)
			{
				archive_obj = this.DataSource.GetArchive();
			}
			try
			{
				string archive_json = Newtonsoft.Json.JsonConvert.SerializeObject(archive_obj).Replace("],[", "],\n[").Replace("[[", "[\n[");
				byte[] archive_data = System.Text.Encoding.UTF8.GetBytes(archive_json);
				Dropbox d = new Dropbox();
				Debug.WriteLine("DB:\t\tDropbox: pushing archive: " + archive_path);
				Dropbox.MetadataResponse resp = await d.FilesPut(archive_data, archive_path);
				Debug.WriteLine("DB:\t\tDropbox: pushed: " + resp.Path);
				if (resp != null)
					return; // return if push succeeded
			}
			catch (Exception e)
			{
				Debug.WriteLine("DB:\t\tDropbox: push failed: " + e.Message);
				this.DataSource.RestoreArchive(archive_obj);
			}
			// otherwise if push failed, restore archive so the data isn't lost
			lock (data_lock)
			{
				this.DataSource.RestoreArchive(archive_obj);
			}
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
		JsonDb db;

		public JsonDataSource()
		{
			this.LoadJson();
			db.count = (db.count == db.data.Count) ? db.count : db.data.Count;
		}

		public JsonDb GetConnection()
		{
			return this.db;
		}

		public JsonDb GetArchive()
		{
			JsonDb archive = this.db;
			this.db = new JsonDb
			{
				count = 0,
				headers = new List<string>(archive.headers), 
				data = new List<List<object>>{ } 
			};
			return archive;
		}

		public void RestoreArchive(JsonDb archive)
		{
			this.db.data.InsertRange(0, archive.data);
			this.db.count += archive.count;
		}

		void LoadJson()
		{
			try
			{
				using (var stream = System.IO.File.OpenText(Config.DB_CAR_FILE))
				{
					this.db = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonDb>(stream.ReadToEnd());
					stream.Close();
					return;
				}
			}
			catch (System.NullReferenceException)
			{
				Debug.WriteLine("DB Loading: Unrecognizable JsonDb");
			}
			catch (System.IO.FileNotFoundException)
			{
				Debug.WriteLine("DB Loading: FNF: " + Config.DB_CAR_FILE);
			}

			// if load failed, create new and initialize new JsonDb for Solar.Status
			this.db = new JsonDb { count = 0, headers = new List<string> { }, data = new List<List<object>> { } };
			foreach (var prop in typeof(Status).GetProperties())
				this.db.headers.Add(prop.Name);
		}

		void Save()
		{
			using (System.IO.StreamWriter stream = System.IO.File.CreateText(Config.Resource_Prefix + string.Format(Config.DB_DROPBOX_FILES, Database.UnixTimeNow())))
			{
				stream.WriteAsync(
					Newtonsoft.Json.JsonConvert.SerializeObject(this.db)
				).Wait();
				stream.Close();
				Debug.WriteLine("DB:\t\tArchive: Written to file");
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
				if (this.db != null)
				{
					this.Save();
				}
				disposed = true;
			}
		}

#endregion
	}
}
