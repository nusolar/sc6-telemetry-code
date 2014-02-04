using System;
using System.Net;
using Newtonsoft.Json;

namespace SolarCar
{
	class HttpServer
	{
		HttpListener listener = new HttpListener();
		readonly DataAggregator db = null;

		public string data { get { return JsonConvert.SerializeObject(db.Status); } }

		public HttpServer(DataAggregator InDb)
		{
			db = InDb;
			listener.Prefixes.Add(Config.HTTPSERVER_PREFIX);
		}

		/**
		 * Respond with current JSON data.
		 */
		void SendResponse(HttpListenerResponse response, string message)
		{
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);
			response.ContentLength64 = buffer.Length;

			using (System.IO.Stream output = response.OutputStream)
			{
				output.Write(buffer, 0, buffer.Length);
			}
		}

		/// <summary>
		/// Executes commands from Query parameters. This query API must match the CoffeeScript code.
		/// </summary>
		/// <param name="query">Query.</param>
		void DoCommand(System.Collections.Specialized.NameValueCollection query)
		{
			UserInput input = new UserInput();

			// Do the power & array Mode flags first
			if (query["power"] == "1")
			{
				input.mode |= Car.Mode.Discharging;
			}

			// Gear flags
			if (query["drive"] == "1")
			{
				input.gear |= Car.Gear.Drive;
				// NOTICE drive influences BOTH Mode AND Gear flags.
				// However, Mode.Drive is never enabled without Mode.Discharging for safety reasons,
				// and in fact the BPS considers Drive without Discharging an error!
				if (input.mode.HasFlag(Car.Mode.Discharging))
				{
					input.mode |= Car.Mode.Drive;
				}
			}
			if (query["reverse"] == "1")
			{
				input.gear |= Car.Gear.Reverse;
			}

			// Signal flags
			int sigs = 0;
			Int32.TryParse(query["signals"], out sigs);
			input.sigs = (Car.Signals)sigs;
			if (query["headlights"] == "1")
			{
				input.sigs |= Car.Signals.Headlights;
			}
			if (query["horn"] == "1")
			{
				input.sigs |= Car.Signals.Horn;
			}

			db.HandleUserInput(input);
		}

		/**
		 * Indefinitely serve HTTP, unless this.commander == null.
		 */
		public void RunLoop()
		{
			listener.Start();
			while (true)
			{
				HttpListenerContext context = listener.GetContext(); // Blocks until HTTP request
				HttpListenerRequest request = context.Request;

#if DEBUG
				Console.WriteLine("URL: " + request.RawUrl);
#endif

				string url = request.RawUrl;
				int query_index = url.IndexOf('?');
				if (query_index > -1)
				{
					url = url.Substring(0, query_index);
				}

				if (url == "/data.json")
				{
					this.DoCommand(request.QueryString);

					string callback = request.QueryString["callback"];
					if (callback == String.Empty)
					{
						callback = "callback";
					}
					string jsonp = String.Format("{0}({1})", callback, this.data);
					context.Response.ContentType = "application/json";

					this.SendResponse(context.Response, jsonp);
				}
				else if (url == "/favicon.ico")
				{
					// TODO favicon
					context.Response.Abort();
				}
				else
				{
					context.Response.Abort();
				}
			}
//			listener.Stop();
		}
	}
}
