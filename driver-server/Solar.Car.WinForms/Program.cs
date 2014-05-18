using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using Debug = System.Diagnostics.Debug;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Solar.Car.WinForms
{
	public class MainForm: Form
	{
		Label label1 = new Label();
		Button button1 = new Button();
		Button button2 = new Button();
		TableLayoutPanel panel1 = new TableLayoutPanel();
		bool solarcar_running = false;
		CancellationTokenSource solarcar_cancel = null;

		public MainForm()
		{
			this.SuspendLayout();

			// Initialize your components here
			label1.Text = "GUI and Telemetry are: " + "STOPPED";
//			label1.Dock = DockStyle.Fill;

			button1.DialogResult = DialogResult.OK;
			button1.Text = "STOP";
			button1.Click += new EventHandler(this.Button_Click);
//			button1.Dock = DockStyle.Fill;

			button2.DialogResult = DialogResult.OK;
			button2.Text = "Show";
			button2.Click += new EventHandler(this.Button2_Click);
//			button2.Dock = DockStyle.Fill;

			// Add components to panel
			panel1.ColumnCount = 1;
			panel1.Controls.Add(label1);
			panel1.Controls.Add(button1);
			panel1.Controls.Add(button2);
			foreach (ColumnStyle style in panel1.ColumnStyles)
			{
				style.SizeType = SizeType.Percent;
				style.Width = 100;
			}
			foreach (RowStyle style in panel1.RowStyles)
			{
				style.SizeType = SizeType.Percent;
				style.Height = 100f / panel1.RowCount;
			}
			this.Controls.Add(panel1);

			this.ResumeLayout();

			this.Name = "ZELDA";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "ZELDA Telemetry";
			this.FormClosing += new FormClosingEventHandler(this.Form_Closing);

			this.SetRunning(true);
		}

		public void Button_Click(object sender, EventArgs e)
		{
			if (!this.solarcar_running)
			{
				this.SetRunning(true);
			}
			else if (!this.solarcar_cancel.IsCancellationRequested)
			{
				this.solarcar_cancel.Cancel();
			}
		}

		public void Button2_Click(object sender, EventArgs e)
		{
			if (this.solarcar_running)
			{
				System.Diagnostics.Process.Start(Config.HTTPSERVER_CAR_URL);
			}
		}

		public void Form_Closing(object sender, FormClosingEventArgs e)
		{
			if (!this.solarcar_cancel.IsCancellationRequested)
				this.solarcar_cancel.Cancel();
		}

		public void SetRunning(bool run)
		{
			if (run)
			{
				this.solarcar_running = true;
				button1.Text = "STOP";
				label1.Text = "GUI and Telemetry are: RUNNING";
				this.solarcar_cancel = new CancellationTokenSource();
				this.SolarCarMain();
			}
			else
			{
				this.solarcar_running = false;
				button1.Text = "RUN";
				label1.Text = "GUI and Telemetry are: STOPPED";
				if (!this.solarcar_cancel.IsCancellationRequested)
					this.solarcar_cancel.Cancel();
			}
		}

		public async Task SolarCarMain()
		{
			try
			{
				using (var ds = new JsonDataSource(Config.Resource_Prefix + Config.DB_JSON_CAR_FILE))
				{
					await Solar.Program.RunProgram(this.solarcar_cancel.Token, ds, new CommManager(), new HttpGui());
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine("PROGRAM: EXCEPTION: " + e.ToString());
			}
			finally
			{
				this.SetRunning(false);
			}
		}
	}

	public class Program
	{
		public static void Main(string[] args)
		{
			// Set resource prefix
			Config.Resource_Prefix = "";
			// Enable debugging
			Debug.Listeners.Add(new System.Diagnostics.TextWriterTraceListener("debug.log"));
			Debug.WriteLine("PROGRAM: Hello World!");
			// Setup SolarCar environment
			if (Environment.OSVersion.Platform.HasFlag(PlatformID.MacOSX) ||
			    Environment.OSVersion.Platform.HasFlag(PlatformID.Unix))
			{
				Config.Platform = Config.PlatformID.Unix;
			}
			else
			{
				Config.Platform = Config.PlatformID.Win32;
			}
			// Load configuration from file
			Config.LoadConfig(System.IO.File.ReadAllText(@"Config.json"));


			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
	}
}
