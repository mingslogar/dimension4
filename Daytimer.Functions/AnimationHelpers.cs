using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Daytimer.Functions
{
	public class AnimationHelpers
	{
		#region General

		private static IEasingFunction _easingFunction = new PowerEase() { Power = 4 };

		public static IEasingFunction EasingFunction
		{
			get { return _easingFunction; }
		}

		private static Duration _animationDuration = new Duration(TimeSpan.FromMilliseconds(400));

		public static Duration AnimationDuration
		{
			get { return _animationDuration; }
		}

		public enum SlideDirection { Left, Right };
		public enum ZoomDirection { In, Out };
		public enum FadeDirection { In, Out };

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

		private static Transform GetRenderTransform(UIElement element, Type type)
		{
			TransformGroup group = element.RenderTransform as TransformGroup;

			if (group == null)
			{
				group = new TransformGroup();
				element.RenderTransform = group;
			}

			foreach (Transform each in group.Children)
				if (each.GetType() == type)
					return each;

			Transform transform = (Transform)type.GetConstructor(new Type[0]).Invoke(null);
			group.Children.Add(transform);

			return transform;
		}

		#endregion

		#region Zoom Display

		public class ZoomDisplay
		{
			public ZoomDisplay(FrameworkElement switchOut, FrameworkElement switchIn)
			{
				_switchOut = switchOut;
				_switchIn = switchIn;
			}

			private FrameworkElement _switchOut;
			private FrameworkElement _switchIn;

			private double centerX;
			private double centerY;

			//private Duration AnimationDuration
			//{
			//	get { return new Duration(TimeSpan.FromMilliseconds(300)); }
			//}

			public void SwitchViews(ZoomDirection direction)
			{
				_switchOut.IsHitTestVisible = false;
				_switchIn.IsHitTestVisible = false;

				if (direction == ZoomDirection.In)
					ZoomIn(_switchOut, _switchIn);
				else
					ZoomOut(_switchOut, _switchIn);
			}

			private void ZoomIn(FrameworkElement switchOut, FrameworkElement switchIn)
			{
				QuarticEase Ease = new QuarticEase();
				Ease.EasingMode = EasingMode.EaseIn;

				centerX = switchOut.ActualWidth / 2;
				centerY = switchOut.ActualHeight / 2;

				ScaleTransform scaleOut = (ScaleTransform)GetRenderTransform(switchOut, typeof(ScaleTransform));

				scaleOut.CenterX = centerX;
				scaleOut.CenterY = centerY;
				scaleOut.ScaleX = scaleOut.ScaleY = 1;

				DoubleAnimation scaleOutAnim = new DoubleAnimation(1.1, AnimationDuration, FillBehavior.Stop);
				scaleOutAnim.Completed += scaleOutAnim_Completed;
				scaleOutAnim.EasingFunction = Ease;

				DoubleAnimation opacityOutAnim = new DoubleAnimation(1, 0, AnimationDuration, FillBehavior.Stop);
				opacityOutAnim.EasingFunction = Ease;
				_switchOut.BeginAnimation(FrameworkElement.OpacityProperty, opacityOutAnim);

				scaleOut.BeginAnimation(ScaleTransform.ScaleXProperty, scaleOutAnim);
				scaleOut.BeginAnimation(ScaleTransform.ScaleYProperty, scaleOutAnim);
			}

			private void scaleOutAnim_Completed(object sender, EventArgs e)
			{
				QuarticEase Ease = new QuarticEase();
				Ease.EasingMode = EasingMode.EaseOut;

				_switchOut.Visibility = Visibility.Collapsed;
				_switchOut.ApplyAnimationClock(FrameworkElement.OpacityProperty, null);

				ScaleTransform scaleOut = (ScaleTransform)GetRenderTransform(_switchOut, typeof(ScaleTransform));

				scaleOut.ApplyAnimationClock(ScaleTransform.ScaleXProperty, null);
				scaleOut.ApplyAnimationClock(ScaleTransform.ScaleYProperty, null);

				ScaleTransform scaleIn = (ScaleTransform)GetRenderTransform(_switchIn, typeof(ScaleTransform));

				scaleIn.CenterX = centerX;
				scaleIn.CenterY = centerY;
				scaleIn.ScaleX = scaleIn.ScaleY = 0.9;

				DoubleAnimation scaleInAnim = new DoubleAnimation(1, AnimationDuration, FillBehavior.Stop);
				scaleInAnim.Completed += scaleInAnim_Completed;
				scaleInAnim.EasingFunction = Ease;

				_switchIn.Visibility = Visibility.Visible;

				DoubleAnimation opacityInAnim = new DoubleAnimation(0, 1, AnimationDuration, FillBehavior.Stop);

				// To do, or not to do
				opacityInAnim.EasingFunction = Ease;

				scaleIn.BeginAnimation(ScaleTransform.ScaleXProperty, scaleInAnim);
				scaleIn.BeginAnimation(ScaleTransform.ScaleYProperty, scaleInAnim);

				_switchIn.BeginAnimation(FrameworkElement.OpacityProperty, opacityInAnim);
			}

			private void ZoomOut(FrameworkElement switchOut, FrameworkElement switchIn)
			{
				QuarticEase Ease = new QuarticEase();
				Ease.EasingMode = EasingMode.EaseIn;

				centerX = switchOut.ActualWidth / 2;
				centerY = switchOut.ActualHeight / 2;

				ScaleTransform scaleOut = (ScaleTransform)GetRenderTransform(switchOut, typeof(ScaleTransform));

				scaleOut.CenterX = centerX;
				scaleOut.CenterY = centerY;
				scaleOut.ScaleX = scaleOut.ScaleY = 1;

				DoubleAnimation scaleOutAnim2 = new DoubleAnimation(0.9, AnimationDuration, FillBehavior.Stop);
				scaleOutAnim2.Completed += scaleOut2Anim_Completed;
				scaleOutAnim2.EasingFunction = Ease;

				DoubleAnimation opacityOutAnim = new DoubleAnimation(1, 0, AnimationDuration, FillBehavior.Stop);
				opacityOutAnim.EasingFunction = Ease;
				_switchOut.BeginAnimation(FrameworkElement.OpacityProperty, opacityOutAnim);

				scaleOut.BeginAnimation(ScaleTransform.ScaleXProperty, scaleOutAnim2);
				scaleOut.BeginAnimation(ScaleTransform.ScaleYProperty, scaleOutAnim2);
			}

			private void scaleOut2Anim_Completed(object sender, EventArgs e)
			{
				QuarticEase Ease = new QuarticEase();
				Ease.EasingMode = EasingMode.EaseOut;

				_switchOut.Visibility = Visibility.Collapsed;
				_switchOut.ApplyAnimationClock(FrameworkElement.OpacityProperty, null);

				ScaleTransform scaleOut = (ScaleTransform)GetRenderTransform(_switchOut, typeof(ScaleTransform));

				scaleOut.ApplyAnimationClock(ScaleTransform.ScaleXProperty, null);
				scaleOut.ApplyAnimationClock(ScaleTransform.ScaleYProperty, null);

				ScaleTransform scaleIn = (ScaleTransform)GetRenderTransform(_switchIn, typeof(ScaleTransform));

				scaleIn.CenterX = centerX;
				scaleIn.CenterY = centerY;
				scaleIn.ScaleX = scaleIn.ScaleY = 1.1;

				DoubleAnimation scaleInAnim = new DoubleAnimation(1, AnimationDuration, FillBehavior.Stop);
				scaleInAnim.Completed += scaleInAnim_Completed;
				scaleInAnim.EasingFunction = Ease;

				_switchIn.Visibility = Visibility.Visible;

				DoubleAnimation opacityInAnim = new DoubleAnimation(0, 1, AnimationDuration, FillBehavior.Stop);

				// To do, or not to do
				opacityInAnim.EasingFunction = Ease;

				scaleIn.BeginAnimation(ScaleTransform.ScaleXProperty, scaleInAnim);
				scaleIn.BeginAnimation(ScaleTransform.ScaleYProperty, scaleInAnim);

				_switchIn.BeginAnimation(FrameworkElement.OpacityProperty, opacityInAnim);
			}

			private void scaleInAnim_Completed(object sender, EventArgs e)
			{
				_switchIn.ApplyAnimationClock(FrameworkElement.OpacityProperty, null);

				ScaleTransform scaleTransform = (ScaleTransform)GetRenderTransform(_switchIn, typeof(ScaleTransform));

				scaleTransform.ApplyAnimationClock(ScaleTransform.ScaleXProperty, null);
				scaleTransform.ApplyAnimationClock(ScaleTransform.ScaleYProperty, null);
				scaleTransform.ScaleX = scaleTransform.ScaleY = 1;

				_switchIn.Opacity = 1;
				_switchIn.IsHitTestVisible = true;

				AnimationCompletedEvent(e);
			}

			public delegate void OnAnimationCompleted(object sender, EventArgs e);

			public event OnAnimationCompleted OnAnimationCompletedEvent;

			protected void AnimationCompletedEvent(EventArgs e)
			{
				if (OnAnimationCompletedEvent != null)
					OnAnimationCompletedEvent(this, e);
			}
		}

		#endregion

		#region Slide Display

		public class SlideDisplay
		{
			public SlideDisplay(FrameworkElement grid, Image image)
			{
				_grid = grid;
				_image = image;
			}

			private FrameworkElement _grid;
			private Image _image;

			//private Duration AnimationDuration
			//{
			//	get { return new Duration(TimeSpan.FromMilliseconds(300)); }
			//}

			public void SwitchViews(SlideDirection direction)
			{
				_image.Visibility = Visibility.Visible;
				_grid.Visibility = Visibility.Hidden;

				if (direction == SlideDirection.Right)
					SlideRight();
				else
					SlideLeft();
			}

			private void SlideLeft()
			{
				QuarticEase Ease = new QuarticEase();
				Ease.EasingMode = EasingMode.EaseIn;

				TranslateTransform translate = (TranslateTransform)GetRenderTransform(_image, typeof(TranslateTransform));

				DoubleAnimation leftAnim1 = new DoubleAnimation(0, 50, AnimationDuration, FillBehavior.Stop);

				//ThicknessAnimation leftAnim1 = new ThicknessAnimation(new Thickness(0), new Thickness(50, 0, -50, 0), AnimationDuration, FillBehavior.Stop);
				leftAnim1.EasingFunction = Ease;
				leftAnim1.Completed += leftAnim1_Completed;

				DoubleAnimation opac = new DoubleAnimation(1, 0, AnimationDuration, FillBehavior.Stop);
				opac.EasingFunction = Ease;

				_image.BeginAnimation(FrameworkElement.OpacityProperty, opac);
				translate.BeginAnimation(TranslateTransform.XProperty, leftAnim1);
				//_image.BeginAnimation(FrameworkElement.MarginProperty, leftAnim1);
			}

			private void leftAnim1_Completed(object sender, EventArgs e)
			{
				_image.Visibility = Visibility.Hidden;
				_image.ApplyAnimationClock(FrameworkElement.OpacityProperty, null);
				GetRenderTransform(_image, typeof(TranslateTransform)).ApplyAnimationClock(TranslateTransform.XProperty, null);
				//_image.ApplyAnimationClock(FrameworkElement.MarginProperty, null);

				QuarticEase Ease = new QuarticEase();
				Ease.EasingMode = EasingMode.EaseOut;

				_grid.Visibility = Visibility.Visible;

				TranslateTransform gridTransform = (TranslateTransform)GetRenderTransform(_grid, typeof(TranslateTransform));

				DoubleAnimation leftAnim2 = new DoubleAnimation(-50, 0, AnimationDuration, FillBehavior.Stop);
				leftAnim2.EasingFunction = Ease;
				leftAnim2.Completed += anim_Completed;

				//ThicknessAnimation leftAnim2 = new ThicknessAnimation(new Thickness(-60, 0, 60, 0), new Thickness(0), AnimationDuration, FillBehavior.Stop);
				//leftAnim2.EasingFunction = Ease;
				//leftAnim2.Completed += anim_Completed;

				DoubleAnimation opac = new DoubleAnimation(0, 1, AnimationDuration, FillBehavior.Stop);
				opac.EasingFunction = Ease;

				_grid.BeginAnimation(FrameworkElement.OpacityProperty, opac);
				gridTransform.BeginAnimation(TranslateTransform.XProperty, leftAnim2);
				//_grid.BeginAnimation(FrameworkElement.MarginProperty, leftAnim2);
			}

			private void SlideRight()
			{
				QuarticEase Ease = new QuarticEase();
				Ease.EasingMode = EasingMode.EaseIn;

				TranslateTransform translate = (TranslateTransform)GetRenderTransform(_image, typeof(TranslateTransform));

				DoubleAnimation rightAnim1 = new DoubleAnimation(0, -50, AnimationDuration, FillBehavior.Stop);

				//ThicknessAnimation rightAnim1 = new ThicknessAnimation(new Thickness(0), new Thickness(-50, 0, 50, 0), AnimationDuration, FillBehavior.Stop);
				rightAnim1.EasingFunction = Ease;
				rightAnim1.Completed += rightAnim1_Completed;

				DoubleAnimation opac = new DoubleAnimation(1, 0, AnimationDuration, FillBehavior.Stop);
				opac.EasingFunction = Ease;

				_image.BeginAnimation(FrameworkElement.OpacityProperty, opac);
				translate.BeginAnimation(TranslateTransform.XProperty, rightAnim1);
				//_image.BeginAnimation(FrameworkElement.MarginProperty, rightAnim1);
			}

			private void rightAnim1_Completed(object sender, EventArgs e)
			{
				_image.Visibility = Visibility.Hidden;
				_image.ApplyAnimationClock(FrameworkElement.OpacityProperty, null);
				GetRenderTransform(_image, typeof(TranslateTransform)).ApplyAnimationClock(TranslateTransform.XProperty, null);
				//_image.ApplyAnimationClock(FrameworkElement.MarginProperty, null);

				QuarticEase Ease = new QuarticEase();
				Ease.EasingMode = EasingMode.EaseOut;

				_grid.Visibility = Visibility.Visible;

				TranslateTransform gridTransform = (TranslateTransform)GetRenderTransform(_grid, typeof(TranslateTransform));

				DoubleAnimation leftAnim2 = new DoubleAnimation(50, 0, AnimationDuration, FillBehavior.Stop);
				leftAnim2.EasingFunction = Ease;
				leftAnim2.Completed += anim_Completed;

				//ThicknessAnimation leftAnim2 = new ThicknessAnimation(new Thickness(60, 0, -60, 0), new Thickness(0), AnimationDuration, FillBehavior.Stop);
				//leftAnim2.EasingFunction = Ease;
				//leftAnim2.Completed += anim_Completed;

				DoubleAnimation opac = new DoubleAnimation(0, 1, AnimationDuration, FillBehavior.Stop);
				opac.EasingFunction = Ease;

				_grid.BeginAnimation(FrameworkElement.OpacityProperty, opac);
				gridTransform.BeginAnimation(TranslateTransform.XProperty, leftAnim2);
				//_grid.BeginAnimation(FrameworkElement.MarginProperty, leftAnim2);
			}

			private void anim_Completed(object sender, EventArgs e)
			{
				_grid.ApplyAnimationClock(FrameworkElement.OpacityProperty, null);
				//_grid.ApplyAnimationClock(FrameworkElement.MarginProperty, null);

				GetRenderTransform(_grid, typeof(TranslateTransform)).ApplyAnimationClock(TranslateTransform.XProperty, null);

				AnimationCompletedEvent(e);
			}

			public delegate void OnAnimationCompleted(object sender, EventArgs e);

			public event OnAnimationCompleted OnAnimationCompletedEvent;

			protected void AnimationCompletedEvent(EventArgs e)
			{
				if (OnAnimationCompletedEvent != null)
					OnAnimationCompletedEvent(this, e);
			}
		}

		#endregion

		#region Delete Animation

		public class DeleteAnimation
		{
			public DeleteAnimation(FrameworkElement control)
			{
				_control = control;
			}

			private FrameworkElement _control;

			public FrameworkElement Control
			{
				get { return _control; }
			}

			public void Animate()
			{
				_control.IsHitTestVisible = false;
				_control.IsEnabled = false;

				DoubleAnimation opacityAnim = new DoubleAnimation(0, AnimationDuration, FillBehavior.Stop);
				opacityAnim.EasingFunction = EasingFunction;
				opacityAnim.Completed += opacityAnim_Completed;

				_control.BeginAnimation(FrameworkElement.OpacityProperty, opacityAnim);
			}

			private void opacityAnim_Completed(object sender, EventArgs e)
			{
				_control.Opacity = 0;
				_control.ApplyAnimationClock(FrameworkElement.OpacityProperty, null);

				DoubleAnimation sizeAnim = new DoubleAnimation(_control.RenderSize.Height, 0, AnimationDuration, FillBehavior.Stop);
				sizeAnim.EasingFunction = EasingFunction;
				sizeAnim.Completed += sizeAnim_Completed;

				Thickness margin = _control.Margin;
				Thickness toMargin = margin;
				toMargin.Top = 0;
				toMargin.Bottom = 0;

				ThicknessAnimation marginAnim = new ThicknessAnimation(margin, toMargin, AnimationDuration, FillBehavior.Stop);
				marginAnim.EasingFunction = EasingFunction;

				_control.BeginAnimation(FrameworkElement.MarginProperty, marginAnim);
				_control.BeginAnimation(FrameworkElement.HeightProperty, sizeAnim);
			}

			private void sizeAnim_Completed(object sender, EventArgs e)
			{
				_control.Height = 0;
				Thickness margin = _control.Margin;
				margin.Top = 0;
				margin.Bottom = 0;
				_control.Margin = margin;

				_control.ApplyAnimationClock(FrameworkElement.HeightProperty, null);
				_control.ApplyAnimationClock(FrameworkElement.MarginProperty, null);

				AnimationCompletedEvent(new EventArgs());
			}

			public delegate void OnAnimationCompleted(object sender, EventArgs e);

			public event OnAnimationCompleted OnAnimationCompletedEvent;

			protected void AnimationCompletedEvent(EventArgs e)
			{
				if (OnAnimationCompletedEvent != null)
					OnAnimationCompletedEvent(this, e);
			}
		}

		#endregion

		#region Load Animation

		public class LoadAnimation
		{
			public LoadAnimation(FrameworkElement control)
			{
				_control = control;
			}

			private FrameworkElement _control;

			public FrameworkElement Control
			{
				get { return _control; }
			}

			private Thickness _originalMargin;
			private double _height;

			public void Animate(double height)
			{
				_control.IsHitTestVisible = false;
				_control.IsEnabled = false;

				_height = height;

				_control.Opacity = 0;

				DoubleAnimation sizeAnim = new DoubleAnimation(0, height, AnimationDuration, FillBehavior.Stop);
				sizeAnim.EasingFunction = EasingFunction;
				sizeAnim.Completed += sizeAnim_Completed;

				Thickness margin = _control.Margin;
				_originalMargin = margin;
				Thickness fromMargin = margin;
				fromMargin.Top = 0;
				fromMargin.Bottom = 0;

				ThicknessAnimation marginAnim = new ThicknessAnimation(fromMargin, margin, AnimationDuration, FillBehavior.Stop);
				marginAnim.EasingFunction = EasingFunction;

				_control.BeginAnimation(FrameworkElement.MarginProperty, marginAnim);
				_control.BeginAnimation(FrameworkElement.HeightProperty, sizeAnim);
			}

			private void sizeAnim_Completed(object sender, EventArgs e)
			{
				_control.Margin = _originalMargin;
				_control.Height = _height;
				_control.ApplyAnimationClock(FrameworkElement.HeightProperty, null);
				_control.ApplyAnimationClock(FrameworkElement.MarginProperty, null);

				DoubleAnimation opacityAnim = new DoubleAnimation(1, AnimationDuration, FillBehavior.Stop);
				opacityAnim.EasingFunction = EasingFunction;
				opacityAnim.Completed += opacityAnim_Completed;

				_control.BeginAnimation(FrameworkElement.OpacityProperty, opacityAnim);
			}

			private void opacityAnim_Completed(object sender, EventArgs e)
			{
				_control.IsHitTestVisible = true;
				_control.IsEnabled = true;
				_control.Opacity = 1;
				_control.ApplyAnimationClock(FrameworkElement.OpacityProperty, null);
				AnimationCompletedEvent(new EventArgs());
			}

			public delegate void OnAnimationCompleted(object sender, EventArgs e);

			public event OnAnimationCompleted OnAnimationCompletedEvent;

			protected void AnimationCompletedEvent(EventArgs e)
			{
				if (OnAnimationCompletedEvent != null)
					OnAnimationCompletedEvent(this, e);
			}
		}

		#endregion

		#region Single Zoom Display

		public class SingleZoomDisplay
		{
			public SingleZoomDisplay(FrameworkElement switchIn)
			{
				_switchIn = switchIn;
			}

			private FrameworkElement _switchIn;

			private double centerX;
			private double centerY;

			//private Duration AnimationDuration
			//{
			//	get { return new Duration(TimeSpan.FromMilliseconds(400)); }
			//}

			public void SwitchViews(ZoomDirection direction)
			{
				centerX = _switchIn.RenderSize.Width / 2;
				centerY = _switchIn.RenderSize.Height / 2;

				if (direction == ZoomDirection.In)
					ZoomIn();
				else
					ZoomOut();
			}

			private void ZoomIn()
			{
				QuarticEase Ease = new QuarticEase();
				Ease.EasingMode = EasingMode.EaseOut;

				ScaleTransform scaleIn = (ScaleTransform)GetRenderTransform(_switchIn, typeof(ScaleTransform));

				scaleIn.CenterX = centerX;
				scaleIn.CenterY = centerY;
				scaleIn.ScaleX = scaleIn.ScaleY = 0.9;

				DoubleAnimation scaleInAnim = new DoubleAnimation(1, AnimationDuration, FillBehavior.Stop);
				scaleInAnim.Completed += scaleInAnim_Completed;
				scaleInAnim.EasingFunction = Ease;

				_switchIn.Visibility = Visibility.Visible;

				DoubleAnimation opacityInAnim = new DoubleAnimation(0, 1, AnimationDuration, FillBehavior.Stop);
				opacityInAnim.EasingFunction = Ease;

				scaleIn.BeginAnimation(ScaleTransform.ScaleXProperty, scaleInAnim);
				scaleIn.BeginAnimation(ScaleTransform.ScaleYProperty, scaleInAnim);

				_switchIn.BeginAnimation(FrameworkElement.OpacityProperty, opacityInAnim);
			}

			private void scaleInAnim_Completed(object sender, EventArgs e)
			{
				_switchIn.ApplyAnimationClock(FrameworkElement.OpacityProperty, null);

				ScaleTransform scaleTransform = (ScaleTransform)GetRenderTransform(_switchIn, typeof(ScaleTransform));

				scaleTransform.ApplyAnimationClock(ScaleTransform.ScaleXProperty, null);
				scaleTransform.ApplyAnimationClock(ScaleTransform.ScaleYProperty, null);

				scaleTransform.ScaleX = scaleTransform.ScaleY = 1;

				_switchIn.Opacity = 1;

				AnimationCompletedEvent(new EventArgs());
			}

			private void ZoomOut()
			{
				QuarticEase Ease = new QuarticEase();
				Ease.EasingMode = EasingMode.EaseOut;

				ScaleTransform scaleIn = (ScaleTransform)GetRenderTransform(_switchIn, typeof(ScaleTransform));

				scaleIn.CenterX = centerX;
				scaleIn.CenterY = centerY;
				scaleIn.ScaleX = scaleIn.ScaleY = 1;

				DoubleAnimation scaleOutAnim = new DoubleAnimation(0.9, AnimationDuration, FillBehavior.Stop);
				scaleOutAnim.Completed += scaleOutAnim_Completed;
				scaleOutAnim.EasingFunction = Ease;

				_switchIn.Visibility = Visibility.Visible;

				DoubleAnimation opacityInAnim = new DoubleAnimation(1, 0, AnimationDuration, FillBehavior.Stop);
				opacityInAnim.EasingFunction = Ease;
				scaleIn.BeginAnimation(ScaleTransform.ScaleXProperty, scaleOutAnim);
				scaleIn.BeginAnimation(ScaleTransform.ScaleYProperty, scaleOutAnim);

				_switchIn.BeginAnimation(FrameworkElement.OpacityProperty, opacityInAnim);
			}

			private void scaleOutAnim_Completed(object sender, EventArgs e)
			{
				_switchIn.ApplyAnimationClock(FrameworkElement.OpacityProperty, null);
				_switchIn.Visibility = Visibility.Hidden;

				ScaleTransform scaleTransform = (ScaleTransform)GetRenderTransform(_switchIn, typeof(ScaleTransform));

				scaleTransform.ApplyAnimationClock(ScaleTransform.ScaleXProperty, null);
				scaleTransform.ApplyAnimationClock(ScaleTransform.ScaleYProperty, null);

				scaleTransform.ScaleX = scaleTransform.ScaleY = 1;

				AnimationCompletedEvent(new EventArgs());
			}

			public delegate void OnAnimationCompleted(object sender, EventArgs e);

			public event OnAnimationCompleted OnAnimationCompletedEvent;

			protected void AnimationCompletedEvent(EventArgs e)
			{
				if (OnAnimationCompletedEvent != null)
					OnAnimationCompletedEvent(this, e);
			}
		}

		#endregion

		#region Basic Fade

		public class Fade
		{
			public Fade(FrameworkElement element, FadeDirection direction, bool hideOnComplete = false, double transparentOpacity = 0, double opaqueOpacity = 1, bool useExistingValues = false)
			{
				_element = element;
				_direction = direction;
				_hideoncomplete = hideOnComplete;
				_transparentOpacity = transparentOpacity;
				_opaqueOpacity = opaqueOpacity;

				if (direction == FadeDirection.In)
				{
					QuarticEase ease = new QuarticEase();
					ease.EasingMode = EasingMode.EaseIn;

					if (!useExistingValues)
					{
						DoubleAnimation anim = new DoubleAnimation(transparentOpacity, opaqueOpacity, AnimationDuration, FillBehavior.Stop);
						anim.EasingFunction = ease;
						anim.Completed += anim_Completed;

						if (!(element is Window))
							element.Visibility = Visibility.Visible;

						element.BeginAnimation(FrameworkElement.OpacityProperty, anim);
					}
					else
					{
						DoubleAnimation anim = new DoubleAnimation(opaqueOpacity, AnimationDuration);
						anim.EasingFunction = ease;

						if (!(element is Window))
							element.Visibility = Visibility.Visible;

						element.BeginAnimation(FrameworkElement.OpacityProperty, anim);
					}
				}
				else
				{
					QuarticEase ease = new QuarticEase();
					ease.EasingMode = EasingMode.EaseOut;

					if (!useExistingValues)
					{
						DoubleAnimation anim = new DoubleAnimation(opaqueOpacity, transparentOpacity, AnimationDuration, FillBehavior.Stop);
						anim.EasingFunction = ease;
						anim.Completed += anim_Completed;

						element.BeginAnimation(FrameworkElement.OpacityProperty, anim);
					}
					else
					{
						DoubleAnimation anim = new DoubleAnimation(transparentOpacity, AnimationDuration);
						anim.EasingFunction = ease;

						element.BeginAnimation(FrameworkElement.OpacityProperty, anim);
					}
				}
			}

			private FadeDirection _direction;
			private FrameworkElement _element;
			private bool _hideoncomplete;
			private double _opaqueOpacity;
			private double _transparentOpacity;

			private void anim_Completed(object sender, EventArgs e)
			{
				if (_direction == FadeDirection.In)
					_element.Opacity = _opaqueOpacity;
				else
				{
					_element.Opacity = _transparentOpacity;

					if (_hideoncomplete)
						_element.Visibility = Visibility.Hidden;
				}

				_element.ApplyAnimationClock(FrameworkElement.OpacityProperty, null);

				AnimationCompletedEvent(e);
			}

			public delegate void OnAnimationCompleted(object sender, EventArgs e);

			public event OnAnimationCompleted OnAnimationCompletedEvent;

			protected void AnimationCompletedEvent(EventArgs e)
			{
				if (OnAnimationCompletedEvent != null)
					OnAnimationCompletedEvent(this, e);
			}
		}

		#endregion

		#region SortAnimation

		public class SortAnimation
		{
			public SortAnimation(FrameworkElement element, Thickness newMargin)
			{
				SetSortAnimation(element, this);
				Animate(element, newMargin);
			}

			private bool _canceled = false;

			private void Animate(FrameworkElement element, Thickness newMargin)
			{
				if (Settings.AnimationsEnabled)
				{
					ThicknessAnimation anim = new ThicknessAnimation(newMargin, new Duration(TimeSpan.FromMilliseconds(200)));
					anim.EasingFunction = EasingFunction;
					anim.Completed += (sender, e) =>
					{
						if (!_canceled && GetSortAnimation(element) == this)
						{
							element.Margin = newMargin;
							element.ApplyAnimationClock(FrameworkElement.MarginProperty, null);
						}
					};
					element.BeginAnimation(FrameworkElement.MarginProperty, anim);
				}
				else
					element.Margin = newMargin;
			}

			public static void Stop(FrameworkElement element)
			{
				SortAnimation animation = GetSortAnimation(element);

				if (animation != null)
					animation._canceled = true;

				element.ApplyAnimationClock(FrameworkElement.MarginProperty, null);
			}

			public static readonly DependencyProperty SortAnimationProperty = DependencyProperty.Register(
				"SortAnimation", typeof(SortAnimation), typeof(FrameworkElement), new PropertyMetadata(null));

			public static void SetSortAnimation(FrameworkElement element, SortAnimation value)
			{
				element.SetValue(SortAnimationProperty, value);
			}

			public static SortAnimation GetSortAnimation(FrameworkElement element)
			{
				return (SortAnimation)element.GetValue(SortAnimationProperty);
			}
		}

		#endregion
	}
}