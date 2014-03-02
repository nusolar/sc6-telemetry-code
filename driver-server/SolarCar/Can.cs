using System;
using System.Runtime.InteropServices;


namespace SolarCar
{
	namespace Can
	{
		class Packet
		{
			[StructLayout(LayoutKind.Explicit)]
			public struct UInt64x1
			{
				[FieldOffset(0)]
				public UInt64 uint64_0;
			}

			[StructLayout(LayoutKind.Explicit)]
			public struct Float64x1
			{
				[FieldOffset(0)]
				public Double double_0;
			}

			[StructLayout(LayoutKind.Explicit)]
			public struct Float32x2
			{
				[FieldOffset(0)]
				public Single single_0;
				[FieldOffset(4)]
				public Single single_1;
			}

			[StructLayout(LayoutKind.Explicit)]
			public struct UInt32x2
			{
				[FieldOffset(0)]
				public UInt32 uint32_0;
				[FieldOffset(4)]
				public UInt32 uint32_1;
			}

			[StructLayout(LayoutKind.Explicit)]
			public struct Int32x2
			{
				[FieldOffset(0)]
				public Int32 int32_0;
				[FieldOffset(4)]
				public Int32 int32_1;
			}

			[StructLayout(LayoutKind.Explicit)]
			public struct UInt16x4
			{
				[FieldOffset(0)]
				public UInt16 uint16_0;
				[FieldOffset(2)]
				public UInt16 uint16_1;
				[FieldOffset(4)]
				public UInt16 uint16_2;
				[FieldOffset(6)]
				public UInt16 uint16_3;
			}

			[StructLayout(LayoutKind.Explicit)]
			public struct Int16x4
			{
				[FieldOffset(0)]
				public Int16 int16_0;
				[FieldOffset(2)]
				public Int16 int16_1;
				[FieldOffset(4)]
				public Int16 int16_2;
				[FieldOffset(6)]
				public Int16 int16_3;
			}

			[StructLayout(LayoutKind.Explicit)]
			public struct Status
			{
				[FieldOffset(0)]
				public UInt32 uint32_0;
				[FieldOffset(4)]
				public UInt32 uint32_1;
			}

			[StructLayout(LayoutKind.Explicit)]
			public struct Trip
			{
				[FieldOffset(0)]
				public Int16 int16_0;
				[FieldOffset(2)]
				public UInt16 uint16_1;
				[FieldOffset(4)]
				public UInt16 uint16_2;
				[FieldOffset(6)]
				public UInt16 uint16_3;
			}

			[StructLayout(LayoutKind.Explicit)]
			public struct UInt8x8
			{
				[FieldOffset(0)]
				public Byte byte_0;
				[FieldOffset(1)]
				public Byte byte_1;
				[FieldOffset(2)]
				public Byte byte_2;
				[FieldOffset(3)]
				public Byte byte_3;
				[FieldOffset(4)]
				public Byte byte_4;
				[FieldOffset(5)]
				public Byte byte_5;
				[FieldOffset(6)]
				public Byte byte_6;
				[FieldOffset(7)]
				public Byte byte_7;
			}

			[StructLayout(LayoutKind.Explicit)]
			public struct Frame
			{
				[FieldOffset(0)]
				public UInt64 data;
				[FieldOffset(0)]
				public Float64x1 float64x1;
				[FieldOffset(0)]
				public Float32x2 float32x2;
				[FieldOffset(0)]
				public UInt64x1 uint64x1;
				[FieldOffset(0)]
				public UInt32x2 uint32x2;
				[FieldOffset(0)]
				public Int32x2 int32x2;
				[FieldOffset(0)]
				public UInt16x4 uint16x4;
				[FieldOffset(0)]
				public Int16x4 int16x4;
				[FieldOffset(0)]
				public UInt8x8 uint8x8;
				[FieldOffset(0)]
				public Trip trip;
				[FieldOffset(0)]
				public Status status;
			}

			UInt16 id = 0;
			Byte length = 0;
			public Frame frame = new Frame();

			public UInt16 ID { get { return this.id; } }

			public Byte Length { get { return this.length; } }

			public UInt64 Data { get { return this.frame.data; } }

			/// <summary>
			/// Initializes a new instance of the <see cref="SolarCar.Can.Packet"/> class.
			/// </summary>
			/// <param name="id">CAN ID, maximum 0x7FF for standard CAN.</param>
			/// <param name="length">Length of CAN Data.</param>
			/// <param name="data">Data in CAN frame.</param>
			public Packet(UInt16 id, Byte length, UInt64 data)
			{
				this.id = id;
				this.length = length;
				this.frame.data = data;
			}
		}

		namespace Addr
		{
			namespace bps_rx
			{
				class trip: Packet
				{
					public trip(UInt64 data = 0) : base(0x200, 8, data)
					{
					}

					public const UInt16 _id = 0x200;

						public Int16 trip_code
					{
						get { return this.frame.trip.int16_0; }
						set { this.frame.trip.int16_0 = value; }
					}

					public UInt16 module
					{
						get { return this.frame.trip.uint16_1; }
						set { this.frame.trip.uint16_1 = value; }
					}

				}

				class reset_cc_batt: Packet
				{
					public reset_cc_batt(UInt64 data = 0) : base(0x201, 8, data)
					{
					}

					public const UInt16 _id = 0x201;

	
				}

				class reset_cc_array: Packet
				{
					public reset_cc_array(UInt64 data = 0) : base(0x202, 8, data)
					{
					}

					public const UInt16 _id = 0x202;

	
				}

				class reset_cc_mppt1: Packet
				{
					public reset_cc_mppt1(UInt64 data = 0) : base(0x203, 8, data)
					{
					}

					public const UInt16 _id = 0x203;

	
				}

				class reset_cc_mppt2: Packet
				{
					public reset_cc_mppt2(UInt64 data = 0) : base(0x204, 8, data)
					{
					}

					public const UInt16 _id = 0x204;

	
				}

