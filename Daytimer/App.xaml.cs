using Daytimer.Controls;
using Daytimer.Controls.Ribbon;
using Daytimer.DatabaseHelpers;
using Daytimer.DatabaseHelpers.Quotes;
using Daytimer.DatabaseHelpers.Recovery;
using Daytimer.DatabaseHelpers.Reminder;
using Daytimer.DatabaseHelpers.Sync;
using Daytimer.Dialogs;
using Daytimer.Functions;
using Daytimer.GoogleCalendarHelpers;
using Daytimer.Help;
using Daytimer.Themes;
using Microsoft.Shell;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;
using System.Windows.Threading;
using UpdateManager;

namespace Daytimer
{
	[ComVisible(false)]
	public partial class App : Application, ISingleInstanceApp
	{
		[STAThread]
		public static void Main()
		{
			try
			{
				string[] args = Environment.GetCommandLineArgs();

				if (args.Length >= 2)
				{
					switch (args[1].ToLower())
					{
						case "/update":
							string localPath = (new FileInfo(Process.GetCurrentProcess().MainModule.FileName)).DirectoryName;
							string oldFile = localPath + "\\UpdateManager.exe";
							string tempDir = Data.UpdatesWorkingDirectory;
							string newFile = tempDir + "\\~UpdateManager.exe";

							try
							{
								File.Delete(oldFile);
								File.Move(newFile, oldFile);
							}
							catch { }

							if (Directory.GetFiles(tempDir, "*", SearchOption.AllDirectories).Length == 0)
								Directory.Delete(tempDir, true);

							return;

						case "/nosingleinstance":
							try { DisableProcessWindowsGhosting(); }
							catch { }

							if (Settings.FirstRun)
								FirstRun();

							args = null;

							new App().Run();

							return;

						case "/setkey":
							Activation.ActivationGracePeriodStart = DateTime.Now;
							Activation.Key = args[2];
							return;

						case "/activate":
							string keyHash = args[2];
							string key = Activation.Key;
							if (Encryption.DecryptStringFromBytes(Convert.FromBase64String(keyHash),
								SecurityKeys.GenerateKey(key), SecurityKeys.GenerateIV(key))
								== key)
								Activation.Activate();
							return;

						default:
							break;
					}
				}

				if (SingleInstance<App>.InitializeAsFirstInstance(AssemblyAttributeAccessors.AssemblyTitle + "_" + AssemblyAttributeAccessors.AssemblyVersion))
				{
					try { DisableProcessWindowsGhosting(); }
					catch { }

					if (Settings.FirstRun)
						FirstRun();

					args = null;

					new App().Run();

					// Allow single instance code to perform cleanup operations
					SingleInstance<App>.Cleanup();
				}
			}
			catch (Exception exc)
			{
				// Log the exception
				Log(exc);

				// Restart
				Process.Start(Process.GetCurrentProcess().MainModule.FileName, string.Join(" ", Environment.GetCommandLineArgs()));

				// Shutdown
				Environment.Exit(-1);
			}
		}

		public App()
		{
			DispatcherUnhandledException += Application_DispatcherUnhandledException;
			CreateSplash();
			InitializeComponent();
		}

		private static void FirstRun()
		{
			string localPath = (new FileInfo(Process.GetCurrentProcess().MainModule.FileName)).DirectoryName;
			ProcessStartInfo pInfo = new ProcessStartInfo(localPath + "\\FirstRun.exe");
			pInfo.UseShellExecute = false;
			Process p = Process.Start(pInfo);
			p.WaitForExit();

			if (p.ExitCode == -1)
			{
				try
				{
					// Allow single instance code to perform cleanup operations
					SingleInstance<App>.Cleanup();
				}
				catch { }

				Environment.Exit(-1);
			}
		}

		public static string[] Args;
		private WeakReference _splash;

