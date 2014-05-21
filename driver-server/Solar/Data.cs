using System;

namespace Solar
{
	/// BMS state, taken from Tritium BMS Manual
	public enum Precharge
	{
		/// BMS is tripping
		Error = 0,
		/// BMS is ready, but all Contactors are open.
		Idle = 1,
		Enable = 5,
		Measure = 2,
		/// Contactor 1 is closed, MC is charging.
		Precharge = 3,
		/// Contactor 2 is closed, MC is charged, car is running (not necessarily driving).
		Run = 4
	}

	/// The driver controls' gear, and "key position"
	[Flags]
	public enum Gear
	{
		/// Key is in off
		None = 0,
		/// Car has "started," BMS is Running, and MC is precharged, but driving is disabled (velocity = 0).
		Run = 1,
		/// Car moves, velocity is positive.
		Drive = 2,
		/// Velocity is negative, car moves backwards
		Reverse = 4
	}

	/// The driver controls' signals state.
	[Flags]
	public enum Signals
	{
		/// All lights disabled, Horn off
		None = 0,
		/// Left signal flashes
		Left = 1,
		/// Right signal flashes
		Right = 2,
		/// Headlights flash
		Headlights = 4,
		/// Horn is sounding
		Horn = 8
	}

	/// The aggregate status of the car. Status only contains ValueTypes.
	// [Table(Name = "Status")]
	public class Status
	{
		/// <summary>
		/// Timestamp, used by CarDatabase.
		/// </summary>
		// [Column(IsPrimaryKey = true, Storage = "Timestamp", DbType = "INT", CanBeNull = false)]
		public double Timestamp { get; set; }

#region Driver Controls

		/// The motor setting
		// [Column(Storage = "Gear", DbType = "INT", CanBeNull = false)]
		public Gear Gear { get; set; }

		/// The signals
		// [Column(Storage = "Signals", DbType = "INT", CanBeNull = false)]
		public Signals Signals { get; set; }

		/// The value of the Analog accel pedal, between 0-1023
		// [Column(Storage = "AccelPedal", DbType = "INT", CanBeNull = false)]
		public UInt16 AccelPedal { get; set; }

		/// The value of the Analog accel pedal, between 0-1023
		// [Column(Storage = "RegenPedal", DbType = "INT", CanBeNull = false)]
		public UInt16 RegenPedal { get; set; }

		/// <summary>
		/// Whether the car's brake pedal is pressed.
		/// </summary>
		/// <value><c>true</c> if braking; otherwise, <c>false</c>.</value>
		// [Column(Storage = "BrakePedal", DbType = "INT", CanBeNull = false)]
		public bool BrakePedal { get; set; }

#endregion

#region Motor

		/// MotorRpm
		// [Column(Storage = "MotorVoltage", DbType = "REAL", CanBeNull = false)]
		public float MotorRpm { get; set; }

		/// Motor velocity in m/s, computed by Motor Controller using the tire diameter.
		// [Column(Storage = "MotorVelocity", DbType = "REAL", CanBeNull = false)]
		public float MotorVelocity { get; set; }

		/// HV-DC bus voltage in Volts
		// [Column(Storage = "MotorVoltage", DbType = "REAL", CanBeNull = false)]
		public float MotorVoltage { get; set; }

		/// Current through motor controller in Amps
		// [Column(Storage = "MotorCurrent", DbType = "REAL", CanBeNull = false)]
		public float MotorCurrent{ get; set; }

		/// <summary>
		/// Distance vehicle has travelled since reset.
		/// </summary>
		public float MotorOdometer { get; set; }

		/// <summary>
		/// Charge flow into controller since reset.
		/// </summary>
		public float MotorAmpHours { get; set; }

#endregion

#region BPS

		/// <summary>
		/// The Ah consumed from the pack. 0=Full, counts to user-set max capacity.
		/// </summary>
		// [Column(Storage = "PackSOC", DbType = "REAL", CanBeNull = false)]
		public Single PackSOC  { get; set; }

		/// <summary>
		/// The pack State-Of-Charge as a percentage.
		/// </summary>
		// [Column(Storage = "PackSOCPerc", DbType = "REAL", CanBeNull = false)]
		public Single PackSOCPerc  { get; set; }

		/// <summary>
		/// The BMS State/Precharge value, taken from Tritium's BMS manual.
		/// </summary>
		// [Column(Storage = "BMSPrecharge", DbType = "INT", CanBeNull = false)]
		public Precharge BMSPrecharge  { get; set; }

		/// <summary>
		/// The highest cell voltage in mV.
		/// </summary>
		// [Column(Storage = "MaxVoltage", DbType = "INT", CanBeNull = false)]
		public UInt16 MaxVoltage  { get; set; }

		/// The lowest cell voltage in mV.
		// [Column(Storage = "MinVoltage", DbType = "INT", CanBeNull = false)]
		public UInt16 MinVoltage  { get; set; }

		/// <summary>
		/// The highest cell temp in decidegrees Celcius.
		/// </summary>
		// [Column(Storage = "MaxTemp", DbType = "INT", CanBeNull = false)]
		public UInt16 MaxTemp  { get; set; }

		/// <summary>
		/// The lowest cell temp in decidegrees Celcius.
		/// </summary>
		// [Column(Storage = "MinTemp", DbType = "INT", CanBeNull = false)]
		public UInt16 MinTemp  { get; set; }

		/// <summary>
		/// The pack voltage in mV.
		/// </summary>
		// [Column(Storage = "PackVoltage", DbType = "INT", CanBeNull = false)]
		public Int32 PackVoltage  { get; set; }

		/// <summary>
		/// The pack current in mA.
		/// </summary>
		// [Column(Storage = "PackCurrent", DbType = "INT", CanBeNull = false)]
		public Int32 PackCurrent  { get; set; }

		/// <summary>
		/// The BMS extended status flags.
		/// </summary>
		// [Column(Storage = "BMSExtendedStatusFlags", DbType = "INT", CanBeNull = false)]
		public UInt32 BMSExtendedStatusFlags  { get; set; }

#endregion

		/// <summary>
		/// Clone the Status values.
		/// </summary>
		public object DeepClone()
		{
			return this.MemberwiseClone();
		}
	}

	/// <summary>
	/// Driver inputs from GUI.
	/// </summary>
	public class DriverInput
	{
		/// <summary>
		/// The requested gear.
		/// </summary>
		volatile public Solar.Gear gear = Solar.Gear.None;
		/// <summary>
		/// The requested signals.
		/// </summary>
		volatile public Solar.Signals sigs = Solar.Signals.None;
	}
}