				class reset_cc_mppt3: Packet
				{
					public reset_cc_mppt3(UInt64 data = 0) : base(0x205, 8, data)
					{
					}

					public const UInt16 _id = 0x205;

	
				}

				class reset_cc_Wh: Packet
				{
					public reset_cc_Wh(UInt64 data = 0) : base(0x206, 8, data)
					{
					}

					public const UInt16 _id = 0x206;

	
				}

				class reset_cc_all: Packet
				{
					public reset_cc_all(UInt64 data = 0) : base(0x207, 8, data)
					{
					}

					public const UInt16 _id = 0x207;

	
				}
			}

			namespace bps_tx
			{
				class heartbeat: Packet
				{
					public heartbeat(UInt64 data = 0) : base(0x210, 8, data)
					{
					}

					public const UInt16 _id = 0x210;

						public UInt32 bmsId
					{
						get { return this.frame.uint32x2.uint32_0; }
						set { this.frame.uint32x2.uint32_0 = value; }
					}

					public UInt32 uptime_s
					{
						get { return this.frame.uint32x2.uint32_1; }
						set { this.frame.uint32x2.uint32_1 = value; }
					}

				}

				class bps_status: Packet
				{
					public bps_status(UInt64 data = 0) : base(0x211, 8, data)
					{
					}

					public const UInt16 _id = 0x211;

						public UInt16 mode
					{
						get { return this.frame.uint16x4.uint16_0; }
						set { this.frame.uint16x4.uint16_0 = value; }
					}

					public UInt16 disabled_module
					{
						get { return this.frame.uint16x4.uint16_1; }
						set { this.frame.uint16x4.uint16_1 = value; }
					}

					public UInt16 last_error
					{
						get { return this.frame.uint16x4.uint16_2; }
						set { this.frame.uint16x4.uint16_2 = value; }
					}

					public UInt16 last_error_value
					{
						get { return this.frame.uint16x4.uint16_3; }
						set { this.frame.uint16x4.uint16_3 = value; }
					}

				}

				class current: Packet
				{
					public current(UInt64 data = 0) : base(0x212, 8, data)
					{
					}

					public const UInt16 _id = 0x212;

						public UInt16 array
					{
						get { return this.frame.uint16x4.uint16_0; }
						set { this.frame.uint16x4.uint16_0 = value; }
					}

					public UInt16 battery
					{
						get { return this.frame.uint16x4.uint16_1; }
						set { this.frame.uint16x4.uint16_1 = value; }
					}

					public UInt16 reserved
					{
						get { return this.frame.uint16x4.uint16_2; }
						set { this.frame.uint16x4.uint16_2 = value; }
					}

					public UInt16 reserved1
					{
						get { return this.frame.uint16x4.uint16_3; }
						set { this.frame.uint16x4.uint16_3 = value; }
					}

				}

				class voltage_temp: Packet
				{
					public voltage_temp(UInt64 data = 0) : base(0x213, 8, data)
					{
					}

					public const UInt16 _id = 0x213;

						public UInt16 module
					{
						get { return this.frame.uint16x4.uint16_0; }
						set { this.frame.uint16x4.uint16_0 = value; }
					}

					public UInt16 voltage
					{
						get { return this.frame.uint16x4.uint16_1; }
						set { this.frame.uint16x4.uint16_1 = value; }
					}

					public UInt16 temp
					{
						get { return this.frame.uint16x4.uint16_2; }
						set { this.frame.uint16x4.uint16_2 = value; }
					}

					public UInt16 reserved
					{
						get { return this.frame.uint16x4.uint16_3; }
						set { this.frame.uint16x4.uint16_3 = value; }
					}

				}

				class cc_array: Packet
				{
					public cc_array(UInt64 data = 0) : base(0x214, 8, data)
					{
					}

					public const UInt16 _id = 0x214;

						public Double count
					{
						get { return this.frame.float64x1.double_0; }
						set { this.frame.float64x1.double_0 = value; }
					}

				}

				class cc_batt: Packet
				{
					public cc_batt(UInt64 data = 0) : base(0x215, 8, data)
					{
					}

					public const UInt16 _id = 0x215;

						public Double count
					{
						get { return this.frame.float64x1.double_0; }
						set { this.frame.float64x1.double_0 = value; }
					}

				}

				class cc_mppt1: Packet
				{
					public cc_mppt1(UInt64 data = 0) : base(0x216, 8, data)
					{
					}

					public const UInt16 _id = 0x216;

						public Double count
					{
						get { return this.frame.float64x1.double_0; }
						set { this.frame.float64x1.double_0 = value; }
					}

				}

				class cc_mppt2: Packet
				{
					public cc_mppt2(UInt64 data = 0) : base(0x217, 8, data)
					{
					}

					public const UInt16 _id = 0x217;

						public Double count
					{
						get { return this.frame.float64x1.double_0; }
						set { this.frame.float64x1.double_0 = value; }
					}

				}

				class cc_mppt3: Packet
				{
					public cc_mppt3(UInt64 data = 0) : base(0x218, 8, data)
					{
					}

					public const UInt16 _id = 0x218;

						public Double count
					{
						get { return this.frame.float64x1.double_0; }
						set { this.frame.float64x1.double_0 = value; }
					}

				}

				class Wh_batt: Packet
				{
					public Wh_batt(UInt64 data = 0) : base(0x219, 8, data)
					{
					}

					public const UInt16 _id = 0x219;

						public Double count
					{
						get { return this.frame.float64x1.double_0; }
						set { this.frame.float64x1.double_0 = value; }
					}

				}

				class Wh_mppt1: Packet
				{
					public Wh_mppt1(UInt64 data = 0) : base(0x21a, 8, data)
					{
					}

					public const UInt16 _id = 0x21a;

						public Double count
					{
						get { return this.frame.float64x1.double_0; }
						set { this.frame.float64x1.double_0 = value; }
					}

				}

				class Wh_mppt2: Packet
				{
					public Wh_mppt2(UInt64 data = 0) : base(0x21b, 8, data)
					{
					}

