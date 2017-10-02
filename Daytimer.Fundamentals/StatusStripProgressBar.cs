using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Daytimer.Fundamentals
{
	[ComVisible(false)]
	public class StatusStripProgressBar : ProgressBar
	{
		#region Constructors

		static StatusStripProgressBar()
		{
			Type ownerType = typeof(StatusStripProgressBar);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
		}

		public StatusStripProgressBar()
		{
			Loaded += StatusStripProgressBar_Loaded;
		}

		#endregion

		#region Dependency Properties

		/// <summary>
		///     The DependencyProperty for the IsIndeterminate property.
		///     Flags:          none
		///     DefaultValue:   false
		/// </summary>
		new public static readonly DependencyProperty IsIndeterminateProperty =
				DependencyProperty.Register(
						"IsIndeterminate",
						typeof(bool),
						typeof(StatusStripProgressBar),
						new FrameworkPropertyMetadata(
								false,
								new PropertyChangedCallback(OnIsIndeterminateChanged)));

		/// <summary>
		///     Determines if StatusStripProgressBar shows actual values (false)
		///     or generic, continuous progress feedback (true).
		/// </summary>
		/// <value></value>
		new public bool IsIndeterminate
		{
			get { return (bool)GetValue(IsIndeterminateProperty); }
			set { SetValue(IsIndeterminateProperty, value); }
		}

		/// <summary>
		///     Called when IsIndeterminateProperty is changed on "d".
		/// </summary>
		private static void OnIsIndeterminateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			bool value = (bool)e.NewValue;
			((ProgressBar)d).IsIndeterminate = value;

			if (value)
				((StatusStripProgressBar)d).StartAnimation();
			else
				((StatusStripProgressBar)d).StopAnimation();
		}

		#endregion

		#region Private Methods

		private void StatusStripProgressBar_Loaded(object sender, RoutedEventArgs e)
		{
			if (IsIndeterminate)
				StartAnimation();
		}

		private void StartAnimation()
		{
			if (!IsLoaded)
				return;

			try
			{
				ThicknessAnimation anim = new ThicknessAnimation(new Thickness(-13, 0, 0, 0),
					new Thickness(0, 0, -13, 0), new Duration(TimeSpan.FromMilliseconds(300)));
				anim.RepeatBehavior = RepeatBehavior.Forever;
				(GetTemplateChild("IndeterminateFill") as UIElement).BeginAnimation(MarginProperty, anim);

				//DoubleAnimation anim = new DoubleAnimation(-13, 0, new Duration(TimeSpan.FromMilliseconds(300)));
				//anim.RepeatBehavior = RepeatBehavior.Forever;
				//(progress.Template.FindName("IndeterminateFill", progress) as UIElement).RenderTransform.BeginAnimation(TranslateTransform.XProperty, anim);
			}
			catch { }
		}

		private void StopAnimation()
		{
			if (!IsLoaded)
				return;

			try
			{
				(GetTemplateChild("IndeterminateFill") as UIElement).ApplyAnimationClock(MarginProperty, null);
				//(progress.Template.FindName("IndeterminateFill", progress) as UIElement).RenderTransform.ApplyAnimationClock(TranslateTransform.XProperty, null);
			}
			catch { }
		}

		#endregion
	}
}
