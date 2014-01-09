using System;
using System.Collections.Generic;

namespace SolarCar {
	namespace CanAddr {
		static class Pedals {
			public const int Base = 0x110;
			public const int Status = Base + 1;
		}

		static class BPS {
			public const int RxBase = 0x200;
		}

		static class WS20 {
			public const int Base = 0x400;
			public const int motor_bus = Base + 2;
			public const int motor_velocity = Base + 3;
		}
	}
	class CanPacket {
		UInt16 id = 0;
		Byte length = 0;
		byte[] data = null;

		public UInt16 ID { get { return this.id; } }

		public Byte Length { get { return this.length; } }

		public byte[] Data { get { return this.data; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="SolarCar.CanPacket"/> class.
		/// </summary>
		/// <param name="id">CAN ID, maximum 0x7FF for standard CAN.</param>
		/// <param name="length">Length of CAN Data.</param>
		/// <param name="data">Data in CAN frame.</param>
		public CanPacket(UInt16 id, Byte length, byte[] data) {
			this.id = id;
			this.length = length;
			this.data = new byte[8];
			Array.Copy(data, this.data, length);
		}

		public float GetFloat0() {
			return BitConverter.ToSingle(this.Data, 0);
		}

		public float GetFloat1() {
			return BitConverter.ToSingle(this.Data, 4);
		}

		public double GetDouble0() {
			return BitConverter.ToDouble(this.Data, 0);
		}

		public UInt64 GetUInt64() {
			return BitConverter.ToUInt64(this.Data, 0);
		}
	}

	class CanHandler {
		public delegate void CallbackDelegate(CanPacket p);

		public UInt16 Address;
		public UInt16 Bitmask;
		public CallbackDelegate Callback;

		public CanHandler(UInt16 InAddress, UInt16 InBitmask, CallbackDelegate InCallback) {
			this.Address = InAddress;
			this.Bitmask = InBitmask;
			this.Callback = InCallback;
		}
	}

	/// <summary>
	/// A CAN-USB wrapper. When running, constantly updates its MotorReport.
	/// </summary>
	class CanUsb: AsyncSerialPort {
		const string NEWLINE = "\r";
		public List<CanHandler> handlers = new List<CanHandler>();

		public CanUsb(string path) : base(path) {
			this.NewLine = NEWLINE; // CANUSB uses carriage returns
			this.LineReceived += new LineReceivedDelegate(this.HandleLine);
			this.SyncWriteLine("S8"); // set bitrate = 1 Mbit/s
			this.SyncWriteLine("O"); // open CAN channel
		}

		~CanUsb() {
			this.SyncWriteLine("C");
		}

		void TransmitPacket(CanPacket packet) {
			string hex_id = packet.ID.ToString("X4").Substring(1, 3);
			string hex_length = packet.Length.ToString("X");
			string hex_data = BitConverter.ToString(packet.Data).Replace("-", String.Empty);
			this.SyncWriteLine("t" + hex_id + hex_length + hex_data);
		}

		/// <summary>
		/// Sends a drive cmd, with SyncWriteLine.
		/// </summary>
		/// <param name="motor_velocity">Motor_velocity.</param>
		/// <param name="motor_current">Motor_current.</param>
		public void SendDriveCmd(float motor_velocity, float motor_current) {
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
		/// </summary>
		void HandleLine(string InLine) {
			// If bad packet, throw away.
			if (InLine[0] != 't') {
				Console.WriteLine("CANBUS: Expected first character to be t");
			} else if (InLine[InLine.Length - 1] != '\r') {
				Console.WriteLine("CANBUS: Expected last character to be Carriage Return");
			} else if (InLine.Length > 21) {
				// 1 't', 3 ID, 1 length, upto 16 data
				Console.WriteLine("CANBUS: Max packet size is 22 characters.");
			} else {
				// parse data from CAN packet
				UInt16 id = Convert.ToUInt16(InLine.Substring(1, 3), 16); // max ID == 0x7FF, and "7FF" is three characters
				Byte length = Convert.ToByte(InLine.Substring(4, 1), 16);
				byte[] data = System.Text.Encoding.ASCII.GetBytes(InLine.Substring(5, length));

				CanPacket packet = new CanPacket(id, length, data);

				foreach (var handler in this.handlers) {
					// TODO check ID against handler.Bitmask and handler.Address
					handler.Callback(packet);
				}
			}
		}
	}
}