					public const UInt16 _id = 0x21b;

						public Double count
					{
						get { return this.frame.float64x1.double_0; }
						set { this.frame.float64x1.double_0 = value; }
					}

				}

				class Wh_mppt3: Packet
				{
					public Wh_mppt3(UInt64 data = 0) : base(0x21c, 8, data)
					{
					}

					public const UInt16 _id = 0x21c;

						public Double count
					{
						get { return this.frame.float64x1.double_0; }
						set { this.frame.float64x1.double_0 = value; }
					}

				}

				class last_trip: Packet
				{
					public last_trip(UInt64 data = 0) : base(0x21d, 8, data)
					{
					}

					public const UInt16 _id = 0x21d;

						public Int16 trip_code
					{
						get { return this.frame.trip.int16_0; }
						set { this.frame.trip.int16_0 = value; }
					}

					public UInt16 module
					{
						get { return this.frame.trip.uint16_1; }
						set { this.frame.trip.uint16_1 = value; }
					}

					public UInt16 low_current
					{
						get { return this.frame.trip.uint16_2; }
						set { this.frame.trip.uint16_2 = value; }
					}

					public UInt16 high_current
					{
						get { return this.frame.trip.uint16_3; }
						set { this.frame.trip.uint16_3 = value; }
					}

				}

				class last_trip_voltage_temp: Packet
				{
					public last_trip_voltage_temp(UInt64 data = 0) : base(0x21e, 8, data)
					{
					}

					public const UInt16 _id = 0x21e;

						public UInt16 low_volt
					{
						get { return this.frame.uint16x4.uint16_0; }
						set { this.frame.uint16x4.uint16_0 = value; }
					}

					public UInt16 high_volt
					{
						get { return this.frame.uint16x4.uint16_1; }
						set { this.frame.uint16x4.uint16_1 = value; }
					}

					public UInt16 low_temp
					{
						get { return this.frame.uint16x4.uint16_2; }
						set { this.frame.uint16x4.uint16_2 = value; }
					}

					public UInt16 high_temp
					{
						get { return this.frame.uint16x4.uint16_3; }
						set { this.frame.uint16x4.uint16_3 = value; }
					}

				}
			}

			namespace bms0
			{
				class heartbeat: Packet
				{
					public heartbeat(UInt64 data = 0) : base(0x600, 8, data)
					{
					}

					public const UInt16 _id = 0x600;

						public UInt32 bmsId
					{
						get { return this.frame.uint32x2.uint32_0; }
						set { this.frame.uint32x2.uint32_0 = value; }
					}

					public UInt32 serialNo
					{
						get { return this.frame.uint32x2.uint32_1; }
						set { this.frame.uint32x2.uint32_1 = value; }
					}

				}

				class cmu1_status: Packet
				{
					public cmu1_status(UInt64 data = 0) : base(0x601, 8, data)
					{
					}

					public const UInt16 _id = 0x601;

						public Int16 serialNo0
					{
						get { return this.frame.int16x4.int16_0; }
						set { this.frame.int16x4.int16_0 = value; }
					}

					public Int16 serialNo1
					{
						get { return this.frame.int16x4.int16_1; }
						set { this.frame.int16x4.int16_1 = value; }
					}

					public Int16 pcb_temp
					{
						get { return this.frame.int16x4.int16_2; }
						set { this.frame.int16x4.int16_2 = value; }
					}

					public Int16 cell_temp
					{
						get { return this.frame.int16x4.int16_3; }
						set { this.frame.int16x4.int16_3 = value; }
					}

				}

				class cmu1_volts0: Packet
				{
					public cmu1_volts0(UInt64 data = 0) : base(0x602, 8, data)
					{
					}

					public const UInt16 _id = 0x602;

						public Int16 cell0
					{
						get { return this.frame.int16x4.int16_0; }
						set { this.frame.int16x4.int16_0 = value; }
					}

					public Int16 cell1
					{
						get { return this.frame.int16x4.int16_1; }
						set { this.frame.int16x4.int16_1 = value; }
					}

					public Int16 cell2
					{
						get { return this.frame.int16x4.int16_2; }
						set { this.frame.int16x4.int16_2 = value; }
					}

					public Int16 cell3
					{
						get { return this.frame.int16x4.int16_3; }
						set { this.frame.int16x4.int16_3 = value; }
					}

				}

				class cmu1_volts1: Packet
				{
					public cmu1_volts1(UInt64 data = 0) : base(0x603, 8, data)
					{
					}

					public const UInt16 _id = 0x603;

						public Int16 cell4
					{
						get { return this.frame.int16x4.int16_0; }
						set { this.frame.int16x4.int16_0 = value; }
					}

					public Int16 cell5
					{
						get { return this.frame.int16x4.int16_1; }
						set { this.frame.int16x4.int16_1 = value; }
					}

					public Int16 cell6
					{
						get { return this.frame.int16x4.int16_2; }
						set { this.frame.int16x4.int16_2 = value; }
					}

					public Int16 cell7
					{
						get { return this.frame.int16x4.int16_3; }
						set { this.frame.int16x4.int16_3 = value; }
					}

				}

				class cmu2_status: Packet
				{
					public cmu2_status(UInt64 data = 0) : base(0x604, 8, data)
					{
					}

					public const UInt16 _id = 0x604;

						public Int16 serialNo0
					{
						get { return this.frame.int16x4.int16_0; }
						set { this.frame.int16x4.int16_0 = value; }
					}

					public Int16 serialNo1
					{
						get { return this.frame.int16x4.int16_1; }
						set { this.frame.int16x4.int16_1 = value; }
					}

					public Int16 pcb_temp
					{
						get { return this.frame.int16x4.int16_2; }
						set { this.frame.int16x4.int16_2 = value; }
					}

					public Int16 cell_temp
					{
						get { return this.frame.int16x4.int16_3; }
						set { this.frame.int16x4.int16_3 = value; }
					}

				}

