using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace Daytimer.Functions
{
	class AnimationHelpers
	{
		public static bool AnimationsEnabled = true;

		public static IEasingFunction EasingFunction
		{
			get
			{
				PowerEase ease = new PowerEase();
				ease.Power = 4;
				return ease;
			}
		}

		public static Duration AnimationDuration
		{
			get { return new Duration(TimeSpan.FromMilliseconds(400)); }
		}

		/// <summary>
		/// Time to wait before accepting another zoom event
		/// </summary>
		public static TimeSpan ZoomDelay
		{
			get
			{
				if (Settings.AnimationsEnabled)
					return TimeSpan.FromMilliseconds(500);
				else
					return TimeSpan.Zero;
			}
		}
	}
}
