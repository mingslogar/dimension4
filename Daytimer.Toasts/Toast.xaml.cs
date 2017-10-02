using CoreAudioApi;
using Daytimer.Functions;
using Gma.UserActivityMonitor;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Daytimer.Toasts
{
	/// <summary>
	/// Interaction logic for Toast.xaml
	/// </summary>
	public partial class Toast : Window
	{
		public Toast(string header, string line1, string line2, ImageSource icon,
			Uri audio, ToastDuration duration, bool unmuteSpeakers)
		{
			InitializeComponent();

			Title = header;
			line1Text.Text = line1;
			line2Text.Text = line2;

			//
			// BUG FIX:
			//
			// I'm not sure exactly what the deal is here, but if we
			// don't access a property of the icon, the toast will
			// throw an IOException on load.
			//
			if (icon != null)
			{ double x = icon.Width; }

			Icon = icon;
			mediaElement.Source = audio;

			_toastDuration = duration;
			_unmuteSpeakers = unmuteSpeakers;
		}

		/// <summary>
		/// Attempts to open toast. If unsuccessful, toast is placed in
		/// a queue to wait for an opening.
		/// </summary>
		public void Open()
		{
			ToastManager.AddToOpen(this);
		}

		private bool _startTimerEnabled = false;

		protected override void OnContentRendered(EventArgs e)
		{
			base.OnContentRendered(e);

			if (timeOutTimer != null)
				return;

			try
			{
				HookManager.MouseMove += HookManager_MouseMove;
				_startTimerEnabled = true;
			}
			catch
			{
				timeOutTimer = new DispatcherTimer();
				timeOutTimer.Interval = TimeSpan.FromSeconds((int)_toastDuration);
				timeOutTimer.Tick += timeOutTimer_Tick;
				timeOutTimer.Start();
			}
		}

		private void HookManager_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (!_startTimerEnabled)
				return;

			HookManager.MouseMove -= HookManager_MouseMove;

			_startTimerEnabled = false;

			timeOutTimer = new DispatcherTimer();
			timeOutTimer.Interval = TimeSpan.FromSeconds((int)_toastDuration);
			timeOutTimer.Tick += timeOutTimer_Tick;
			timeOutTimer.Start();
		}

		private void timeOutTimer_Tick(object sender, EventArgs e)
		{
			timeOutTimer.Stop();
			timeOutTimer.Tick -= timeOutTimer_Tick;
			timeOutTimer = null;

			_toastResult = ToastResult.TimedOut;
			Close();
		}

		private ToastDuration _toastDuration = ToastDuration.Standard;
		private ToastResult _toastResult = ToastResult.Null;
		private DispatcherTimer timeOutTimer = null;
		private bool _unmuteSpeakers = false;

		public ToastResult ToastResult
		{
			get { return _toastResult; }
		}

		private void dismissButton_Click(object sender, RoutedEventArgs e)
		{
			_toastResult = ToastResult.Dismissed;
			Close();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			// Handle Alt+F4 closing of toast.
			if (_toastResult == ToastResult.Null)
			{
				e.Cancel = true;
				_toastResult = ToastResult.Dismissed;
				Close();
			}
			else if (_isMediaPlaying)
				ToastManager.IsAudioRunning = false;
		}

		new public void Close()
		{
			if (_toastResult == ToastResult.TimedOut)
			{
				Duration duration = new Duration(TimeSpan.FromMilliseconds(1500));

				DoubleAnimation closeAnim = new DoubleAnimation(0, duration);
				closeAnim.Completed += closeAnim_Completed;
				outerBorder.BeginAnimation(OpacityProperty, closeAnim);

				FadeOutAudio(duration);
			}
			else
			{
				IsHitTestVisible = false;

				Duration duration = new Duration(TimeSpan.FromMilliseconds(300));

				DoubleAnimation closeAnim = new DoubleAnimation(Width, duration);
				closeAnim.EasingFunction = new PowerEase() { Power = 4, EasingMode = EasingMode.EaseOut };
				closeAnim.Completed += closeAnim_Completed;

				translateTransform.BeginAnimation(TranslateTransform.XProperty, closeAnim);

				FadeOutAudio(duration);
			}
		}

		private void closeAnim_Completed(object sender, EventArgs e)
		{
			if (_toastResult != ToastResult.Null)
				base.Close();
		}

		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);

			if (timeOutTimer != null)
				timeOutTimer.Stop();

			if (_toastResult != ToastResult.Null)
			{
				_toastResult = ToastResult.Null;

				Duration duration = new Duration(TimeSpan.FromMilliseconds(100));

				outerBorder.BeginAnimation(OpacityProperty, new DoubleAnimation(1, duration));
				FadeInAudio(duration);
			}
		}

		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);

			if (timeOutTimer != null)
				timeOutTimer.Start();
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);
			Mouse.Capture(this);
		}

		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonUp(e);

			bool captured = IsMouseCaptured;
			ReleaseMouseCapture();

			if (IsMouseOver && captured)
			{
				_toastResult = ToastResult.Activated;
				Close();
			}
		}

		private bool _isMediaPlaying = false;

		private void mediaElement_Loaded(object sender, RoutedEventArgs e)
		{
			if (!ToastManager.IsAudioRunning)
			{
				ToastManager.IsAudioRunning = true;
				_isMediaPlaying = true;

				if (_unmuteSpeakers)
					Unmute();

				mediaElement.Play();
			}
		}

		private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
		{
			_isMediaPlaying = false;
			ToastManager.IsAudioRunning = false;
		}

		private void FadeInAudio(Duration duration)
		{
			DoubleAnimation fade = new DoubleAnimation(100, duration);
			mediaElement.BeginAnimation(MediaElement.VolumeProperty, fade);
		}

		private void FadeOutAudio(Duration duration)
		{
			DoubleAnimation fade = new DoubleAnimation(0, duration);
			mediaElement.BeginAnimation(MediaElement.VolumeProperty, fade);
		}

		private void Unmute()
		{
			// The unmute functionality is only supported on Vista and newer.
			if (Environment.OSVersion.Version >= OSVersions.Win_Vista)
			{
				MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator();
				MMDevice device = deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
				device.AudioEndpointVolume.Mute = false;
				device.AudioEndpointVolume.MasterVolumeLevelScalar = 1F;
			}
		}
	}

	public enum ToastDuration
	{
		/// <summary>
		/// 7 seconds.
		/// </summary>
		Standard = 7,

		/// <summary>
		/// 25 seconds.
		/// </summary>
		Long = 25
	};

	public enum ToastResult
	{
		/// <summary>
		/// Toast has not yet been closed.
		/// </summary>
		Null,

		/// <summary>
		/// User clicked the toast.
		/// </summary>
		Activated,

		/// <summary>
		/// User closed the toast.
		/// </summary>
		Dismissed,

		/// <summary>
		/// Toast closed by timer after the ToastDuration.
		/// </summary>
		TimedOut
	};
}