				class cmu2_volts0: Packet
				{
					public cmu2_volts0(UInt64 data = 0) : base(0x605, 8, data)
					{
					}

					public const UInt16 _id = 0x605;

						public Int16 cell0
					{
						get { return this.frame.int16x4.int16_0; }
						set { this.frame.int16x4.int16_0 = value; }
					}

					public Int16 cell1
					{
						get { return this.frame.int16x4.int16_1; }
						set { this.frame.int16x4.int16_1 = value; }
					}

					public Int16 cell2
					{
						get { return this.frame.int16x4.int16_2; }
						set { this.frame.int16x4.int16_2 = value; }
					}

					public Int16 cell3
					{
						get { return this.frame.int16x4.int16_3; }
						set { this.frame.int16x4.int16_3 = value; }
					}

				}

				class cmu2_volts1: Packet
				{
					public cmu2_volts1(UInt64 data = 0) : base(0x606, 8, data)
					{
					}

					public const UInt16 _id = 0x606;

						public Int16 cell4
					{
						get { return this.frame.int16x4.int16_0; }
						set { this.frame.int16x4.int16_0 = value; }
					}

					public Int16 cell5
					{
						get { return this.frame.int16x4.int16_1; }
						set { this.frame.int16x4.int16_1 = value; }
					}

					public Int16 cell6
					{
						get { return this.frame.int16x4.int16_2; }
						set { this.frame.int16x4.int16_2 = value; }
					}

					public Int16 cell7
					{
						get { return this.frame.int16x4.int16_3; }
						set { this.frame.int16x4.int16_3 = value; }
					}

				}

				class cmu3_status: Packet
				{
					public cmu3_status(UInt64 data = 0) : base(0x607, 8, data)
					{
					}

					public const UInt16 _id = 0x607;

						public Int16 serialNo0
					{
						get { return this.frame.int16x4.int16_0; }
						set { this.frame.int16x4.int16_0 = value; }
					}

					public Int16 serialNo1
					{
						get { return this.frame.int16x4.int16_1; }
						set { this.frame.int16x4.int16_1 = value; }
					}

					public Int16 pcb_temp
					{
						get { return this.frame.int16x4.int16_2; }
						set { this.frame.int16x4.int16_2 = value; }
					}

					public Int16 cell_temp
					{
						get { return this.frame.int16x4.int16_3; }
						set { this.frame.int16x4.int16_3 = value; }
					}

				}

				class cmu3_volts0: Packet
				{
					public cmu3_volts0(UInt64 data = 0) : base(0x608, 8, data)
					{
					}

					public const UInt16 _id = 0x608;

						public Int16 cell0
					{
						get { return this.frame.int16x4.int16_0; }
						set { this.frame.int16x4.int16_0 = value; }
					}

					public Int16 cell1
					{
						get { return this.frame.int16x4.int16_1; }
						set { this.frame.int16x4.int16_1 = value; }
					}

					public Int16 cell2
					{
						get { return this.frame.int16x4.int16_2; }
						set { this.frame.int16x4.int16_2 = value; }
					}

					public Int16 cell3
					{
						get { return this.frame.int16x4.int16_3; }
						set { this.frame.int16x4.int16_3 = value; }
					}

				}

				class cmu3_volts1: Packet
				{
					public cmu3_volts1(UInt64 data = 0) : base(0x609, 8, data)
					{
					}

					public const UInt16 _id = 0x609;

						public Int16 cell4
					{
						get { return this.frame.int16x4.int16_0; }
						set { this.frame.int16x4.int16_0 = value; }
					}

					public Int16 cell5
					{
						get { return this.frame.int16x4.int16_1; }
						set { this.frame.int16x4.int16_1 = value; }
					}

					public Int16 cell6
					{
						get { return this.frame.int16x4.int16_2; }
						set { this.frame.int16x4.int16_2 = value; }
					}

					public Int16 cell7
					{
						get { return this.frame.int16x4.int16_3; }
						set { this.frame.int16x4.int16_3 = value; }
					}

				}

				class cmu4_status: Packet
				{
					public cmu4_status(UInt64 data = 0) : base(0x60a, 8, data)
					{
					}

					public const UInt16 _id = 0x60a;

						public Int16 serialNo0
					{
						get { return this.frame.int16x4.int16_0; }
						set { this.frame.int16x4.int16_0 = value; }
					}

					public Int16 serialNo1
					{
						get { return this.frame.int16x4.int16_1; }
						set { this.frame.int16x4.int16_1 = value; }
					}

					public Int16 pcb_temp
					{
						get { return this.frame.int16x4.int16_2; }
						set { this.frame.int16x4.int16_2 = value; }
					}

					public Int16 cell_temp
					{
						get { return this.frame.int16x4.int16_3; }
						set { this.frame.int16x4.int16_3 = value; }
					}

				}

				class cmu4_volts0: Packet
				{
					public cmu4_volts0(UInt64 data = 0) : base(0x60b, 8, data)
					{
					}

					public const UInt16 _id = 0x60b;

						public Int16 cell0
					{
						get { return this.frame.int16x4.int16_0; }
						set { this.frame.int16x4.int16_0 = value; }
					}

					public Int16 cell1
					{
						get { return this.frame.int16x4.int16_1; }
						set { this.frame.int16x4.int16_1 = value; }
					}

					public Int16 cell2
					{
						get { return this.frame.int16x4.int16_2; }
						set { this.frame.int16x4.int16_2 = value; }
					}

					public Int16 cell3
					{
						get { return this.frame.int16x4.int16_3; }
						set { this.frame.int16x4.int16_3 = value; }
					}

				}

				class cmu4_volts1: Packet
				{
					public cmu4_volts1(UInt64 data = 0) : base(0x60c, 8, data)
					{
					}

					public const UInt16 _id = 0x60c;

						public Int16 cell4
					{
						get { return this.frame.int16x4.int16_0; }
						set { this.frame.int16x4.int16_0 = value; }
					}

