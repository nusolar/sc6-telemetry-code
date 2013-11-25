using System;

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
		int id = -1;
		int length = -1;
		byte[] data = null;

		public int ID { get { return this.id; } }

		public int Length { get { return this.length; } }

		public byte[] Data { get { return this.data; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="SolarCar.CanPacket"/> class, from a CANUSB line.
		/// </summary>
		/// <param name="InPacket">A CANUSB packet.</param>
		public CanPacket(byte[] InPacket) {

			byte[] temp_id = new byte[3];
			Array.Copy(InPacket, 1, temp_id, 0, 3); // max ID == 0x7FF, and "7FF" is three characters
			this.id = Convert.ToInt32(System.Text.Encoding.ASCII.GetString(temp_id), 16);

			this.length = InPacket[4];

			this.data = new byte[this.Length];
			Array.Copy(InPacket, 5, this.Data, 0, this.Length);
		}

		/// <summary>
		/// Reinterprets first 4 bytes as float
		/// </summary>
		public float Float1() {
			return BitConverter.ToSingle(this.Data, 0);
		}

		/// <summary>
		/// Reinterprets last 4 bytes as float
		/// </summary>
		public float Float2() {
			return BitConverter.ToSingle(this.Data, 4);
		}
	}

	class CanHandler {
		public delegate void CallbackDelegate(CanPacket p);

		public UInt16 Address;
		public UInt16 Bitmask;
		public CallbackDelegate Callback;

		public CanHandler(UInt16 InAddress, UInt16 InBitmask, UInt16 InCallback) {
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
		CanHandler[] handlers;

		public CanUsb(string path) : base(path) {
			this.NewLine = NEWLINE; // CANUSB uses carriage returns
			this.LineReceived += new LineReceivedDelegate(this.HandleLine);
			this.SyncWriteLine("S8"); // set bitrate = 1 Mbit/s
			this.SyncWriteLine("O"); // open CAN channel
		}

		~CanUsb() {
			this.SyncWriteLine("C");
		}

		string SerializePacket(CanPacket packet) {
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
			byte[] bytes = System.Text.Encoding.ASCII.GetBytes(InLine);

			// If bad packet, throw away.
			if (bytes[0] != 116) { // 't' == 116
				Console.WriteLine("CANBUS: Expected first character to be t");
			} else if (bytes[bytes.Length - 1] != 13) {
				Console.WriteLine("CANBUS: Expected last character to be Carriage Return");
			} else if (bytes.Length > 21) {
				// 1 't', 3 ID, 1 length, upto 16 data
				Console.WriteLine("CANBUS: Max packet size is 22 characters.");
			} else {
				// parse data from CAN packet
				CanPacket packet = new CanPacket(bytes);
				switch (packet.ID) {
					case 0x403: // motor velocity
						this.motor_report.motor_rpm = packet.Float1();
						this.motor_report.motor_velocity = packet.Float2();
						break;
				// TODO more motor values
				// TODO get array values
				}
			}
		}
	}
}

