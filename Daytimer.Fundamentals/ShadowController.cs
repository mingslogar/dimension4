using System;
using System.Windows;
using System.Windows.Threading;

namespace Daytimer.Fundamentals
{
	public class ShadowController
	{
		/// <summary>
		/// Create a new drop shadow.
		/// </summary>
		/// <param name="window"></param>
		public ShadowController(Window window)
		{
			left = new Shadow(Direction.Left);
			top = new Shadow(Direction.Top);
			right = new Shadow(Direction.Right);
			bottom = new Shadow(Direction.Bottom);

			if (window.IsLoaded)
			{
				left.Owner = window;
				top.Owner = window;
				right.Owner = window;
				bottom.Owner = window;
			}

			_window = window;

			window.ContentRendered += window_ContentRendered;
			window.Activated += window_Activated;
			window.Deactivated += window_Deactivated;
			window.StateChanged += window_StateChanged;
			window.Closed += window_Closed;
		}

		/// <summary>
		/// Create a new drop shadow which only supports resizing in one direction.
		/// </summary>
		/// <param name="window"></param>
		/// <param name="resizeDirection"></param>
		public ShadowController(Window window, Direction resizeDirection)
		{
			left = new Shadow(Direction.Left);
			top = new Shadow(Direction.Top);
			right = new Shadow(Direction.Right);
			bottom = new Shadow(Direction.Bottom);

			switch (resizeDirection)
			{
				case Direction.Left:
					top.IsHitTestVisible = false;
					right.IsHitTestVisible = false;
					bottom.IsHitTestVisible = false;
					break;

				case Direction.Top:
					left.IsHitTestVisible = false;
					right.IsHitTestVisible = false;
					bottom.IsHitTestVisible = false;
					break;

				case Direction.Right:
					left.IsHitTestVisible = false;
					top.IsHitTestVisible = false;
					bottom.IsHitTestVisible = false;
					break;

				case Direction.Bottom:
					left.IsHitTestVisible = false;
					top.IsHitTestVisible = false;
					right.IsHitTestVisible = false;
					break;
			}

			if (window.IsLoaded)
			{
				left.Owner = window;
				top.Owner = window;
				right.Owner = window;
				bottom.Owner = window;
			}

			_window = window;

			window.ContentRendered += window_ContentRendered;
			window.Activated += window_Activated;
			window.Deactivated += window_Deactivated;
			window.StateChanged += window_StateChanged;
			window.Closed += window_Closed;
		}

		private Shadow left;
		private Shadow top;
		private Shadow right;
		private Shadow bottom;

		private Window _window;

		private void window_ContentRendered(object sender, EventArgs e)
		{
			left.Owner = _window;
			top.Owner = _window;
			right.Owner = _window;
			bottom.Owner = _window;

			if (_window.ResizeMode == ResizeMode.CanMinimize || _window.ResizeMode == ResizeMode.NoResize)
				left.IsHitTestVisible = top.IsHitTestVisible = right.IsHitTestVisible = bottom.IsHitTestVisible = false;

			if (_window.IsActive && _window.WindowState == WindowState.Normal)
			{
				_window.Dispatcher.BeginInvoke(() =>
				{
					UpdateBorders();
					Show();
				});
			}
		}

		private void window_LocationChanged(object sender, EventArgs e)
		{
			UpdateBorders();
		}

		private void window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateBorders();
		}

		private void window_StateChanged(object sender, EventArgs e)
		{
			if (_window.WindowState == WindowState.Normal)
			{
				UpdateBorders();
				_show();
			}
			else
				_hide();

			_window.Topmost = _window.WindowState == WindowState.Minimized;
		}

		private void window_Deactivated(object sender, EventArgs e)
		{
			Hide();
		}

		private void window_Activated(object sender, EventArgs e)
		{
			UpdateBorders();
			Show();
		}

		public void UpdateBorders()
		{
			if (_window.WindowState == WindowState.Normal)
			{
				left.Left = _window.Left - 8;
				left.Top = right.Top = _window.Top - 6;

				top.Top = _window.Top - 8;
				top.Left = bottom.Left = _window.Left - 6;

				right.Left = _window.Left + _window.ActualWidth;
				bottom.Top = _window.Top + _window.ActualHeight;

				left.Height = right.Height = _window.ActualHeight + 12;
				top.Width = bottom.Width = _window.ActualWidth + 12;

				left.UpdateLayout();
				top.UpdateLayout();
				right.UpdateLayout();
				bottom.UpdateLayout();
			}
		}

		private void window_Closed(object sender, EventArgs e)
		{
			Close();
		}

		private void _hide()
		{
			left.Hide();
			top.Hide();
			right.Hide();
			bottom.Hide();

			_window.LocationChanged -= window_LocationChanged;
			_window.SizeChanged -= window_SizeChanged;
		}

		private void _show()
		{
			try
			{
				if (_window.WindowState == WindowState.Normal)
				{
					_window.LocationChanged -= window_LocationChanged;
					_window.SizeChanged -= window_SizeChanged;
					_window.LocationChanged += window_LocationChanged;
					_window.SizeChanged += window_SizeChanged;

					left.Show();
					top.Show();
					right.Show();
					bottom.Show();

					left.SetResourceReference(Window.ForegroundProperty, "WindowBorderFocused");
					top.SetResourceReference(Window.ForegroundProperty, "WindowBorderFocused");
					right.SetResourceReference(Window.ForegroundProperty, "WindowBorderFocused");
					bottom.SetResourceReference(Window.ForegroundProperty, "WindowBorderFocused");
				}
			}
			catch
			{
				// Shadow windows have already been closed.
			}
		}

		/// <summary>
		/// Set to unfocused state.
		/// </summary>
		public void Hide()
		{
			left.SetResourceReference(Window.ForegroundProperty, "WindowBorderUnfocused");
			top.SetResourceReference(Window.ForegroundProperty, "WindowBorderUnfocused");
			right.SetResourceReference(Window.ForegroundProperty, "WindowBorderUnfocused");
			bottom.SetResourceReference(Window.ForegroundProperty, "WindowBorderUnfocused");
		}

		/// <summary>
		/// Completely hide shadow.
		/// </summary>
		public void FullHide()
		{
			_hide();
		}

		public void Show()
		{
			_show();
		}

		public void Close()
		{
			_window.ContentRendered -= window_ContentRendered;
			_window.LocationChanged -= window_LocationChanged;
			_window.SizeChanged -= window_SizeChanged;
			_window.Activated -= window_Activated;
			_window.Deactivated -= window_Deactivated;
			_window.StateChanged -= window_StateChanged;
			_window.Closed -= window_Closed;

			left.Owner = null;
			left.Close();

			top.Owner = null;
			top.Close();

			right.Owner = null;
			right.Close();

			bottom.Owner = null;
			bottom.Close();
		}
	}
}