					public Int16 cell5
					{
						get { return this.frame.int16x4.int16_1; }
						set { this.frame.int16x4.int16_1 = value; }
					}

					public Int16 cell6
					{
						get { return this.frame.int16x4.int16_2; }
						set { this.frame.int16x4.int16_2 = value; }
					}

					public Int16 cell7
					{
						get { return this.frame.int16x4.int16_3; }
						set { this.frame.int16x4.int16_3 = value; }
					}

				}
			}

			namespace bms1
			{
				class reserved0: Packet
				{
					public reserved0(UInt64 data = 0) : base(0x6f0, 8, data)
					{
					}

					public const UInt16 _id = 0x6f0;

	
				}

				class reserved1: Packet
				{
					public reserved1(UInt64 data = 0) : base(0x6f1, 8, data)
					{
					}

					public const UInt16 _id = 0x6f1;

	
				}

				class reserved2: Packet
				{
					public reserved2(UInt64 data = 0) : base(0x6f2, 8, data)
					{
					}

					public const UInt16 _id = 0x6f2;

	
				}

				class reserved3: Packet
				{
					public reserved3(UInt64 data = 0) : base(0x6f3, 8, data)
					{
					}

					public const UInt16 _id = 0x6f3;

	
				}

				class pack_soc: Packet
				{
					public pack_soc(UInt64 data = 0) : base(0x6f4, 8, data)
					{
					}

					public const UInt16 _id = 0x6f4;

						public Single soc_Ah
					{
						get { return this.frame.float32x2.single_0; }
						set { this.frame.float32x2.single_0 = value; }
					}

					public Single soc_percentage
					{
						get { return this.frame.float32x2.single_1; }
						set { this.frame.float32x2.single_1 = value; }
					}

				}

				class pack_bal_soc: Packet
				{
					public pack_bal_soc(UInt64 data = 0) : base(0x6f5, 8, data)
					{
					}

					public const UInt16 _id = 0x6f5;

						public Single soc_Ah
					{
						get { return this.frame.float32x2.single_0; }
						set { this.frame.float32x2.single_0 = value; }
					}

					public Single soc_percentage
					{
						get { return this.frame.float32x2.single_1; }
						set { this.frame.float32x2.single_1 = value; }
					}

				}

				class charger_cmd: Packet
				{
					public charger_cmd(UInt64 data = 0) : base(0x6f6, 8, data)
					{
					}

					public const UInt16 _id = 0x6f6;

						public Int16 charging_mV_err
					{
						get { return this.frame.int16x4.int16_0; }
						set { this.frame.int16x4.int16_0 = value; }
					}

					public Int16 temp_margin
					{
						get { return this.frame.int16x4.int16_1; }
						set { this.frame.int16x4.int16_1 = value; }
					}

					public Int16 discharging_mV_err
					{
						get { return this.frame.int16x4.int16_2; }
						set { this.frame.int16x4.int16_2 = value; }
					}

					public Int16 pack_capacity_Ah
					{
						get { return this.frame.int16x4.int16_3; }
						set { this.frame.int16x4.int16_3 = value; }
					}

				}

				class precharge: Packet
				{
					public precharge(UInt64 data = 0) : base(0x6f7, 8, data)
					{
					}

					public const UInt16 _id = 0x6f7;

						public UInt16 precharge_flags
					{
						get { return this.frame.uint16x4.uint16_0; }
						set { this.frame.uint16x4.uint16_0 = value; }
					}

					public UInt16 unused0
					{
						get { return this.frame.uint16x4.uint16_1; }
						set { this.frame.uint16x4.uint16_1 = value; }
					}

					public UInt16 unused1
					{
						get { return this.frame.uint16x4.uint16_2; }
						set { this.frame.uint16x4.uint16_2 = value; }
					}

					public UInt16 precharge_timer_flags
					{
						get { return this.frame.uint16x4.uint16_3; }
						set { this.frame.uint16x4.uint16_3 = value; }
					}

				}

				class max_min_volts: Packet
				{
					public max_min_volts(UInt64 data = 0) : base(0x6f8, 8, data)
					{
					}

					public const UInt16 _id = 0x6f8;

	
				}

				class max_min_temps: Packet
				{
					public max_min_temps(UInt64 data = 0) : base(0x6f9, 8, data)
					{
					}

					public const UInt16 _id = 0x6f9;

	
				}

				class pack_volt_curr: Packet
				{
					public pack_volt_curr(UInt64 data = 0) : base(0x6fa, 8, data)
					{
					}

					public const UInt16 _id = 0x6fa;

						public Int32 voltage
					{
						get { return this.frame.int32x2.int32_0; }
						set { this.frame.int32x2.int32_0 = value; }
					}

					public Int32 current
					{
						get { return this.frame.int32x2.int32_1; }
						set { this.frame.int32x2.int32_1 = value; }
					}

				}

				class pack_status: Packet
				{
					public pack_status(UInt64 data = 0) : base(0x6fb, 8, data)
					{
					}

					public const UInt16 _id = 0x6fb;

						public UInt16 thresh_rising
					{
						get { return this.frame.uint16x4.uint16_0; }
						set { this.frame.uint16x4.uint16_0 = value; }
					}

					public UInt16 thresh_falling
					{
						get { return this.frame.uint16x4.uint16_1; }
						set { this.frame.uint16x4.uint16_1 = value; }
					}

					public UInt16 pack_flags
					{
						get { return this.frame.uint16x4.uint16_2; }
						set { this.frame.uint16x4.uint16_2 = value; }
					}

					public UInt16 firmwareNo
					{
						get { return this.frame.uint16x4.uint16_3; }
						set { this.frame.uint16x4.uint16_3 = value; }
					}

				}

				class fan_status: Packet
				{
					public fan_status(UInt64 data = 0) : base(0x6fc, 8, data)
					{
					}

					public const UInt16 _id = 0x6fc;

						public UInt16 fan0_rpm
					{
						get { return this.frame.uint16x4.uint16_0; }
						set { this.frame.uint16x4.uint16_0 = value; }
					}

