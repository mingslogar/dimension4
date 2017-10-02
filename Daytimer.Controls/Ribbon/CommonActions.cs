using Daytimer.Functions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Daytimer.Controls.Ribbon
{
	class CommonActions
	{
		public static void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count == 0 || e.RemovedItems.Count == 0)
				return;

			TabControl _control = sender as TabControl;

			if (_control.IsLoaded && Settings.AnimationsEnabled && e.AddedItems[0] is TabItem)
			{
				TranslateTransform tabControlTransform = (TranslateTransform)_control.Template.FindName("ContentPanelTransform", _control);
				UIElement tabControlContent = (UIElement)_control.Template.FindName("ContentPanel", _control);

				tabControlContent.Opacity = 0;
				tabControlContent.IsHitTestVisible = false;
				tabControlContent.UpdateLayout();

				_control.Dispatcher.BeginInvoke(() =>
				{
					DoubleAnimation translateAnimation = new DoubleAnimation(50, 0, AnimationHelpers.AnimationDuration);
					translateAnimation.EasingFunction = new QuadraticEase();
					translateAnimation.Completed += (anim, args) =>
					{
						tabControlContent.IsHitTestVisible = true;
						tabControlContent.ApplyAnimationClock(TranslateTransform.XProperty, null);
					};

					DoubleAnimation opacityAnimation = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(200)));
					opacityAnimation.Completed += (anim, args) =>
					{
						tabControlContent.Opacity = 1;
						tabControlContent.ApplyAnimationClock(FrameworkElement.OpacityProperty, null);
					};

					tabControlTransform.BeginAnimation(TranslateTransform.XProperty, translateAnimation);
					tabControlContent.BeginAnimation(FrameworkElement.OpacityProperty, opacityAnimation);

				}, DispatcherPriority.Loaded);
			}
		}
	}
}
