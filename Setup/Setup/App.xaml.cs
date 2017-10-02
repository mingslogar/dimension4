using Daytimer.Functions;
using Microsoft.Shell;
using Microsoft.Win32;
using Setup.Install;
using Setup.InstallHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Setup
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class App : Application, ISingleInstanceApp
	{
		[STAThread]
		public static void Main()
		{
			string[] args = Environment.GetCommandLineArgs();

			if (args.Length >= 2)
			{
				if (ContainsArg(args, "/quiet"))
				{
					DetermineInstallMode(true);

					if (InstallerData.UninstallMode)
						Uninstall();
					else
					{
						foreach (string each in args)
							if (each.StartsWith("key=", StringComparison.InvariantCultureIgnoreCase))
							{
								InstallerData.ProductKey = each.Substring(5);
								break;
							}

						Install();
					}

					return;
				}
				else if (args[1].ToLowerInvariant() == "/update")
				{
					InstallerWorker.UpdateRegistry();
					return;
				}
			}

			if (SingleInstance<App>.InitializeAsFirstInstance(GlobalAssemblyInfo.AssemblyName + " Setup"))
			{
				DisableProcessWindowsGhosting();

				App app = new App();
				app.DispatcherUnhandledException += app_DispatcherUnhandledException;
				app.Run();

				// Allow single instance code to perform cleanup operations
				SingleInstance<App>.Cleanup();
			}
		}

		public App()
		{
			CreateSplash();
			InitializeComponent();
		}

		private static bool ContainsArg(string[] args, string query)
		{
			query = query.ToLower();

			foreach (string each in args)
				if (each.ToLower() == query)
					return true;

			return false;
		}

		private static void app_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			e.Handled = true;
			//TaskDialog td = new TaskDialog(null, "Oops...", e.Exception.ToString(), MessageType.Error);
			//td.ShowDialog();
		}

		#region ISingleInstanceApp Members

		public bool SignalExternalCommandLineArgs(IList<string> args)
		{
			try
			{
				if (MainWindow != null && MainWindow.IsLoaded)
				{
					MainWindow.WindowState = WindowState.Normal;
					MainWindow.Activate();

					return true;
				}
				else
					return false;
			}
			catch
			{
				return false;
			}
		}

		#endregion

		private WeakReference _splash;

		public Splash SplashWindow
		{
			get
			{
				if (_splash != null && _splash.IsAlive)
					return _splash.Target as Splash;

				return null;
			}
		}

		private void CreateSplash()
		{
			Thread splashThread = new Thread(ShowSplash);
			splashThread.SetApartmentState(ApartmentState.STA);
			splashThread.IsBackground = true;
			splashThread.Priority = ThreadPriority.Highest;
			splashThread.Start();
		}

		private void ShowSplash()
		{
			Splash splashWindow = new Splash();
			_splash = new WeakReference(splashWindow);
			splashWindow.Show();
			Dispatcher.Run();
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			DetermineInstallMode(false);
			ThemeHelpers.UpdateTheme(true);
		}

		private static void DetermineInstallMode(bool quiet)
		{
			string hkey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" + InstallerData.DisplayName;
			RegistryKey key = Registry.LocalMachine.OpenSubKey(hkey);

			if (key != null)
			{
				InstallerData.InstallLocation = key.GetValue("InstallLocation").ToString();

				Version installedVersion = Version.Parse(key.GetValue("DisplayVersion").ToString());
				Version currentVersion = InstallerData.CurrentVersion;

				if (currentVersion < installedVersion)
				{
					if (!quiet)
						MessageBox.Show("This installer is for an older version of " + InstallerData.DisplayName + ", and cannot be used to modify the currently installed version.",
							"Setup Error", MessageBoxButton.OK, MessageBoxImage.Error);
					//Environment.Exit(-1);
					Application.Current.Shutdown(-1);
				}

				InstallerData.UninstallMode = currentVersion == installedVersion;
				InstallerData.UpdateMode = currentVersion > installedVersion;
			}
		}

		private static void Uninstall()
		{
			UninstallerWorker uninstall = new UninstallerWorker();
			uninstall.OnComplete += uninstall_OnComplete;
			uninstall.OnError += uninstall_OnError;
			uninstall.Uninstall();
		}

		private static void uninstall_OnComplete(object sender, EventArgs e)
		{
			DeleteProgram();
			//Environment.Exit(0);
			Application.Current.Shutdown(0);
		}

		private static void uninstall_OnError(object sender, InstallErrorEventArgs e)
		{
			//Environment.Exit(-1);
			Application.Current.Shutdown(-1);
		}

		private static void Install()
		{
			InstallerWorker installer = new InstallerWorker();
			installer.OnError += installer_OnError;
			installer.OnComplete += installer_OnComplete;
			installer.Install();
		}

		private static void installer_OnError(object sender, InstallErrorEventArgs e)
		{
			//Environment.Exit(-1);
			Application.Current.Shutdown(-1);
		}

		private static void installer_OnComplete(object sender, EventArgs e)
		{
			//Environment.Exit(0);
			Application.Current.Shutdown(0);
		}

		protected override void OnExit(ExitEventArgs e)
		{
			base.OnExit(e);

			if (InstallerData.UninstallMode)
				DeleteProgram();
		}

		private static void DeleteProgram()
		{
			string appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			string location = appdata + "\\delete.bat";

			// Write batch file to loop until directory is 
			// deleted. This is necessary if the program
			// has not completely shut down when the first
			// loop has run.
			File.WriteAllLines(location,
				new string[] {
				"REM This batch file is only to complete the uninstallation of " + InstallerData.DisplayName + ".",
				"REM If " + InstallerData.DisplayName + " is no longer installed, you may safely delete this file.",
				":1",
				"IF EXIST \"" + InstallerData.InstallLocation + "\" (",
				"RD /S /Q \"" + InstallerData.InstallLocation + "\"",
				"GOTO 1",
				")",
				"ERASE /Q \"" + location + "\""
				});

			Process delete = new Process();
			delete.StartInfo.WorkingDirectory = appdata;
			delete.StartInfo.FileName = "cmd.exe";
			delete.StartInfo.Arguments = "/C \"" + location + "\"";
			delete.StartInfo.UseShellExecute = true;

			if (Environment.OSVersion.Version >= OSVersions.Win_Vista)
				delete.StartInfo.Verb = "runas";

			delete.StartInfo.CreateNoWindow = true;
			delete.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			delete.Start();
		}

		/// <summary>
		/// Switch out the current theme file with another.
		/// </summary>
		/// <param name="dict">the resource dictionary</param>
		public void Add(ResourceDictionary dict, bool firstTime)
		{
			if (!firstTime)
				Resources.MergedDictionaries.RemoveAt(0);

			Resources.MergedDictionaries.Insert(0, dict);
		}

		[DllImport("user32.dll")]
		private static extern void DisableProcessWindowsGhosting();
	}
}
