using System;

namespace SolarCar {
	/// <summary>
	/// Wraps User commands.
	/// </summary>
	class UserCommands {
		readonly Hardware hardware = null;

		public UserCommands(Hardware hw) {
			this.hardware = hw;
		}

		public bool Drive { 
			get { return this.hardware.UserDrive; } 
			set { this.hardware.UserDrive = value; }
		}

		public bool HeadLights {
			get { return this.hardware.UserHeadlights;}
			set { this.hardware.UserHeadlights = value;}
		}

		/// <summary>
		/// Assignable value representing the left turn signal.
		/// </summary>
		/// <value><c>true</c> if left signal; otherwise, <c>false</c>.</value>
		public bool LeftSignal {
			get { return this.hardware.UserSignal == Hardware.Signals.Left;}
			set { this.hardware.UserSignal = value ? Hardware.Signals.Left : this.hardware.UserSignal;}
		}

		/// <summary>
		/// Assignable value representing the right turn signal.
		/// </summary>
		/// <value><c>true</c> if right signal; otherwise, <c>false</c>.</value>
		public bool RightSignal {
			get { return this.hardware.UserSignal == Hardware.Signals.Right;}
			set { this.hardware.UserSignal = value ? Hardware.Signals.Right : this.hardware.UserSignal;}
		}

		/// <summary>
		/// Assignable value representing the hazard lights.
		/// </summary>
		/// <value><c>true</c> if hazards; otherwise, <c>false</c>.</value>
		public bool Hazards {
			get { return this.hardware.UserSignal == Hardware.Signals.Hazards;}
			set { this.hardware.UserSignal = value ? Hardware.Signals.Hazards : this.hardware.UserSignal;}
		}

		/// <summary>
		/// Assignable value representing the horn.
		/// </summary>
		/// <value><c>true</c> if horn; otherwise, <c>false</c>.</value>
		public bool Horn {
			get { return this.hardware.Horn;}
			set { this.hardware.Horn = value;}
		}

		public CarReport Report { get { return this.hardware.Report; } }
	}
}

