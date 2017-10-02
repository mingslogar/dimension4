using Daytimer.Functions;
using Daytimer.Fundamentals;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace Daytimer.GoogleCalendarHelpers
{
	/// <summary>
	/// Interaction logic for BackgroundSyncDialog.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class BackgroundSyncDialog : BalloonTip
	{
		public static BackgroundSyncDialog LastUsedSyncDialog = null;

		public BackgroundSyncDialog(UIElement ownerControl, SyncHelper syncHelper)
			: base(ownerControl)
		{
			InitializeComponent();
			LastUsedSyncDialog = this;

			_syncHelper = syncHelper;

			PositionOrder = new PositionOrder(Location.Top, Location.Bottom, Location.Right, Location.Left, Location.Top);

			syncHelper.OnStatusChangedEvent += syncHelper_OnStatusChangedEvent;
			syncHelper.OnProgressChangedEvent += syncHelper_OnProgressChangedEvent;
			syncHelper.OnCompletedEvent += syncHelper_OnCompletedEvent;

			Unloaded += BackgroundSyncDialog_Unloaded;

			InitializeDisplay();
		}

		private SyncHelper _syncHelper;

		private void BackgroundSyncDialog_Unloaded(object sender, RoutedEventArgs e)
		{
			_syncHelper.OnStatusChangedEvent += syncHelper_OnStatusChangedEvent;
			_syncHelper.OnProgressChangedEvent += syncHelper_OnProgressChangedEvent;
			_syncHelper.OnCompletedEvent += syncHelper_OnCompletedEvent;
		}

		private void InitializeDisplay()
		{
			if (!_syncHelper.Done)
			{
				UpdateStatus();

				if (double.IsNaN(_syncHelper.Progress))
				{
					progress.Visibility = Visibility.Hidden;
				}
				else
				{
					indeterminateProgress.Stop();
					indeterminateProgress.Visibility = Visibility.Collapsed;

					progress.Visibility = Visibility.Visible;
					progress.Value = _syncHelper.Progress;
				}
			}
			else
			{
				if (_syncHelper.Error != null)
				{
					if (Settings.JoinedCEIP)
						status.Text = "Error: " + _syncHelper.Error.Message;
					else
						status.Text = "Error completing send/receive. Please re-sync to try again.";

					//ContentHeight = 200;
					//ContentWidth = 350;
				}
				else
					status.Text = "Send/receive completed";

				progress.Visibility = Visibility.Visible;
				progress.Value = 100;

				indeterminateProgress.Stop();
				indeterminateProgress.Visibility = Visibility.Collapsed;
			}
		}

		private void syncHelper_OnStatusChangedEvent(object sender, EventArgs e)
		{
			UpdateStatus();
		}

		private void syncHelper_OnProgressChangedEvent(object sender, EventArgs e)
		{
			indeterminateProgress.Stop();
			indeterminateProgress.Visibility = Visibility.Collapsed;

			progress.Visibility = Visibility.Visible;
			progress.Value = _syncHelper.Progress;
		}

		private void syncHelper_OnCompletedEvent(object sender, EventArgs e)
		{
			if (_syncHelper.Error != null)
			{
				if (Settings.JoinedCEIP)
					status.Text = "Error: " + _syncHelper.Error.Message;
				else
					status.Text = "Error completing send/receive. Please re-sync to try again.";

				//ContentHeight = 200;
				//ContentWidth = 350;

				UpdateLayout();
				Dispatcher.BeginInvoke(RefreshPosition);
			}
			else
				status.Text = "Send/receive completed";

			progress.Visibility = Visibility.Visible;
			progress.Value = 100;

			indeterminateProgress.Stop();
			indeterminateProgress.Visibility = Visibility.Collapsed;
		}

		private void closeButton_Click(object sender, RoutedEventArgs e)
		{
			FastClose();
		}

		private void UpdateStatus()
		{
			status.Text = _syncHelper.Status + (!_syncHelper.Status.EndsWith("...") ? "..." : "");
		}
	}
}
