using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using Debug = System.Diagnostics.Debug;

namespace Solar.Car.WinForms
{
	public class MainForm: Form
	{
		Label label1 = new Label();
		Button main_button = new Button();
		Button gui_button = new Button();
		TableLayoutPanel panel1 = new TableLayoutPanel();
		bool solarcar_running = false;
		CancellationTokenSource solarcar_cancel = null;

		public MainForm()
		{
			this.SuspendLayout();

			// Initialize your components here
			label1.Text = "GUI and Telemetry are: " + "STOPPED";
			//			label1.Dock = DockStyle.Fill;

			main_button.DialogResult = DialogResult.OK;
			main_button.Text = "STOP";
			main_button.Click += new EventHandler(this.MainButton_Click);
			//			button1.Dock = DockStyle.Fill;

			gui_button.DialogResult = DialogResult.OK;
			gui_button.Text = "Show";
			gui_button.Click += new EventHandler(this.GuiButton_Click);
			//			button2.Dock = DockStyle.Fill;

			// Add components to panel
			panel1.ColumnCount = 1;
			panel1.Controls.Add(label1);
			panel1.Controls.Add(main_button);
			panel1.Controls.Add(gui_button);
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

		public void MainButton_Click(object sender, EventArgs e)
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

		public void GuiButton_Click(object sender, EventArgs e)
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
				main_button.Text = "STOP";
				label1.Text = "GUI and Telemetry are: RUNNING";
				this.solarcar_cancel = new CancellationTokenSource();
				Task.Run(function: this.SolarCarMain);
			}
			else
			{
				this.solarcar_running = false;
				main_button.Text = "RUN";
				label1.Text = "GUI and Telemetry are: STOPPED";
				if (!this.solarcar_cancel.IsCancellationRequested)
					this.solarcar_cancel.Cancel();
			}
		}

		public async Task SolarCarMain()
		{
			try
			{
				using (var ds = new JsonDataSource())
				{
					await Solar.Program.RunProgram(this.solarcar_cancel.Token, ds, new CommManager(), new HttpGui());
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine("PROGRAM:\tRunProgram: EXCEPTION: " + e.ToString());
			}
			finally
			{
				this.SetRunning(false);
			}
		}
	}
}