/**
 * Gemorroj
 */
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace TraySysInfo
{
	public sealed class NotificationIcon
	{
		const string version = "0.1";
		const string name = "TraySysInfo";
		
		// Очистка RAM
		[System.Runtime.InteropServices.DllImport("psapi.dll", EntryPoint = "EmptyWorkingSet")]
		private static extern bool EmptyWorkingSet(IntPtr hProcess);

		private Timer timer;
		private NotifyIcon notifyIcon;
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
				
	
				notifyIcon = new NotifyIcon();
	
				
				notifyIcon.MouseClick += new MouseEventHandler(this.IconClick);
				//notifyIcon.MouseDoubleClick += new MouseEventHandler(this.IconDoubleClick); //не работает(

	    		Icon icon = this.DrawIcon();
				notifyIcon.Icon = icon;
				icon.Dispose();
				
				notifyIcon.Text = this.DrawText();

				notifyIcon.ContextMenu = this.InitializeMenu();
				notifyIcon.Visible = true;
			} catch (Exception e) {
				MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Application.Exit();
			}
		}
		
		
		/**
		 * Рисуем заначения CPU и RAM на иконке
		 */
		private Icon DrawIcon()
		{
			graphics.Clear(Color.Transparent);
			
			Brush brushCpu;
			Brush brushRam;
			
			if (cpu > 50) {
				brushCpu = Brushes.Red;
			} else if (cpu > 30) {
				brushCpu = Brushes.Yellow;
			} else {
				brushCpu = Brushes.Black;
			}
			
			
			if (ram < 500) {
				brushRam = Brushes.Yellow;
			} else if (ram < 100) {
				brushRam = Brushes.Red;
			} else {
				brushRam = Brushes.Black;
			}
			
			
			graphics.DrawString(this.FormatCpu(cpu), font, brushCpu, cpuPoint);
			graphics.DrawString(this.FormatRam(ram), font, brushRam, ramPoint);

			try {
				IntPtr hIcon = bitmap.GetHicon();
				return Icon.FromHandle(hIcon);
			} catch (Exception e) {
				MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Application.Exit();
			}
			return null;
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
			cpuPoint = new PointF(0, 0);
			ramPoint = new PointF(0, 7);
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
			font = new Font("Calibri", 6, FontStyle.Regular, GraphicsUnit.Point);
		}


		/**
		 * Инициализируем объект графики для счетчиков
		 */
		private void InitializeGraphics()
		{
			graphics = Graphics.FromImage(bitmap);
			

			graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
			//graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
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
			} catch (Exception/* e*/) {
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
			} catch (Exception/* e*/) {
				return (float)-1;
			}
		}
		
		
		/**
		 * Форматируем занчение CPU
		 */
		private string FormatCpu(float cpu)
		{
			return Math.Round((decimal)cpu, 1).ToString() + "%";
		}
		

		/**
		 * Форматируем занчение RAM
		 */		
		private string FormatRam(float ram)
		{
			if (ram < 1000) {
				return ram.ToString() + "M";
			} else if (ram < 10000) {
				return Math.Round((decimal)(ram / 1000), 1).ToString() + "G";
			} else {
				return Math.Round((decimal)(ram / 10000), 1).ToString() + "G";
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
				} catch (Exception/* e*/) {
					//no++;
				}
            }

			MessageBox.Show("Cleaned " + yes.ToString() + " processes", "Clean RAM", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
		
		
		/**
		 * Показываем балун
		 */
		private void ShowBalloon()
		{
			notifyIcon.ShowBalloonTip(
				5000,
				name + " " + version,
				"CPU: " + this.FormatCpuEx(cpu) + Environment.NewLine + "RAM: " + this.FormatRamEx(ram),
				ToolTipIcon.Info
			);
		}
		

		/**
		 * Инициализируем меню
		 */
		private ContextMenu InitializeMenu()
		{
			MenuItem autoStartup = new MenuItem("Auto start", this.MenuAutoStartClick);
			autoStartup.Checked = this.IsAutoStarted();

			return new ContextMenu(new MenuItem[] {
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
			notificationIcon.notifyIcon.Dispose();
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

			Icon icon = this.DrawIcon();
			notifyIcon.Icon = icon;
			icon.Dispose();

			notifyIcon.Text = this.DrawText();
		}
		

		private void MenuAboutClick(object sender, EventArgs e)
		{
			MessageBox.Show(name + " " + version + Environment.NewLine + "Powered by Gemorroj", name + " " + version, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
		

		private void MenuExitClick(object sender, EventArgs e)
		{
			Application.Exit();
		}
		

		private void IconClick(object sender, MouseEventArgs e) 
		{
			if (e.Button == MouseButtons.Left) {
				this.ShowBalloon();
			} else if (e.Button == MouseButtons.Middle) {
				this.CleanRam();
			}
		}
		
		
		/*private void IconDoubleClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left) {
				this.CleanRam();
			}
		}*/
		
		
		private void MenuAutoStartClick(object sender, EventArgs e)
		{
			if (this.IsAutoStarted()) {
				this.DelAutoStart();
			} else {
				this.AddAutoStart();
			}

			notifyIcon.ContextMenu = this.InitializeMenu();
		}
		#endregion
	}
}