		public StartupWindow SplashWindow
		{
			get
			{
				if (_splash != null && _splash.IsAlive)
					return (StartupWindow)_splash.Target;

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
			StartupWindow splashWindow = new StartupWindow();
			_splash = new WeakReference(splashWindow);
			splashWindow.Show();
			Dispatcher.Run();
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			Args = e.Args;

			CreateJumpList();

			// Make the main process appear connected to the update process
			// in the taskbar.
			try { SetCurrentProcessExplicitAppUserModelID(GlobalAssemblyInfo.AssemblyName); }
			catch { }

			AppointmentDatabase.Load();
			AppointmentDatabase.OnSaveCompletedEvent += AppointmentDatabase_OnSaveCompletedEvent;
			ContactDatabase.Load();
			ContactDatabase.OnSaveCompletedEvent += ContactDatabase_OnSaveCompletedEvent;
			TaskDatabase.Load();
			TaskDatabase.OnSaveCompletedEvent += TaskDatabase_OnSaveCompletedEvent;
			NoteDatabase.Load();
			NoteDatabase.OnSaveCompletedEvent += NoteDatabase_OnSaveCompletedEvent;
			SyncDatabase.Load();
			SyncDatabase.OnSaveCompletedEvent += SyncDatabase_OnSaveCompletedEvent;
			QuoteDatabase.Load();
			QuoteDatabase.OnSaveCompletedEvent += QuoteDatabase_OnSaveCompletedEvent;

			ThemeHelpers.UpdateTheme(true);

			MainWindow mainWindow = new MainWindow();
			mainWindow.ContentRendered += mainWindow_ContentRendered;
			mainWindow.Show();

			SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
			SystemEvents.TimeChanged += SystemEvents_TimeChanged;

			if (BackstageEvents.StaticUpdater == null)
				BackstageEvents.StaticUpdater = new BackstageEvents();

			BackstageEvents.StaticUpdater.OnForceUpdateEvent += StaticUpdater_OnForceUpdateEvent;
			BackstageEvents.StaticUpdater.OnThemeChangedEvent += StaticUpdater_OnThemeChangedEvent;
			BackstageEvents.StaticUpdater.OnExportEvent += StaticUpdater_OnExportEvent;
			BackstageEvents.StaticUpdater.OnHelpEvent += StaticUpdater_OnHelpEvent;
			BackstageEvents.StaticUpdater.OnDocumentRequestEvent += StaticUpdater_OnDocumentRequestEvent;
			BackstageEvents.StaticUpdater.OnPrintStartedEvent += StaticUpdater_OnPrintStartedEvent;
			BackstageEvents.StaticUpdater.OnImportEvent += StaticUpdater_OnImportEvent;
			BackstageEvents.StaticUpdater.OnQuotesChangedEvent += StaticUpdater_OnQuotesChangedEvent;
		}

		private bool _isFirstTime = true;

		private Timer _delayTimer = null;

		private void mainWindow_ContentRendered(object sender, EventArgs e)
		{
			if (_isFirstTime)
			{
				_isFirstTime = false;

				Dispatcher.BeginInvoke(() =>
				{
					InitializeAutoSave();
					CheckActivation();

					_delayTimer = new Timer(state => Task.Factory.StartNew(RunBackgroundTasks),
						null, 2000, Timeout.Infinite);

					DispatcherTimer syncTimer = new DispatcherTimer(DispatcherPriority.Background);
					syncTimer.Tick += (timer, args) =>
					{
						Task.Factory.StartNew(() =>
						{
							if (GoogleAccounts.Exist() && !Settings.WorkOffline)
							{
								syncTimer.Stop();
								SyncWithServer();

								SyncHelper.LastUsedSyncHelper.OnCompletedEvent += (sync, syncArgs) =>
								{
									syncTimer.Interval = TimeSpan.FromMinutes(1.5);
									syncTimer.Start();
								};
							}
						});
					};
					syncTimer.Interval = TimeSpan.FromSeconds(5);
					syncTimer.Start();
				});
			}
		}

		private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
		{
			//
			// BUG FIX: Enabling/disabling Aero would cause the window to be too large
			// and overflow out of the screen or too small.
			//
			if (e.Category == UserPreferenceCategory.VisualStyle)
			{
				Dispatcher.BeginInvoke(UpdateWindowStateVisuals);
			}
		}

		private void UpdateWindowStateVisuals()
		{
			foreach (Window each in Windows)
			{
				if (each.WindowState == WindowState.Maximized)
				{
					each.StateChanged += each_StateChanged;
					each.WindowState = WindowState.Normal;
				}
			}
		}

		private void each_StateChanged(object sender, EventArgs e)
		{
			Window window = (Window)sender;

			window.StateChanged -= each_StateChanged;
			window.WindowState = WindowState.Maximized;
		}

		private void SystemEvents_TimeChanged(object sender, EventArgs e)
		{
			Dispatcher.BeginInvoke(UpdateTime);
		}

		private void UpdateTime()
		{
			// Make sure all alerts update to reflect time change
			ReminderQueue.Populate();

			((MainWindow)MainWindow).UpdateTimeFormat();
		}

		private void StaticUpdater_OnForceUpdateEvent(object sender, ForceUpdateEventArgs e)
		{
			if (e.UpdateTheme)
				ThemeHelpers.UpdateTheme(false);

			MainWindow window = (MainWindow)MainWindow;

			if (e.UpdateBackground)
				if (window.ActualWidth >= 550)
					window.UpdateHeaderBackground(false);

			if (e.UpdateHours)
				window.UpdateHours();

			if (e.UpdateTimeFormat)
				window.UpdateTimeFormat();

			if (e.UpdateWeatherMetric)
				window.UpdateWeatherMetric();

			if (e.UpdateAutoSave)
				InitializeAutoSave();
		}

		private void StaticUpdater_OnThemeChangedEvent(object sender, ThemeChangedEventArgs e)
		{
			ThemeHelpers.UpdateTheme(e.Theme);
		}

		private void StaticUpdater_OnExportEvent(object sender, ExportEventArgs e)
		{
			MainWindow window = (MainWindow)MainWindow;

			switch (e.ExportType)
			{
				case ExportType.Screenshot:
					window.ExportScreenshot();
					break;

				case ExportType.Individual:
					switch (e.EditType)
					{
						case EditType.Appointment:
							window.ExportAppointment();
							break;

						case EditType.Contact:
							window.ExportContact();
							break;

						case EditType.Task:
							window.ExportTask();
							break;

						case EditType.Note:
							window.ExportNote();
							break;

						default:
							break;
					}
					break;

				default:
					break;
			}
		}

		private void StaticUpdater_OnHelpEvent(object sender, EventArgs e)
		{
			HelpManager.ShowHelp();
		}

		private void StaticUpdater_OnDocumentRequestEvent(object sender, DocumentRequestEventArgs e)
		{
			MainWindow window = (MainWindow)MainWindow;

			switch (window.ActiveDisplayPane)
			{
				case Daytimer.MainWindow.DisplayPane.Calendar:
					e.DocumentType = EditType.Appointment;
					e.DatabaseObject = window.LiveAppointment;
					break;

				case Daytimer.MainWindow.DisplayPane.Notes:
					e.DocumentType = EditType.Note;
					e.DatabaseObject = window.LiveNote;
					break;

				case Daytimer.MainWindow.DisplayPane.People:
					e.DocumentType = EditType.Contact;
					e.DatabaseObject = window.LiveContact;
					break;

				case Daytimer.MainWindow.DisplayPane.Tasks:
					e.DocumentType = EditType.Task;
					e.DatabaseObject = window.LiveTask;
					break;

				default:
					return;
			}
		}

		private void StaticUpdater_OnPrintStartedEvent(object sender, PrintEventArgs e)
		{
			((MainWindow)MainWindow).statusStrip.AddPrintMonitor(e.XpsDocumentWriter);
		}

		private void StaticUpdater_OnImportEvent(object sender, ImportEventArgs e)
		{
			MainWindow window = (MainWindow)MainWindow;

			switch (e.Type)
			{
				case EditType.Appointment:
					//window.ImportAppointment();
					break;

				case EditType.Contact:
					window.ImportContact();
					break;

				case EditType.Task:
					//window.ImportTask();
					break;

				case EditType.Note:
					//window.ImportNote();
					break;

				default:
					break;
			}
		}

		private void StaticUpdater_OnQuotesChangedEvent(object sender, EventArgs e)
		{
			MainWindow window = (MainWindow)MainWindow;

			if (window != null)
				window.RefreshQuotes();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			base.OnExit(e);

			//
			// Save databases
			//
			AppointmentDatabase.OnSaveCompletedEvent -= AppointmentDatabase_OnSaveCompletedEvent;
			AppointmentDatabase.Save();
			ContactDatabase.OnSaveCompletedEvent -= AppointmentDatabase_OnSaveCompletedEvent;
			ContactDatabase.Save();
			TaskDatabase.OnSaveCompletedEvent -= TaskDatabase_OnSaveCompletedEvent;
			TaskDatabase.Save();
			NoteDatabase.OnSaveCompletedEvent -= NoteDatabase_OnSaveCompletedEvent;
			NoteDatabase.Save();
			SyncDatabase.OnSaveCompletedEvent -= SyncDatabase_OnSaveCompletedEvent;
			SyncDatabase.Save();

			//
			// Unregister system hooks
			//
			SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
			SystemEvents.TimeChanged -= SystemEvents_TimeChanged;

			//
			// Delete autorecover information
			//
			new RecoveryDatabase(RecoveryVersion.Current).Delete();
			new RecoveryDatabase(RecoveryVersion.LastRun).Delete();

			//
			// Install any updates which may have been downloaded.
			//
			string localPath = (new FileInfo(Process.GetCurrentProcess().MainModule.FileName)).DirectoryName;
			Process.Start(localPath + "\\UpdateManager.exe", "/update " + Process.GetCurrentProcess().Id.ToString());
		}

		#region ISingleInstanceApp Members

		public bool SignalExternalCommandLineArgs(IList<string> args)
		{
			try
			{
				if (MainWindow != null && MainWindow.IsLoaded)
					return ((MainWindow)MainWindow).ProcessCommandLineArgs(args);
				else
					return false;
			}
			catch
			{
				return false;
			}
		}

		#endregion

		#region Error Handling

		// Flag to prevent more than one dialog opening simultaneously.
		private bool _isErrorDialogShowing = false;

		private async void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			// We don't want to overwhelm the user with multiple error dialogs. However,
			// we will still log the error for debugging purposes.
			if (_isErrorDialogShowing)
			{
				e.Handled = true;

				await Task.Factory.StartNew(() => { Log(e.Exception); });

				return;
			}

			_isErrorDialogShowing = true;

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

					_isErrorDialogShowing = false;

					return;
				}

