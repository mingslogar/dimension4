using Microsoft.Windows.Shell;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Daytimer.Fundamentals
{
	[ComVisible(false)]
	[TemplatePart(Name = AppBar.TemplateRootName, Type = typeof(Grid))]
	public class AppBar : Window
	{
		#region Constructors

		static AppBar()
		{
			Type ownerType = typeof(AppBar);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));

			CommandBinding close = new CommandBinding(SystemCommands.CloseWindowCommand, new ExecutedRoutedEventHandler(CloseWindowExecuted));

			CommandManager.RegisterClassCommandBinding(ownerType, close);
		}

		public AppBar()
		{
			// Absolutely necessary for resizing to function properly.
			WindowStyle = WindowStyle.ToolWindow;

			// Register a unique message as our callback message
			CallbackMessageID = RegisterCallbackMessage();
			if (CallbackMessageID == 0)
				throw new Exception("RegisterCallbackMessage failed");

			Loaded += ApplicationDesktopToolbar_Loaded;
		}

		#endregion

		#region Fields

		private const string TemplateRootName = "PART_TemplateRoot";

		#endregion

		#region Internal Properties

		internal Grid TemplateRoot;

		#endregion

		#region Dependency Properties

		/// <summary>
		/// Gets or sets if the title is visible in the caption.
		/// </summary>
		public bool IsTitleVisible
		{
			get { return (bool)GetValue(IsTitleVisibleProperty); }
			set { SetValue(IsTitleVisibleProperty, value); }
		}

		public static readonly DependencyProperty IsTitleVisibleProperty = DependencyProperty.Register(
			"IsTitleVisible", typeof(bool), typeof(AppBar), new PropertyMetadata(false));

		#endregion

		#region Public Methods

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			TemplateRoot = GetTemplateChild(TemplateRootName) as Grid;
		}

		#endregion

		#region Private Methods

		private static void CloseWindowExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			SystemCommands.CloseWindow(sender as Window);
		}

		#endregion

		#region Application Desktop Toolbar

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public Int32 left;
			public Int32 top;
			public Int32 right;
			public Int32 bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct APPBARDATA
		{
			public UInt32 cbSize;
			public IntPtr hWnd;
			public UInt32 uCallbackMessage;
			public UInt32 uEdge;
			public RECT rc;
			public Int32 lParam;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dwMessage">Appbar message value to send.</param>
		/// <param name="pData">Address of an APPBARDATA structure. The content of the structure
		/// depends on the value set in the dwMessage parameter.
		/// </param>
		/// <returns></returns>
		[DllImport("shell32.dll")]
		public static extern UInt32 SHAppBarMessage(UInt32 dwMessage, ref APPBARDATA pData);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="lpString">Pointer to a null-terminated string that specifies the message to be registered.</param>
		/// <returns></returns>
		[DllImport("user32.dll")]
		public static extern UInt32 RegisterWindowMessage(
			[MarshalAs(UnmanagedType.LPTStr)]
			String lpString);

		#region Enums

		public enum AppBarMessages
		{
			/// <summary>
			/// Registers a new appbar and specifies the message identifier
			/// that the system should use to send notification messages to 
			/// the appbar. 
			/// </summary>
			New = 0x00000000,
			/// <summary>
			/// Unregisters an appbar, removing the bar from the system's 
			/// internal list.
			/// </summary>
			Remove = 0x00000001,
			/// <summary>
			/// Requests a size and screen position for an appbar.
			/// </summary>
			QueryPos = 0x00000002,
			/// <summary>
			/// Sets the size and screen position of an appbar. 
			/// </summary>
			SetPos = 0x00000003,
			/// <summary>
			/// Retrieves the autohide and always-on-top states of the 
			/// Microsoft® Windows® taskbar. 
			/// </summary>
			GetState = 0x00000004,
			/// <summary>
			/// Retrieves the bounding rectangle of the Windows taskbar. 
			/// </summary>
			GetTaskBarPos = 0x00000005,
			/// <summary>
			/// Notifies the system that an appbar has been activated. 
			/// </summary>
			Activate = 0x00000006,
			/// <summary>
			/// Retrieves the handle to the autohide appbar associated with
			/// a particular edge of the screen. 
			/// </summary>
			GetAutoHideBar = 0x00000007,
			/// <summary>
			/// Registers or unregisters an autohide appbar for an edge of 
			/// the screen. 
			/// </summary>
			SetAutoHideBar = 0x00000008,
			/// <summary>
			/// Notifies the system when an appbar's position has changed. 
			/// </summary>
			WindowPosChanged = 0x00000009,
			/// <summary>
			/// Sets the state of the appbar's autohide and always-on-top 
			/// attributes.
			/// </summary>
			SetState = 0x0000000a
		}

		public enum AppBarNotifications
		{
			/// <summary>
			/// Notifies an appbar that the taskbar's autohide or 
			/// always-on-top state has changed—that is, the user has selected 
			/// or cleared the "Always on top" or "Auto hide" check box on the
			/// taskbar's property sheet. 
			/// </summary>
			StateChange = 0x00000000,
			/// <summary>
			/// Notifies an appbar when an event has occurred that may affect 
			/// the appbar's size and position. Events include changes in the
			/// taskbar's size, position, and visibility state, as well as the
			/// addition, removal, or resizing of another appbar on the same 
			/// side of the screen.
			/// </summary>
			PosChanged = 0x00000001,
			/// <summary>
			/// Notifies an appbar when a full-screen application is opening or
			/// closing. This notification is sent in the form of an 
			/// application-defined message that is set by the ABM_NEW message. 
			/// </summary>
			FullScreenApp = 0x00000002,
			/// <summary>
			/// Notifies an appbar that the user has selected the Cascade, 
			/// Tile Horizontally, or Tile Vertically command from the 
			/// taskbar's shortcut menu.
			/// </summary>
			WindowArrange = 0x00000003
		}

		[Flags]
		public enum AppBarStates
		{
			AutoHide = 0x00000001,
			AlwaysOnTop = 0x00000002
		}

		public enum AppBarEdges
		{
			Left = 0,
			Top = 1,
			Right = 2,
			Bottom = 3,
			Float = 4
		}

		// Window Messages		
		public enum WM
		{
			ACTIVATE = 0x0006,
			WINDOWPOSCHANGED = 0x0047,
			NCHITTEST = 0x0084
		}

		public enum MousePositionCodes
		{
			HTERROR = (-2),
			HTTRANSPARENT = (-1),
			HTNOWHERE = 0,
			HTCLIENT = 1,
			HTCAPTION = 2,
			HTSYSMENU = 3,
			HTGROWBOX = 4,
			HTSIZE = HTGROWBOX,
			HTMENU = 5,
			HTHSCROLL = 6,
			HTVSCROLL = 7,
			HTMINBUTTON = 8,
			HTMAXBUTTON = 9,
			HTLEFT = 10,
			HTRIGHT = 11,
			HTTOP = 12,
			HTTOPLEFT = 13,
			HTTOPRIGHT = 14,
			HTBOTTOM = 15,
			HTBOTTOMLEFT = 16,
			HTBOTTOMRIGHT = 17,
			HTBORDER = 18,
			HTREDUCE = HTMINBUTTON,
			HTZOOM = HTMAXBUTTON,
			HTSIZEFIRST = HTLEFT,
			HTSIZELAST = HTBOTTOMRIGHT,
			HTOBJECT = 19,
			HTCLOSE = 20,
			HTHELP = 21
		}

		#endregion Enums

		#region AppBar Functions

		private Boolean AppbarNew()
		{
			if (CallbackMessageID == 0)
				throw new Exception("CallbackMessageID is 0");

			if (IsAppbarMode)
				return true;

			m_PrevSize = new Size(Width, Height);
			m_PrevLocation = new Point(Left, Top);

			// prepare data structure of message
			APPBARDATA msgData = new APPBARDATA();
			msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
			msgData.hWnd = new WindowInteropHelper(this).Handle;
			msgData.uCallbackMessage = CallbackMessageID;

			// install new appbar
			UInt32 retVal = SHAppBarMessage((UInt32)AppBarMessages.New, ref msgData);
			IsAppbarMode = (retVal != 0);

			SizeAppBar();

			Topmost = true;

			return IsAppbarMode;
		}

		private Boolean AppbarRemove()
		{
			// prepare data structure of message
			APPBARDATA msgData = new APPBARDATA();
			msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
			msgData.hWnd = new WindowInteropHelper(this).Handle;

			// remove appbar
			UInt32 retVal = SHAppBarMessage((UInt32)AppBarMessages.Remove, ref msgData);

			IsAppbarMode = false;

			Width = m_PrevSize.Width;
			Height = m_PrevSize.Height;

			Left = m_PrevLocation.X;
			Top = m_PrevLocation.Y;

			return (retVal != 0) ? true : false;
		}

		private void AppbarQueryPos(ref RECT appRect)
		{
			// prepare data structure of message
			APPBARDATA msgData = new APPBARDATA();
			msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
			msgData.hWnd = new WindowInteropHelper(this).Handle;
			msgData.uEdge = (UInt32)m_Edge;
			msgData.rc = appRect;

			// query postion for the appbar
			SHAppBarMessage((UInt32)AppBarMessages.QueryPos, ref msgData);
			appRect = msgData.rc;
		}

		private void AppbarSetPos(ref RECT appRect)
		{
			// prepare data structure of message
			APPBARDATA msgData = new APPBARDATA();
			msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
			msgData.hWnd = new WindowInteropHelper(this).Handle;
			msgData.uEdge = (UInt32)m_Edge;
			msgData.rc = appRect;

			// set postion for the appbar
			SHAppBarMessage((UInt32)AppBarMessages.SetPos, ref msgData);
			appRect = msgData.rc;
		}

		private void AppbarActivate()
		{
			// prepare data structure of message
			APPBARDATA msgData = new APPBARDATA();
			msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
			msgData.hWnd = new WindowInteropHelper(this).Handle;

			// send activate to the system
			SHAppBarMessage((UInt32)AppBarMessages.Activate, ref msgData);
		}

		private void AppbarWindowPosChanged()
		{
			// prepare data structure of message
			APPBARDATA msgData = new APPBARDATA();
			msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
			msgData.hWnd = new WindowInteropHelper(this).Handle;

			// send windowposchanged to the system 
			SHAppBarMessage((UInt32)AppBarMessages.WindowPosChanged, ref msgData);
		}

		private Boolean AppbarSetAutoHideBar(Boolean hideValue)
		{
			// prepare data structure of message
			APPBARDATA msgData = new APPBARDATA();
			msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
			msgData.hWnd = new WindowInteropHelper(this).Handle;
			msgData.uEdge = (UInt32)m_Edge;
			msgData.lParam = (hideValue) ? 1 : 0;

			// set auto hide
			UInt32 retVal = SHAppBarMessage((UInt32)AppBarMessages.SetAutoHideBar, ref msgData);
			return (retVal != 0) ? true : false;
		}

		private IntPtr AppbarGetAutoHideBar(AppBarEdges edge)
		{
			// prepare data structure of message
			APPBARDATA msgData = new APPBARDATA();
			msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
			msgData.uEdge = (UInt32)edge;

			// get auto hide
			IntPtr retVal = (IntPtr)SHAppBarMessage((UInt32)AppBarMessages.GetAutoHideBar, ref msgData);
			return retVal;
		}

		private AppBarStates AppbarGetTaskbarState()
		{
			// prepare data structure of message
			APPBARDATA msgData = new APPBARDATA();
			msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);

			// get taskbar state
			UInt32 retVal = SHAppBarMessage((UInt32)AppBarMessages.GetState, ref msgData);
			return (AppBarStates)retVal;
		}

		private void AppbarSetTaskbarState(AppBarStates state)
		{
			// prepare data structure of message
			APPBARDATA msgData = new APPBARDATA();
			msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
			msgData.lParam = (Int32)state;

			// set taskbar state
			SHAppBarMessage((UInt32)AppBarMessages.SetState, ref msgData);
		}

		private void AppbarGetTaskbarPos(out RECT taskRect)
		{
			// prepare data structure of message
			APPBARDATA msgData = new APPBARDATA();
			msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);

			// get taskbar position
			SHAppBarMessage((UInt32)AppBarMessages.GetTaskBarPos, ref msgData);
			taskRect = msgData.rc;
		}

		#endregion AppBar Functions

		#region Private Variables

		// saves the current edge
		private AppBarEdges m_Edge = AppBarEdges.Float;

		// saves the callback message id
		private UInt32 CallbackMessageID = 0;

		// are we in dock mode?
		private Boolean IsAppbarMode = false;

		// save the floating size and location
		private Size m_PrevSize;
		private Point m_PrevLocation;

		#endregion Private Variables

		private UInt32 RegisterCallbackMessage()
		{
			String uniqueMessageString = Guid.NewGuid().ToString();
			return RegisterWindowMessage(uniqueMessageString);
		}

		private void SizeAppBar()
		{
			RECT rt = new RECT();

			if ((m_Edge == AppBarEdges.Left) ||
				(m_Edge == AppBarEdges.Right))
			{
				rt.top = 0;
				rt.bottom = (int)SystemParameters.PrimaryScreenHeight;

				if (m_Edge == AppBarEdges.Left)
				{
					rt.right = (int)m_PrevSize.Width;
				}
				else
				{
					rt.right = (int)SystemParameters.PrimaryScreenWidth;
					rt.left = (int)(rt.right - m_PrevSize.Width);
				}
			}
			else
			{
				rt.left = 0;
				rt.right = (int)SystemParameters.PrimaryScreenWidth;

				if (m_Edge == AppBarEdges.Top)
				{
					rt.bottom = (int)m_PrevSize.Height;
				}
				else
				{
					rt.bottom = (int)SystemParameters.PrimaryScreenHeight;
					rt.top = (int)(rt.bottom - m_PrevSize.Height);
				}
			}

			AppbarQueryPos(ref rt);

			switch (m_Edge)
			{
				case AppBarEdges.Left:
					rt.right = (int)(rt.left + m_PrevSize.Width);
					break;
				case AppBarEdges.Right:
					rt.left = (int)(rt.right - m_PrevSize.Width);
					break;
				case AppBarEdges.Top:
					rt.bottom = (int)(rt.top + m_PrevSize.Height);
					break;
				case AppBarEdges.Bottom:
					rt.top = (int)(rt.bottom - m_PrevSize.Height);
					break;
			}

			AppbarSetPos(ref rt);

			Left = rt.left;
			Top = rt.top;

			Width = rt.right - rt.left;
			Height = rt.bottom - rt.top;
		}

		#region Message Handlers

		private IntPtr OnAppbarNotification(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			//AppBarStates state;
			AppBarNotifications msgType = (AppBarNotifications)wParam;

			switch (msgType)
			{
				case AppBarNotifications.PosChanged:
					SizeAppBar();
					break;

				//case AppBarNotifications.StateChange:
				//	state = AppbarGetTaskbarState();
				//	if ((state & AppBarStates.AlwaysOnTop) != 0)
				//		Topmost = true;
				//	else
				//		Topmost = false;
				//	break;

				case AppBarNotifications.FullScreenApp:
					if ((int)lParam != 0)
						Topmost = false;
					else
					{
						Topmost = true;
						AppbarActivate();
					}

					//if ((int)lParam != 0)
					//	Topmost = false;
					//else
					//{
					//	state = AppbarGetTaskbarState();
					//	if ((state & AppBarStates.AlwaysOnTop) != 0)
					//		Topmost = true;
					//	else
					//		Topmost = false;
					//}

					break;

				case AppBarNotifications.WindowArrange:
					if ((int)lParam != 0)	// before
						Visibility = Visibility.Collapsed;
					else						// after
						Visibility = Visibility.Visible;

					break;
			}

			return IntPtr.Zero;
		}

		#endregion Message Handlers

		#region Window Procedure

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (IsAppbarMode)
			{
				if (msg == CallbackMessageID)
					return OnAppbarNotification(hwnd, msg, wParam, lParam, ref handled);
				else if (msg == (int)WM.ACTIVATE)
					AppbarActivate();
				else if (msg == (int)WM.WINDOWPOSCHANGED)
					AppbarWindowPosChanged();
			}

			return IntPtr.Zero;
		}

		#endregion Window Procedure

		private void ApplicationDesktopToolbar_Loaded(object sender, RoutedEventArgs e)
		{
			m_PrevSize = new Size(Width, Height);
			m_PrevLocation = new Point(Left, Top);

			HwndSource.FromHwnd(new WindowInteropHelper(this).Handle).AddHook(WndProc);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			Visibility = Visibility.Collapsed;

			AppbarRemove();
			HwndSource.FromHwnd(new WindowInteropHelper(this).Handle).RemoveHook(WndProc);

			base.OnClosing(e);
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			if (IsAppbarMode)
			{
				if (m_Edge == AppBarEdges.Top || m_Edge == AppBarEdges.Bottom)
					m_PrevSize.Height = ActualHeight;
				else
					m_PrevSize.Width = ActualWidth;

				SizeAppBar();
			}

			base.OnRenderSizeChanged(sizeInfo);
		}
		
		private ShadowController _shadowController = null;

		public AppBarEdges Edge
		{
			get
			{
				return m_Edge;
			}
			set
			{
				m_Edge = value;

				if (value == AppBarEdges.Float)
					AppbarRemove();
				else
					AppbarNew();

				if (IsAppbarMode)
					SizeAppBar();


				if (_shadowController != null)
					_shadowController.Close();

				if (ResizeMode == ResizeMode.CanMinimize || ResizeMode == ResizeMode.NoResize)
					WindowChrome.GetWindowChrome(this).ResizeBorderThickness = new Thickness(0);
				else
				{
					Thickness resizeThickness;

					switch (Edge)
					{
						case AppBarEdges.Bottom:
							resizeThickness = new Thickness(0, 1, 0, 0);
							_shadowController = new ShadowController(this, Direction.Top);
							break;

						case AppBarEdges.Float:
						default:
							resizeThickness = new Thickness(1);
							_shadowController = new ShadowController(this);
							break;

						case AppBarEdges.Left:
							resizeThickness = new Thickness(0, 0, 1, 0);
							_shadowController = new ShadowController(this, Direction.Right);
							break;

						case AppBarEdges.Right:
							resizeThickness = new Thickness(1, 0, 0, 0);
							_shadowController = new ShadowController(this, Direction.Left);
							break;

						case AppBarEdges.Top:
							resizeThickness = new Thickness(0, 0, 0, 1);
							_shadowController = new ShadowController(this, Direction.Bottom);
							break;
					}

					WindowChrome.GetWindowChrome(this).ResizeBorderThickness = resizeThickness;
				}
			}
		}

		#endregion
	}
}
