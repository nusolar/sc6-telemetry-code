using System;
using System.Collections.Generic;
using Debug = System.Diagnostics.Debug;

namespace Solar.Car
{
	abstract class ICanUsbHardware: IDisposable
	{
		public delegate void LineReceivedDelegate(string line);

		public event LineReceivedDelegate LineReceived;

		public void RaiseLineReceived(string line)
		{
			this.LineReceived(line);
		}

		public abstract void Open();

		public abstract void Close();

		public abstract bool IsOpen { get; }

		public abstract void LockReadData();

		public abstract void LockWriteLine(string line);

		public abstract void Dispose();
	}

	/// <summary>
	/// Communication with the CAN-USB cable, which is a serial port.
	/// Issues: CAN-USB 
	/// </summary>
	public class CanUsbWrapper: IDisposable
	{
		public delegate void CanHandlerDelegate(Can.Packet p);

		public event CanHandlerDelegate handlers;

		ICanUsbHardware port = null;
		bool is_open = false;

		public CanUsbWrapper()
		{
			port = new CanUsbHardware(); // (ICanUsbHardware)Activator.CreateInstance(Config.CanUsbHardwareType);
			port.LineReceived += this.HandleLineReceived;
		}

		public void Open()
		{
			port.Open();
			port.LockWriteLine("\r\r"); // clear CANUSB buffer
			port.LockWriteLine("S8"); // set bitrate = 1 Mbit/s
			port.LockWriteLine("O"); // open CAN channel
			this.is_open = true;
		}

		public void Close()
		{
			this.is_open = false;
			this.Dispose();
		}

		public bool IsOpen
		{
			get { return this.port.IsOpen && this.is_open; }
		}

		public void PollPackets()
		{
			this.port.LockReadData();
		}

		/// <summary>
		/// Writes a CAN packet with SyncWriteLine.
		/// </summary>
		public void TransmitPacket(Can.Packet packet)
		{
			string hex_id = packet.ID.ToString("X3");
			string hex_length = packet.Length.ToString("X1");
			byte[] bytes = BitConverter.GetBytes(packet.Data);
			string hex_data = BitConverter.ToString(bytes).Replace("-", String.Empty);
			this.port.LockWriteLine("t" + hex_id + hex_length + hex_data);
		}

		/// <summary>
		/// Interpret line as CAN packet, update corresponding Report.
		/// Character layout of a CANUSB line, from datasheet: tiiildd...[CR]
		/// </summary>
		void HandleLineReceived(string InLine)
		{
			// Validate packet
			if (InLine[0] == 'z')
			{
				Debug.WriteLine("CANBUS Line: [z] packet written");
			}
			if (InLine[0] != 't')
			{
				Debug.WriteLine("CANBUS Line: Expected first character to be t");
			}
			else if (InLine[InLine.Length - 1] != '\r')
			{
				Debug.WriteLine("CANBUS Line: Expected last character to be Carriage Return");
			}
			else if (InLine.Length > 22)
			{
				// 1 't', 3 ID, 1 length, upto 16 data, 1 CR
				Debug.WriteLine("CANBUS Line: Max packet size is 22 characters.");
			}
			else
			{
				// trim Carriage Return
				InLine = InLine.Substring(0, InLine.Length - 1);
				Can.Packet packet = null;

				// Parse CAN packet
				try
				{
					UInt16 id = Convert.ToUInt16(InLine.Substring(1, 3), 16); // max ID == 0x7FF, and "7FF" is three characters
					Byte length = Convert.ToByte(InLine.Substring(4, 1), 16); // length is 0-8

					// Check bounds
					if (id > 0x7FF)
					{
						throw new ArgumentOutOfRangeException("CAN Packet ID is > 0x7FF.");
					}
					if (length > 8)
					{
						throw new ArgumentOutOfRangeException("CAN Packet Length is > 8.");
					}

					// We have a string of hex pairs representing little-endian data.
					// Create a little-endian byte array, then BitConvert to UInt64
					byte[] bytes = new byte[8];
					for (int i = 0; i < length; i++)
					{
						bytes[i] = Convert.ToByte(InLine.Substring(5 + 2 * i, 2), 16);
					}
					UInt64 data = BitConverter.ToUInt64(bytes, 0);
					packet = new Can.Packet(id, length, data);

				}
				catch (Exception e)
				{
					Debug.WriteLine("CANUSB LINE: EXCEPTION:  " + e.Message);
					Debug.WriteLine("CANUSB LINE: for packet: " + InLine);
					return;
				}
				this.handlers(packet);
			}
		}

#region IDisposable implementation

		bool disposed = false;

		public void Dispose()
		{
			Debug.WriteLine("CANUSB Dispose(): called");

			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			Debug.WriteLine("CANUSB disposing: {0}", disposed);
			if (!disposed)
			{
				// dispose unmanaged resources
				try
				{
					if (this.port != null)
					{
						if (this.port.IsOpen)
						{
							port.LockWriteLine("C");
						}
						this.port.Close(); // WARNING: MANAGED!
					}
				}
				catch (Exception)
				{
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

