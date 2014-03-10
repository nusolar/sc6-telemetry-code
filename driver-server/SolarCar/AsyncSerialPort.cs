#define SIMULATE_HARDWARE
using System;
using System.IO.Ports;

namespace SolarCar
{
	/// <summary>
	/// Wrapper to send to and receive packets from NUSolar Serial devices.
	/// </summary>
	class AsyncSerialPort
	{
		// The underlying SerialPort & its mutex:
		readonly object port_lock = new Object();
		readonly SerialPort port = new SerialPort();
		// The string buffer & its mutex:
		readonly object buffer_lock = new Object();
		string buffer = "";

		public delegate void LineReceivedDelegate(string line);

		public event LineReceivedDelegate LineReceived;

		/// <summary>
		/// Initializes a new instance of the SolarCar.SyncSerialPort class.
		/// </summary>
		/// <param name="name">Serial Port's name</param>
		protected AsyncSerialPort(string name)
		{
			port.PortName = name;
			port.BaudRate = 115200;
			port.ReadTimeout = 50; // 50 ms
			port.WriteTimeout = 50; // 50 ms
			port.DataReceived += new SerialDataReceivedEventHandler(this.ReadData);
#if !SIMULATE_HARDWARE
			port.Open();
#endif
		}

		/// <summary>
		/// Releases the SerialPort and performs other cleanup operations before the SolarCar.SyncSerialPort is
		/// reclaimed by garbage collection.
		/// </summary>
		~AsyncSerialPort()
		{
#if !SIMULATE_HARDWARE
			port.Close();
#endif
		}

		protected string NewLine
		{
			get { return port.NewLine; }
			set { port.NewLine = value; }
		}

		/// <summary>
		/// Private Event Handler for this.port's SerialPort.DataReceived event.
		/// Acquires the SerialPort, reads data into buffer. It is invoked on a separate thread by SerialPort.
		/// Lines terminated by the NewLine character are passed to the LineReceivedDelegate.
		/// </summary>
		/// <param name="sender">Sender, a SerialPort.</param>
		/// <param name="e">Event Args</param>
		void ReadData(object sender, SerialDataReceivedEventArgs e)
		{
			if (e.EventType != System.IO.Ports.SerialData.Chars)
			{
				return;
			}
			if ((SerialPort)sender != this.port)
			{
				Console.WriteLine("UART Warning: ReadData was called with sender != this.port");
			}

			// acquire lock on CAN bus, read upto 21 bytes.
			string temp_buffer = null;
			try
			{
				lock (port_lock)
				{
					temp_buffer = this.port.ReadExisting();
				}
			}
			catch (TimeoutException)
			{
				Console.WriteLine("UART: read timed out. SerialPort may be busy.");
				return;
			}

			// Add data to this.buffer, check for newlines.
			string new_line = null;
			lock (buffer_lock)
			{
				this.buffer += temp_buffer;
				if (this.buffer.Contains(this.NewLine))
				{
					int newline_index = this.buffer.IndexOf(this.NewLine);
					// copy the first line in the buffer.
					new_line = this.buffer.Substring(0, newline_index + 1);
					// then remove copied data from buffer.
					this.buffer = this.buffer.Substring(newline_index + 1);
				}
			}

			// Handle newline, if exists.
			if (new_line != null)
			{
#if DEBUG
				Console.WriteLine("UART recv: " + new_line);
#endif
				this.LineReceived(new_line);
			}
		}

		/// <summary>
		/// Acquire the SerialPort, and write a line.
		/// </summary>
		/// <param name="line">Line.</param>
		public void SyncWriteLine(string line)
		{
			try
			{
				lock (port_lock)
				{
#if !SIMULATE_HARDWARE
					port.WriteLine(line);
#endif
#if DEBUG
					Console.WriteLine("UART send: " + line);
#endif
				}
			}
			catch (TimeoutException)
			{
				Console.WriteLine("UART: write timed out. SerialPort may be busy.");
			}
		}
	}
}