					public UInt16 fan1_rpm
					{
						get { return this.frame.uint16x4.uint16_1; }
						set { this.frame.uint16x4.uint16_1 = value; }
					}

					public UInt16 fans_relays_mA
					{
						get { return this.frame.uint16x4.uint16_2; }
						set { this.frame.uint16x4.uint16_2 = value; }
					}

					public UInt16 cmus_mA
					{
						get { return this.frame.uint16x4.uint16_3; }
						set { this.frame.uint16x4.uint16_3 = value; }
					}

				}

				class extended_status: Packet
				{
					public extended_status(UInt64 data = 0) : base(0x6fd, 8, data)
					{
					}

					public const UInt16 _id = 0x6fd;

						public UInt16 pack_flags0
					{
						get { return this.frame.uint16x4.uint16_0; }
						set { this.frame.uint16x4.uint16_0 = value; }
					}

					public UInt16 pack_flags1
					{
						get { return this.frame.uint16x4.uint16_1; }
						set { this.frame.uint16x4.uint16_1 = value; }
					}

					public UInt16 hardware_model_id
					{
						get { return this.frame.uint16x4.uint16_2; }
						set { this.frame.uint16x4.uint16_2 = value; }
					}

					public UInt16 unused0
					{
						get { return this.frame.uint16x4.uint16_3; }
						set { this.frame.uint16x4.uint16_3 = value; }
					}

				}
			}

			namespace ws20
			{
				class motor_id: Packet
				{
					public motor_id(UInt64 data = 0) : base(0x400, 8, data)
					{
					}

					public const UInt16 _id = 0x400;

						public UInt32 tritiumId
					{
						get { return this.frame.uint32x2.uint32_0; }
						set { this.frame.uint32x2.uint32_0 = value; }
					}

					public UInt32 serialNo
					{
						get { return this.frame.uint32x2.uint32_1; }
						set { this.frame.uint32x2.uint32_1 = value; }
					}

				}

				class motor_status_info: Packet
				{
					public motor_status_info(UInt64 data = 0) : base(0x401, 8, data)
					{
					}

					public const UInt16 _id = 0x401;

						public UInt16 limitFlags
					{
						get { return this.frame.uint16x4.uint16_0; }
						set { this.frame.uint16x4.uint16_0 = value; }
					}

					public UInt16 errorFlags
					{
						get { return this.frame.uint16x4.uint16_1; }
						set { this.frame.uint16x4.uint16_1 = value; }
					}

					public UInt16 activeMotor
					{
						get { return this.frame.uint16x4.uint16_2; }
						set { this.frame.uint16x4.uint16_2 = value; }
					}

					public UInt16 reserved
					{
						get { return this.frame.uint16x4.uint16_3; }
						set { this.frame.uint16x4.uint16_3 = value; }
					}

				}

				class motor_bus: Packet
				{
					public motor_bus(UInt64 data = 0) : base(0x402, 8, data)
					{
					}

					public const UInt16 _id = 0x402;

						public Single busVoltage
					{
						get { return this.frame.float32x2.single_0; }
						set { this.frame.float32x2.single_0 = value; }
					}

					public Single busCurrent
					{
						get { return this.frame.float32x2.single_1; }
						set { this.frame.float32x2.single_1 = value; }
					}

				}

				class motor_velocity: Packet
				{
					public motor_velocity(UInt64 data = 0) : base(0x403, 8, data)
					{
					}

					public const UInt16 _id = 0x403;

						public Single motorVelocity
					{
						get { return this.frame.float32x2.single_0; }
						set { this.frame.float32x2.single_0 = value; }
					}

					public Single vehicleVelocity
					{
						get { return this.frame.float32x2.single_1; }
						set { this.frame.float32x2.single_1 = value; }
					}

				}

				class motor_phase: Packet
				{
					public motor_phase(UInt64 data = 0) : base(0x404, 8, data)
					{
					}

					public const UInt16 _id = 0x404;

						public Single phaseBCurrent
					{
						get { return this.frame.float32x2.single_0; }
						set { this.frame.float32x2.single_0 = value; }
					}

					public Single phaseACurrent
					{
						get { return this.frame.float32x2.single_1; }
						set { this.frame.float32x2.single_1 = value; }
					}

				}

				class voltage_vector: Packet
				{
					public voltage_vector(UInt64 data = 0) : base(0x405, 8, data)
					{
					}

					public const UInt16 _id = 0x405;

						public Single voltageIm
					{
						get { return this.frame.float32x2.single_0; }
						set { this.frame.float32x2.single_0 = value; }
					}

					public Single voltageRe
					{
						get { return this.frame.float32x2.single_1; }
						set { this.frame.float32x2.single_1 = value; }
					}

				}

				class current_vector: Packet
				{
					public current_vector(UInt64 data = 0) : base(0x406, 8, data)
					{
					}

					public const UInt16 _id = 0x406;

						public Single currentIm
					{
						get { return this.frame.float32x2.single_0; }
						set { this.frame.float32x2.single_0 = value; }
					}

					public Single currentRe
					{
						get { return this.frame.float32x2.single_1; }
						set { this.frame.float32x2.single_1 = value; }
					}

				}

				class backemf: Packet
				{
					public backemf(UInt64 data = 0) : base(0x407, 8, data)
					{
					}

					public const UInt16 _id = 0x407;

						public Single backEmfIm
					{
						get { return this.frame.float32x2.single_0; }
						set { this.frame.float32x2.single_0 = value; }
					}

					public Single backEmfRe
					{
						get { return this.frame.float32x2.single_1; }
						set { this.frame.float32x2.single_1 = value; }
					}

				}

				class rail_15v_1pt65v: Packet
				{
					public rail_15v_1pt65v(UInt64 data = 0) : base(0x408, 8, data)
					{
					}

					public const UInt16 _id = 0x408;

						public Single onePtSixtyFiveVRef
					{
						get { return this.frame.float32x2.single_0; }
						set { this.frame.float32x2.single_0 = value; }
					}

