using System;
using System.IO;
using System.Net;
using MediaTypeNames = System.Net.Mime.MediaTypeNames;
using Encoding = System.Text.Encoding;
using NameValueCollection = System.Collections.Specialized.NameValueCollection;
using JsonConvert = Newtonsoft.Json.JsonConvert;
using Assembly = System.Reflection.Assembly;

namespace SolarCar
{
	class HttpServer
	{
		HttpListener listener = new HttpListener();
		readonly DataAggregator db = null;
		NameValueCollection default_query = new NameValueCollection();

		public string json_data { get { return JsonConvert.SerializeObject(db.Status); } }

		public HttpServer(DataAggregator InDb)
		{
			db = InDb;
			default_query["power"] = "0";
			default_query["drive"] = "0";
			default_query["reverse"] = "0";
			default_query["signals"] = "0";
			default_query["headlights"] = "0";
			default_query["horn"] = "0";
		}

		/**
		 * Respond with current JSON data.
		 */
		void SendResponse(HttpListenerResponse response, string message)
		{
			byte[] buffer = Encoding.UTF8.GetBytes(message);
			response.ContentLength64 = buffer.Length;

			using (Stream output = response.OutputStream)
			{
				output.Write(buffer, 0, buffer.Length);
			}
		}

		/// <summary>
		/// Executes commands from Query parameters. This query API must match the CoffeeScript code.
		/// </summary>
		/// <param name="query">Query.</param>
		void DoCommands(NameValueCollection query)
		{
			UserInput input = new UserInput();
			// Gear flags
			int gear = 0;
			Int32.TryParse(query["gear"], out gear);
			input.gear = (Car.Gear)gear;

			// Signal flags
			int sigs = 0;
			Int32.TryParse(query["signals"], out sigs);
			input.sigs = (Car.Signals)sigs;

			db.HandleUserInput(input);
		}

		public void ListenerCallback(IAsyncResult result)
		{
			HttpListener listener = (HttpListener)result.AsyncState;
			HttpListenerContext context = listener.EndGetContext(result); // Blocks until HTTP request
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
				response.ContentType = "application/json";
#if DEBUG
				Console.WriteLine("HTTP json: " + this.json_data);
#endif
				this.SendResponse(response, this.json_data);
			}
			else // e.g. url == "/index.html"
			{
				url = url.Replace('/', '.');
				Assembly _assembly = Assembly.GetExecutingAssembly();
				using (Stream _stream = _assembly.GetManifestResourceStream("SolarCar." + Config.GUI_SUBDIR + url))
				{
					response.ContentLength64 = _stream.Length;
					response.SendChunked = false;
//					response.ContentType = MediaTypeNames.Text.Html;
					response.StatusCode = (int)HttpStatusCode.OK;
					response.StatusDescription = "OK";

					// transfer file in buffered 64kB blocks
					byte[] buffer = new byte[64 * 1024];
					int read;
					using (BinaryWriter bw = new BinaryWriter(response.OutputStream))
					{
						while ((read = _stream.Read(buffer, 0, buffer.Length)) > 0)
						{
							bw.Write(buffer, 0, read);
							bw.Flush(); //seems to have no effect
						}

						bw.Close();
					}

					response.OutputStream.Close();
				}
				this.DoCommands(this.default_query);
			}
		}

		/**
		 * Indefinitely serve HTTP.
		 */
		public void RunLoop()
		{
			try
			{
				listener.Prefixes.Add(Config.HTTPSERVER_PREFIX);
				listener.Start();

				while (listener.IsListening)
				{
					IAsyncResult result = listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
					bool received_http = result.AsyncWaitHandle.WaitOne(Config.HTTP_TIMEOUT_MS);
#if DEBUG
					Console.WriteLine("HTTP timed out: " + !received_http);
#endif
					if (!received_http)
					{
						this.DoCommands(this.default_query);
					}
				}
			}
			catch (HttpListenerException)
			{
				// Bail out - this happens on shutdown
				return;
			}
			catch (Exception e)
			{
				Console.WriteLine("HTTP Unexpected exception: {0}", e.Message);
			}
		}
	}
}
