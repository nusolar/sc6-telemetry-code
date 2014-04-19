using System;
using System.Threading;
using System.Threading.Tasks;

namespace BaseStation
{
	class Config
	{
		public const string HTTPSERVER_SERVER_PREFIX = "http://+:8081/";
		public const int HTTPSERVER_TIMEOUT_MS = 1000;
	}

	class Program
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");

			SolarCar.CarDatabase db = new SolarCar.CarDatabase();
			BaseStation.HttpServer web = new BaseStation.HttpServer(db);

			var tokenSource = new CancellationTokenSource();
			var token = tokenSource.Token;
			Task web_loop = web.ReceiveLoop(token);

			Console.ReadKey();
			tokenSource.Cancel();
			Console.WriteLine("Aborted!");
			web_loop.Wait();
		}
	}
}
