using Daytimer.Dialogs;
using Daytimer.Functions;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace UpdateManager
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class App : Application
	{
		/// <summary>
		/// Gets or sets whether the UI should be shown.
		/// </summary>
		public static bool QuietMode = false;

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			// On main program shutdown, this program will be called
			// with two switches: /update and PID.
			if (e.Args.Length >= 2 && e.Args[0].ToLower() == "/update")
			{
				// Wait for the main program to exit
				try
				{
					Process main = Process.GetProcessById(int.Parse(e.Args[1]));
					main.WaitForExit();
				}
				catch (ArgumentException) { }

				if (Directory.Exists(Data.UpdatesWorkingDirectory) &&
					Directory.GetFileSystemEntries(Data.UpdatesWorkingDirectory).Length > 0)
				{
					// Make sure we have UAC
					if (UserInfo.IsElevated)
					{
						string localPath = (new FileInfo(Process.GetCurrentProcess().MainModule.FileName)).DirectoryName;
						string[] files = Directory.GetFiles(Data.UpdatesWorkingDirectory, "*", SearchOption.AllDirectories);
						int tempDirLength = Data.UpdatesWorkingDirectory.Length;

						foreach (string each in files)
						{
							FileInfo info = new FileInfo(each);
							string filename = info.Name;

							if (filename.StartsWith("~"))
							{
								string relativePath = info.DirectoryName.Substring(tempDirLength);

								string delete = localPath + relativePath + "\\" + filename.Substring(1);

								try
								{
									if (File.Exists(delete))
										File.Delete(delete);

									if (!Directory.Exists(localPath + relativePath))
										Directory.CreateDirectory(localPath + relativePath);

									File.Move(each, delete);
								}
								catch { }
							}
						}

						if (Directory.GetFiles(Data.UpdatesWorkingDirectory, "*", SearchOption.AllDirectories).Length == 0)
							Directory.Delete(Data.UpdatesWorkingDirectory, true);

						QueueNgen();
						UpdateRegistry();
					}
					else
					{
						try
						{
							// We need UAC
							ProcessStartInfo pInfo = new ProcessStartInfo(Process.GetCurrentProcess().MainModule.FileName);

							if (Environment.OSVersion.Version.Major >= 6)
								pInfo.Verb = "runas";

							pInfo.Arguments = "/update " + e.Args[1];
							Process.Start(pInfo);
						}
						catch (Win32Exception)
						{
							TaskDialog td = new TaskDialog(null, "Update Error",
								"Updates could not complete due to insufficient privileges.", MessageType.Error);
							td.ShowDialog();
						}
					}
				}

				Shutdown();
			}
			else
			{
				try { DisableProcessWindowsGhosting(); }
				catch { }

				// Make the update process appear connected to the main process
				// in the taskbar.
				try { SetCurrentProcessExplicitAppUserModelID(GlobalAssemblyInfo.AssemblyName); }
				catch { }

				if (e.Args.Length >= 2)
					new MainWindow(int.Parse(e.Args[0]), bool.Parse(e.Args[1]));
				else
					new MainWindow(-1, false);
			}
		}

		/// <summary>
		/// Update the local native image cache.
		/// </summary>
		/// <param name="mainmodule"></param>
		private void QueueNgen()
		{
			ProcessStartInfo info = new ProcessStartInfo(Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\Microsoft.NET\\Framework\\v4.0.30319\\ngen.exe", "update");

			if (Environment.OSVersion.Version.Major >= 6)
				info.Verb = "runas";

			info.CreateNoWindow = true;
			info.WindowStyle = ProcessWindowStyle.Hidden;
			Process.Start(info);
		}

		/// <summary>
		/// Update the data that is displayed in the registry.
		/// </summary>
		private void UpdateRegistry()
		{
			string installer = new FileInfo(Process.GetCurrentProcess().MainModule.FileName).DirectoryName + "\\Uninstall\\Uninstall.exe";

			if (File.Exists(installer))
			{
				ProcessStartInfo pInfo = new ProcessStartInfo(installer);

				if (Environment.OSVersion.Version.Major >= 6)
					pInfo.Verb = "runas";

				pInfo.Arguments = "/update";
				Process.Start(pInfo);
			}
			// Else, we are running out of the debug folder.
		}

		#region Error Handling

		//int errorCount = 0;

		//private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		//{
		//	// We handle our own errors, so that we can write error logs
		//	// and attempt application recovery.
		//	e.Handled = true;

		//	errorCount++;

		//	// Try and show an error message, but again we still place this in a catch
		//	// in case an error in the dialog caused the crash.
		//	try
		//	{
		//		TaskDialog error = new TaskDialog(null, "Oops...", Settings.JoinedCEIP ? e.Exception.ToString() : e.Exception.Message, MessageType.Error);
		//		error.ShowDialog();
		//	}
		//	catch
		//	{
		//		try
		//		{
		//			MessageBox.Show("Oops...\r\n\r\n"
		//				+ (Settings.JoinedCEIP ? e.Exception.ToString() : e.Exception.Message),
		//				"Daytimer Error",
		//				MessageBoxButton.OK, MessageBoxImage.Error);
		//		}
		//		catch { }
		//	}
		//}

		private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			try
			{
				// We handle our own errors, so that we can write error logs
				// and attempt application recovery.
				e.Handled = true;

				if (e.Exception is NotImplementedException)
				{
					TaskDialog td = new TaskDialog(MainWindow, "Not Implemented",
						!string.IsNullOrEmpty(e.Exception.Message) ? e.Exception.Message
						: "This isn't hooked up yet. Check for updates and try again.", MessageType.Error);
					td.ShowDialog();
					return;
				}

				Log(e.Exception);

				// Try and show an error message, but again we still place this in a catch
				// in case an error in the dialog caused the crash.
				try
				{
					TaskDialog error = new TaskDialog(null, "Oops...", Settings.JoinedCEIP ? e.Exception.ToString() : e.Exception.Message, MessageType.Error);
					error.ShowDialog();
				}
				catch
				{
					if (Settings.JoinedCEIP)
					{
						MessageBox.Show("Oops...\r\n\r\n"
							+ e.Exception.ToString(),
							GlobalAssemblyInfo.AssemblyName + " Error",
							MessageBoxButton.OK, MessageBoxImage.Error);
					}
					else
					{
						MessageBox.Show("Oops...\r\n\r\n"
							+ e.Exception.Message,
							GlobalAssemblyInfo.AssemblyName + " Error",
							MessageBoxButton.OK, MessageBoxImage.Error);
					}
				}

#if DEBUG
				Debug.WriteLine(e.Exception.ToString());
#endif

				//Environment.Exit(-1);
				Shutdown(-1);
			}
			catch { }
		}

		private static void Log(Exception exc)
		{
			try
			{
				EventLog.WriteEntry(GlobalAssemblyInfo.AssemblyName, exc.ToString(), EventLogEntryType.Error);
			}
			catch { }
		}

		#endregion

		[DllImport("user32.dll")]
		private static extern void DisableProcessWindowsGhosting();

		[DllImport("shell32.dll")]
		private static extern void SetCurrentProcessExplicitAppUserModelID(
			[MarshalAs(UnmanagedType.LPWStr)] string AppID);

		[DllImport("shell32.dll")]
		private static extern void GetCurrentProcessExplicitAppUserModelID(
			[Out(), MarshalAs(UnmanagedType.LPWStr)] out string AppID);
	}
}
