using Daytimer.Functions;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Setup.InstallHelpers
{
	class UninstallerWorker
	{
		private void uninstall()
		{
			try
			{
				Thread.Sleep(200);

				int total = 5;
				int progress = 0;

				Progress(new ProgressEventArgs(progress++, total, "Deleting files..."));

				// Something could have happened and the files were already deleted.
				// If so, we don't want to crash, merely continue with the uninstall.
				if (Directory.Exists(InstallerData.InstallLocation))
				{
					string[] files = Directory.GetFileSystemEntries(InstallerData.InstallLocation);
					foreach (string each in files)
					{
						try
						{
							if (Directory.Exists(each))
								Directory.Delete(each, true);
							else
								File.Delete(each);
						}
						catch { }
					}
				}

				string databaseloc = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + GlobalAssemblyInfo.AssemblyName;

				if (InstallerData.DeleteDatabase)
				{
					string[] databases = new string[] {
						databaseloc + "\\Appointments",
						databaseloc + "\\Contacts",
						databaseloc + "\\Notes",
						databaseloc + "\\Quotes",
						databaseloc + "\\Sync",
						databaseloc + "\\Tasks",
						databaseloc + "\\Weather"
					};

					foreach (string each in databases)
						if (Directory.Exists(each))
							Directory.Delete(each, true);
				}

				if (InstallerData.DeleteDictionary)
				{
					string dictionaryloc = databaseloc + "\\CustomDictionary.lex";

					if (File.Exists(dictionaryloc))
						File.Delete(dictionaryloc);
				}

				if (InstallerData.DeleteSettings)
				{
					string qatloc = databaseloc + "\\QatLayout.xml";

					if (File.Exists(qatloc))
						File.Delete(qatloc);
				}

				// Delete directory if empty
				if (Directory.Exists(databaseloc) && Directory.GetFileSystemEntries(databaseloc).Length == 0)
					Directory.Delete(databaseloc, true);

				Progress(new ProgressEventArgs(progress++, total, "Deleting shortcuts..."));

				if (File.Exists(InstallerData.ShortcutLocation))
					File.Delete(InstallerData.ShortcutLocation);

				Progress(new ProgressEventArgs(progress++, total, "Deleting registry keys..."));

				if (InstallerData.DeleteSettings)
				{
					Registry.CurrentUser.DeleteSubKeyTree(@"Software\" + GlobalAssemblyInfo.AssemblyName + @"\Settings", false);
					Registry.CurrentUser.DeleteSubKeyTree(@"Software\" + GlobalAssemblyInfo.AssemblyName + @"\Experiments", false);
				}

				if (InstallerData.DeleteAccounts)
					Registry.CurrentUser.DeleteSubKeyTree(@"Software\" + GlobalAssemblyInfo.AssemblyName + @"\Accounts", false);

				try { UninstallEventLog(); }
				catch { }

				Progress(new ProgressEventArgs(progress++, total, "Deregistering product..."));

				//
				// Don't delete trial information if it exists.
				//
				try
				{
					RegistryKey key = Registry.LocalMachine.OpenSubKey("Software\\" + GlobalAssemblyInfo.AssemblyName + @"\\Activation");
					object _k = key.GetValue("Key", null);

					if ((_k != null && Activation.IsKeyValid((string)_k)) || !key.GetValueNames().Contains("IDT") || !key.GetValueNames().Contains("IDA"))
					{
						// Deactivate
						try
						{
							Activation.DeactivateOnline(5);
							Registry.LocalMachine.DeleteSubKey("Software\\" + GlobalAssemblyInfo.AssemblyName + @"\\Activation", false);
						}
						catch
						{
							// If we can't deactivate the product online, we don't
							// want to delete licensing data on the client computer.
							// Otherwise this could create a situation where the user
							// has no installed product and also no available keys.
						}
					}
				}
				catch { }

				try
				{
					if (Registry.CurrentUser.OpenSubKey("Software\\" + GlobalAssemblyInfo.AssemblyName).SubKeyCount == 0)
						Registry.CurrentUser.DeleteSubKey("Software\\" + GlobalAssemblyInfo.AssemblyName, false);
				}
				catch { }

				try
				{
					if (Registry.LocalMachine.OpenSubKey("Software\\" + GlobalAssemblyInfo.AssemblyName).SubKeyCount == 0)
						Registry.LocalMachine.DeleteSubKey("Software\\" + GlobalAssemblyInfo.AssemblyName, false);
				}
				catch { }

				string hkey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" + InstallerData.DisplayName;

				try
				{
					Registry.LocalMachine.DeleteSubKey(hkey, false);
				}
				catch { }

				// Internet Exploder
				try { Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION").DeleteValue("Daytimer.exe", false); }
				catch { }
				try { Registry.LocalMachine.OpenSubKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION").DeleteValue("Daytimer.exe", false); }
				catch { }

				Progress(new ProgressEventArgs(progress++, total, "Removing application cache..."));
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

		#region Do not modify

		public UninstallerWorker()
		{

		}

		public void Uninstall()
		{
			uninstallThread = new Thread(uninstall);
			uninstallThread.Start();
		}

		Thread uninstallThread;

		public void Uninstall(bool background)
		{
			if (background)
			{
				uninstallThread = new Thread(uninstall);
				uninstallThread.Start();
			}
			else
				uninstall();
		}

		private void UninstallEventLog()
		{
			if (EventLog.SourceExists(GlobalAssemblyInfo.AssemblyName))
				EventLog.DeleteEventSource(GlobalAssemblyInfo.AssemblyName);
		}

		/// <summary>
		/// Uninstall the assembly from the local native image cache.
		/// </summary>
		/// <param name="module"></param>
		private void QueueNgen(string module)
		{
			try
			{
				ProcessStartInfo info = new ProcessStartInfo(Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\Microsoft.NET\\Framework\\v4.0.30319\\ngen.exe", "uninstall \"" + module + "\"");

				if (Environment.OSVersion.Version >= OSVersions.Win_Vista)
					info.Verb = "runas";

				info.UseShellExecute = true;
				info.CreateNoWindow = true;
				info.WindowStyle = ProcessWindowStyle.Hidden;
				Process.Start(info);
			}
			catch { }
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
}
