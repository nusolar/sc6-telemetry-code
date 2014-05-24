using System;
using System.IO;
using System.Net;
using Encoding = System.Text.Encoding;
using NameValueCollection = System.Collections.Specialized.NameValueCollection;
using JsonConvert = Newtonsoft.Json.JsonConvert;
using Assembly = System.Reflection.Assembly;
using System.Threading;
using System.Threading.Tasks;
using Debug = System.Diagnostics.Debug;

namespace Solar.Car
{
	/// <summary>
	/// Application-Layer connecting the Web GUI with the Business-Layer Comm.
	/// </summary>
	public class HttpGui: IAppLayer
	{
		/// <summary>
		/// Injected by App
		/// </summary>
		/// <value>The Data/Hardware manager.</value>
		public IBusinessLayer Manager { get; set; }

		public string json_data { get { return JsonConvert.SerializeObject(this.Manager.Status); } }

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
			this.Manager.HandleUserInput((Solar.Gear)gear, (Solar.Signals)sigs);
		}

		public void ListenerCallback(object result)
		{
			try
			{
				// HttpListener listener = (HttpListener)result.AsyncState;
				// context = listener.EndGetContext(result); // Blocks until HTTP request
				HttpListenerContext context = (HttpListenerContext)result;
				HttpListenerRequest request = context.Request;
				HttpListenerResponse response = context.Response;
				string url = request.Url.AbsolutePath;

				Debug.WriteLine("HTTP:\t\tURL: " + request.RawUrl);

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

					Debug.WriteLine("HTTP:\t\tListenerCallback: json: " + this.json_data);
				}
//				else if (url == "/shutdown")
//				{
//					Program.Shutdown();
//					context.Response.StatusCode = (int)HttpStatusCode.OK;
//					context.Response.Close();
//				}
				else // e.g. url == "/index.html"
				{
					try
					{
						using (Stream _stream = File.OpenRead(Config.Resource_Prefix + Config.HTTPSERVER_GUI_SUBDIR + url))
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
					}
					catch (FileNotFoundException e)
					{
						Debug.WriteLine("HTTP:\t\tListenerCallback: FileNotFound: " + e.FileName);
					}
				}
			}
			catch (NullReferenceException e)
			{
				Debug.WriteLine("HTTP:\t\tListenerCallback: NullReferenceException: " + e.TargetSite);
			}
			catch (Exception e)
			{
				Debug.WriteLine("HTTP:\t\tListenerCallback: EXCEPTION: " + e.ToString());
			}
		}

		/// <summary>
		/// Indefinitely serve the Car's HTTP GUI.
		/// </summary>
		/// <param name="obj">A Task cancellation token.</param>
		public async Task HttpReceiveLoop(CancellationToken token)
		{
			try
			{
				using (HttpListener listener = new HttpListener())
				{
					listener.Prefixes.Add(Config.HTTPSERVER_CAR_PREFIX);
					listener.Start();

					while (!token.IsCancellationRequested && listener.IsListening)
					{
						Task<HttpListenerContext> task = listener.GetContextAsync();
						// IAsyncResult result = listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
						// bool received_http = result.AsyncWaitHandle.WaitOne(Config.HTTP_TIMEOUT_MS);

						while (!(task.IsCompleted || task.IsCanceled || task.IsFaulted || token.IsCancellationRequested))
						{
							await Task.WhenAny(task, Task.Delay(Config.HTTPSERVER_TIMEOUT_MS));

							if (token.IsCancellationRequested)
								break;
							if (task.Status == TaskStatus.RanToCompletion)
							{
								Debug.WriteLine("HTTP:\t\tContext: Received");
								this.ListenerCallback(task.Result);
								break;
							}
							else if (task.Status == TaskStatus.Canceled || task.Status == TaskStatus.Faulted)
							{
								Debug.WriteLine("HTTP:\t\tContext: Errored");
							}
							else
							{
								Debug.WriteLine("HTTP:\t\tContext: Timedout/Still waiting");
							}
						}
					}
				}
			}
			catch (HttpListenerException e)
			{
				// Bail out - this happens on shutdown
				Debug.WriteLine("HTTP:\t\tListener has shutdown: {0}", e.Message);
			}
			catch (TaskCanceledException e)
			{
				Debug.WriteLine("HTTP:\t\tTask Cancelled: {0}", e.Message);
			}
			catch (Exception e)
			{
				Debug.WriteLine("HTTP:\t\tUnexpected exception: {0}", e.Message);
			}
		}

		public async Task AppLayerLoop(CancellationToken token)
		{
			// open GUI
			Task http_loop = this.HttpReceiveLoop(token);
			await http_loop;
		}
	}
}
