using System;
using System.IO;
using System.Net;
using MediaTypeNames = System.Net.Mime.MediaTypeNames;
using Encoding = System.Text.Encoding;
using NameValueCollection = System.Collections.Specialized.NameValueCollection;
using JsonConvert = Newtonsoft.Json.JsonConvert;
using Assembly = System.Reflection.Assembly;
using System.Threading;
using System.Threading.Tasks;

namespace SolarCar
{
	class HttpServer
	{
		readonly NameValueCollection default_query = new NameValueCollection { { "gear", "0" }, { "signals", "0" } };
		readonly CarFrontend data = null;

		public string json_data { get { return JsonConvert.SerializeObject(data.Status); } }

		public HttpServer(CarFrontend InDb)
		{
			data = InDb;
		}

		/**
		 * Respond with current JSON data.
		 */
		void SendResponse(HttpListenerResponse response, string message)
		{
			byte[] buffer = Encoding.UTF8.GetBytes(message);
			response.ContentLength64 = buffer.Length;
			using (Stream output = response.OutputStream)
				output.Write(buffer, 0, buffer.Length);
		}

		/// <summary>
		/// Executes commands from Query parameters. This query API must match the CoffeeScript code.
		/// </summary>
		/// <param name="query">Query.</param>
		void DoCommands(NameValueCollection query)
		{
			// Gear flags and Signal flags
			int gear = 0, sigs = 0;
			Int32.TryParse(query["gear"], out gear);
			Int32.TryParse(query["signals"], out sigs);
			this.data.HandleUserInput((Car.Gear)gear, (Car.Signals)sigs);
		}

		public void ListenerCallback(object result)
		{
			// HttpListener listener = (HttpListener)result.AsyncState;
			// context = listener.EndGetContext(result); // Blocks until HTTP request
			HttpListenerContext context = (HttpListenerContext)result;
			HttpListenerRequest request = context.Request;
			HttpListenerResponse response = context.Response;
			string url = request.Url.AbsolutePath;
#if DEBUG
			Console.WriteLine("HTTP URL: " + request.RawUrl);
#endif

			if (context.Request.HttpMethod != "GET")
			{
				context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				context.Response.Close();
			}

			if (url == "/data.json")
			{
				this.DoCommands(request.QueryString);

				byte[] buffer = Encoding.Default.GetBytes(this.json_data);
				response.ContentLength64 = buffer.Length;
				response.ContentType = "application/json";

				using (Stream output = response.OutputStream)
					output.Write(buffer, 0, buffer.Length);
#if DEBUG
				Console.WriteLine("HTTP json: " + this.json_data);
#endif
			}
			else // e.g. url == "/index.html"
			{
				url = url.Replace('/', '.');
				Assembly _assembly = Assembly.GetExecutingAssembly();
				using (Stream _stream = _assembly.GetManifestResourceStream("SolarCar." + Config.HTTPSERVER_GUI_SUBDIR + url))
				{
					response.ContentLength64 = _stream.Length;
					response.SendChunked = false;
					// response.ContentType = MediaTypeNames.Text.Html;
					response.StatusCode = (int)HttpStatusCode.OK;
					response.StatusDescription = "OK";

					using (Stream output = response.OutputStream)
					{
						// transfer file in buffered 64kB blocks
						byte[] buffer = new byte[64 * 1024];
						int read;
						using (BinaryWriter bw = new BinaryWriter(output))
						{
							while ((read = _stream.Read(buffer, 0, buffer.Length)) > 0)
							{
								bw.Write(buffer, 0, read);
								bw.Flush(); //seems to have no effect
							}

							bw.Close();
						}
					}
				}
				this.DoCommands(this.default_query);
			}
		}

		/// <summary>
		/// Indefinitely serve the Car's HTTP GUI.
		/// </summary>
		/// <param name="obj">A Task cancellation token.</param>
		public async Task ReceiveLoop(object obj)
		{
			CancellationToken token = (CancellationToken)obj;

			try
			{
				using (HttpListener listener = new HttpListener())
				{
					listener.Prefixes.Add(Config.HTTPSERVER_CAR_PREFIX);
					listener.Start();

					while (!token.IsCancellationRequested)
					{
						Task<HttpListenerContext> task = listener.GetContextAsync();
						// IAsyncResult result = listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
						// bool received_http = result.AsyncWaitHandle.WaitOne(Config.HTTP_TIMEOUT_MS);

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
							else
							{
								this.DoCommands(this.default_query);
							}
						}
					}
				}
			}
			catch (HttpListenerException)
			{
				// Bail out - this happens on shutdown
				Console.WriteLine("HTTP Listener has shutdown");
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
