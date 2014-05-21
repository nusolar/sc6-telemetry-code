using System;
using System.Net;
using System.Threading.Tasks;
using CancellationToken = System.Threading.CancellationToken;
using NameValueCollection = System.Collections.Specialized.NameValueCollection;
using Database = Solar.Database;
using Stream = System.IO.Stream;
using Encoding = System.Text.Encoding;
using JsonConvert = Newtonsoft.Json.JsonConvert;
using Debug = System.Diagnostics.Debug;

namespace Solar.Laptop
{
	/// <summary>
	/// Business-Layer driving the underlying Database and exposing the Status.
	/// Http server.
	/// </summary>
	class HttpServerManager: IBusinessLayer
	{
		public IDataServiceLayer DataLayer { get; set; }

		public Solar.Status Status { get { return (Solar.Status)this.DataLayer.GetFirstStatus().DeepClone(); } }

		public void HandleUserInput(Solar.Gear gear, Solar.Signals sigs)
		{
			// TODO
		}

		void ListenerCallback(HttpListenerContext context)
		{
			try
			{
				HttpListenerRequest request = context.Request;
				HttpListenerResponse response = context.Response;
				string url = request.Url.AbsolutePath;

				Debug.WriteLine("HTTP URL: " + request.RawUrl);

				if (url == "/telemetry" && context.Request.HttpMethod == "POST")
				{
					byte[] buffer = new byte[request.ContentLength64];
					using (Stream input = request.InputStream)
						input.Read(buffer, 0, buffer.Length);
					string decoded = Encoding.Default.GetString(buffer);
					Status status = JsonConvert.DeserializeObject<Status>(decoded);
					this.DataLayer.PushStatus(status);

					Debug.WriteLine("HTTP telemetry: " + decoded);

					buffer = Encoding.Default.GetBytes("{\"Response\": true}\n");
					response.StatusCode = (int)HttpStatusCode.OK;
					response.StatusDescription = "OK";
					response.ContentLength64 = buffer.LongLength;
					response.ContentEncoding = Encoding.Default;
					using (Stream output = response.OutputStream)
						output.Write(buffer, 0, buffer.Length);
					response.Close();
				}
			}
			catch (NullReferenceException e)
			{
				Debug.WriteLine("HTTP ListenerCallback: NullReferenceException: " + e.TargetSite);
			}
			catch (Exception e)
			{
				Debug.WriteLine("HTTP ListenerCallback: EXCEPTION: " + e.ToString());
			}
		}

#region Tasks

		public async Task BusinessLoop(CancellationToken token)
		{
			try
			{
				using (HttpListener listener = new HttpListener())
				{
					listener.Prefixes.Add(Config.HTTPSERVER_LAPTOP_PREFIX);
					listener.Start();

					while (!token.IsCancellationRequested && listener.IsListening)
					{
						Task<HttpListenerContext> task = listener.GetContextAsync();

						while (!(task.IsCompleted || task.IsCanceled || task.IsFaulted || token.IsCancellationRequested))
						{
							await Task.WhenAny(task, Task.Delay(Config.HTTPSERVER_TIMEOUT_MS));

							if (task.Status == TaskStatus.RanToCompletion)
							{
								Debug.WriteLine("HTTP Context: Received");
								this.ListenerCallback(task.Result);
								break;
							}
							else if (task.Status == TaskStatus.Canceled || task.Status == TaskStatus.Faulted)
							{
								Debug.WriteLine("HTTP Context: Errored");
								// Error, do nothing
							}
							else
							{
								Debug.WriteLine("HTTP Context: Timedout/Still waiting");
								// Timeout, do nothing
							}
						}
					}
				}
			}
			catch (HttpListenerException e)
			{
				// Bail out - this happens on shutdown
				Debug.WriteLine("HTTP Listener has shutdown: {0}", e.Message);
			}
			catch (TaskCanceledException)
			{
				Debug.WriteLine("HTTP Task Cancelled");
			}
			catch (Exception e)
			{
				Debug.WriteLine("HTTP Unexpected exception: {0}", e.Message);
			}
		}

#endregion

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
					if (this.DataLayer != null)
					{
						this.DataLayer.Dispose();
					}
				}
				disposed = true;
			}
		}

#endregion
	}
}
