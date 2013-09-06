using System;
using System.Net;
using Newtonsoft.Json;

namespace SolarCar {
	class HttpServer {
		HttpListener listener = new HttpListener();
		readonly UserCommands commander = null;

		public string data { get { return JsonConvert.SerializeObject(commander.Report); } }

		public HttpServer(UserCommands cmd) {
			commander = cmd;
			listener.Prefixes.Add("http://+:8080/");
		}

		/**
		 * Respond with current JSON data.
		 */
		void SendResponse(HttpListenerResponse response, string message) {
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);
			response.ContentLength64 = buffer.Length;

			using (System.IO.Stream output = response.OutputStream) {
				output.Write(buffer, 0, buffer.Length);
			}
		}

		/// <summary>
		/// Executes commands from Query parameters. This query API must match the CoffeeScript code.
		/// </summary>
		/// <param name="query">Query.</param>
		void DoCommand(System.Collections.Specialized.NameValueCollection query) {
			switch (query["power"]) {
				case "1":
					// TODO implement User power command
					break;
				default:
					// turn off
					break;
			}

			switch (query["drive"]) {
				case "1": 
					this.commander.Drive = true;
					break;
				default: 
					this.commander.Drive = false;
					break;
			}

			switch (query["signals"]) {
				case "1": 
					this.commander.LeftSignal = true;
					break;
				case "2":
					this.commander.RightSignal = true;
					break;
				case "3":
					this.commander.Hazards = true;
					break;
				default: 
					this.commander.LeftSignal = false;
					break;
			}

			switch (query["horn"]) {
				case "1": 
					this.commander.Horn = true;
					break;
				default: 
					this.commander.Horn = false;
					break;
			}

			switch (query["headlights"]) {
				case "1": 
					this.commander.HeadLights = true;
					break;
				default: 
					this.commander.HeadLights = false;
					break;
			}
		}

		/**
		 * Indefinitely serve HTTP, unless this.commander == null.
		 */
		public void RunLoop() {
			listener.Start();
			while (true) {
				HttpListenerContext context = listener.GetContext(); // Blocking
				HttpListenerRequest request = context.Request;

#if DEBUG
				Console.WriteLine("URL: " + request.RawUrl);
#endif

				string url = request.RawUrl;
				int query_index = url.IndexOf('?');
				if (query_index > -1) {
					url = url.Substring(0, query_index);
				}

				if (url == "/data.json") {
					this.DoCommand(request.QueryString);
					string callback = request.QueryString["callback"];
					if (callback == String.Empty) {
						callback = "callback";
					}
					string jsonp = String.Format("{0}({1})", callback, this.data);
					context.Response.ContentType = "application/json";
					this.SendResponse(context.Response, jsonp);
				} else if (url == "/favicon.ico") {
					// TODO favicon
					context.Response.Abort();
				} else {
					context.Response.Abort();
				}
			}
//			listener.Stop();
		}
	}
}
