using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace TraySysInfo
{
	public sealed class NotificationIcon
	{
		const string version = "0.2";
		const string name = "TraySysInfo";
		
		// Очистка RAM
		[System.Runtime.InteropServices.DllImport("psapi.dll", EntryPoint = "EmptyWorkingSet")]
		private static extern bool EmptyWorkingSet(IntPtr hProcess);

		private Timer timer;
		private NotifyIcon notifyIconCpu;
		private NotifyIcon notifyIconRam;
		private PerformanceCounter cpuCounter;
        private PerformanceCounter ramCounter;
        private Bitmap bitmap;
        private Font font;
        private Graphics graphics;
        private PointF cpuPoint;
        private PointF ramPoint;
        private float cpu;
        private float ram;
        
		
		#region Initialize icon, perfomance and menu
		public NotificationIcon()
		{
			try {
				this.InitializePerformanceCounters();
				this.InitializePoints();
				this.InitializeFont();
				this.InitializeBitmap();
				this.InitializeGraphics();
				this.InitializeTimer();
				
	
				notifyIconCpu = new NotifyIcon();
				notifyIconRam = new NotifyIcon();


	    		Icon iconCpu = this.DrawIconCpu();
	    		Icon iconRam = this.DrawIconRam();
				notifyIconCpu.Icon = iconCpu;
				notifyIconRam.Icon = iconRam;
				iconCpu.Dispose();
				iconRam.Dispose();
				
				String text = this.DrawText();
				notifyIconCpu.Text = text;
				notifyIconRam.Text = text;

				notifyIconCpu.ContextMenu = this.InitializeMenu();
				notifyIconRam.ContextMenu = this.InitializeMenu();
				notifyIconCpu.Visible = true;
				notifyIconRam.Visible = true;
			} catch (Exception exception) {
				//MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Application.Exit();
			}
		}
		
		
		/**
		 * Рисуем заначения CPU на иконке
		 */
		private Icon DrawIconCpu()
		{
			graphics.Clear(Color.Transparent);
			
			Brush brushCpu;
			
			if (cpu > 50) {
				brushCpu = Brushes.Red;
			} else if (cpu > 30) {
				brushCpu = Brushes.Yellow;
			} else {
				brushCpu = Brushes.Black;
			}
			
			graphics.DrawString(this.FormatCpu(cpu), font, brushCpu, cpuPoint);

			return Icon.FromHandle(this.GetHicon());
		}
		
		
		/**
		 * Рисуем заначения RAM на иконке
		 */
		private Icon DrawIconRam()
		{
			graphics.Clear(Color.Transparent);
			
			Brush brushRam;
			
			if (ram < 500) {
				brushRam = Brushes.Yellow;
			} else if (ram < 100) {
				brushRam = Brushes.Red;
			} else {
				brushRam = Brushes.Black;
			}
			
			graphics.DrawString(this.FormatRam(ram), font, brushRam, ramPoint);

			return Icon.FromHandle(this.GetHicon());
		}

		/**
		 * Получаем палитру
		 */
		private IntPtr GetHicon()
		{
			return bitmap.GetHicon();
		}

		
		/**
		 * Пишем заначения CPU и RAM
		 */
		private String DrawText()
		{
			return name + " " + version + Environment.NewLine + "CPU: " + this.FormatCpuEx(cpu) + Environment.NewLine + "RAM: " + this.FormatRamEx(ram);
		}

		
		/**
		 * Инициализируем объекты со счетчиками
		 */
		private void InitializePerformanceCounters()
		{
			cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
			ramCounter = new PerformanceCounter("Memory", "Available MBytes", true);

			cpu = this.GetCpu();
        	ram = this.GetRam();
		}
		
		
		/**
		 * Инициализируем объекты с позициями счетчиков
		 */
		private void InitializePoints()
		{
			cpuPoint = new PointF(0, 2);
			ramPoint = new PointF(0, 2);
		}
		
		
		/**
		 * Инициализируем объект таймера обновления счетчиков
		 */
		private void InitializeTimer()
		{
			timer = new Timer();
			timer.Enabled = true;
            timer.Interval = 2000;
            timer.Tick += new EventHandler(this.TimerTick);
		}
		
		
		/**
		 * Инициализируем объект шрифта для счетчиков
		 */
		private void InitializeFont()
		{
			font = new Font("Calibri", 8, FontStyle.Bold, GraphicsUnit.Point);
		}


		/**
		 * Инициализируем объект графики для счетчиков
		 */
		private void InitializeGraphics()
		{
			graphics = Graphics.FromImage(bitmap);
			

			//graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
			graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
		}
	
		
		/**
		 * Инициализируем объект цветовой палитры счетчиков
		 */
		private void InitializeBitmap()
		{
			bitmap = new Bitmap(16, 16);
			//bitmap.MakeTransparent(Color.White);
		}
		
		
		/**
		 * Получаем занчение CPU
		 */
		private float GetCpu()
		{
			try {
				return cpuCounter.NextValue();
			} catch (Exception/* exception*/) {
				return (float)-1;
			}
		}
		
		
		/**
		 * Получаем занчение RAM
		 */
		private float GetRam()
		{
			try {
				return ramCounter.NextValue();
			} catch (Exception/* exception*/) {
				return (float)-1;
			}
		}
		
		
		/**
		 * Форматируем занчение CPU
		 */
		private string FormatCpu(float cpu)
		{
			return Math.Round((decimal)cpu, 1).ToString()/* + "%"*/;
		}
		

		/**
		 * Форматируем занчение RAM
		 */		
		private string FormatRam(float ram)
		{
			if (ram < 1000) {
				return ram.ToString()/* + "M"*/;
			} else if (ram < 10000) {
				return Math.Round((decimal)(ram / 1000), 1).ToString()/* + "G"*/;
			} else {
				return Math.Round((decimal)(ram / 10000), 1).ToString()/* + "G"*/;
			}
		}
		
		
		/**
		 * Форматируем занчение CPU
		 */
		private string FormatCpuEx(float cpu)
		{
			return Math.Round((decimal)cpu, 2).ToString() + " %";
		}
		

		/**
		 * Форматируем занчение RAM
		 */		
		private string FormatRamEx(float ram)
		{
			if (ram < 1000) {
				return ram.ToString() + " M";
			} else {
				return Math.Round((decimal)(ram / 1000), 3).ToString() + " G";
			}
		}
		
		
		/**
		 * Добавлена ли программа в автозагрузку
		 */
		private bool IsAutoStarted()
		{
			Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false);
			bool output = (regKey.GetValue(name) != null);
			regKey.Close();

			return output;
		}
		
		
		/**
		 * Добавляем программу в автозагрузку
		 */
		private void AddAutoStart()
		{
			Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
			regKey.SetValue(name, Application.ExecutablePath);
			regKey.Close();
		}
		
		
		/**
		 * Удаляем программу из автозагрузки
		 */
		private void DelAutoStart()
		{
			Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
			regKey.DeleteValue(name);
			regKey.Close();
		}
		
		
		/**
		 * Чистим RAM
		 */
		private void CleanRam()
		{
			int yes = 0;
			//int no = 0;
			
			foreach (Process p in Process.GetProcesses()) {
				try{
					EmptyWorkingSet(p.Handle);
					yes++;
				} catch (Exception/* exception*/) {
					//no++;
				}
            }

			MessageBox.Show("Cleaned " + yes.ToString() + " processes", "Clean RAM", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
		

		/**
		 * Инициализируем меню
		 */
		private ContextMenu InitializeMenu()
		{
			MenuItem autoStartup = new MenuItem("Auto start", this.MenuAutoStartClick);
			autoStartup.Checked = this.IsAutoStarted();

			return new ContextMenu(new MenuItem[] {
			    new MenuItem("Clean RAM", this.MenuCleanRamClick),
				autoStartup,
				new MenuItem("About", this.MenuAboutClick),
				new MenuItem("Exit", this.MenuExitClick)
			});
		}
		#endregion
		
		
		#region Main - Program entry point
		/// <summary>Program entry point.</summary>
		/// <param name="args">Command Line Arguments</param>
		[STAThread]
		public static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			NotificationIcon notificationIcon = new NotificationIcon();
			Application.Run();

			notificationIcon.timer.Dispose();
			notificationIcon.notifyIconCpu.Dispose();
			notificationIcon.notifyIconRam.Dispose();
			notificationIcon.cpuCounter.Dispose();
			notificationIcon.ramCounter.Dispose();
			notificationIcon.bitmap.Dispose();
			notificationIcon.font.Dispose();
			notificationIcon.graphics.Dispose();
			//notificationIcon.cpuPoint;
			//notificationIcon.ramPoint;

		}
		#endregion
		
		
		#region Event Handlers
		private void TimerTick(object sender, EventArgs e)
		{
			cpu = this.GetCpu();
			ram = this.GetRam();

			try {
				String text = this.DrawText();
				Icon iconCpu = this.DrawIconCpu();
				Icon iconRam = this.DrawIconRam();
				notifyIconCpu.Icon = iconCpu;
				notifyIconRam.Icon = iconRam;
				notifyIconCpu.Text = text;
				notifyIconRam.Text = text;
				iconCpu.Dispose();
				iconRam.Dispose();
			} catch (Exception exception) {
				//
			}
		}
		
		
		private void MenuCleanRamClick(object sender, EventArgs e)
		{
			this.CleanRam();
		}

		private void MenuAboutClick(object sender, EventArgs e)
		{
			MessageBox.Show(name + " " + version + Environment.NewLine + "Powered by Gemorroj", name + " " + version, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
		

		private void MenuExitClick(object sender, EventArgs e)
		{
			Application.Exit();
		}
		
		
		private void MenuAutoStartClick(object sender, EventArgs e)
		{
			if (this.IsAutoStarted()) {
				this.DelAutoStart();
			} else {
				this.AddAutoStart();
			}

			notifyIconCpu.ContextMenu = this.InitializeMenu();
			notifyIconRam.ContextMenu = this.InitializeMenu();
		}
		#endregion
	}
}
