using Daytimer.Dialogs;
using Daytimer.Functions;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using UpdateManager;

namespace Daytimer.Controls.Ribbon
{
	/// <summary>
	/// Interaction logic for InfoControl.xaml
	/// </summary>
	public partial class InfoControl : Grid
	{
		public InfoControl()
		{
			InitializeComponent();

			assemblyVersion.Text = AssemblyVersion.ToString();

			DateTime lastSuccessfulUpdate = Settings.LastSuccessfulUpdate;

			if (lastSuccessfulUpdate != DateTime.MinValue)
				lastUpdated.Text = "Last updated "
					+ (lastSuccessfulUpdate.Date == DateTime.Now.Date ? "today"
					: (lastSuccessfulUpdate.Date == DateTime.Now.Date.AddDays(-1) ? "yesterday"
					: lastSuccessfulUpdate.ToString("MMMM d, yyyy"))) + " at " + RandomFunctions.FormatTime(lastSuccessfulUpdate.TimeOfDay)
					+ ".";
			else
				lastUpdated.Text = "Last update: Never.";

			Loaded += InfoControl_Loaded;
		}

		private async void InfoControl_Loaded(object sender, RoutedEventArgs e)
		{
			// If the current user is not an admin, don't even enable the
			// check for updates button.
			if (await Task.Factory.StartNew<bool>(() => { return UserInfo.IsCurrentUserAdmin; }))
				await EnableUpdatesButton();
			else
				updatesButton.Visibility = Visibility.Collapsed;
		}

		private async Task EnableUpdatesButton()
		{
			updatesButton.IsEnabled = await Task.Factory.StartNew<bool>(() =>
			{
				try
				{
					Process[] procs = Process.GetProcessesByName("UpdateManager");

					string filename = new FileInfo(Process.GetCurrentProcess().MainModule.FileName).DirectoryName + "\\UpdateManager.exe";

					foreach (Process each in procs)
					{
						if (each.MainModule.FileName == filename)
						{
							Dispatcher.BeginInvoke(() => { updatesButton.IsEnabled = false; });
							each.EnableRaisingEvents = true;
							each.Exited -= p_Exited;
							each.Exited += p_Exited;

							return false;
						}
					}
				}
				catch { }

				return true;
			});
		}

		#region Assembly Attribute Accessors

		private Version AssemblyVersion
		{
			get
			{
				return Assembly.GetEntryAssembly().GetName().Version;
			}
		}

		#endregion

		#region Updates

		private void CheckForUpdates()
		{
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
					TaskDialog td = new TaskDialog(Application.Current.MainWindow, "Update Error", "Updates could not complete due to insufficient privileges.", MessageType.Error);
					td.ShowDialog();
					return;
				}
			}

			string localPath = (new FileInfo(Process.GetCurrentProcess().MainModule.FileName)).DirectoryName;
			ProcessStartInfo pUpdateInfo = new ProcessStartInfo(localPath + "\\UpdateManager.exe");
			pUpdateInfo.UseShellExecute = false;
			pUpdateInfo.Arguments = Process.GetCurrentProcess().Id.ToString() + " False";
			pUpdateInfo.RedirectStandardOutput = true;
			Process p = Process.Start(pUpdateInfo);
			p.EnableRaisingEvents = true;
			p.BeginOutputReadLine();
			p.OutputDataReceived += p_OutputDataReceived;
			p.Exited += p_Exited;
		}

		private void p_Exited(object sender, EventArgs e)
		{
			if (((Process)sender).ExitCode != 0)
			{
				// Something happened to the update process
				Dispatcher.BeginInvoke(() => { updatesButton.IsEnabled = true; });
			}
		}

		private void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (e.Data == "restart")
			{
				Dispatcher.BeginInvoke(ShutDown);
			}
		}

		private void ShutDown()
		{
			try { Application.Current.MainWindow.Close(); }
			catch { }
		}

		private void updatesButton_Click(object sender, RoutedEventArgs e)
		{
			(sender as Button).IsEnabled = false;

			Process[] procs = Process.GetProcessesByName("UpdateManager");

			string filename = new FileInfo(Process.GetCurrentProcess().MainModule.FileName).DirectoryName + "\\UpdateManager.exe";

			foreach (Process each in procs)
			{
				if (each.MainModule.FileName == filename)
				{
					TaskDialog td = new TaskDialog(Application.Current.MainWindow, "Updates",
						"Daytimer is already checking for, downloading, or installing updates.", MessageType.Information);
					td.ShowDialog();

					return;
				}
			}

			Thread checkupdates = new Thread(CheckForUpdates);
			checkupdates.Start();
		}

		#endregion

		private void creditsButton_Click(object sender, RoutedEventArgs e)
		{
			Credits credits = new Credits();
			credits.Owner = Application.Current.MainWindow;
			credits.ShowDialog();
		}
	}
}