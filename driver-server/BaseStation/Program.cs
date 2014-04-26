using System;
using System.Threading;
using System.Threading.Tasks;

namespace BaseStation
{
	class Program
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");

			SolarCar.Database db = new SolarCar.Database();
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
