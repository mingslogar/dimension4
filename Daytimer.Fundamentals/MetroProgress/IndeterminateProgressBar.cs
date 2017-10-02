using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Daytimer.Fundamentals.MetroProgress
{
	[ComVisible(false)]
	[TemplatePart(Name = IndeterminateProgressBar.GridName, Type = typeof(Grid))]
	public class IndeterminateProgressBar : ProgressBase
	{
		#region Constructors

		static IndeterminateProgressBar()
		{
			Type ownerType = typeof(IndeterminateProgressBar);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
		}

		public IndeterminateProgressBar()
		{
			Loaded += IndeterminateProgressControl_Loaded;
			Unloaded += IndeterminateProgressControl_Unloaded;
		}

		#endregion

		#region Fields

		private const string GridName = "PART_Grid";

		#endregion

		#region Private Fields

		private Grid PART_Grid;

		#endregion

		#region Dependency Properties

		public static readonly DependencyProperty StartOnLoadProperty = DependencyProperty.Register(
			"StartOnLoad", typeof(bool), typeof(IndeterminateProgressBar), new PropertyMetadata(true));

		public bool StartOnLoad
		{
			get { return (bool)GetValue(StartOnLoadProperty); }
			set { SetValue(StartOnLoadProperty, value); }
		}

		#endregion

		#region Protected Methods

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);

			if (!IsLoaded)
				return;

			if (sizeInfo.WidthChanged)
			{
				// We need to restart animations.
				if (IsAnimationRunning)
				{
					Stop();
					Start();
				}
			}
		}

		#endregion

		#region Public Override Methods

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			PART_Grid = GetTemplateChild(GridName) as Grid;
		}

		public override void Reset()
		{
			Stop();
			Start();
		}

		public override void Start()
		{
			if (PART_Grid == null)
				return;

			IsAnimationRunning = true;

			AnimationTimeline anim = CreateAnimation();
			anim.BeginTime = TimeSpan.FromMilliseconds(500);

			foreach (UIElement each in PART_Grid.Children)
			{
				each.Visibility = Visibility.Hidden;

				TranslateTransform transform = new TranslateTransform();
				each.RenderTransform = transform;

				transform.BeginAnimation(TranslateTransform.XProperty, anim);

				DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Input);

				// Delay the timer by one 60FPS frame.
				timer.Interval = anim.BeginTime.Value.Add(TimeSpan.FromMilliseconds(16.667));

				timer.Tick += (sender, e) =>
				{
					each.Visibility = Visibility.Visible;
					timer.Stop();
					timer = null;
				};

				timer.Start();

				anim.BeginTime = anim.BeginTime.Value.Add(TimeSpan.FromMilliseconds(150));
			}

			Visibility = Visibility.Visible;
		}

		public override void Stop()
		{
			try
			{
				if (PART_Grid == null)
					return;

				foreach (UIElement each in PART_Grid.Children)
				{
					if (!each.RenderTransform.IsFrozen)
						each.RenderTransform.ApplyAnimationClock(TranslateTransform.XProperty, null);

					each.Visibility = Visibility.Hidden;
				}

				Visibility = Visibility.Hidden;
				IsAnimationRunning = false;
			}
			catch { }
		}

		#endregion

		#region Private Methods

		private void IndeterminateProgressControl_Loaded(object sender, RoutedEventArgs e)
		{
			if (StartOnLoad)
				Dispatcher.BeginInvoke(() =>
				{
					Start();
				});
		}

		private void IndeterminateProgressControl_Unloaded(object sender, RoutedEventArgs e)
		{
			Stop();
		}

		private AnimationTimeline CreateAnimation()
		{
			double edge = ActualWidth / 2 + 50;

			DoubleAnimationUsingKeyFrames anim = new DoubleAnimationUsingKeyFrames();

			if (ActualWidth > 300)
			{
				anim.KeyFrames.Add(new DiscreteDoubleKeyFrame(-edge,
					KeyTime.FromTimeSpan(TimeSpan.Zero)));

				anim.KeyFrames.Add(new EasingDoubleKeyFrame(-85,
					KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200)),
					new PowerEase() { Power = 0.4, EasingMode = EasingMode.EaseIn }));

				anim.KeyFrames.Add(new LinearDoubleKeyFrame(85,
					KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1400))));

				anim.KeyFrames.Add(new EasingDoubleKeyFrame(edge,
					KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1600)),
					new PowerEase() { Power = 0.4, EasingMode = EasingMode.EaseOut }));

				anim.KeyFrames.Add(new DiscreteDoubleKeyFrame(edge,
					KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(2400))));
			}
			else
			{
				anim.KeyFrames.Add(new DiscreteDoubleKeyFrame(-edge,
					KeyTime.FromTimeSpan(TimeSpan.Zero)));

				anim.KeyFrames.Add(new EasingDoubleKeyFrame(-50,
					KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200)),
					new PowerEase() { Power = 0.4, EasingMode = EasingMode.EaseIn }));

				anim.KeyFrames.Add(new LinearDoubleKeyFrame(50,
					KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1400))));

				anim.KeyFrames.Add(new EasingDoubleKeyFrame(edge,
					KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1600)),
					new PowerEase() { Power = 0.4, EasingMode = EasingMode.EaseOut }));

				anim.KeyFrames.Add(new DiscreteDoubleKeyFrame(edge,
					KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(2400))));
			}

			anim.RepeatBehavior = RepeatBehavior.Forever;

			return anim;
		}

		#endregion
	}
}
