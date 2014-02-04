using System;
using System.Collections.Generic;

namespace SolarCar
{
	class CanHandler
	{
		public delegate void CallbackDelegate(Can.Packet p);

		public UInt16 Address;
		public UInt16 Bitmask;
		public CallbackDelegate Callback;

		public CanHandler(UInt16 InAddress, UInt16 InBitmask, CallbackDelegate InCallback)
		{
			this.Address = InAddress;
			this.Bitmask = InBitmask;
			this.Callback = InCallback;
		}
	}

	/// <summary>
	/// A CAN-USB wrapper. When running, constantly updates its MotorReport.
	/// </summary>
	class CanUsb: AsyncSerialPort
	{
		const string NEWLINE = "\r";
		public List<CanHandler> handlers = new List<CanHandler>();

		public CanUsb(string path) : base(path)
		{
			this.NewLine = NEWLINE; // CANUSB uses carriage returns
			this.LineReceived += new LineReceivedDelegate(this.HandleLine);
			this.SyncWriteLine("S8"); // set bitrate = 1 Mbit/s
			this.SyncWriteLine("O"); // open CAN channel
		}

		~CanUsb()
		{
			this.SyncWriteLine("C");
		}

		public void TransmitPacket(Can.Packet packet)
		{
			string hex_id = packet.ID.ToString("X4").Substring(1, 3);
			string hex_length = packet.Length.ToString("X");
			byte[] bytes = BitConverter.GetBytes(packet.Data);
			string hex_data = BitConverter.ToString(bytes).Replace("-", String.Empty);
			this.SyncWriteLine("t" + hex_id + hex_length + hex_data);
		}

		/// <summary>
		/// Sends a drive cmd, with SyncWriteLine.
		/// </summary>
		/// <param name="motor_velocity">Motor_velocity.</param>
		/// <param name="motor_current">Motor_current.</param>
		public void SendDriveCmd(float motor_velocity, float motor_current)
		{
			byte[] vel_bytes = BitConverter.GetBytes(motor_velocity);
			byte[] cur_bytes = BitConverter.GetBytes(motor_current);
			// WARNING: Endianness of output HEX characters!
			string vel_hex = BitConverter.ToString(vel_bytes).Replace("-", String.Empty);
			string cur_hex = BitConverter.ToString(cur_bytes).Replace("-", String.Empty);
			// write packet in CAN-USB format:
			string packet = "t4038" + vel_hex + cur_hex;
			this.SyncWriteLine(packet);
		}

		/// <summary>
		/// Interpret line as CAN packet, update corresponding Report.
		/// Character layout of a CANUSB line, from datasheet: tiiildd...[CR]
		/// </summary>
		void HandleLine(string InLine)
		{
			// If bad packet, throw away.
			if (InLine[0] != 't')
			{
				Console.WriteLine("CANBUS: Expected first character to be t");
			}
			else if (InLine[InLine.Length - 1] != '\r')
			{
				Console.WriteLine("CANBUS: Expected last character to be Carriage Return");
			}
			else if (InLine.Length > 21)
			{
				// 1 't', 3 ID, 1 length, upto 16 data
				Console.WriteLine("CANBUS: Max packet size is 22 characters.");
			}
			else
			{
				// parse data from CAN packet
				UInt16 id = Convert.ToUInt16(InLine.Substring(1, 3), 16); // max ID == 0x7FF, and "7FF" is three characters
				Byte length = Convert.ToByte(InLine.Substring(4, 1), 16); // length is 0-8

				// We have a string of hex pairs repr'ing little-endian data.
				// Create a little-endian byte array, then BitConvert to UInt64
				byte[] bytes = new byte[8];
				for (int i = 0; i < length; i++)
				{
					bytes[i] = Convert.ToByte(InLine.Substring(5 + 2 * i, 2), 16);
				}
				UInt64 data = BitConverter.ToUInt64(bytes, 0);
				Can.Packet packet = new Can.Packet(id, length, data);

				foreach (var handler in this.handlers)
				{
					// TODO check ID against handler.Bitmask and handler.Address
					handler.Callback(packet);
				}
			}
		}
	}
}

