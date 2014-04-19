using System;
using System.Net;
using System.Threading.Tasks;
using CancellationToken = System.Threading.CancellationToken;
using NameValueCollection = System.Collections.Specialized.NameValueCollection;
using CarDatabase = SolarCar.CarDatabase;
using Stream = System.IO.Stream;
using Encoding = System.Text.Encoding;
using JsonConvert = Newtonsoft.Json.JsonConvert;
using SolarCar.Car;

namespace BaseStation
{
	class HttpServer
	{
		CarDatabase _db = null;

		public HttpServer(CarDatabase InDb)
		{
			this._db = InDb;
		}

		void ListenerCallback(HttpListenerContext context)
		{
			HttpListenerRequest request = context.Request;
			HttpListenerResponse response = context.Response;
			string url = request.Url.AbsolutePath;
#if DEBUG
			Console.WriteLine("HTTP URL: " + request.RawUrl);
#endif

			if (url == "/telemetry" && context.Request.HttpMethod == "POST")
			{
				byte[] buffer = new byte[request.ContentLength64];
				using (Stream input = request.InputStream)
					input.Read(buffer, 0, buffer.Length);
				string decoded = Encoding.Default.GetString(buffer);
				Status status = JsonConvert.DeserializeObject<Status>(decoded);
				// this._db.PushStatus(status);
#if DEBUG
				Console.WriteLine("HTTP telemetry: " + decoded);
#endif

				buffer = Encoding.Default.GetBytes("{Response = true}\n");
				response.StatusCode = (int)HttpStatusCode.OK;
				response.StatusDescription = "OK";
				response.ContentLength64 = buffer.LongLength;
				response.ContentEncoding = Encoding.Default;
				using (Stream output = response.OutputStream)
					output.Write(buffer, 0, buffer.Length);
				response.Close();
			}
		}

		public async Task ReceiveLoop(object obj)
		{
			CancellationToken token = (CancellationToken)obj;

			try
			{
				using (HttpListener listener = new HttpListener())
				{
					listener.Prefixes.Add(Config.HTTPSERVER_SERVER_PREFIX);
					listener.Start();

					while (!token.IsCancellationRequested && listener.IsListening)
					{
						Task<HttpListenerContext> task = listener.GetContextAsync();

						while (!task.IsCompleted && !task.IsCanceled && !task.IsFaulted)
						{
							await Task.Delay(Config.HTTPSERVER_TIMEOUT_MS, token);
#if DEBUG
							Console.WriteLine("HTTP timed out: " + (task.Status != TaskStatus.RanToCompletion));
#endif
							if (task.Status == TaskStatus.RanToCompletion)
							{
								this.ListenerCallback(task.Result);
								break;
							}
						}
					}
				}
			}
			catch (HttpListenerException)
			{
				// Bail out - this happens on shutdown
				return;
			}
			catch (TaskCanceledException)
			{
				Console.WriteLine("HTTP Task Cancelled");
			}
			catch (Exception e)
			{
				Console.WriteLine("HTTP Unexpected exception: {0}", e.Message);
			}
		}
	}
}

