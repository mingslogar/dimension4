using Daytimer.Dialogs;
using Daytimer.Functions;
using Setup.InstallHelpers;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Setup.Install
{
	/// <summary>
	/// Interaction logic for InstallProgress.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class InstallProgress : UserControl
	{
		public InstallProgress()
		{
			InitializeComponent();
			Loaded += InstallProgress_Loaded;
		}

		private void InstallProgress_Loaded(object sender, RoutedEventArgs e)
		{
			if (InstallerData.ProductKey == null)
				if (Activation.IsActivated(false))
					doneMsg.Text = "We hope you enjoy your new Dimension 4!";
				else
					doneMsg.Text = "We're all done with the install. Enjoy your Dimension 4 trial!";
			else
				doneMsg.Text = "We're all done, and you can now go offline if you need to. Enjoy!";

			DispatcherTimer timer = new DispatcherTimer();
			timer.Interval = Extensions.AnimationDuration.TimeSpan;
			timer.Tick += timer_Tick;
			timer.Start();
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			DispatcherTimer timer = (DispatcherTimer)sender;
			timer.Stop();
			timer.Tick -= timer_Tick;
			timer = null;

			if (InstallerData.InstalledVersion != null
				&& InstallerData.BackwardsCompatibleTo >= InstallerData.InstalledVersion)
			{
				TaskDialog td = new TaskDialog(Window.GetWindow(this),
					"Version conflict",
					"This version of " + InstallerData.DisplayName + " is not compatible with the currently installed version. The old version will be completely uninstalled, and you will lose any cloud accounts, appointments, contacts, tasks, notes, or custom dictionaries that you may have saved.",
					MessageType.Exclamation, true);

				if (td.ShowDialog() == true)
				{
					InstallerWorker installer = new InstallerWorker();
					installer.OnProgress += installer_OnProgress;
					installer.OnError += installer_OnError;
					installer.OnComplete += installer_OnComplete;
					installer.Install();
				}
				else
				{
					CancelSetup(null);
				}
			}
			else
			{
				InstallerWorker installer = new InstallerWorker();
				installer.OnProgress += installer_OnProgress;
				installer.OnError += installer_OnError;
				installer.OnComplete += installer_OnComplete;
				installer.Install();
			}
		}

		private void installer_OnProgress(object sender, ProgressEventArgs e)
		{
			ShowProgress(e);
		}

		private void installer_OnError(object sender, InstallErrorEventArgs e)
		{
			CancelSetup(e);
		}

		private void installer_OnComplete(object sender, EventArgs e)
		{
			CompleteSetup();
		}

		private void CancelSetup(InstallErrorEventArgs e)
		{
			Dispatcher.Invoke(new StringDelegate(showerror), new object[] { e != null ? e.Message : null });
		}

		private void CompleteSetup()
		{
			Dispatcher.Invoke(new VoidDelegate(complete));
		}

		private void ShowProgress(ProgressEventArgs e)
		{
			Dispatcher.Invoke(new ProgressEventDelegate(showprogress), new object[] { e });
		}

		private delegate void StringDelegate(string data);
		private delegate void ProgressEventDelegate(ProgressEventArgs e);
		private delegate void VoidDelegate();

		private void showprogress(ProgressEventArgs e)
		{
			DoubleAnimation anim = new DoubleAnimation((double)e.Progress * 100 / (double)e.Total, Extensions.AnimationDuration);
			setupProgress.BeginAnimation(ProgressBar.ValueProperty, anim);

			setupMessage.Text = e.Message;
		}

		private void showerror(string data)
		{
			if (data != null)
			{
				setupMessage.Text = "ERROR: " + data;
				setupMessage.FontSize = 12;
				setupMessage.Foreground = Brushes.Red;
			}
			else
			{
				setupMessage.Text = "Operation canceled.";
			}

			DoubleAnimation slideanim = new DoubleAnimation(100, Extensions.AnimationDuration);
			setupProgress.BeginAnimation(ProgressBar.ValueProperty, slideanim);

			OnErrorEvent(new EventArgs());
		}

		private void complete()
		{
			installGrid.FadeOut(completeGrid);

			DoubleAnimation anim = new DoubleAnimation(100, Extensions.AnimationDuration);
			setupProgress.BeginAnimation(ProgressBar.ValueProperty, anim);

			OnCompletedEvent(new EventArgs());
		}

		#region Events

		public delegate void CompletedEvent(object sender, EventArgs e);

		public event CompletedEvent Completed;

		protected void OnCompletedEvent(EventArgs e)
		{
			if (Completed != null)
				Completed(this, e);
		}

		public delegate void ErrorEvent(object sender, EventArgs e);

		public event ErrorEvent Error;

		protected void OnErrorEvent(EventArgs e)
		{
			if (Error != null)
				Error(this, e);
		}

		#endregion
	}
}
