using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Daytimer.GoogleCalendarHelpers
{
	/// <summary>
	/// Interaction logic for BackgroundSyncMonitor.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class BackgroundSyncMonitor : Button
	{
		public BackgroundSyncMonitor()
		{
			InitializeComponent();

			syncHelper = new SyncHelper();
			syncHelper.OnStatusChangedEvent += syncHelper_OnStatusChangedEvent;
			syncHelper.OnProgressChangedEvent += syncHelper_OnProgressChangedEvent;
			syncHelper.OnCompletedEvent += syncHelper_OnCompletedEvent;

			Loaded += BackgroundSyncMonitor_Loaded;
		}

		public SyncHelper SyncHelper
		{
			get { return syncHelper; }
		}

		private SyncHelper syncHelper;

		private bool _isFirstTime = true;

		private void BackgroundSyncMonitor_Loaded(object sender, RoutedEventArgs e)
		{
			if (!_isFirstTime)
				return;

			_isFirstTime = false;

			Dispatcher.BeginInvoke(() =>
			{
				syncHelper.Start();
			});
		}

		private void syncHelper_OnStatusChangedEvent(object sender, EventArgs e)
		{
			status.Text = syncHelper.Status.ToUpper();
		}

		private void syncHelper_OnProgressChangedEvent(object sender, EventArgs e)
		{
			progress.IsIndeterminate = false;
			progress.Value = syncHelper.Progress;
		}

		private void syncHelper_OnCompletedEvent(object sender, EventArgs e)
		{
			if (syncHelper.Error != null)
			{
				status.Text = "SEND/RECEIVE ERROR";
				exclamation.Visibility = Visibility.Visible;
			}
			else
				status.Text = "SEND/RECEIVE COMPLETED";

			progress.Visibility = Visibility.Collapsed;
			progress.IsIndeterminate = false;

			RaiseSyncCompletedEvent();
		}

		protected override void OnClick()
		{
			base.OnClick();

			if (BackgroundSyncDialog.LastUsedSyncDialog != null)
			{
				//BackgroundSyncDialog.LastUsedSyncDialog.WindowState = WindowState.Normal;
				BackgroundSyncDialog.LastUsedSyncDialog.Activate();
				return;
			}

			BackgroundSyncDialog.LastUsedSyncDialog = new BackgroundSyncDialog(this, SyncHelper.LastUsedSyncHelper);
			BackgroundSyncDialog.LastUsedSyncDialog.Closed += LastUsedSyncDialog_Closed;
			BackgroundSyncDialog.LastUsedSyncDialog.Owner = Window.GetWindow(this);
			BackgroundSyncDialog.LastUsedSyncDialog.FastShow();

			//if (syncHelper.Error != null || syncHelper.Done)
			//	(Parent as ItemsControl).Items.Remove(this);
		}

		private void LastUsedSyncDialog_Closed(object sender, EventArgs e)
		{
			BackgroundSyncDialog.LastUsedSyncDialog.Closed -= LastUsedSyncDialog_Closed;
			BackgroundSyncDialog.LastUsedSyncDialog = null;
		}

		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);

			if (BackgroundSyncDialog.LastUsedSyncDialog != null)
				BackgroundSyncDialog.LastUsedSyncDialog.Close();
		}

		#region Routed Events

		public static readonly RoutedEvent SyncCompletedEvent = EventManager.RegisterRoutedEvent(
			"SyncCompleted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BackgroundSyncMonitor));

		public event RoutedEventHandler SyncCompleted
		{
			add { AddHandler(SyncCompletedEvent, value); }
			remove { RemoveHandler(SyncCompletedEvent, value); }
		}

		private void RaiseSyncCompletedEvent()
		{
			RaiseEvent(new RoutedEventArgs(SyncCompletedEvent));
		}

		#endregion
	}
}