				await Task.Factory.StartNew(() => { Log(e.Exception); });

				// Try and save the databases, but we still place this in a catch
				// in case an error in the database caused the crash.
				bool saveError = false;

				await Task.Factory.StartNew(() => { try { AppointmentDatabase.Save(); } catch { saveError = true; } });

				if (saveError)
				{
					saveError = false;
					await Task.Factory.StartNew(() => { try { ContactDatabase.Save(); } catch { saveError = true; } });
				}

				if (saveError)
				{
					saveError = false;
					await Task.Factory.StartNew(() => { try { TaskDatabase.Save(); } catch { saveError = true; } });
				}

				if (saveError)
				{
					saveError = false;

					try { ((MainWindow)MainWindow).SaveNotesView(); }
					catch { }

					await Task.Factory.StartNew(() => { try { NoteDatabase.Save(); } catch { saveError = true; } });
				}

				if (saveError)
				{
					saveError = false;
					await Task.Factory.StartNew(() => { try { SyncDatabase.Save(); } catch { saveError = true; } });
				}

				if (saveError)
				{
					saveError = false;
					await Task.Factory.StartNew(() => { try { QuoteDatabase.Save(); } catch { saveError = true; } });
				}

				if (saveError)
				{
					try
					{
						// Autorecover
						RecoveryDatabase recovery = new RecoveryDatabase(RecoveryVersion.LastRun);

						MainWindow window = (MainWindow)MainWindow;

						try { recovery.RecoveryAppointment = window.LiveAppointment; }
						catch { }
						try { recovery.RecoveryContact = window.LiveContact; }
						catch { }
						try { recovery.RecoveryTask = window.LiveTask; }
						catch { }
					}
					catch { }
				}

