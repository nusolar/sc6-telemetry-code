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

		public Database()
		{
			this.DataSource = new JsonDataSource();
		}

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
				string archive_json = JsonConvert.SerializeObject(archive_obj).Replace("],[", "],\n[").Replace("[[", "[\n[");
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
		}*/

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
				if (disposing)
				{
					if (this.DataSource != null)
					{
						// attempt to push remaining data to Dropbox
						try
						{
							this.PushToDropbox().Wait();
						}
						catch (Exception e)
						{
							Debug.WriteLine("DB:\t\tDispose: Exception when Dropboxing: " + e.Message);
						}
						finally
						{
							this.DataSource.Dispose();
						}
					}
				}
				disposed = true;
			}
		}

#endregion
	}

	/// <summary>
	/// Maintains a ConcurrentQueue in memory, saves to JSON at shutdown.
	/// WARNING: Can leak memory!
	/// </summary>
	public class JsonDataSource: IDataSource
	{
		JsonDb db = null;

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
			// load cache
			try
			{
				using (var stream = System.IO.File.OpenText(Config.Resource_Prefix + Config.DB_CAR_FILE))
					this.db = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonDb>(stream.ReadToEnd());
				return; // return and delete cache
			}
			catch (System.NullReferenceException)
			{
				Debug.WriteLine("DB:\t\tLoading: Unrecognizable JsonDb");
			}
			catch (System.IO.FileNotFoundException)
			{
				Debug.WriteLine("DB:\t\tLoading: FNF: " + Config.DB_CAR_FILE);
			}
			finally
			{
				// delete cache
				try
				{
					if (System.IO.File.Exists(Config.Resource_Prefix + Config.DB_CAR_FILE))
						System.IO.File.Delete(Config.Resource_Prefix + Config.DB_CAR_FILE);
				}
				catch (System.IO.IOException e)
				{
					Debug.WriteLine("DB:\t\tLoading: Couldn't delete cache: " + e.Message);
				}
			}

			// if load failed, create new and initialize new JsonDb for Solar.Status
			this.db = new JsonDb { count = 0, headers = new List<string> { }, data = new List<List<object>> { } };
			foreach (var prop in typeof(Status).GetProperties())
				this.db.headers.Add(prop.Name);
		}

		void Save()
		{
			if (this.db.data.Count > 0)
				using (System.IO.StreamWriter stream = System.IO.File.CreateText(Config.Resource_Prefix + Config.DB_CAR_FILE))
				{
					stream.WriteAsync(JsonConvert.SerializeObject(this.db)).Wait();
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
