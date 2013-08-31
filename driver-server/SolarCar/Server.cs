using System;
using System.Net;

namespace SolarCar {
	class HttpServer {
		HttpListener listener = new HttpListener();
		public String data = "{}";

		public HttpServer() {
			listener.Prefixes.Add("http://+:8080/");
		}

		/**
		 * Respond with current JSON data.
		 */
		void SendData(HttpListenerResponse response) {
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(data);
			response.ContentLength64 = buffer.Length;

			using (System.IO.Stream output = response.OutputStream) {
				output.Write(buffer, 0, buffer.Length);
			}
		}

		/**
		 * Indefinitely serve HTTP, unless this.data == null.
		 */
		public void Serve() {
			listener.Start();
			while (data != null) {
				HttpListenerContext context = listener.GetContext(); // Blocking
				HttpListenerRequest request = context.Request;
				Console.WriteLine(request.RawUrl);

				if (request.RawUrl == "/data.json") {
					this.SendData(context.Response);
				}
			}
			listener.Stop();
		}
	}
}