				try { new RecoveryDatabase(RecoveryVersion.Current).Delete(); }
				catch { }

				if (MainWindow != null)
					try { ((MainWindow)MainWindow).SaveSettings(); }
					catch { }

				// Try and show an error message, but again we still place this in a catch
				// in case an error in the dialog caused the crash.
				try
				{
					Error error = new Error(null, e.Exception);
					if (error.ShowDialog() == true)
					{
						Feedback f = new Feedback(FeedbackType.Error, e.Exception.ToString());

						if (MainWindow != null && MainWindow.IsLoaded)
							f.Owner = MainWindow;
						else
							f.WindowStartupLocation = WindowStartupLocation.CenterScreen;

						f.ShowDialog();
					}
					else if (error.IgnoreError)
					{
						_isErrorDialogShowing = false;
						return;
					}
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

				// Restart Dimension 4
				ProcessStartInfo restartInfo = new ProcessStartInfo(Process.GetCurrentProcess().MainModule.FileName, "/nosingleinstance");
				restartInfo.UseShellExecute = true;
				Process.Start(restartInfo);

#if DEBUG
				Debug.WriteLine(e.Exception.ToString());
#endif

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

		#region Background Tasks

		private void CreateJumpList()
		{
			string iconPath = Process.GetCurrentProcess().MainModule.FileName;
			iconPath = iconPath.Remove(iconPath.LastIndexOf('\\')) + "\\JumpTaskImages.dll";

			foreach (JumpTask task in jumpList.JumpItems)
				task.IconResourcePath = iconPath;

			jumpList.Apply();
		}

		private void RunBackgroundTasks()
		{
			ReminderQueue.InitializeAlerts();
			ShowConnectivity();

			if (Settings.WorkOffline)
				return;

			_delayTimer = new Timer(state =>
			{
				Task.Factory.StartNew(CheckForUpdates);
				_delayTimer = null;

			}, null, 30000, Timeout.Infinite);
		}

		private void SyncWithServer()
		{
			Dispatcher.Invoke(() =>
			{
				if (MainWindow != null)
				{
					if (SyncHelper.LastUsedSyncHelper == null
						|| SyncHelper.LastUsedSyncHelper.Done
						|| SyncHelper.LastUsedSyncHelper.Error != null)
						((MainWindow)MainWindow).statusStrip.SyncWithServer();
				}
			});
		}

		private void CheckForUpdates()
		{
			//
			// If the current user is not the admin, don't even bother
			// running the update check.
			//
			if (!UserInfo.IsCurrentUserAdmin)
				return;

			//
			// If there is a new version of the updater which has not yet
			// been installed, install that before running updates.
			//
			string newFile = Data.UpdatesWorkingDirectory + "\\~UpdateManager.exe";

			if (File.Exists(newFile))
			{
				try
				{
					// We need UAC
					ProcessStartInfo pInfo = new ProcessStartInfo(Process.GetCurrentProcess().MainModule.FileName);

					if (Environment.OSVersion.Version >= OSVersions.Win_Vista)
						pInfo.Verb = "runas";

					pInfo.Arguments = "/update";
					Process.Start(pInfo).WaitForExit();
				}
				catch (Win32Exception)
				{
					TaskDialog td = new TaskDialog(MainWindow, "Update Error", "Updates could not complete due to insufficient privileges.", MessageType.Error);
					td.ShowDialog();
					return;
				}
			}

			//
			// Check for updates once a week
			//
			if (DateTime.Now > Settings.LastSuccessfulUpdate.AddDays(7))
			{
				Process[] procs = Process.GetProcessesByName("UpdateManager");
				string filename = new FileInfo(Process.GetCurrentProcess().MainModule.FileName).DirectoryName + "\\UpdateManager.exe";

				foreach (Process each in procs)
					if (each.MainModule.FileName == filename)
						return;

				Dispatcher.BeginInvoke(() =>
				{
					if (MainWindow != null)
						((MainWindow)MainWindow).statusStrip.CheckForUpdates();
				}, DispatcherPriority.ApplicationIdle);
			}
		}

		private void ShowConnectivity()
		{
			Dispatcher.BeginInvoke(() =>
			{
				if (MainWindow != null)
					((MainWindow)MainWindow).statusStrip.ShowConnectivity();
			});
		}

		#region Auto Save

		private static DispatcherTimer autoSaveTimer;

		public static void InitializeAutoSave()
		{
			if (autoSaveTimer != null)
			{
				autoSaveTimer.Tick -= autoSaveTimer_Tick;
				autoSaveTimer.Stop();
			}

			autoSaveTimer = new DispatcherTimer();

			TimeSpan autosavefreq = Settings.AutoSaveFrequency;

			if (autosavefreq != TimeSpan.FromSeconds(-1))
			{
				autoSaveTimer.Interval = autosavefreq;
				autoSaveTimer.Tick += autoSaveTimer_Tick;
				autoSaveTimer.Start();
			}
			else
				autoSaveTimer.Stop();
		}

		private static void autoSaveTimer_Tick(object sender, EventArgs e)
		{
			((DispatcherTimer)sender).Stop();

			try
			{
				((MainWindow)App.Current.MainWindow).ShowStatus("SAVING DATABASE...");
			}
			catch { }

			Thread save = new Thread(AppointmentDatabase.Save);
			save.SetApartmentState(ApartmentState.STA);
			save.Priority = ThreadPriority.BelowNormal;
			save.Start();
		}

		private void AppointmentDatabase_OnSaveCompletedEvent(object sender, EventArgs e)
		{
			ContactDatabase.Save();
		}

		private void ContactDatabase_OnSaveCompletedEvent(object sender, EventArgs e)
		{
			TaskDatabase.Save();
		}

		private void TaskDatabase_OnSaveCompletedEvent(object sender, EventArgs e)
		{
			try
			{
				Dispatcher.Invoke(() =>
				{
					if (MainWindow != null)
						((MainWindow)MainWindow).SaveNotesView();
				});
			}
			catch { }

			NoteDatabase.Save();
		}

		private void NoteDatabase_OnSaveCompletedEvent(object sender, EventArgs e)
		{
			SyncDatabase.Save();
		}

		private void SyncDatabase_OnSaveCompletedEvent(object sender, EventArgs e)
		{
			QuoteDatabase.Save();
		}

		private void QuoteDatabase_OnSaveCompletedEvent(object sender, EventArgs e)
		{
			Dispatcher.BeginInvoke(() =>
			{
				MainWindow window = (MainWindow)MainWindow;

				window.ShowStatus("SAVING AUTORECOVER INFORMATION...");

				RecoveryDatabase recovery = new RecoveryDatabase(RecoveryVersion.Current);

				recovery.RecoveryAppointment = window.LiveAppointment;
				recovery.RecoveryContact = window.LiveContact;
				recovery.RecoveryTask = window.LiveTask;

				window.ShowStatus("DATABASE SAVED");
			});
		}

		#endregion

		private void CheckActivation()
		{
			Task.Factory.StartNew(() =>
			{
				if (!Activation.IsActivated() && !Activation.IsWithinTrial())
				{
					if (!Activation.IsKeyValid(Activation.Key))
					{
						Dispatcher.Invoke(() =>
						{
							SubscriptionExpired key = new SubscriptionExpired();
							key.Owner = MainWindow;
							key.ShowDialog();
						});
					}
					else
					{
						// Key is valid, but we just aren't activated online.
						Dispatcher.Invoke(() =>
						{
							OnlineActivation online = new OnlineActivation();
							online.Owner = MainWindow;

							if (online.ShowDialog() == false)
								Shutdown();
						});
					}
				}
			});
		}

		#endregion

		/// <summary>
		/// Switch out the current theme file with another.
		/// </summary>
		/// <param name="dict">the resource dictionary</param>
		public void Add(ResourceDictionary dict, bool firstTime)
		{
			if (!firstTime)
				for (int i = 0; i < 4; i++)
					AppResourceDictionary.MergedDictionaries.RemoveAt(0);
			//else
			//	for (int i = 0; i < 1; i++)
			//		AppResourceDictionary.MergedDictionaries.RemoveAt(0);

			AppResourceDictionary.MergedDictionaries.Insert(0, dict);

			ResourceDictionary dictionary0 = new ResourceDictionary();
			dictionary0.Source = new Uri("pack://application:,,,/Daytimer.Styles;component/Resources/NavButton.xaml", UriKind.Absolute);
			AppResourceDictionary.MergedDictionaries.Insert(1, dictionary0);

			ResourceDictionary dictionary1 = new ResourceDictionary();
			dictionary1.Source = new Uri("pack://application:,,,/Daytimer.Styles;component/Resources/ScrollBar.xaml", UriKind.Absolute);
			AppResourceDictionary.MergedDictionaries.Insert(1, dictionary1);

			ResourceDictionary dictionary2 = new ResourceDictionary();
			dictionary2.Source = new Uri("pack://application:,,,/Daytimer.Controls;component/Panes/Calendar/ShowAsFill.xaml", UriKind.Absolute);
			AppResourceDictionary.MergedDictionaries.Insert(1, dictionary2);
		}

		[DllImport("user32.dll")]
		private static extern void DisableProcessWindowsGhosting();

		[DllImport("shell32.dll")]
		private static extern void SetCurrentProcessExplicitAppUserModelID(
			[MarshalAs(UnmanagedType.LPWStr)] string AppID);

		//[DllImport("shell32.dll")]
		//private static extern void GetCurrentProcessExplicitAppUserModelID(
		//	[Out(), MarshalAs(UnmanagedType.LPWStr)] out string AppID);
	}
}
