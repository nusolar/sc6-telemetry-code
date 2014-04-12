//#define SIMULATE_HARDWARE
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
		private SerialPort port = null;
		// The string buffer & its mutex:
		readonly object buffer_lock = new Object();
		string buffer = "";

		public delegate void LineReceivedDelegate(string line);

		public event LineReceivedDelegate LineReceived;

		/// <summary>
		/// Initializes a new instance of the SolarCar.SyncSerialPort class.
		/// </summary>
		public AsyncSerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
		{
			this.port = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
			port.ReadTimeout = 50; // 50 ms
			port.WriteTimeout = 50; // 50 ms
			port.DataReceived += new SerialDataReceivedEventHandler(this.ReadData);
		}

		/// <summary>
		/// Releases the SerialPort and performs other cleanup operations before the SolarCar.SyncSerialPort is
		/// reclaimed by garbage collection.
		/// </summary>
		~AsyncSerialPort()
		{
#if !SIMULATE_HARDWARE
			this.Close();
#endif
		}

		public void Open()
		{
#if !SIMULATE_HARDWARE
			port.Open();
#endif
		}

		public void Close()
		{
			Console.WriteLine("UART : closing");
			if (port.IsOpen)
			{
				port.Close();
			}
			Console.WriteLine("UART : closed");
		}

		public string NewLine
		{
			get { return port.NewLine; }
			set { port.NewLine = value; }
		}

		/// <summary>
		/// Event Handler for this.port's SerialPort.DataReceived event.
		/// Acquires the SerialPort, reads data into buffer. It is invoked on a separate thread by SerialPort.
		/// Lines terminated by the NewLine character are passed to the LineReceivedDelegate.
		/// </summary>
		/// <param name="sender">Sender, a SerialPort.</param>
		/// <param name="e">Event Args</param>
		void ReadData(object sender, SerialDataReceivedEventArgs e)
		{
#if DEBUG
			Console.WriteLine("UART data received.");
#endif
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
#if DEBUG
					Console.WriteLine("UART data: " + temp_buffer);
#endif
					if (temp_buffer.Length >= 1020)
					{
						temp_buffer = temp_buffer.Insert(0, "\r");
					}
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

