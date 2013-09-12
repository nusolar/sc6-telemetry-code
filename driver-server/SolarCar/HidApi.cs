using System;
using System.Runtime.InteropServices;

namespace SolarCar {
	/// <summary>
	/// Wrapper to native USB HID API.
	/// Internally, Acquire the api_lock before calling ANY DllImport function!
	/// @todo manage allocation/deallocation of devices internally, using RAII.
	/// </summary>
	class HidDevice {
		static object api_lock = new Object();
		static object refcount_lock = new Object();
		static int refcount = 0;
		static bool _Initialized = false;
		IntPtr ptr = IntPtr.Zero;
		bool _Open = false;
		#region P/Invoked USB HID APÎ
		/// <summary>
		/// Initialize HID stack. Call once, before any other thread access.
		/// </summary>
		[DllImport("hidapi.so")]
		static extern int hid_init();

		[DllImport("hidapi.so")]
		static extern int hid_exit();

		/// <summary>
		/// Open the HID device specified by vendor_id, product_id and serial_number.
		/// Note: serial_number is encoded UTF32 on Linux/Mac, but UTF16 on Windows!!!!!
		/// </summary>
		/// <param name="vendor_id">Vendor_id.</param>
		/// <param name="product_id">Product_id.</param>
		/// <param name="serial_number">Serial_number.</param>
		[DllImport("hidapi.so")]
		static extern IntPtr hid_open(
			ushort vendor_id,
			ushort product_id,
			//			[MarshalAs(UnmanagedType.LPArray)]
			IntPtr serial_number);

		[DllImport("hidapi.so")]
		static extern int hid_set_nonblocking(
			IntPtr device,
			int nonblock);

		[DllImport("hidapi.so")]
		static extern int hid_write(
			IntPtr device,
			[In, MarshalAs(UnmanagedType.LPArray)]
			byte[] data,
			[MarshalAs(UnmanagedType.SysUInt)]
			int length);

		[DllImport("hidapi.so")]
		static extern int hid_read(
			IntPtr device,
			[Out, MarshalAs(UnmanagedType.LPArray)]
			byte[] data,
			[MarshalAs(UnmanagedType.SysUInt)]
			int length);

		[DllImport("hidapi.so")]
		static extern int hid_read_timeout(
			IntPtr device,
			[Out, MarshalAs(UnmanagedType.LPArray)]
			byte[] data,
			[MarshalAs(UnmanagedType.SysUInt)]
			int length,
			int milliseconds);

		[DllImport("hidapi.so")]
		static extern void hid_close(
			IntPtr device);
		#endregion
		#region API Wrapper
		// TODO: make these functions static, or use this.ptr instead of taking IntPtr device. Null-check all IntPtrs
		static Int32[] StringToUtf32(string s) {
			byte[] b = System.Text.Encoding.UTF32.GetBytes(s);
			Int32[] output = new Int32[b.Length / 4];
			Buffer.BlockCopy(b, 0, output, 0, b.Length);
			return output;
		}

		bool init() {
			lock (api_lock) {
				return hid_init() == 0 ? true : false;
			}
		}

		bool exit() {
			lock (api_lock) {
				return hid_exit() == 0 ? true : false;
			}
		}

		IntPtr open(ushort vendor_id, ushort product_id, string serial_number) {
			lock (api_lock) {
				return hid_open(vendor_id, product_id, IntPtr.Zero);
			}
		}

		bool set_nonblocking(IntPtr device, bool nonblock) {
			lock (api_lock) {
				return hid_set_nonblocking(device, nonblock ? 1 : 0) == 0 ? true : false;
			}
		}

		int write(IntPtr device, byte[] data) {
			lock (api_lock) {
				return hid_write(device, data, data.Length);
			}
		}

		int read(IntPtr device, byte[] data) {
			lock (api_lock) {
				return hid_read(device, data, data.Length);
			}
		}

		int read_timeout(IntPtr device, byte[] data, int milliseconds) {
			lock (api_lock) {
				return hid_read_timeout(device, data, data.Length, milliseconds);
			}
		}

		void close(IntPtr device) {
			if (device != IntPtr.Zero) {
				lock (api_lock) {
					hid_close(device);
				}
			}
		}
		#endregion
		/// <summary>
		/// Initializes a new instance of the SolarCar.HidApi class.
		/// The first instance must call this.init();
		/// </summary>
		public HidDevice(ushort vendor_id, ushort product_id, string serial_number) {
			lock (refcount_lock) {
				if (refcount == 0) {
					_Initialized = this.init();
				}
				refcount++;
			}
			this.ptr = this.open(vendor_id, product_id, serial_number);
			if (this.ptr != IntPtr.Zero) {
				this._Open = true;
				this.set_nonblocking(this.ptr, true);
			}
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the SolarCar.HidApi is
		/// reclaimed by garbage collection. The last instance must call this.exit();
		/// </summary>
		~HidDevice() {
			this.close(this.ptr);
			this._Open = false;
			lock (refcount_lock) {
				refcount--;
				if (refcount == 0) {
					this.exit();
					_Initialized = false;
				}
			}
		}

		public bool Open { get { return this._Open; } }

		public int Write(byte[] data) {
			return (this.Open) ? this.write(this.ptr, data) : -2;
		}

		public int Read(byte[] data) {
			return (this.Open) ? this.read(this.ptr, data) : -2;
		}

		public static bool Initialized { get { return _Initialized; } }
	}
}

