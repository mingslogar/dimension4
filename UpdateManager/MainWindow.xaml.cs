using Daytimer.Dialogs;
using Daytimer.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Xml;

namespace UpdateManager
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class MainWindow : DialogBase
	{
		public MainWindow(int _pid, bool _quiet)
		{
			pid = _pid;
			quiet = _quiet;

			if (!quiet)
			{
				InitializeComponent();
				Loaded += MainWindow_Loaded;
				Show();
			}
			else
			{
				ShowActivated = false;

				Thread consoleListen = new Thread(Listen);
				consoleListen.IsBackground = true;
				consoleListen.Start();

				check = new Thread(CheckForUpdates);
				check.IsBackground = true;
				check.Start();
			}
		}

		private void Listen()
		{
			try
			{
				while (true)
				{
					string console = Console.ReadLine().ToLower();

					switch (console)
					{
						case "show":
							preventClose = false;
							quiet = false;
							break;

						default:
							break;
					}

					Dispatcher.BeginInvoke(() => { HandleConsole(console); });
				}
			}
			catch { }
		}

		private void HandleConsole(string message)
		{
			try
			{
				switch (message)
				{
					case "show":
						InitializeComponent();
						Show();
						Activate();
						quiet = false;

						if (_percent.HasValue)
						{
							progress.Visibility = Visibility.Visible;
							progress.Value = _percent.Value;

							indeterminateProgress.Stop();
							indeterminateProgress.Visibility = Visibility.Collapsed;
						}

						if (!error)
						{
							progressText.Text = _message;
						}
						else
						{
							progressText.Foreground = Brushes.Red;
							message = "Error: " + message;
						}
						break;

					default:
						break;
				}
			}
			catch { }
		}

		private int pid;
		private bool quiet;
		private bool preventClose = true;

		private void MainWindow_Loaded(object sender, EventArgs e)
		{
			Activate();
		}

		protected override void OnContentRendered(EventArgs e)
		{
			base.OnContentRendered(e);

			Dispatcher.BeginInvoke(() =>
			{
				check = new Thread(CheckForUpdates);
				check.Start();
			}, DispatcherPriority.Background);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			if (preventClose)
			{
				if (cancelCloseButton != null)
				{
					if (cancelCloseButton.Content.ToString() == "_Cancel")
					{
						TaskDialog dialog = new TaskDialog(this, "Cancel Updates", "Closing the update manager will halt the update process.", MessageType.Exclamation, true);
						if (dialog.ShowDialog() == false)
							e.Cancel = true;
						else
							//Environment.Exit(1);
							Application.Current.Shutdown(1);
					}
					else if (error)
						//Environment.Exit(1);
						Application.Current.Shutdown(1);
				}
				else if (error)
					//Environment.Exit(1);
					Application.Current.Shutdown(1);
			}
			else if (error)
				//Environment.Exit(1);
				Application.Current.Shutdown(1);
		}

		private Thread check;

		#region Functions

		private void CheckForUpdates()
		{
			string url = GlobalData.Website + "/updates/";

			try
			{
				WebClient myClient = new WebClient();

				string data = myClient.DownloadString(url + "version");

				XmlDocument doc = new XmlDocument();
				doc.LoadXml(data);

				List<DownloadObject> downloadQueue = new List<DownloadObject>();
				int totalSize = 0;

				string localPath = (new FileInfo(Process.GetCurrentProcess().MainModule.FileName)).DirectoryName;

				XmlNodeList modules = doc.GetElementsByTagName("file");

				foreach (XmlNode each in modules)
				{
					try
					{
						string id = each.Attributes["id"].Value;
						string path = each.Attributes["path"].Value;
						string localFile = localPath + path + "\\" + id;

						if (File.Exists(localFile))
						{
							string remoteVersion = each.Attributes["version"].Value;
							string localVersion = Version(localFile);

							//if (remoteVersion != localVersion)
							if (IsVersionHigher(localVersion, remoteVersion))
							{
								int size = int.Parse(each.Attributes["size"].Value);
								totalSize += size;
								downloadQueue.Add(new DownloadObject(id, path, size));
							}
						}
						else
						{
							int size = int.Parse(each.Attributes["size"].Value);
							totalSize += size;
							downloadQueue.Add(new DownloadObject(id, path, size));
						}
					}
					catch { }
				}

				string totalSizeString = Math.Round((double)totalSize / 1024, 2).ToString() + " KB";

				if (downloadQueue.Count > 0)
				{
					int currentProgress = 0;

					Dispatcher.Invoke(() => { ShowProgress("Downloading updates... (0 KB of " + totalSizeString + ")", 0, false); });

					if (!Directory.Exists(Data.UpdatesWorkingDirectory))
						Directory.CreateDirectory(Data.UpdatesWorkingDirectory);

					foreach (DownloadObject each in downloadQueue)
					{
						string filename = each.ID;

						string path = Data.UpdatesWorkingDirectory + each.Path;

						if (!Directory.Exists(path))
							Directory.CreateDirectory(path);

						myClient.DownloadFile(url + filename, path + "\\~" + filename);

						// Set date modified on the downloaded file so that it will match with the server
						if (!filename.EndsWith(".exe") && !filename.EndsWith(".dll"))
							(new FileInfo(path + "\\~" + filename)).LastWriteTimeUtc = DateTime.Parse(doc.GetElementById(filename).Attributes["version"].Value);

						currentProgress += each.Size;

						if (currentProgress != totalSize)
							Dispatcher.Invoke(() => { ShowProgress("Downloading updates... (" + Math.Round((double)currentProgress / 1024, 2).ToString() + " KB of " + totalSizeString + ")", (double)currentProgress / (double)totalSize, false); });
						else
							Dispatcher.Invoke(() => { ShowProgress("Download complete.", 1, false); });
					}
				}
				else
				{
					Dispatcher.Invoke(ShowUpToDate);
				}
			}
			catch (ThreadAbortException)
			{
				DeletePendingUpdates();

				Thread.ResetAbort();
				Dispatcher.Invoke(() => { ShowProgress("Download was canceled.", 1, true); });
			}
			catch (Exception exc)
			{
				DeletePendingUpdates();

				Dispatcher.Invoke(() =>
				{
					ShowProgress(
						(exc.Message.EndsWith("'") || exc.Message.EndsWith("\"")) && exc.Message[exc.Message.Length - 2] != '.'
						? exc.Message.Insert(exc.Message.Length - 1, ".")
						: exc.Message[exc.Message.Length - 1] != '.' ? exc.Message + "." : exc.Message, 1, true
					);
				});
			}
		}

		private string Version(string file)
		{
			FileVersionInfo info = FileVersionInfo.GetVersionInfo(file);

			if (!string.IsNullOrEmpty(info.ProductVersion))
				return info.ProductVersion;
			else
				return (new FileInfo(file)).LastWriteTimeUtc.ToString();
		}

		public bool IsVersionHigher(string oldVersion, string newVersion)
		{
			if (oldVersion.Contains("."))
			{
				return System.Version.Parse(newVersion) > System.Version.Parse(oldVersion);
				//string[] oldSplit = oldVersion.Split('.');
				//string[] newSplit = newVersion.Split('.');

				//for (int i = 0; i < 4; i++)
				//{
				//	int n = int.Parse(newSplit[i]);
				//	int o = int.Parse(oldSplit[i]); ;

				//	if (n < o)
				//		return false;

				//	if (n > o)
				//		return true;
				//}

				//return false;
			}
			else
			{
				return DateTime.Parse(newVersion) > DateTime.Parse(oldVersion);
			}
		}

		private void ShowUpToDate()
		{
			Settings.LastSuccessfulUpdate = DateTime.Now;

			if (!quiet)
			{
				indeterminateProgress.Stop();
				indeterminateProgress.Visibility = Visibility.Collapsed;

				cancelCloseButton.Content = "_Close";
				progressText.Text = GlobalAssemblyInfo.AssemblyName + " is up to date.";
			}
			else
				//Environment.Exit(0);
				Application.Current.Shutdown(0);
		}

		private bool error = false;
		private double? _percent = null;
		private string _message = null;

		private void ShowProgress(string message, double percent, bool iserror)
		{
			_message = message;
			_percent = percent;

			if (!quiet)
			{
				progress.Visibility = Visibility.Visible;

				indeterminateProgress.Stop();
				indeterminateProgress.Visibility = Visibility.Collapsed;
			}

			try
			{
				if (!iserror)
					Console.WriteLine(percent);
				else if (Console.Error != null)
					Console.Error.WriteLine(message);
			}
			catch { }

			if (percent == 1)
			{
				if (!iserror)
				{
					if (!quiet)
						restartButton.Visibility = Visibility.Visible;

					Settings.LastSuccessfulUpdate = DateTime.Now;
				}

				if (quiet)
				{
					if (iserror)
						//Environment.Exit(1);
						Application.Current.Shutdown(1);
					else
						//Environment.Exit(0);
						Application.Current.Shutdown(0);
				}
				else
					cancelCloseButton.Content = "_Close";
			}

			if (iserror)
			{
				error = true;

				if (!quiet)
				{
					progressText.Foreground = Brushes.Red;
					message = "Error: " + message;
				}
				else
					//Environment.Exit(1);
					Application.Current.Shutdown(1);
			}

			if (!quiet)
			{
				DoubleAnimation valueAnim = new DoubleAnimation(percent * 100, new Duration(TimeSpan.FromMilliseconds(200)));
				progress.BeginAnimation(ProgressBar.ValueProperty, valueAnim);

				progressText.Text = message;
			}
		}

		/// <summary>
		/// Delete all update files which were downloaded.
		/// </summary>
		private void DeletePendingUpdates()
		{
			if (Directory.Exists(Data.UpdatesWorkingDirectory))
				try { Directory.Delete(Data.UpdatesWorkingDirectory, true); }
				catch { }
		}

		#endregion

		private void cancelButton_Click(object sender, RoutedEventArgs e)
		{
			if (cancelCloseButton.Content.ToString() == "_Cancel")
			{
				if (check.IsAlive)
					check.Abort();
				else
					Close();
			}
			else
			{
				if (error)
					//Environment.Exit(1);
					Application.Current.Shutdown(1);
				else
					Close();
			}
		}

		private void restartButton_Click(object sender, RoutedEventArgs e)
		{
			restartButton.IsEnabled = false;

			Thread restart = new Thread(Restart);
			restart.Start();
		}

		private void Restart()
		{
			Process proc = Process.GetProcessById(pid);

			if (proc != null)
			{
				Console.WriteLine("restart");
				proc.WaitForExit();
			}

			string localPath = (new FileInfo(Process.GetCurrentProcess().MainModule.FileName)).DirectoryName;
			ProcessStartInfo pInfo = new ProcessStartInfo(localPath + "\\Daytimer.exe");
			pInfo.UseShellExecute = true;
			Process.Start(pInfo);

			Dispatcher.Invoke(CloseThreadSafe);
		}

		private void CloseThreadSafe()
		{
			if (CheckAccess())
				try
				{
					Close();
					return;
				}
				catch
				{ }

			if (!error)
				//Environment.Exit(0);
				Application.Current.Shutdown(0);
			else
				//Environment.Exit(1);
				Application.Current.Shutdown(1);
		}
	}
}