					public Single fifteenVPowerRail
					{
						get { return this.frame.float32x2.single_1; }
						set { this.frame.float32x2.single_1 = value; }
					}

				}

				class rail_2pt5v_1pt2v: Packet
				{
					public rail_2pt5v_1pt2v(UInt64 data = 0) : base(0x409, 8, data)
					{
					}

					public const UInt16 _id = 0x409;

						public Single onePtTwoVSupply
					{
						get { return this.frame.float32x2.single_0; }
						set { this.frame.float32x2.single_0 = value; }
					}

					public Single twoPtFiveVSupply
					{
						get { return this.frame.float32x2.single_1; }
						set { this.frame.float32x2.single_1 = value; }
					}

				}

				class fanspeed: Packet
				{
					public fanspeed(UInt64 data = 0) : base(0x40a, 8, data)
					{
					}

					public const UInt16 _id = 0x40a;

						public Single fanDrive
					{
						get { return this.frame.float32x2.single_0; }
						set { this.frame.float32x2.single_0 = value; }
					}

					public Single fanRpm
					{
						get { return this.frame.float32x2.single_1; }
						set { this.frame.float32x2.single_1 = value; }
					}

				}

				class sinks_temp: Packet
				{
					public sinks_temp(UInt64 data = 0) : base(0x40b, 8, data)
					{
					}

					public const UInt16 _id = 0x40b;

						public Single motorTemp
					{
						get { return this.frame.float32x2.single_0; }
						set { this.frame.float32x2.single_0 = value; }
					}

					public Single heatsinkTemp
					{
						get { return this.frame.float32x2.single_1; }
						set { this.frame.float32x2.single_1 = value; }
					}

				}

				class cpu_airin_temp: Packet
				{
					public cpu_airin_temp(UInt64 data = 0) : base(0x40c, 8, data)
					{
					}

					public const UInt16 _id = 0x40c;

						public Single processorTemp
					{
						get { return this.frame.float32x2.single_0; }
						set { this.frame.float32x2.single_0 = value; }
					}

					public Single airInletTemp
					{
						get { return this.frame.float32x2.single_1; }
						set { this.frame.float32x2.single_1 = value; }
					}

				}

				class cap_airout_temp: Packet
				{
					public cap_airout_temp(UInt64 data = 0) : base(0x40d, 8, data)
					{
					}

					public const UInt16 _id = 0x40d;

						public Single capacitorTemp
					{
						get { return this.frame.float32x2.single_0; }
						set { this.frame.float32x2.single_0 = value; }
					}

					public Single airOutTemp
					{
						get { return this.frame.float32x2.single_1; }
						set { this.frame.float32x2.single_1 = value; }
					}

				}

				class odom_bus_ah: Packet
				{
					public odom_bus_ah(UInt64 data = 0) : base(0x40e, 8, data)
					{
					}

					public const UInt16 _id = 0x40e;

						public Single odom
					{
						get { return this.frame.float32x2.single_0; }
						set { this.frame.float32x2.single_0 = value; }
					}

					public Single dcBusAmpHours
					{
						get { return this.frame.float32x2.single_1; }
						set { this.frame.float32x2.single_1 = value; }
					}

				}
			}

			namespace dc
			{
				class driver_controls_id: Packet
				{
					public driver_controls_id(UInt64 data = 0) : base(0x500, 8, data)
					{
					}

					public const UInt16 _id = 0x500;

						public UInt32 drvId
					{
						get { return this.frame.uint32x2.uint32_0; }
						set { this.frame.uint32x2.uint32_0 = value; }
					}

					public UInt32 serialNo
					{
						get { return this.frame.uint32x2.uint32_1; }
						set { this.frame.uint32x2.uint32_1 = value; }
					}

				}

				class drive_cmd: Packet
				{
					public drive_cmd(UInt64 data = 0) : base(0x501, 8, data)
					{
					}

					public const UInt16 _id = 0x501;

						public Single motorVelocity
					{
						get { return this.frame.float32x2.single_0; }
						set { this.frame.float32x2.single_0 = value; }
					}

					public Single motorCurrent
					{
						get { return this.frame.float32x2.single_1; }
						set { this.frame.float32x2.single_1 = value; }
					}

				}

				class power_cmd: Packet
				{
					public power_cmd(UInt64 data = 0) : base(0x502, 8, data)
					{
					}

					public const UInt16 _id = 0x502;

						public Single reserved
					{
						get { return this.frame.float32x2.single_0; }
						set { this.frame.float32x2.single_0 = value; }
					}

					public Single busCurrent
					{
						get { return this.frame.float32x2.single_1; }
						set { this.frame.float32x2.single_1 = value; }
					}

				}

				class reset_cmd: Packet
				{
					public reset_cmd(UInt64 data = 0) : base(0x503, 8, data)
					{
					}

					public const UInt16 _id = 0x503;

						public UInt32 unused0
					{
						get { return this.frame.uint32x2.uint32_0; }
						set { this.frame.uint32x2.uint32_0 = value; }
					}

					public UInt32 unused1
					{
						get { return this.frame.uint32x2.uint32_1; }
						set { this.frame.uint32x2.uint32_1 = value; }
					}

				}

				class unused0: Packet
				{
					public unused0(UInt64 data = 0) : base(0x504, 8, data)
					{
					}

					public const UInt16 _id = 0x504;

	
				}

				class switches: Packet
				{
					public switches(UInt64 data = 0) : base(0x505, 8, data)
					{
					}

					public const UInt16 _id = 0x505;

						public UInt16 switchFlags
					{
						get { return this.frame.uint16x4.uint16_0; }
						set { this.frame.uint16x4.uint16_0 = value; }
					}

					public UInt16 unused0
					{
						get { return this.frame.uint16x4.uint16_1; }
						set { this.frame.uint16x4.uint16_1 = value; }
					}

					public UInt16 unused1
					{
						get { return this.frame.uint16x4.uint16_2; }
						set { this.frame.uint16x4.uint16_2 = value; }
					}

