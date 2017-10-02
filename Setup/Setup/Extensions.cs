using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Setup
{
	public static class Extensions
	{
		#region Fade Out

		public static void FadeOut(this FrameworkElement element, FrameworkElement crossFade = null)
		{
			if (element.Opacity != 0)
			{
				DoubleAnimation fadeOutAnim = new DoubleAnimation(0, AnimationDuration);

				fadeOutAnim.Completed += (sender, e) =>
				{
					if (element != null)
					{
						element.Visibility = Visibility.Hidden;
						element.Opacity = 0;
						element.ApplyAnimationClock(FrameworkElement.OpacityProperty, null);
					}

					if (crossFade != null)
						crossFade.FadeIn();
				};

				element.BeginAnimation(FrameworkElement.OpacityProperty, fadeOutAnim);
			}
			else if (crossFade != null)
			{
				crossFade.FadeIn();
			}
		}

		#endregion

		#region Fade In

		public static void FadeIn(this FrameworkElement element)
		{
			if (element.Opacity != 1)
			{
				DoubleAnimation fadeInAnim = new DoubleAnimation(1, AnimationDuration);

				fadeInAnim.Completed += (sender, e) =>
				{
					if (element != null)
					{
						element.Opacity = 1;
						element.ApplyAnimationClock(FrameworkElement.OpacityProperty, null);
					}
				};

				element.Visibility = Visibility.Visible;
				element.BeginAnimation(FrameworkElement.OpacityProperty, fadeInAnim);
			}
		}

		#endregion

		#region Slide Display

		public static void SlideDisplay(this FrameworkElement element, Image image, SlideDirection direction)
		{
			SwitchViews(element, image, direction);
		}

		public enum SlideDirection { Left, Right };

		private static void SwitchViews(FrameworkElement slideElement, Image image, SlideDirection direction)
		{
			image.Visibility = Visibility.Visible;
			slideElement.Visibility = Visibility.Hidden;

			if (direction == SlideDirection.Right)
				SlideRight(slideElement, image);
			else
				SlideLeft(slideElement, image);
		}

		private static void SlideLeft(FrameworkElement slideElement, Image image)
		{
			QuarticEase Ease = new QuarticEase();
			Ease.EasingMode = EasingMode.EaseIn;

			ThicknessAnimation leftAnim1 = new ThicknessAnimation(new Thickness(0), new Thickness(60, 0, -60, 0), AnimationDuration, FillBehavior.Stop);
			leftAnim1.EasingFunction = Ease;

			leftAnim1.Completed += (sender, e) =>
			{
				image.Visibility = Visibility.Hidden;
				image.ApplyAnimationClock(FrameworkElement.OpacityProperty, null);
				image.ApplyAnimationClock(FrameworkElement.MarginProperty, null);

				QuarticEase Ease2 = new QuarticEase();
				Ease2.EasingMode = EasingMode.EaseOut;

				slideElement.Visibility = Visibility.Visible;

				ThicknessAnimation leftAnim2 = new ThicknessAnimation(new Thickness(-60, 0, 60, 0), new Thickness(0), AnimationDuration, FillBehavior.Stop);
				leftAnim2.EasingFunction = Ease2;
				leftAnim2.Completed += (_sender, _e) =>
				{
					slideElement.ApplyAnimationClock(FrameworkElement.OpacityProperty, null);
					slideElement.ApplyAnimationClock(FrameworkElement.MarginProperty, null);
				};

				DoubleAnimation opac2 = new DoubleAnimation(0, 1, AnimationDuration, FillBehavior.Stop);
				opac2.EasingFunction = Ease2;

				slideElement.BeginAnimation(FrameworkElement.OpacityProperty, opac2);
				slideElement.BeginAnimation(FrameworkElement.MarginProperty, leftAnim2);
			};

			DoubleAnimation opac = new DoubleAnimation(1, 0, AnimationDuration, FillBehavior.Stop);
			opac.EasingFunction = Ease;

			image.BeginAnimation(FrameworkElement.OpacityProperty, opac);
			image.BeginAnimation(FrameworkElement.MarginProperty, leftAnim1);
		}

		private static void SlideRight(FrameworkElement slideElement, Image image)
		{
			QuarticEase Ease = new QuarticEase();
			Ease.EasingMode = EasingMode.EaseIn;

			ThicknessAnimation rightAnim1 = new ThicknessAnimation(new Thickness(0), new Thickness(-60, 0, 60, 0), AnimationDuration, FillBehavior.Stop);
			rightAnim1.EasingFunction = Ease;

			rightAnim1.Completed += (sender, e) =>
			{
				image.Visibility = Visibility.Hidden;
				image.ApplyAnimationClock(FrameworkElement.OpacityProperty, null);
				image.ApplyAnimationClock(FrameworkElement.MarginProperty, null);

				QuarticEase Ease2 = new QuarticEase();
				Ease2.EasingMode = EasingMode.EaseOut;

				slideElement.Visibility = Visibility.Visible;

				ThicknessAnimation leftAnim2 = new ThicknessAnimation(new Thickness(60, 0, -60, 0), new Thickness(0), AnimationDuration, FillBehavior.Stop);
				leftAnim2.EasingFunction = Ease2;
				leftAnim2.Completed += (_sender, _e) =>
				{
					slideElement.ApplyAnimationClock(FrameworkElement.OpacityProperty, null);
					slideElement.ApplyAnimationClock(FrameworkElement.MarginProperty, null);
				};

				DoubleAnimation opac2 = new DoubleAnimation(0, 1, AnimationDuration, FillBehavior.Stop);
				opac2.EasingFunction = Ease2;

				slideElement.BeginAnimation(FrameworkElement.OpacityProperty, opac2);
				slideElement.BeginAnimation(FrameworkElement.MarginProperty, leftAnim2);
			};

			DoubleAnimation opac = new DoubleAnimation(1, 0, AnimationDuration, FillBehavior.Stop);
			opac.EasingFunction = Ease;

			image.BeginAnimation(FrameworkElement.OpacityProperty, opac);
			image.BeginAnimation(FrameworkElement.MarginProperty, rightAnim1);
		}

		#endregion

		public static Duration AnimationDuration = new Duration(TimeSpan.FromMilliseconds(300));
	}
}
