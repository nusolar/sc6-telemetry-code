using System;
using System.IO.Ports;

namespace SolarCar {
	class CanPacket {
		public int id = -1;
		public int length = -1;
		public byte[] data = null;

		public int ID { get { return this.id; } }

		public int Length { get { return this.length; } }

		public byte[] Data { get { return this.data; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="SolarCar.CanPacket"/> class, from a CANUSB line.
		/// </summary>
		/// <param name="InPacket">A CANUSB packet.</param>
		public CanPacket(byte[] InPacket) {

			byte[] temp_id = new byte[3];
			Array.Copy(InPacket, 1, temp_id, 0, 3); // max ID == 0x7FF, 7FF is three characters
			this.id = Convert.ToInt32(System.Text.Encoding.ASCII.GetString(temp_id), 16);

			this.length = InPacket[4];

			this.data = new byte[this.Length];
			Array.Copy(InPacket, 5, this.Data, 0, this.Length);
		}

		/// <summary>
		/// Indiscriminately reinterprets first 4 bytes as float
		/// </summary>
		public float Float1() {
			return BitConverter.ToSingle(this.Data, 0);
		}

		/// <summary>
		/// Indiscriminately reinterprets last 4 bytes as float
		/// </summary>
		public float Float2() {
			return BitConverter.ToSingle(this.Data, 4);
		}
	}

	/// <summary>
	/// A CAN-USB wrapper. When running, constantly updates its MotorReport.
	/// </summary>
	class CanBus {
		readonly Object can_lock = new Object();
		readonly SerialPort can_bus = new SerialPort();
		readonly public MotorReport motor_report = new MotorReport();

		public CanBus(string path) {
			can_bus.PortName = path;
			can_bus.BaudRate = 115200; // TODO check the max CANUSB baudrate
			can_bus.NewLine = "\r"; // CANUSB uses carriage returns
			can_bus.ReadTimeout = 100; // 100 ms
			can_bus.Open();
			can_bus.WriteLine("S8"); // set bitrate = 1 Mbit/s
			can_bus.WriteLine("O"); // open CAN channel
		}

		~CanBus() {
			can_bus.WriteLine("C");
			can_bus.Close();
		}

		/// <summary>
		/// Aquires CAN bus, and sends a drive cmd.
		/// </summary>
		/// <param name="motor_velocity">Motor_velocity.</param>
		/// <param name="motor_current">Motor_current.</param>
		public void SendDriveCmd(float motor_velocity, float motor_current) {
			byte[] vel_bytes = BitConverter.GetBytes(motor_velocity);
			byte[] cur_bytes = BitConverter.GetBytes(motor_current);
			string vel_hex = System.Text.Encoding.ASCII.GetString(vel_bytes);
			string cur_hex = System.Text.Encoding.ASCII.GetString(cur_bytes);
			string packet = "t4038" + vel_hex + cur_hex;
			try {
				lock (can_lock) {
					can_bus.WriteLine(packet);
				}
			} catch (TimeoutException) {
				Console.WriteLine("CANBUS: write timed out. SerialPort may be busy.");
			}
		}

		/// <summary>
		/// Indefinitely reads from CAN bus. Run in separate Thread!
		/// </summary>
		public void RunLoop() {
			while (can_bus.IsOpen) {
				byte[] bytes = null;
				if (can_bus.BytesToRead > 0) {
					// lock CAN, read upto 21 bytes.
					try {
						lock (can_lock) {
							bytes = System.Text.Encoding.ASCII.GetBytes(can_bus.ReadLine());
						}
					} catch (TimeoutException) {
						Console.WriteLine("CANBUS: read timed out. SerialPort may be busy.");
						continue;
					}

					// If bad packet, throw away and try again.
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
						}
					}
				} 
			}
		}
	}
}

