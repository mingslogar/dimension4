using Daytimer.Functions;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Daytimer.Controls.Friction
{
	[ComVisible(false)]
	public class FrictionScrollViewer : ScrollViewer
	{
		public FrictionScrollViewer()
		{
			Loaded += FrictionScrollViewer_Loaded;
			Unloaded += FrictionScrollViewer_Unloaded;
		}

		private Window _window;

		private void FrictionScrollViewer_Loaded(object sender, RoutedEventArgs e)
		{
			_window = Window.GetWindow(this);

			if (_window == null)
				return;

			IntPtr windowhandle = new WindowInteropHelper(_window).Handle;

			if (windowhandle == IntPtr.Zero)
				return;

			HwndSource hwndSource = HwndSource.FromHwnd(windowhandle);
			hwndSource.AddHook(new HwndSourceHook(WndProc));

			f = new FrictionAnimation(this);
			f.VerticalSnapToValue = VerticalSnapToValue;
		}

		private void FrictionScrollViewer_Unloaded(object sender, RoutedEventArgs e)
		{
			if (_window == null)
				return;

			IntPtr windowhandle = new WindowInteropHelper(_window).Handle;

			if (windowhandle == IntPtr.Zero)
				return;

			HwndSource hwndSource = HwndSource.FromHwnd(windowhandle);
			hwndSource.RemoveHook(new HwndSourceHook(WndProc));

			if (f != null)
				f.Cleanup();
		}

		private const int WM_HSCROLL = 0x0114;

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == WM_HSCROLL)
			{
				if (IsMouseOver)
				{
					int nScrollCode = ((short)(((long)(wParam)) & 0xffff));

					if (nScrollCode == 0)
						AddHorizontalPixels(-15);
					else if (nScrollCode == 1)
						AddHorizontalPixels(15);
					else if (nScrollCode == 2)
						ScrollToHorizontalPixel(HorizontalOffset - ViewportWidth);
					else if (nScrollCode == 3)
						ScrollToHorizontalPixel(HorizontalOffset + ViewportWidth);
					else if (nScrollCode == 6)
						ScrollToHorizontalPixel(0);
					else if (nScrollCode == 7)
						ScrollToHorizontalPixel(ScrollableWidth);
				}
			}

			return IntPtr.Zero;
		}

		private FrictionAnimation f;

		protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
		{
			if (!IsHitTestScrollingEnabled)
				return;
			else
				base.OnPreviewMouseWheel(e);

			ProcessMouseWheel(e);
		}

		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			if (IsHitTestScrollingEnabled)
				base.OnMouseWheel(e);
			else
				RaiseUnhandledMouseWheelEvent(e);
		}

		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			base.OnPreviewKeyDown(e);

			if (Settings.AnimationsEnabled)
			{
				if (e.OriginalSource != this)
					return;

				e.Handled = true;

				switch (e.Key)
				{
					case Key.PageUp:
						f.AddVerticalPixels(-ViewportHeight);
						break;

					case Key.PageDown:
						f.AddVerticalPixels(ViewportHeight);
						break;

					case Key.Home:
						f.GoToVerticalPixel(0);
						break;

					case Key.End:
						f.GoToVerticalPixel(ScrollableHeight);
						break;

					case Key.Up:
						f.AddVerticalPixels(-FontFamily.LineSpacing * FontSize);
						break;

					case Key.Down:
						f.AddVerticalPixels(FontFamily.LineSpacing * FontSize);
						break;

					default:
						e.Handled = false;
						break;
				}
			}
		}

		public void ScrollToVerticalPixel(double pixel)
		{
			if (Settings.AnimationsEnabled)
				f.GoToVerticalPixel(pixel);
			else
				ScrollToVerticalOffset(pixel);
		}

		public void ScrollToHorizontalPixel(double pixel)
		{
			if (Settings.AnimationsEnabled)
				f.GoToHorizontalPixel(pixel);
			else
				ScrollToHorizontalOffset(pixel);
		}

		public void AddVerticalPixels(double pixels)
		{
			if (Settings.AnimationsEnabled)
				f.AddVerticalPixels(pixels);
			else
				ScrollToVerticalOffset(VerticalOffset + pixels);
		}

		public void AddHorizontalPixels(double pixels)
		{
			if (Settings.AnimationsEnabled)
				f.AddHorizontalPixels(pixels);
			else
				ScrollToHorizontalOffset(HorizontalOffset + pixels);
		}

		public void ProcessMouseWheel(MouseWheelEventArgs e)
		{
			//!Mouse.DirectlyOver.Focusable && 
			if (Settings.AnimationsEnabled)
			{
				if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
				{
					//
					// TEMPORARY BUG FIX:	Prevents mousewheel on ComboBox from scrolling
					//						this FrictionScrollViewer.
					//
					if (Mouse.DirectlyOver is FrameworkElement)
					{
						FrameworkElement _over = Mouse.DirectlyOver as FrameworkElement;

						FrameworkElement _comboitem = _over.FindTemplatedAncestor(typeof(ComboBoxItem));
						if (_comboitem != null)
							return;

						FrameworkElement _combo = _over.FindTemplatedAncestor(typeof(ComboBox));
						if (_combo != null && (((ComboBox)_combo).IsDropDownOpen || _combo.IsFocused))
							return;
					}

					double scrollBarViewportSize = ViewportHeight / ScrollableHeight;

					if (!double.IsNaN(scrollBarViewportSize) && !double.IsInfinity(scrollBarViewportSize) && scrollBarViewportSize >= 0)
						e.Handled = true;

					if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
						f.AddVerticalPixels(-e.Delta);
					else
					{
						f.AddHorizontalPixels(-e.Delta);
						e.Handled = true;
					}
				}
			}
			else
				base.OnMouseWheel(e);
		}

		public static DependencyProperty IsHitTestScrollingEnabledProperty = DependencyProperty.Register(
			"IsHitTestScrollingEnabled", typeof(bool), typeof(FrictionScrollViewer), new PropertyMetadata(true));

		public bool IsHitTestScrollingEnabled
		{
			get { return (bool)GetValue(IsHitTestScrollingEnabledProperty); }
			set { SetValue(IsHitTestScrollingEnabledProperty, value); }
		}

		public static DependencyProperty VerticalSnapToValueProperty = DependencyProperty.Register(
			"VerticalSnapToValue", typeof(double), typeof(FrictionScrollViewer),
			new PropertyMetadata(1d, UpdateVerticalSnapToValueCallback));

		private static void UpdateVerticalSnapToValueCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			FrictionScrollViewer fsv = d as FrictionScrollViewer;

			if (fsv != null && fsv.f != null)
				fsv.f.VerticalSnapToValue = (double)e.NewValue;
		}

		/// <summary>
		/// Specifies that vertical scrolling should occur in increments of a number.
		/// </summary>
		public double VerticalSnapToValue
		{
			get { return (double)GetValue(VerticalSnapToValueProperty); }
			set { SetValue(VerticalSnapToValueProperty, value); }
		}

		public static readonly RoutedEvent UnhandledMouseWheelEvent = EventManager.RegisterRoutedEvent(
			"UnhandledMouseWheel", RoutingStrategy.Bubble, typeof(MouseWheelEventHandler), typeof(FrictionScrollViewer));

		public event MouseWheelEventHandler UnhandledMouseWheel
		{
			add { AddHandler(UnhandledMouseWheelEvent, value); }
			remove { RemoveHandler(UnhandledMouseWheelEvent, value); }
		}

		private void RaiseUnhandledMouseWheelEvent(MouseWheelEventArgs e)
		{
			RaiseEvent(new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) { RoutedEvent = UnhandledMouseWheelEvent });
		}

		class FrictionAnimation
		{
			public FrictionAnimation(ScrollViewer scroller)
			{
				this.scroller = scroller;

				clock = new DispatcherTimer();
				clock.Interval = TimeSpan.FromMilliseconds(Friction.TimerInterval);
				clock.Tick += clock_Tick;
			}

			private DispatcherTimer clock;
			private ScrollViewer scroller;

			private double verticalFinal;
			private double horizontalFinal;

			public double VerticalSnapToValue = 1;

			public bool IsAnimating
			{
				get { return clock.IsEnabled; }
			}

			private double GetVerticalSnappedPixel(double pixel)
			{
				return Math.Round(pixel / VerticalSnapToValue) * VerticalSnapToValue;
			}

			private void clock_Tick(object sender, EventArgs e)
			{
				double value = Math.Abs(verticalFinal - scroller.VerticalOffset) / Friction.FrictionDivisor;

				bool stopClock = true;

				if (value > Friction.FrictionTolerance)
				{
					if (scroller.VerticalOffset < verticalFinal - value)
						scroller.ScrollToVerticalOffset(scroller.VerticalOffset + value);
					else if (scroller.VerticalOffset > verticalFinal + value)
						scroller.ScrollToVerticalOffset(scroller.VerticalOffset - value);

					stopClock = false;
				}

				value = Math.Abs(horizontalFinal - scroller.HorizontalOffset) / Friction.FrictionDivisor;

				if (value > Friction.FrictionTolerance)
				{
					if (scroller.HorizontalOffset < horizontalFinal - value)
						scroller.ScrollToHorizontalOffset(scroller.HorizontalOffset + value);
					else if (scroller.HorizontalOffset > horizontalFinal + value)
						scroller.ScrollToHorizontalOffset(scroller.HorizontalOffset - value);

					stopClock = false;
				}

				if (stopClock)
				{
					clock.Stop();


					if (scroller.VerticalOffset <= 2)
						scroller.ScrollToTop();
					else if (scroller.VerticalOffset >= scroller.ScrollableHeight - 2)
						scroller.ScrollToBottom();

					if (scroller.HorizontalOffset <= 2)
						scroller.ScrollToLeftEnd();
					else if (scroller.HorizontalOffset >= scroller.ScrollableWidth - 2)
						scroller.ScrollToRightEnd();
				}
			}

			public void AddVerticalPixels(double pixels)
			{
				GoToVerticalPixel(scroller.VerticalOffset + pixels);
			}

			public void GoToVerticalPixel(double pixel)
			{
				verticalFinal = GetVerticalSnappedPixel(pixel);

				verticalFinal = verticalFinal < 0 ? 0 : verticalFinal;
				verticalFinal = verticalFinal > scroller.ScrollableHeight ? scroller.ScrollableHeight : verticalFinal;

				if (verticalFinal != scroller.VerticalOffset)
					clock.Start();
			}

			public void AddHorizontalPixels(double pixels)
			{
				GoToHorizontalPixel(scroller.HorizontalOffset + pixels);
			}

			public void GoToHorizontalPixel(double pixel)
			{
				horizontalFinal = pixel;

				horizontalFinal = horizontalFinal < 0 ? 0 : horizontalFinal;
				horizontalFinal = horizontalFinal > scroller.ScrollableWidth ? scroller.ScrollableWidth : horizontalFinal;

				if (horizontalFinal != scroller.HorizontalOffset)
					clock.Start();
			}

			public void Cleanup()
			{
				if (clock != null)
				{
					clock.Stop();
					clock.Tick -= clock_Tick;
					clock = null;
				}
			}
		}
	}
}
