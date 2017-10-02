using Daytimer.Functions;
using Microsoft.Windows.Shell;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Daytimer.Fundamentals
{
	[ComVisible(false)]
	[TemplatePart(Name = OfficeWindow.TemplateRootName, Type = typeof(Grid))]
	[TemplatePart(Name = OfficeWindow.ImageName, Type = typeof(Button))]
	public class OfficeWindow : Window
	{
		#region Constructors

		static OfficeWindow()
		{
			Type ownerType = typeof(OfficeWindow);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));

			CommandBinding close = new CommandBinding(SystemCommands.CloseWindowCommand, new ExecutedRoutedEventHandler(CloseWindowExecuted));
			CommandBinding maximize = new CommandBinding(SystemCommands.MaximizeWindowCommand, new ExecutedRoutedEventHandler(MaximizeWindowExecuted), new CanExecuteRoutedEventHandler(MaximizeWindowCanExecute));
			CommandBinding restore = new CommandBinding(SystemCommands.RestoreWindowCommand, new ExecutedRoutedEventHandler(RestoreWindowExecuted), new CanExecuteRoutedEventHandler(MaximizeWindowCanExecute));
			CommandBinding minimize = new CommandBinding(SystemCommands.MinimizeWindowCommand, new ExecutedRoutedEventHandler(MinimizeWindowExecuted), new CanExecuteRoutedEventHandler(MinimizeWindowCanExecute));

			CommandManager.RegisterClassCommandBinding(ownerType, close);
			CommandManager.RegisterClassCommandBinding(ownerType, maximize);
			CommandManager.RegisterClassCommandBinding(ownerType, restore);
			CommandManager.RegisterClassCommandBinding(ownerType, minimize);
		}

		private ShadowController shadowController;

		public OfficeWindow()
		{
			shadowController = new ShadowController(this);
			SystemParameters2.Current.PropertyChanged += SystemParameters2_PropertyChanged;

			//if (ResizeMode == ResizeMode.CanMinimize || ResizeMode == ResizeMode.NoResize)
			//	_originalResizeBorder = WindowChrome.GetWindowChrome(this).ResizeBorderThickness = new Thickness(0);
			//else
			//	_originalResizeBorder = SystemParameters2.Current.WindowResizeBorderThickness;

			Loaded += OfficeWindow_Loaded;
			Unloaded += OfficeWindow_Unloaded;
		}

		#endregion

		#region Fields

		private const string TemplateRootName = "PART_TemplateRoot";
		private const string ImageName = "PART_Image";

		#endregion

		#region Public Methods

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			TemplateRoot = GetTemplateChild(TemplateRootName) as Grid;
			Image = GetTemplateChild(ImageName) as Button;

			if (Image != null)
			{
				Image.PreviewMouseLeftButtonDown += Image_PreviewMouseLeftButtonDown;
				Image.MouseRightButtonUp += Image_MouseRightButtonUp;
				Image.PreviewMouseDoubleClick += Image_PreviewMouseDoubleClick;
			}

			Dispatcher.BeginInvoke(() =>
			{
				HandleWindowStateChanged();
			});
		}

		/// <summary>
		/// Call to indicate layout with taskbar in autohide mode.
		/// </summary>
		public bool? IsTaskbarAutoHidden
		{
			get;
			set;
		}

		public void CloseShadow()
		{
			shadowController.Close();
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets if Aero glass is enabled.
		/// </summary>
		public bool IsGlassEnabled
		{
			get { return (bool)GetValue(IsGlassEnabledProperty); }
		}

		public static readonly DependencyProperty IsGlassEnabledProperty = DependencyProperty.Register(
			"IsGlassEnabled", typeof(bool), typeof(OfficeWindow),
			new PropertyMetadata(SystemParameters2.Current.IsGlassEnabled));

		/// <summary>
		/// Gets or sets if the title is visible in the caption.
		/// </summary>
		public bool IsTitleVisible
		{
			get { return (bool)GetValue(IsTitleVisibleProperty); }
			set { SetValue(IsTitleVisibleProperty, value); }
		}

		public static readonly DependencyProperty IsTitleVisibleProperty = DependencyProperty.Register(
			"IsTitleVisible", typeof(bool), typeof(OfficeWindow), new PropertyMetadata(true));

		/// <summary>
		/// Gets or sets if the window is currently in flash mode.
		/// </summary>
		public bool IsFlashing
		{
			get { return (bool)GetValue(IsFlashingProperty); }
			set { SetValue(IsFlashingProperty, value); }
		}

		public static readonly DependencyProperty IsFlashingProperty = DependencyProperty.Register(
			"IsFlashing", typeof(bool), typeof(OfficeWindow), new PropertyMetadata(false));

		/// <summary>
		/// Gets or sets if the maximize/restore button should be forced to hide. Intended to be used
		/// to force a minimalistic caption bar.
		/// </summary>
		public bool ForceHideMaximizeRestore
		{
			get { return (bool)GetValue(ForceHideMaximizeRestoreProperty); }
			set { SetValue(ForceHideMaximizeRestoreProperty, value); }
		}

		public static readonly DependencyProperty ForceHideMaximizeRestoreProperty = DependencyProperty.Register(
			"ForceHideMaximizeRestore", typeof(bool), typeof(OfficeWindow), new PropertyMetadata(false));

		/// <summary>
		/// Gets or sets the tooltip text for the close button.
		/// </summary>
		public string CloseButtonToolTip
		{
			get { return (string)GetValue(CloseButtonToolTipProperty); }
			set { SetValue(CloseButtonToolTipProperty, value); }
		}

		public static readonly DependencyProperty CloseButtonToolTipProperty = DependencyProperty.Register(
			"CloseButtonToolTip", typeof(string), typeof(OfficeWindow), new PropertyMetadata("Close"));

		public ShadowController ShadowController
		{
			get { return shadowController; }
		}

		#endregion

		#region Internal Properties

		internal Grid TemplateRoot;
		internal Button Image;

		#endregion

		#region Internal Methods

		private static void CloseWindowExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			SystemCommands.CloseWindow(sender as Window);
		}

		private static void MaximizeWindowCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = (sender as Window).ResizeMode == ResizeMode.CanResize || (sender as Window).ResizeMode == ResizeMode.CanResizeWithGrip;
		}

		private static void MaximizeWindowExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			SystemCommands.MaximizeWindow(sender as Window);
		}

		private static void RestoreWindowExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			SystemCommands.RestoreWindow(sender as Window);
		}

		private static void MinimizeWindowCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = (sender as Window).ResizeMode != ResizeMode.NoResize;
		}

		private static void MinimizeWindowExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			SystemCommands.MinimizeWindow(sender as Window);
		}

		private void OfficeWindow_Loaded(object sender, RoutedEventArgs e)
		{
			if (Owner != null)
				HwndSource.FromHwnd((new WindowInteropHelper(Owner)).Handle).AddHook(WndProc);
		}

		private void OfficeWindow_Unloaded(object sender, RoutedEventArgs e)
		{
			if (Owner != null)
			{
				IntPtr handle = new WindowInteropHelper(Owner).Handle;

				if (handle != IntPtr.Zero)
					HwndSource.FromHwnd(handle).RemoveHook(WndProc);
			}

			SystemParameters2.Current.PropertyChanged -= SystemParameters2_PropertyChanged;

			if (flashTimer != null)
			{
				flashTimer.Stop();
				flashTimer.Tick -= flashTimer_Tick;
				flashTimer = null;
			}
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == 0x0020)
			{
				if ((short)((long)lParam & 0xffff) == (-2))
				{
					short hiword = (short)((((long)lParam) >> 16) & 0xffff);

					if (hiword == 0x0201 || hiword == 0x0204)
						Flash();
				}
			}

			return IntPtr.Zero;
		}

		#region Flash window

		private void Flash()
		{
			if (!isFlashing)
			{
				isFlashing = true;

				if (flashTimer != null)
				{
					flashTimer.Stop();
					flashTimer.Tick -= flashTimer_Tick;
					flashTimer = null;
				}

				flashTimer = new DispatcherTimer();
				flashTimer.Interval = TimeSpan.FromMilliseconds(50);
				flashTimer.Tick += flashTimer_Tick;
				flashTimer.Start();
			}
			else
			{
				flashCount = 0;
				flashTimer.Stop();
				flashTimer.Start();
			}
		}

		private int flashCount = 0;
		private bool isFlashing = false;
		private DispatcherTimer flashTimer;

		private void flashTimer_Tick(object sender, EventArgs e)
		{
			if (flashCount < SystemParameters.ForegroundFlashCount * 2)
			{
				flashCount++;

				if (flashCount % 2 == 1)
				{
					ShadowController.FullHide();
					IsFlashing = true;
				}
				else
				{
					ShadowController.Show();
					IsFlashing = false;
				}
			}
			else
			{
				flashCount = 0;
				isFlashing = false;
				IsFlashing = false;
				ShadowController.Show();
				(sender as DispatcherTimer).Stop();
			}
		}

		#endregion

		private void Image_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (WindowState == WindowState.Maximized)
			{
				Rect rect = MonitorHelper.MonitorWorkingAreaFromWindow(this);
				SystemCommands.ShowSystemMenu(this, new Point(rect.Left, rect.Top + 29));
			}
			else
				SystemCommands.ShowSystemMenu(this, new Point(Left, 29 + Top));
		}

		private void Image_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			SystemCommands.ShowSystemMenu(this, PointToScreen(e.GetPosition(this)));
		}

		private void Image_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			Close();
			//SystemCommands.CloseWindow(this);
		}

		//private Thickness _originalResizeBorder;

		protected override void OnStateChanged(EventArgs e)
		{
			base.OnStateChanged(e);
			HandleWindowStateChanged();
		}

		private void SystemParameters2_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "IsGlassEnabled")
			{
				SetValue(IsGlassEnabledProperty, SystemParameters2.Current.IsGlassEnabled);

				if (WindowState == WindowState.Maximized)
				{
					if (IsGlassEnabled)
						TemplateRoot.Margin = new Thickness(SystemParameters.BorderWidth + 2);
					else
						TemplateRoot.Margin = new Thickness(SystemParameters.BorderWidth - 1);
				}
			}
		}

		private void HandleWindowStateChanged()
		{
			//
			// BUG FIX:	Window on minimize would go behind any other windows
			//			on desktop, preventing user from seeing the Windows
			//			minimize animation.
			Topmost = WindowState == WindowState.Minimized;

			if (TemplateRoot != null)
				if (WindowState == WindowState.Normal)
					TemplateRoot.Margin = new Thickness(0);
				else if (WindowState == WindowState.Maximized)
					TemplateRoot.Margin = new Thickness(SystemParameters.BorderWidth + 3);
			//else if (WindowState == WindowState.Minimized)
			//{
			//	if (Owner != null)
			//		Owner.WindowState = WindowState.Minimized;
			//}

			if (WindowState == WindowState.Minimized)
			{
				if (Owner != null)
					Owner.WindowState = WindowState.Minimized;
			}
			else if (WindowState == WindowState.Maximized)
			{
				try
				{
					//Rect rect = MonitorHelper.MonitorWorkingAreaFromWindow(this);
					//IntPtr _mHWND = (new WindowInteropHelper(this)).Handle;

					//int x = (int)rect.Left;
					//int y = (int)rect.Top;
					//int cx = (int)rect.Width;
					//int cy = (int)rect.Height;
					//UnsafeNativeMethods.SetWindowPos(_mHWND, new IntPtr(-2), x, y, cx, cy - 1, 0x0040);

					WindowChrome chrome = WindowChrome.GetWindowChrome(this);
					//_originalResizeBorder = chrome.ResizeBorderThickness;
					chrome.ResizeBorderThickness = new Thickness(0);
				}
				catch (InvalidOperationException) { }
			}
			else
			{
				if (ResizeMode == ResizeMode.CanResize || ResizeMode == ResizeMode.CanResizeWithGrip)
				{
					WindowChrome chrome = WindowChrome.GetWindowChrome(this);
					chrome.ResizeBorderThickness = new Thickness(1);// _originalResizeBorder;
				}
			}
		}

		#endregion
	}
}
