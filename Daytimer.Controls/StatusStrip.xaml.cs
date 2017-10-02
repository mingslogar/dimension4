using Daytimer.Functions;
using Daytimer.GoogleCalendarHelpers;
using Daytimer.PrintHelpers;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows.Xps;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for StatusStrip.xaml
	/// </summary>
	public partial class StatusStrip : CustomStatusStrip
	{
		public StatusStrip()
		{
			InitializeComponent();
		}

		public void UpdateMainStatus(string text)
		{
			if (mainStatus.Text != text)
			{
				UpdateText.Enqueue(text);

				if (UpdateText.Count >= 3)
				{
					List<string> trimmed = new List<string>(UpdateText);
					trimmed.RemoveAt(0);
					UpdateText = new Queue<string>(trimmed);
				}

				if (!IsStatusAnimRunning)
					updateStatusWorker();
			}
		}

		private Queue<string> UpdateText = new Queue<string>(3);
		private bool IsStatusAnimRunning = false;
		private Duration AnimationDuration = new Duration(TimeSpan.FromMilliseconds(300));

		private void updateStatusWorker()
		{
			if (UpdateText.Count > 0)
			{
				IsStatusAnimRunning = true;

				if (UpdateText.Peek() != mainStatus.Text)
				{
					copyMainStatus.Text = mainStatus.Text;
					mainStatus.Text = UpdateText.Dequeue();

					if (Settings.AnimationsEnabled)
					{
						//ThicknessAnimation anim1 = new ThicknessAnimation(new Thickness(0, 10, 0, -10), new Thickness(0, 0, 0, 0), AnimationDuration);
						//DoubleAnimation opacityAnim1 = new DoubleAnimation(0, 1, AnimationDuration);
						//mainStatus.BeginAnimation(MarginProperty, anim1);
						//mainStatus.BeginAnimation(OpacityProperty, opacityAnim1);

						//ThicknessAnimation anim2 = new ThicknessAnimation(new Thickness(0, 0, 0, 0), new Thickness(0, -10, 0, 10), AnimationDuration);
						//DoubleAnimation opacityAnim2 = new DoubleAnimation(1, 0, AnimationDuration);
						//opacityAnim2.Completed += opacityAnim2_Completed;
						//copyMainStatus.BeginAnimation(MarginProperty, anim2);
						//copyMainStatus.BeginAnimation(OpacityProperty, opacityAnim2);

						DoubleAnimation translateAnim = new DoubleAnimation(10, 0, AnimationDuration);
						DoubleAnimation opacityAnim1 = new DoubleAnimation(0, 1, AnimationDuration);
						DoubleAnimation opacityAnim2 = new DoubleAnimation(1, 0, AnimationDuration);
						opacityAnim2.Completed += opacityAnim2_Completed;

						statusTranslate.BeginAnimation(TranslateTransform.YProperty, translateAnim);
						mainStatus.BeginAnimation(OpacityProperty, opacityAnim1);
						copyMainStatus.BeginAnimation(OpacityProperty, opacityAnim2);
					}
					else
					{
						copyMainStatus.Text = null;
						IsStatusAnimRunning = false;
						updateStatusWorker();
					}
				}
				else
				{
					UpdateText.Dequeue();
					IsStatusAnimRunning = false;
					updateStatusWorker();
				}
			}
		}

		private void opacityAnim2_Completed(object sender, EventArgs e)
		{
			IsStatusAnimRunning = false;
			updateStatusWorker();
		}

		public void UpdateTheme()
		{
			slider.UpdateTheme();
		}

		public void EnableZoom(bool value)
		{
			slider.IsEnabled = value;
		}

		public double Zoom
		{
			get { return slider.slider.Value; }
			set { slider.slider.Value = value; }
		}

		public void IncreaseZoom()
		{
			slider.IncreaseValue();
		}

		public void DecreaseZoom()
		{
			slider.DecreaseValue();
		}

		public ViewMode ViewMode
		{
			get { return viewMode; }
		}

		#region Check For Updates

		public void CheckForUpdates()
		{
			DispatcherTimer updateTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
			updateTimer.Interval = TimeSpan.FromMilliseconds(5000);
			updateTimer.Tick += updateTimer_Tick;
			updateTimer.Start();
		}

		private void updateTimer_Tick(object sender, EventArgs e)
		{
			(sender as DispatcherTimer).Stop();

			BackgroundUpdater updater = new BackgroundUpdater();
			Items.Insert(1, updater);
		}

		#endregion

		public BackgroundSyncMonitor SyncMonitor
		{
			get { return syncMonitor; }
		}

		private BackgroundSyncMonitor syncMonitor = null;

		public void SyncWithServer()
		{
			foreach (UIElement each in Items)
				if (each is BackgroundSyncMonitor)
				{
					Items.Remove(each);
					break;
				}

			syncMonitor = new BackgroundSyncMonitor();
			syncMonitor.SyncCompleted += syncMonitor_SyncCompleted;
			Items.Insert(1, syncMonitor);
		}

		private void syncMonitor_SyncCompleted(object sender, RoutedEventArgs e)
		{
			RaiseSyncCompletedEvent(sender);
		}

		private ServerConnectivityMonitor connMonitor = null;

		public void ShowConnectivity()
		{
			if (connMonitor == null)
			{
				connMonitor = new ServerConnectivityMonitor();
				Items.Insert(Items.Count - 2, connMonitor);
			}
			else
				connMonitor.IsConnected = !Settings.WorkOffline;
		}

		private void slider_OnValueChangedEvent(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			SliderValueChangedEvent(e);
		}

		public void AddPrintMonitor(XpsDocumentWriter documentWriter)
		{
			Items.Insert(1, new BackgroundPrintMonitor(documentWriter));
		}

		#region Events

		public delegate void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e);

		public event OnSliderValueChanged OnSliderValueChangedEvent;

		protected void SliderValueChangedEvent(RoutedPropertyChangedEventArgs<double> e)
		{
			if (OnSliderValueChangedEvent != null)
				OnSliderValueChangedEvent(this, e);
		}

		#endregion

		#region Routed Events

		public static readonly RoutedEvent SyncCompletedEvent = EventManager.RegisterRoutedEvent(
			"SyncCompleted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(StatusStrip));

		public event RoutedEventHandler SyncCompleted
		{
			add { AddHandler(SyncCompletedEvent, value); }
			remove { RemoveHandler(SyncCompletedEvent, value); }
		}

		private void RaiseSyncCompletedEvent(object sender)
		{
			RaiseEvent(new RoutedEventArgs(SyncCompletedEvent, sender));
		}

		#endregion
	}
}
