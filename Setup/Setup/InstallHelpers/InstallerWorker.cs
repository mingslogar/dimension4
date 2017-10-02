using Daytimer.Functions;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using MS.WindowsAPICodePack.Internal;
using Setup.InstallHelpers.Shortcut;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Setup.InstallHelpers
{
	class InstallerWorker
	{
		private int total = 8;

		#region Files

		Dictionary<string, Lazy<byte[]>> files = new Dictionary<string, Lazy<byte[]>>()
		{
			{"Resources\\Media\\Birds.mp3", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Birds;}) },
			{"Resources\\Media\\Generic1.mp3", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Generic1;})},
			{"Resources\\Media\\Generic2.mp3", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Generic2;})},
			{"Resources\\Media\\Generic3.mp3",new Lazy<byte[]>(()=>{ return  SetupDependencies.Properties.Resources.Generic3;})},
			{"Resources\\Media\\Google.mp3", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Google;})},
			{"Resources\\Media\\iPhone.mp3", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.iPhone;})},
			{"Resources\\Media\\Nokia.mp3", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Nokia;})},
			{"Resources\\Media\\Windows.mp3", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Windows;})},
			{"Resources\\Files\\DefaultNote", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.DefaultNote;})},
			{"Themes\\BlueTheme.xaml", new Lazy<byte[]>(()=>{ return UTF8NoBOM.GetBytes(SetupDependencies.Properties.Resources.BlueTheme);})},
			{"Themes\\BrownTheme.xaml", new Lazy<byte[]>(()=>{ return UTF8NoBOM.GetBytes(SetupDependencies.Properties.Resources.BrownTheme);})},
			{"Themes\\GreenTheme.xaml", new Lazy<byte[]>(()=>{ return UTF8NoBOM.GetBytes(SetupDependencies.Properties.Resources.GreenTheme);})},
			{"Themes\\MaroonTheme.xaml", new Lazy<byte[]>(()=>{ return UTF8NoBOM.GetBytes(SetupDependencies.Properties.Resources.MaroonTheme);})},
			{"Themes\\PurpleTheme.xaml", new Lazy<byte[]>(()=>{ return UTF8NoBOM.GetBytes(SetupDependencies.Properties.Resources.PurpleTheme);})},
			{"Themes\\SilverTheme.xaml", new Lazy<byte[]>(()=>{ return UTF8NoBOM.GetBytes(SetupDependencies.Properties.Resources.SilverTheme);})},
			{"AddressParser.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.AddressParser;})},
			{"antlr.runtime.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.antlr_runtime;})},
			{"CharMap.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.CharMap;})},
			{"colorpickerlib.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.colorpickerlib;})},
			{"CoreAudioApi.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.CoreAudioApi;})},
			{"Daytimer.Backgrounds.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Daytimer_Backgrounds;})},
			{"Daytimer.Controls.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Daytimer_Controls;})},
			{"Daytimer.DatabaseHelpers.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Daytimer_DatabaseHelpers;})},
			{"Daytimer.Dialogs.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Daytimer_Dialogs;})},
			{"Daytimer.DockableDialogs.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Daytimer_DockableDialogs;})},
			{"Daytimer.exe", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Daytimer;})},
			{"Daytimer.exe.config", new Lazy<byte[]>(()=>{ return UTF8NoBOM.GetBytes(SetupDependencies.Properties.Resources.Daytimer_exe);})},
			{"Daytimer.Fonts.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Daytimer_Fonts;})},
			{"Daytimer.Functions.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Daytimer_Functions;})},
			{"Daytimer.Fundamentals.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Daytimer_Fundamentals;})},
			{"Daytimer.GoogleCalendarHelpers.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Daytimer_GoogleCalendarHelpers;})},
			{"Daytimer.GoogleMapHelpers.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Daytimer_GoogleMapHelpers;})},
			{"Daytimer.Help.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Daytimer_Help;})},
			{"Daytimer.ICalendarHelpers.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Daytimer_ICalendarHelpers;})},
			{"Daytimer.Images.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Daytimer_Images;})},
			{"Daytimer.PrintHelpers.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Daytimer_PrintHelpers;})},
			{"Daytimer.Search.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Daytimer_Search;})},
			{"Daytimer.Styles.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Daytimer_Styles;})},
			{"Daytimer.Toasts.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Daytimer_Toasts;})},
			{"Daytimer.WikiQuoteHelper.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Daytimer_WikiQuoteHelper;})},
			{"DDay.Collections.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.DDay_Collections;})},
			{"DDay.iCal.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.DDay_iCal;})},
			{"DDay.iCal.License.txt", new Lazy<byte[]>(()=>{ return UTF8NoBOM.GetBytes(SetupDependencies.Properties.Resources.License);})},
			{"Demo.Main.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Demo_Main;})},
			{"FirstRun.exe", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.FirstRun;})},
			{"FirstRun.exe.config", new Lazy<byte[]>(()=>{ return UTF8NoBOM.GetBytes(SetupDependencies.Properties.Resources.FirstRun_exe);})},
			{"Gma.UserActivityMonitor.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Gma_UserActivityMonitor;})},
			{"Google.GData.AccessControl.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Google_GData_AccessControl;})},
			{"Google.GData.Calendar.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Google_GData_Calendar;})},
			{"Google.GData.Client.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Google_GData_Client;})},
			{"Google.GData.Extensions.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Google_GData_Extensions;})},
			{"JumpTaskImages.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.JumpTaskImages;})},
			{"Microsoft.Threading.Tasks.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Microsoft_Threading_Tasks;})},
			{"Microsoft.Threading.Tasks.Extensions.Desktop.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Microsoft_Threading_Tasks_Extensions_Desktop;})},
			{"Microsoft.Threading.Tasks.Extensions.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Microsoft_Threading_Tasks_Extensions;})},
			{"Microsoft.Windows.Shell.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Microsoft_Windows_Shell;})},
			{"Microsoft.WindowsAPICodePack.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Microsoft_WindowsAPICodePack;})},
			{"Microsoft.WindowsAPICodePack.Shell.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Microsoft_WindowsAPICodePack_Shell;})},
			{"Modern.FileBrowser.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Modern_FileBrowser;})},
			{"Newtonsoft.Json.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Newtonsoft_Json;})},
			{"RecurrenceGenerator.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.RecurrenceGenerator;})},
			{"RibbonControlsLibrary.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.RibbonControlsLibrary;})},
			{"System.Net.Json.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.System_Net_Json;})},
			{"System.Runtime.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.System_Runtime;})},
			{"System.Threading.Tasks.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.System_Threading_Tasks;})},
			{"Thought.vCards.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Thought_vCards;})},
			{"UpdateManager.exe", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.UpdateManager;})},
			{"UpdateManager.exe.config", new Lazy<byte[]>(()=>{ return UTF8NoBOM.GetBytes(SetupDependencies.Properties.Resources.UpdateManager_exe);})},
			{"Wintellect.Threading.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.Wintellect_Threading;})},
			{"WpfAnimatedGif.dll", new Lazy<byte[]>(()=>{ return SetupDependencies.Properties.Resources.WpfAnimatedGif;})}
		};

		#endregion

		// The high level goal is to be tolerant of encoding errors when we read and very strict 
		// when we write. Hence, default StreamWriter encoding will throw on encoding error.   
		// Note: when StreamWriter throws on invalid encoding chars (for ex, high surrogate character 
		// D800-DBFF without a following low surrogate character DC00-DFFF), it will cause the 
		// internal StreamWriter's state to be irrecoverable as it would have buffered the 
		// illegal chars and any subsequent call to Flush() would hit the encoding error again. 
		// Even Close() will hit the exception as it would try to flush the unwritten data. 
		// Maybe we can add a DiscardBufferedData() method to get out of such situation (like 
		// StreamReader though for different reason). Either way, the buffered data will be lost!
		private static volatile Encoding _UTF8NoBOM;

		internal static Encoding UTF8NoBOM
		{
			get
			{
				if (_UTF8NoBOM == null)
				{
					// No need for double lock - we just want to avoid extra
					// allocations in the common case.
					UTF8Encoding noBOM = new UTF8Encoding(false, true);
					Thread.MemoryBarrier();
					_UTF8NoBOM = noBOM;
				}
				return _UTF8NoBOM;
			}
		}

		private void install()
		{
			try
			{
				Thread.Sleep(Extensions.AnimationDuration.TimeSpan);

				total += files.Count;

				#region Uninstall

				if (InstallerData.InstalledVersion != null
					&& InstallerData.BackwardsCompatibleTo >= InstallerData.InstalledVersion)
				{
					total++;
					step("Uninstalling old version...");

					// Delete everything
					InstallerData.DeleteAccounts = InstallerData.DeleteDatabase = InstallerData.DeleteDictionary = InstallerData.DeleteSettings = true;

					UninstallerWorker uninstall = new UninstallerWorker();
					uninstall.Uninstall(false);
				}

				#endregion

				#region Files

				step("Creating folders...");

				//
				// File system
				//

				string[] directories = new string[] {
					"Uninstall",
					"Resources\\Media",
					"Resources\\Files",
					"Themes"
				};

				foreach (string each in directories)
					Directory.CreateDirectory(InstallerData.InstallLocation + "\\" + each);


				//
				// TODO: Create custom file installations
				//

				//
				// Example
				//

				// File.WriteAllBytes(
				//     InstallerData.InstallLocation + "\\MyProgram.exe",
				//     Properties.Resources.MyProgram
				// );

				foreach (KeyValuePair<string, Lazy<byte[]>> each in files)
				{
					step("Copying files...");
					File.WriteAllBytes(InstallerData.InstallLocation + "\\" + each.Key, each.Value.Value);
				}

				step("Copying uninstaller...");
				CopyUninstaller();

				#endregion

				#region Registry

				step("Updating system registry...");
				UpdateRegistry();
				Settings.JoinedCEIP = InstallerData.JoinedCEIP;
				Settings.FirstRun = false;

				step("Registering product...");

				// Don't reactivate if we are already activated. Otherwise simply installing a new
				// version over an older version of the product will eat product keys.
				if (!Activation.IsActivated(false))
				{
					if (InstallerData.ProductKey == null)
					{
						//
						// Don't overwrite trial information if it exists.
						//
						try
						{
							RegistryKey key = Registry.LocalMachine.OpenSubKey("Software\\"
								+ GlobalAssemblyInfo.AssemblyName + "\\Activation");
							object _k = key.GetValue("IDT", null);

							if (_k == null)
								Activation.TrialStart = DateTime.Now;
						}
						catch
						{
							Activation.TrialStart = DateTime.Now;
						}
					}
					else
					{
						Activation.ActivationGracePeriodStart = DateTime.Now;
						Activation.Key = InstallerData.ProductKey;

						try
						{
							Activation.ActivateOnline(5);
							Activation.Activate();
						}
						catch { }
					}
				}

				#endregion

				#region Shortcuts

				step("Installing shortcuts...");

				//
				// TODO: Create shortcuts to applications
				//

				//
				// Example
				//

				// CreateShortcut(
				//     Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + "\\MyProgram.lnk",
				//     InstallerData.InstallLocation + "\\ProductName.exe",
				//     "Click this link to launch MyProgram"
				// );

				//CreateShortcut(InstallerData.ShortcutLocation,
				//	InstallerData.InstallLocation + "\\Daytimer.exe",
				//	InstallerData.Description
				//);
				InstallShortcut();

				#endregion

				step("Optimizing performance for your system...");
				QueueNgen(InstallerData.MainProgramLocation);

				foreach (string each in InstallerData.IncludedAssemblies)
					QueueNgen(each);

				Complete();
			}
			catch (Exception exc)
			{
				Error(new InstallErrorEventArgs(exc.Message));
			}
		}

		public static void UpdateRegistry()
		{
			string hkey = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" + InstallerData.DisplayName;

			Registry.SetValue(hkey, "Comments", InstallerData.Comments, RegistryValueKind.String);
			Registry.SetValue(hkey, "Description", InstallerData.Description, RegistryValueKind.String);
			Registry.SetValue(hkey, "DisplayIcon", InstallerData.DisplayIcon, RegistryValueKind.String);
			Registry.SetValue(hkey, "DisplayName", InstallerData.DisplayName, RegistryValueKind.String);
			Registry.SetValue(hkey, "DisplayVersion", InstallerData.DisplayVersion, RegistryValueKind.String);
			Registry.SetValue(hkey, "EstimatedSize", CalculateInstallSize(), RegistryValueKind.DWord);
			Registry.SetValue(hkey, "HelpLink", InstallerData.HelpLink, RegistryValueKind.String);
			Registry.SetValue(hkey, "InstallDate", DateTime.Now.ToString("yyyyMMdd"), RegistryValueKind.String);
			Registry.SetValue(hkey, "InstallLocation", InstallerData.InstallLocation, RegistryValueKind.String);
			Registry.SetValue(hkey, "MajorVersion", InstallerData.MajorVersion, RegistryValueKind.String);
			Registry.SetValue(hkey, "MinorVersion", InstallerData.MinorVersion, RegistryValueKind.String);
			Registry.SetValue(hkey, "NoModify", InstallerData.NoModify, RegistryValueKind.DWord);
			Registry.SetValue(hkey, "NoRepair", InstallerData.NoRepair, RegistryValueKind.DWord);
			Registry.SetValue(hkey, "Publisher", InstallerData.Publisher, RegistryValueKind.String);
			Registry.SetValue(hkey, "Readme", InstallerData.Readme, RegistryValueKind.String);
			Registry.SetValue(hkey, "UninstallString", InstallerData.UninstallString, RegistryValueKind.String);
			Registry.SetValue(hkey, "URLInfoAbout", InstallerData.URLInfoAbout, RegistryValueKind.String);
			Registry.SetValue(hkey, "URLUpdateInfo", InstallerData.URLUpdateInfo, RegistryValueKind.String);

			// Internet Exploder
			try { Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", "Daytimer.exe", 0, RegistryValueKind.DWord); }
			catch { }
			try { Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", "Daytimer.exe", 0, RegistryValueKind.DWord); }
			catch { }

			try { InstallEventLog(); }
			catch { }
		}

		#region Do not modify

		public InstallerWorker()
		{

		}

		public void Install()
		{
			installThread = new Thread(install);
			installThread.Start();
		}

		Thread installThread;

		#region Shortcut

		private void InstallShortcut()
		{
			if (Environment.OSVersion.Version >= OSVersions.Win_7)
			{
				// On Windows 8.1+ we can customize the start menu tile.
				if (Environment.OSVersion.Version >= OSVersions.Win_8_1)
					install8Shortcut();

				// On Windows 7+ we want to be able to link jumplist tasks
				// to the main process.
				install7Shortcut();
			}
			else
				// XP/Vista have no special properties.
				installXPShortcut();
		}

		private void installXPShortcut()
		{
			for (int i = 0; i < 2; i++)
			{
				try
				{
					IWshRuntimeLibrary.WshShell wshShell = new IWshRuntimeLibrary.WshShell();
					IWshRuntimeLibrary.IWshShortcut myShortcut = (IWshRuntimeLibrary.IWshShortcut)wshShell.CreateShortcut(InstallerData.ShortcutLocation);
					myShortcut.TargetPath = InstallerData.MainProgramLocation;
					myShortcut.IconLocation = InstallerData.MainProgramLocation + ",0";
					myShortcut.Description = InstallerData.Description;

					myShortcut.Save();

					break;
				}
				catch
				{
					if (i == 1)
						throw;
				}
			}
		}

		private void install7Shortcut()
		{
			for (int i = 0; i < 2; i++)
			{
				try
				{
					IShellLinkW newShortcut = (IShellLinkW)new CShellLink();

					// Create a shortcut to the exe
					ErrorHelper.VerifySucceeded(newShortcut.SetPath(InstallerData.MainProgramLocation));
					ErrorHelper.VerifySucceeded(newShortcut.SetDescription(InstallerData.Description));
					ErrorHelper.VerifySucceeded(newShortcut.SetArguments(""));

					// Open the shortcut property store, set the AppUserModelId property
					IPropertyStore newShortcutProperties = (IPropertyStore)newShortcut;

					using (PropVariant appId = new PropVariant(GlobalAssemblyInfo.AssemblyName))
					{
						ErrorHelper.VerifySucceeded(newShortcutProperties.SetValue(SystemProperties.System.AppUserModel.ID, appId));
						ErrorHelper.VerifySucceeded(newShortcutProperties.Commit());
					}

					// Commit the shortcut to disk
					IPersistFile newShortcutSave = (IPersistFile)newShortcut;

					ErrorHelper.VerifySucceeded(newShortcutSave.Save(InstallerData.ShortcutLocation, true));

					break;
				}
				catch
				{
					if (i == 1)
						throw;
				}
			}
		}

		private void install8Shortcut()
		{
			for (int i = 0; i < 2; i++)
			{
				try
				{
					Directory.CreateDirectory(InstallerData.InstallLocation + "\\Assets");

					File.WriteAllText(InstallerData.InstallLocation + "\\Daytimer.VisualElementsManifest.xml",
						SetupDependencies.Properties.Resources.Daytimer_VisualElementsManifest);

					File.WriteAllBytes(InstallerData.InstallLocation + "\\Assets\\70x70Logo.png",
						SetupDependencies.Properties.Resources._70x70Logo_png);

					File.WriteAllBytes(InstallerData.InstallLocation + "\\Assets\\150x150Logo.png",
						SetupDependencies.Properties.Resources._150x150Logo_png);

					break;
				}
				catch
				{
					if (i == 1)
						throw;
				}
			}
		}

		#endregion

		private static void InstallEventLog()
		{
			if (!EventLog.SourceExists(GlobalAssemblyInfo.AssemblyName))
				EventLog.CreateEventSource(GlobalAssemblyInfo.AssemblyName, "Application");
		}

		/// <summary>
		/// Creates uninstaller for program
		/// </summary>
		private void CopyUninstaller()
		{
			Process proc = Process.GetCurrentProcess();
			string name = proc.MainModule.FileName;
			File.Copy(name, InstallerData.InstallLocation + "\\Uninstall\\Uninstall.exe", true);
			//File.Copy(name.Remove(name.LastIndexOf('\\') + 1) + "en-US\\Setup.resources.dll", InstallerData.InstallLocation + "\\Uninstall\\en-US\\Setup.resources.dll", true);
			File.Copy(name.Remove(name.LastIndexOf('\\') + 1) + "Microsoft.Windows.Shell.dll", InstallerData.InstallLocation + "\\Uninstall\\Microsoft.Windows.Shell.dll", true);
		}

		private int progress = 0;

		private void step(string message)
		{
			Progress(new ProgressEventArgs(progress++, total, message));
		}

		/// <summary>
		/// Install the assembly to the local native image cache.
		/// </summary>
		/// <param name="module"></param>
		private void QueueNgen(string module)
		{
			ProcessStartInfo info = new ProcessStartInfo(Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\Microsoft.NET\\Framework\\v4.0.30319\\ngen.exe", "install \"" + module + "\" /queue:3");

			if (Environment.OSVersion.Version >= OSVersions.Win_Vista)
				info.Verb = "runas";

			info.CreateNoWindow = true;
			info.WindowStyle = ProcessWindowStyle.Hidden;
			Process.Start(info);
		}

		private static int CalculateInstallSize()
		{
			double size = 0;

			string[] files = Directory.GetFiles(InstallerData.InstallLocation, "*", SearchOption.AllDirectories);

			foreach (string each in files)
				size += (double)new FileInfo(each).Length / 1024;

			return (int)size;
		}

		#region Events

		public delegate void OnProgressEvent(object sender, ProgressEventArgs e);

		public event OnProgressEvent OnProgress;

		protected void Progress(ProgressEventArgs e)
		{
			if (OnProgress != null)
				OnProgress(this, e);
		}

		public delegate void OnErrorEvent(object sender, InstallErrorEventArgs e);

		public event OnErrorEvent OnError;

		protected void Error(InstallErrorEventArgs e)
		{
			if (OnError != null)
				OnError(this, e);
		}

		public delegate void OnCompleteEvent(object sender, EventArgs e);

		public event OnCompleteEvent OnComplete;

		protected void Complete()
		{
			if (OnComplete != null)
				OnComplete(this, new EventArgs());
		}

		#endregion

		#endregion
	}

	class ProgressEventArgs : EventArgs
	{
		public ProgressEventArgs(int progress, int total, string message)
		{
			_progress = progress;
			_total = total;
			_message = message;
		}

		private int _progress;
		private int _total;
		private string _message;

		public int Progress
		{
			get { return _progress; }
			set { _progress = value; }
		}

		public int Total
		{
			get { return _total; }
			set { _total = value; }
		}

		public string Message
		{
			get { return _message; }
		}
	}

	class InstallErrorEventArgs : EventArgs
	{
		public InstallErrorEventArgs(string message)
		{
			_message = message;
		}

		private string _message;

		public string Message
		{
			get { return _message; }
		}
	}
}