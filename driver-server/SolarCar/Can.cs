using System;
using System.Runtime.InteropServices;

namespace SolarCar
{
	namespace Can
	{
		namespace Addr
		{
			static class pedals
			{
				public const int Base = 0x110;
				public const int Status = Base + 1;
			}

			static class bps
			{
				public const int RxBase = 0x200;
			}

			static class ws20
			{
				public const int Base = 0x400;
				public const int motor_bus = Base + 2;
				public const int motor_velocity = Base + 3;
			}
			namespace os
			{
				static class tx
				{
					public const int driver_input_k = 0x310;
				}
			}
		}
		[StructLayout(LayoutKind.Explicit)]
		struct UInt64x1
		{
			[FieldOffset(0)]
			public UInt64 uint64_0;
		}

		[StructLayout(LayoutKind.Explicit)]
		struct Float32x2
		{
			[FieldOffset(0)]
			public Single float_0;
			[FieldOffset(4)]
			public Single float_1;
		}

		[StructLayout(LayoutKind.Explicit)]
		struct UInt16x4
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
		struct Int16x4
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
		struct UInt8x8
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
		struct Frame
		{
			[FieldOffset(0)]
			public UInt64 data;
			[FieldOffset(0)]
			public UInt8x8 bytes;
			[FieldOffset(0)]
			public UInt64x1 uint64x1;
			[FieldOffset(0)]
			public UInt16x4 uint16x4;
			[FieldOffset(0)]
			public Int16x4 int16x4;
			[FieldOffset(0)]
			public Float32x2 float32x2;
		}

		class Packet
		{
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
	}
}
