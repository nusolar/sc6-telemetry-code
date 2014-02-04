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
			namespace bps
			{
				namespace rx
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

				namespace tx
				{
					class heartbeat: Packet
					{
						public heartbeat(UInt64 data = 0) : base(0x210, 8, data)
						{
						}

						public const UInt16 _id = 0x210;

							public UInt32 bms_str
						{
							get { return this.frame.status.uint32_0; }
							set { this.frame.status.uint32_0 = value; }
						}

						public UInt32 uptime_s
						{
							get { return this.frame.status.uint32_1; }
							set { this.frame.status.uint32_1 = value; }
						}

					}

					class errors: Packet
					{
						public errors(UInt64 data = 0) : base(0x211, 8, data)
						{
						}

						public const UInt16 _id = 0x211;

							public Int16 error
						{
							get { return this.frame.int16x4.int16_0; }
							set { this.frame.int16x4.int16_0 = value; }
						}

						public Int16 error_value
						{
							get { return this.frame.int16x4.int16_1; }
							set { this.frame.int16x4.int16_1 = value; }
						}

						public Int16 last_error
						{
							get { return this.frame.int16x4.int16_2; }
							set { this.frame.int16x4.int16_2 = value; }
						}

						public Int16 last_error_value
						{
							get { return this.frame.int16x4.int16_3; }
							set { this.frame.int16x4.int16_3 = value; }
						}

					}

					class bps_status: Packet
					{
						public bps_status(UInt64 data = 0) : base(0x212, 8, data)
						{
						}

						public const UInt16 _id = 0x212;

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

					class current: Packet
					{
						public current(UInt64 data = 0) : base(0x213, 8, data)
						{
						}

						public const UInt16 _id = 0x213;

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
						public voltage_temp(UInt64 data = 0) : base(0x214, 8, data)
						{
						}

						public const UInt16 _id = 0x214;

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
						public cc_array(UInt64 data = 0) : base(0x215, 8, data)
						{
						}

						public const UInt16 _id = 0x215;

							public Double count
						{
							get { return this.frame.float64x1.double_0; }
							set { this.frame.float64x1.double_0 = value; }
						}

					}

					class cc_batt: Packet
					{
						public cc_batt(UInt64 data = 0) : base(0x216, 8, data)
						{
						}

						public const UInt16 _id = 0x216;

							public Double count
						{
							get { return this.frame.float64x1.double_0; }
							set { this.frame.float64x1.double_0 = value; }
						}

					}

					class cc_mppt1: Packet
					{
						public cc_mppt1(UInt64 data = 0) : base(0x217, 8, data)
						{
						}

						public const UInt16 _id = 0x217;

							public Double count
						{
							get { return this.frame.float64x1.double_0; }
							set { this.frame.float64x1.double_0 = value; }
						}

					}

					class cc_mppt2: Packet
					{
						public cc_mppt2(UInt64 data = 0) : base(0x218, 8, data)
						{
						}

						public const UInt16 _id = 0x218;

							public Double count
						{
							get { return this.frame.float64x1.double_0; }
							set { this.frame.float64x1.double_0 = value; }
						}

					}

					class cc_mppt3: Packet
					{
						public cc_mppt3(UInt64 data = 0) : base(0x219, 8, data)
						{
						}

						public const UInt16 _id = 0x219;

							public Double count
						{
							get { return this.frame.float64x1.double_0; }
							set { this.frame.float64x1.double_0 = value; }
						}

					}

					class Wh_batt: Packet
					{
						public Wh_batt(UInt64 data = 0) : base(0x21a, 8, data)
						{
						}

						public const UInt16 _id = 0x21a;

							public Double count
						{
							get { return this.frame.float64x1.double_0; }
							set { this.frame.float64x1.double_0 = value; }
						}

					}

					class Wh_mppt1: Packet
					{
						public Wh_mppt1(UInt64 data = 0) : base(0x21b, 8, data)
						{
						}

						public const UInt16 _id = 0x21b;

							public Double count
						{
							get { return this.frame.float64x1.double_0; }
							set { this.frame.float64x1.double_0 = value; }
						}

					}

					class Wh_mppt2: Packet
					{
						public Wh_mppt2(UInt64 data = 0) : base(0x21c, 8, data)
						{
						}

						public const UInt16 _id = 0x21c;

							public Double count
						{
							get { return this.frame.float64x1.double_0; }
							set { this.frame.float64x1.double_0 = value; }
						}

					}

					class Wh_mppt3: Packet
					{
						public Wh_mppt3(UInt64 data = 0) : base(0x21d, 8, data)
						{
						}

						public const UInt16 _id = 0x21d;

							public Double count
						{
							get { return this.frame.float64x1.double_0; }
							set { this.frame.float64x1.double_0 = value; }
						}

					}

					class trip_pt: Packet
					{
						public trip_pt(UInt64 data = 0) : base(0x21e, 8, data)
						{
						}

						public const UInt16 _id = 0x21e;

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

					class trip_pt_voltage_temp: Packet
					{
						public trip_pt_voltage_temp(UInt64 data = 0) : base(0x21f, 8, data)
						{
						}

						public const UInt16 _id = 0x21f;

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
			}

			namespace ws20
			{
				namespace rx
				{
					class driver_controls_id: Packet
					{
						public driver_controls_id(UInt64 data = 0) : base(0x500, 8, data)
						{
						}

						public const UInt16 _id = 0x500;

							public UInt32 drvId
						{
							get { return this.frame.status.uint32_0; }
							set { this.frame.status.uint32_0 = value; }
						}

						public UInt32 serialNo
						{
							get { return this.frame.status.uint32_1; }
							set { this.frame.status.uint32_1 = value; }
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

							public UInt32 unused1
						{
							get { return this.frame.status.uint32_0; }
							set { this.frame.status.uint32_0 = value; }
						}

						public UInt32 unused2
						{
							get { return this.frame.status.uint32_1; }
							set { this.frame.status.uint32_1 = value; }
						}

					}
				}

				namespace tx
				{
					class motor_id: Packet
					{
						public motor_id(UInt64 data = 0) : base(0x400, 8, data)
						{
						}

						public const UInt16 _id = 0x400;

							public UInt32 tritiumId
						{
							get { return this.frame.status.uint32_0; }
							set { this.frame.status.uint32_0 = value; }
						}

						public UInt32 serialNo
						{
							get { return this.frame.status.uint32_1; }
							set { this.frame.status.uint32_1 = value; }
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
			}

			namespace pedals
			{
				namespace rx
				{
					class cruise: Packet
					{
						public cruise(UInt64 data = 0) : base(0x100, 8, data)
						{
						}

						public const UInt16 _id = 0x100;

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

				namespace tx
				{
					class pedals: Packet
					{
						public pedals(UInt64 data = 0) : base(0x110, 8, data)
						{
						}

						public const UInt16 _id = 0x110;

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

						public UInt16 reserved1
						{
							get { return this.frame.uint16x4.uint16_3; }
							set { this.frame.uint16x4.uint16_3 = value; }
						}

					}
				}
			}

			namespace os
			{
				namespace tx
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
				}
			}

			namespace mppt
			{
				namespace rx
				{
					class mppt: Packet
					{
						public mppt(UInt64 data = 0) : base(0x710, 8, data)
						{
						}

						public const UInt16 _id = 0x710;

	
					}
				}

				namespace tx
				{
					class mppt: Packet
					{
						public mppt(UInt64 data = 0) : base(0x710, 8, data)
						{
						}

						public const UInt16 _id = 0x710;

	
					}
				}
			}
		}
	}
}


