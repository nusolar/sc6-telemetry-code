using System;
using System.Threading;
using System.Threading.Tasks;
using Debug = System.Diagnostics.Debug;

namespace Solar.Laptop
{
	class Program
	{
		public static void RunLaptop()
		{
			Solar.Car.Database db = new Solar.Car.Database();
			Solar.Laptop.HttpServer web = new Solar.Laptop.HttpServer(db);

			var tokenSource = new CancellationTokenSource();
			var token = tokenSource.Token;
			Task web_loop = web.ReceiveLoop(token);

			Console.ReadKey();
			tokenSource.Cancel();
			Debug.WriteLine("PROGRAM: Aborted!");
			web_loop.Wait();
		}

		public static void Main(string[] args)
		{
			Debug.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(System.Console.Out));
			Debug.WriteLine("PROGRAM: Hello World!");
			RunLaptop();
		}
	}
}
