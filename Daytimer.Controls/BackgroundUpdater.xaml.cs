using Daytimer.Dialogs;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for BackgroundUpdater.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class BackgroundUpdater : Button
	{
		public BackgroundUpdater()
		{
			InitializeComponent();

			Loaded += BackgroundUpdater_Loaded;
		}

		private bool _isFirstTime = true;

		private void BackgroundUpdater_Loaded(object sender, RoutedEventArgs e)
		{
			if (!_isFirstTime)
				return;

			_isFirstTime = false;

			Task.Factory.StartNew(CheckForUpdates);
		}

		private Process updateProcess;

		private void CheckForUpdates()
		{
			string localPath = (new FileInfo(Process.GetCurrentProcess().MainModule.FileName)).DirectoryName;
			ProcessStartInfo pInfo = new ProcessStartInfo(localPath + "\\UpdateManager.exe");
			pInfo.UseShellExecute = false;
			pInfo.Arguments = Process.GetCurrentProcess().Id.ToString() + " True";
			pInfo.RedirectStandardOutput = true;
			pInfo.RedirectStandardInput = true;
			pInfo.RedirectStandardError = true;
			updateProcess = Process.Start(pInfo);
			updateProcess.PriorityClass = ProcessPriorityClass.BelowNormal;
			updateProcess.EnableRaisingEvents = true;
			updateProcess.BeginOutputReadLine();
			updateProcess.BeginErrorReadLine();
			updateProcess.OutputDataReceived += updateProcess_OutputDataReceived;
			updateProcess.ErrorDataReceived += updateProcess_ErrorDataReceived;
			updateProcess.Exited += updateProcess_Exited;
		}

		private void updateProcess_Exited(object sender, EventArgs e)
		{
			if (((Process)sender).ExitCode == 1)
				Dispatcher.BeginInvoke(ShowError);
			else
				Dispatcher.BeginInvoke(ShowComplete);
		}

		private void updateProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			double val = 0.0;

			if (double.TryParse(e.Data, out val))
				Dispatcher.BeginInvoke(() => { ShowProgress(val); });
		}

		string _error = null;

		private void updateProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(e.Data))
				_error = e.Data;
		}

		private void ShowProgress(double value)
		{
			DisplayMode = DisplayType.Progress;
			status.Text = "DOWNLOADING UPDATES";
			progress.IsIndeterminate = false;
			progress.Value = value * 100;
		}

		private void ShowError()
		{
			DisplayMode = DisplayType.Error;
			status.Text = "UPDATE ERROR";
			progress.Visibility = Visibility.Collapsed;
			progress.IsIndeterminate = false;
			exclamation.Visibility = Visibility.Visible;
		}

		private void ShowComplete()
		{
			DisplayMode = DisplayType.Complete;
			status.Text = "UPDATE COMPLETE";
			progress.Visibility = Visibility.Collapsed;
			progress.IsIndeterminate = false;
		}

		private enum DisplayType : byte { Waiting, Progress, Error, Complete };
		private DisplayType DisplayMode = DisplayType.Waiting;

		protected override void OnClick()
		{
			base.OnClick();

			switch (DisplayMode)
			{
				case DisplayType.Waiting:
				case DisplayType.Progress:
					updateProcess.StandardInput.WriteLine("show");
					break;

				case DisplayType.Error:
					((ItemsControl)Parent).Items.Remove(this);
					TaskDialog td = new TaskDialog(Application.Current.MainWindow, "Update Error", _error, MessageType.Error);
					td.ShowDialog();
					break;

				case DisplayType.Complete:
					((ItemsControl)Parent).Items.Remove(this);
					break;

				default:
					break;
			}
		}
	}
}
