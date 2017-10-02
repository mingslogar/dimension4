using Daytimer.Dialogs;
using Setup.InstallHelpers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Setup.Uninstall
{
	/// <summary>
	/// Interaction logic for InstallProgress.xaml
	/// </summary>
	public partial class UninstallProgress : UserControl
	{
		public UninstallProgress()
		{
			InitializeComponent();
		}

		private void UninstallProgress_Loaded(object sender, RoutedEventArgs e)
		{
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

			UninstallWorker();
		}

		private void UninstallWorker()
		{
			TaskDialog td = new TaskDialog(Window.GetWindow(this), "Uninstall confirmation",
				"Are you sure you want to completely remove " + InstallerData.DisplayName + "?",
				MessageType.Question, true);

			if (td.ShowDialog() == true)
			{
				UninstallerWorker uninstall = new UninstallerWorker();
				uninstall.OnProgress += uninstall_OnProgress;
				uninstall.OnComplete += uninstall_OnComplete;
				uninstall.OnError += uninstall_OnError;
				uninstall.Uninstall();
			}
			else
			{
				InstallerData.UninstallMode = false;
				CancelSetup(null);
			}
		}

		private void uninstall_OnError(object sender, InstallErrorEventArgs e)
		{
			CancelSetup(e);
		}

		private void uninstall_OnComplete(object sender, EventArgs e)
		{
			CompleteSetup();
		}

		private void uninstall_OnProgress(object sender, ProgressEventArgs e)
		{
			ShowProgress(e);
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
			DoubleAnimation anim = new DoubleAnimation((double)e.Progress * 100 / (double)e.Total,
				Extensions.AnimationDuration);
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
