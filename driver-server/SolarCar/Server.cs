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

		void DoCommand(System.Collections.Specialized.NameValueCollection query) {
			foreach (string key in query) {
				switch (key) {
					case "power":
						switch (query[key]) {
							case "on":
								// turn on
								break;
							default:
								// turn off
								break;
						}
						break;

					case "drive":
						switch (query[key]) {
							case "on": 
								this.commander.Drive = true;
								break;
							default: 
								this.commander.Drive = false;
								break;
						}
						break;
				}
			}
		}

		/**
		 * Indefinitely serve HTTP, unless this.data == null.
		 */
		public void RunLoop() {
			listener.Start();
			while (commander != null) {
				HttpListenerContext context = listener.GetContext(); // Blocking
				HttpListenerRequest request = context.Request;
				Console.WriteLine(request.RawUrl);

				if (request.RawUrl == "/data.json") {
					this.SendResponse(context.Response, this.data);
				} else if (request.RawUrl == "/command.json") {
					this.DoCommand(request.QueryString);
					this.SendResponse(context.Response, "{}");
				}
			}
			listener.Stop();
		}
	}
}