					public UInt16 unused2
					{
						get { return this.frame.uint16x4.uint16_3; }
						set { this.frame.uint16x4.uint16_3 = value; }
					}

				}

				class pedals: Packet
				{
					public pedals(UInt64 data = 0) : base(0x506, 8, data)
					{
					}

					public const UInt16 _id = 0x506;

						public UInt16 accel_pedal
					{
						get { return this.frame.uint16x4.uint16_0; }
						set { this.frame.uint16x4.uint16_0 = value; }
					}

					public UInt16 regen_pedal
					{
						get { return this.frame.uint16x4.uint16_1; }
						set { this.frame.uint16x4.uint16_1 = value; }
					}

					public UInt16 brake_pedal
					{
						get { return this.frame.uint16x4.uint16_2; }
						set { this.frame.uint16x4.uint16_2 = value; }
					}

					public UInt16 reserved
					{
						get { return this.frame.uint16x4.uint16_3; }
						set { this.frame.uint16x4.uint16_3 = value; }
					}

				}
			}

			namespace os
			{
				class user_cmds: Packet
				{
					public user_cmds(UInt64 data = 0) : base(0x310, 8, data)
					{
					}

					public const UInt16 _id = 0x310;

						public UInt16 power
					{
						get { return this.frame.uint16x4.uint16_0; }
						set { this.frame.uint16x4.uint16_0 = value; }
					}

					public UInt16 gearFlags
					{
						get { return this.frame.uint16x4.uint16_1; }
						set { this.frame.uint16x4.uint16_1 = value; }
					}

					public UInt16 signalFlags
					{
						get { return this.frame.uint16x4.uint16_2; }
						set { this.frame.uint16x4.uint16_2 = value; }
					}

					public UInt16 reserved
					{
						get { return this.frame.uint16x4.uint16_3; }
						set { this.frame.uint16x4.uint16_3 = value; }
					}

				}

				class cruise_cmd: Packet
				{
					public cruise_cmd(UInt64 data = 0) : base(0x311, 8, data)
					{
					}

					public const UInt16 _id = 0x311;

						public Single velocity
					{
						get { return this.frame.float32x2.single_0; }
						set { this.frame.float32x2.single_0 = value; }
					}

					public Single current
					{
						get { return this.frame.float32x2.single_1; }
						set { this.frame.float32x2.single_1 = value; }
					}

				}
			}

			namespace mppt_master
			{
				class unused0: Packet
				{
					public unused0(UInt64 data = 0) : base(0x710, 8, data)
					{
					}

					public const UInt16 _id = 0x710;

	
				}

				class mppt1: Packet
				{
					public mppt1(UInt64 data = 0) : base(0x711, 8, data)
					{
					}

					public const UInt16 _id = 0x711;

	
				}

				class mppt2: Packet
				{
					public mppt2(UInt64 data = 0) : base(0x712, 8, data)
					{
					}

					public const UInt16 _id = 0x712;

	
				}

				class mppt3: Packet
				{
					public mppt3(UInt64 data = 0) : base(0x713, 8, data)
					{
					}

					public const UInt16 _id = 0x713;

	
				}
			}

			namespace mppt
			{
				class unused0: Packet
				{
					public unused0(UInt64 data = 0) : base(0x770, 8, data)
					{
					}

					public const UInt16 _id = 0x770;

	
				}

				class mppt1: Packet
				{
					public mppt1(UInt64 data = 0) : base(0x771, 8, data)
					{
					}

					public const UInt16 _id = 0x771;

						public UInt16 flags_Vin
					{
						get { return this.frame.uint16x4.uint16_0; }
						set { this.frame.uint16x4.uint16_0 = value; }
					}

					public UInt16 flags_Iin
					{
						get { return this.frame.uint16x4.uint16_1; }
						set { this.frame.uint16x4.uint16_1 = value; }
					}

					public UInt16 flags_Vout
					{
						get { return this.frame.uint16x4.uint16_2; }
						set { this.frame.uint16x4.uint16_2 = value; }
					}

					public UInt16 flags_Tout
					{
						get { return this.frame.uint16x4.uint16_3; }
						set { this.frame.uint16x4.uint16_3 = value; }
					}

				}

				class mppt2: Packet
				{
					public mppt2(UInt64 data = 0) : base(0x772, 8, data)
					{
					}

					public const UInt16 _id = 0x772;

						public UInt16 flags_Vin
					{
						get { return this.frame.uint16x4.uint16_0; }
						set { this.frame.uint16x4.uint16_0 = value; }
					}

					public UInt16 flags_Iin
					{
						get { return this.frame.uint16x4.uint16_1; }
						set { this.frame.uint16x4.uint16_1 = value; }
					}

					public UInt16 flags_Vout
					{
						get { return this.frame.uint16x4.uint16_2; }
						set { this.frame.uint16x4.uint16_2 = value; }
					}

					public UInt16 flags_Tout
					{
						get { return this.frame.uint16x4.uint16_3; }
						set { this.frame.uint16x4.uint16_3 = value; }
					}

				}

				class mppt3: Packet
				{
					public mppt3(UInt64 data = 0) : base(0x773, 8, data)
					{
					}

					public const UInt16 _id = 0x773;

						public UInt16 flags_Vin
					{
						get { return this.frame.uint16x4.uint16_0; }
						set { this.frame.uint16x4.uint16_0 = value; }
					}

					public UInt16 flags_Iin
					{
						get { return this.frame.uint16x4.uint16_1; }
						set { this.frame.uint16x4.uint16_1 = value; }
					}

					public UInt16 flags_Vout
					{
						get { return this.frame.uint16x4.uint16_2; }
						set { this.frame.uint16x4.uint16_2 = value; }
					}

					public UInt16 flags_Tout
					{
						get { return this.frame.uint16x4.uint16_3; }
						set { this.frame.uint16x4.uint16_3 = value; }
					}

				}
			}
		}
	}
}


