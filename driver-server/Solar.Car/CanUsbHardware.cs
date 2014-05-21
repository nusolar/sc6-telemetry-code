//#define SIMULATE_HARDWARE
using System;
using System.IO.Ports;
using Debug = System.Diagnostics.Debug;

namespace Solar.Car
{
	/// <summary>
	/// A customized SerialPort, with support for our BUGGY CAN-USB cable.
	/// * asynchronously reads lines from a SerialPort, firing the LineReceived event.
	/// * locks the SerialPort for all reading and writing.
	/// * drains the SerialPort Read buffer when closing (WARNING this can infinite loop)
	/// </summary>
	class CanUsbHardware: ICanUsbHardware
	{
		// The underlying SerialPort & its mutex:
		private SerialPort port = null;
		readonly object port_lock = new Object();
		// The string buffer & its mutex:
		string buffer = "";
		readonly object buffer_lock = new Object();
		// NewLine character
		readonly string NewLine = "\r";

		/// <summary>
		/// Initializes a new instance of the SolarCar.SyncSerialPort class.
		/// </summary>
		public CanUsbHardware()
		{
			Debug.WriteLine("UART:\t\tpath: " + Config.CANUSB_SERIAL_DEV);
			this.port = new SerialPort(Config.CANUSB_SERIAL_DEV, 115200, Parity.None, 8, StopBits.One);
			port.NewLine = this.NewLine; // CANUSB uses carriage returns
			port.ReadTimeout = 50; // 50 ms
			port.WriteTimeout = 50; // 50 ms
			port.Handshake = Handshake.None; // no flow control
			// port.DataReceived += new SerialDataReceivedEventHandler(this.LockReadData);
			// DataReceived IS NOT IMPLEMENTED IN MONO.NET
		}

		public override void Open()
		{
#if !SIMULATE_HARDWARE
			if (Config.Platform == Config.PlatformID.Unix)
			if (!System.IO.File.Exists(Config.CANUSB_SERIAL_DEV))
				throw new System.IO.IOException("UART:\t\tOpen: Port doesn't exist");
			port.Open();
#endif
		}

		/// <summary>
		/// Releases the SerialPort and performs other cleanup operations before the SolarCar.SyncSerialPort is
		/// reclaimed by garbage collection.
		/// </summary>
		public override void Close()
		{
			this.Dispose();
		}

		public override bool IsOpen
		{
			get { return port.IsOpen; }
		}

		/// <summary>
		/// Event Handler for this.port's SerialPort.DataReceived event.
		/// Acquires the SerialPort, reads data into buffer. It is invoked on a separate thread by SerialPort.
		/// Lines terminated by the NewLine character are passed to the LineReceivedDelegate.
		/// </summary>
		public override void LockReadData()
		{
			Debug.WriteLine("UART:\t\tReadData.");

			// acquire lock on CAN bus, read buffer out.
			string temp_buffer = null;
			try
			{
				lock (port_lock)
				{
#if !SIMULATE_HARDWARE
					Debug.WriteLine("UART:\t\tReadData: BytesToRead before " + port.BytesToRead);
					temp_buffer = this.port.ReadExisting();
					Debug.WriteLine("UART:\t\tReadData: BytesToRead after " + port.BytesToRead);
#endif
					Debug.WriteLine("UART:\t\tReadData: Bytes:\n" + temp_buffer.Substring(0, Math.Min(21, temp_buffer.Length)));
					Debug.WriteLine("UART:\t\tReadData: Bytes# " + temp_buffer.Length);

					// If CANUSB has overflown, then prepend a carriage return
					if (temp_buffer.Length >= Config.CANUSB_READ_BUFFER_LIMIT)
					{
						temp_buffer = temp_buffer.Insert(0, "\r");
					}
				}
			}
			catch (TimeoutException)
			{
				Debug.WriteLine("UART:\t\tReadData: TimeoutException. SerialPort may be busy.");
				return;
			}

			if (temp_buffer == null)
				return;

			// Add data to this.buffer, check for newlines.
			lock (this.buffer_lock)
			{
				this.buffer += temp_buffer;
			}


			// set Empty initially to enter the loop;
			bool found_newline = true;
			while (found_newline == true)
			{
				string new_line = null;
				lock (this.buffer_lock)
				{
					if (this.buffer.Contains(this.NewLine))
					{
						// TODO check status

						int newline_index = this.buffer.IndexOf(this.NewLine);
						// copy the first line in the buffer.
						new_line = this.buffer.Substring(0, newline_index + 1);
						// then remove copied data from buffer.
						this.buffer = this.buffer.Substring(newline_index + 1);
					}
					else
					{
						// break loop if NewLine isn't found
						break;
					}
				}
				Debug.WriteLine("UART:\t\tReadData: NewLine: " + new_line.Substring(0, new_line.Length - 1));
				Debug.WriteLine("UART:\t\tReadData: NewLine# " + new_line.Length);

				// Handle non-empty lines, since a NewLine was found;
				if (new_line.Length > 1)
					this.RaiseLineReceived(new_line);
				new_line = String.Empty;
			}
		}

		/// <summary>
		/// Acquire the SerialPort, and write a line.
		/// </summary>
		/// <param name="line">Line.</param>
		public override void LockWriteLine(string line)
		{
			try
			{
				lock (port_lock)
				{
					Debug.WriteLine("UART:\t\tWriteLine: send " + line);
#if !SIMULATE_HARDWARE
					if (port.BytesToWrite < Config.CANUSB_WRITE_BUFFER_LIMIT)
					{
						port.WriteLine(line);
						Debug.WriteLine("UART:\t\tWriteLine: bytes to write = {0}", port.BytesToWrite);
					}
					else
					{
						Debug.WriteLine("UART:\t\tWriteLine: EXCEPTION: bytes to write = {0}", port.BytesToWrite);
						throw new System.IO.IOException("UART WriteLine: buffer is clogged.");
					}
#endif
				}
			}
			catch (TimeoutException)
			{
				Debug.WriteLine("UART:\t\tWriteLine: timed out. SerialPort may be busy.");
			}
		}

#region IDisposable implementation

		bool disposed = false;

		public override void Dispose()
		{
			Debug.WriteLine("UART:\t\tDispose: Called");

			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			Debug.WriteLine("UART:\t\tdisposing: {0}", disposed);
			if (!disposed)
			{
				// disposing unmanaged resources
				try
				{
					Debug.WriteLine("UART:\t\tclosing: draining buffer...");

					// drain Read buffer before closing
					while (port.BytesToRead > 0)
					{
						var s = port.ReadExisting();
						Debug.WriteLine("UART:\t\tclosing: cleared {0} bytes.", s.Length);
					}
					port.Close();

					Debug.WriteLine("UART:\t\tclosed");
				}
				catch (Exception e)
				{
					Debug.WriteLine("UART:\t\tdisposing: EXCEPTION: " + e.ToString());
				}
				finally
				{
					// disposing managed resources
					if (disposing)
					{
						this.port.Dispose();
					}
					disposed = true;
				}
			}
		}

#endregion
	}
}

