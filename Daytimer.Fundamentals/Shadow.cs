using Daytimer.Fundamentals.Win32;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace Daytimer.Fundamentals
{
	[ComVisible(false)]
	[TemplatePart(Name = Shadow.BorderName, Type = typeof(Border))]
	public class Shadow : Window
	{
		#region Constructors

		static Shadow()
		{
			Type ownerType = typeof(Shadow);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
		}

		public Shadow(Direction dir)
		{
			ResizeMode = ResizeMode.NoResize;
			WindowStyle = WindowStyle.None;
			AllowsTransparency = true;

			_dir = dir;

			switch (_dir)
			{
				case Direction.Left:
					Width = 8;
					getHitTestValue = p => new Rect(0, 0, ActualWidth, _cornerTolerance).Contains(p)
						? HitTestValues.HTTOPLEFT
						: new Rect(0, ActualHeight - _cornerTolerance, ActualWidth, _cornerTolerance).Contains(p)
						? HitTestValues.HTBOTTOMLEFT
						: HitTestValues.HTLEFT;
					break;

				case Direction.Top:
					Height = 8;
					getHitTestValue = p => new Rect(0, 0, _cornerTolerance - glowSize, ActualHeight).Contains(p)
						? HitTestValues.HTTOPLEFT
						: new Rect(Width - _cornerTolerance + glowSize, 0, _cornerTolerance - glowSize,
							ActualHeight).Contains(p)
							? HitTestValues.HTTOPRIGHT
							: HitTestValues.HTTOP;
					break;

				case Direction.Right:
					Width = 8;
					getHitTestValue = p => new Rect(0, 0, ActualWidth, _cornerTolerance).Contains(p)
						? HitTestValues.HTTOPRIGHT
						: new Rect(0, ActualHeight - _cornerTolerance, ActualWidth, _cornerTolerance).Contains(p)
						? HitTestValues.HTBOTTOMRIGHT
						: HitTestValues.HTRIGHT;
					break;

				case Direction.Bottom:
					Height = 8;
					getHitTestValue = p => new Rect(0, 0, _cornerTolerance - glowSize, ActualHeight).Contains(p)
						? HitTestValues.HTBOTTOMLEFT
						: new Rect(Width - _cornerTolerance + glowSize, 0, _cornerTolerance - glowSize,
							ActualHeight).Contains(p)
							? HitTestValues.HTBOTTOMRIGHT
							: HitTestValues.HTBOTTOM;
					break;
			}
		}

		#endregion

		#region Fields

		private const string BorderName = "PART_Glow";

		#endregion

		#region Internal Fields

		internal Border Glow;

		#endregion

		#region Public Override Methods

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			Glow = GetTemplateChild(BorderName) as Border;

			switch (_dir)
			{
				case Direction.Left:
					Glow.Margin = new Thickness(8, 8, -8, 8);
					break;

				case Direction.Top:
					Glow.Margin = new Thickness(8, 8, 8, -8);
					break;

				case Direction.Right:
					Glow.Margin = new Thickness(-8, 8, 8, 8);
					break;

				case Direction.Bottom:
					Glow.Margin = new Thickness(8, -8, 8, 8);
					break;
			}
		}

		#endregion

		#region Resize

		private Direction _dir;
		//private Point _mouseOffset;
		private const int _cornerTolerance = 8;//15;

		private const double glowSize = 8.0;
		private readonly Func<Point, HitTestValues> getHitTestValue;

		private Cursor GetCursor(Point mousePos)
		{
			switch (_dir)
			{
				case Direction.Left:
					if (mousePos.Y < _cornerTolerance)
						return Cursors.SizeNWSE;
					else if (mousePos.Y > ActualHeight - _cornerTolerance)
						return Cursors.SizeNESW;
					else
						return Cursors.SizeWE;

				case Direction.Top:
					if (mousePos.X < _cornerTolerance)
						return Cursors.SizeNWSE;
					else if (mousePos.X > ActualWidth - _cornerTolerance)
						return Cursors.SizeNESW;
					else
						return Cursors.SizeNS;

				case Direction.Right:
					if (mousePos.Y < _cornerTolerance)
						return Cursors.SizeNESW;
					else if (mousePos.Y > ActualHeight - _cornerTolerance)
						return Cursors.SizeNWSE;
					else
						return Cursors.SizeWE;

				case Direction.Bottom:
					if (mousePos.X < _cornerTolerance)
						return Cursors.SizeNESW;
					else if (mousePos.X > ActualWidth - _cornerTolerance)
						return Cursors.SizeNWSE;
					else
						return Cursors.SizeNS;
			}

			return Cursors.Arrow;
		}

		#endregion

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
			HwndSource.FromHwnd((new WindowInteropHelper(this)).Handle).AddHook(new HwndSourceHook(WndProc));

			//HwndSource source = (HwndSource)PresentationSource.FromVisual(this);
			////WS ws = source.Handle.GetWindowLong();
			//WSEX wsex = source.Handle.GetWindowLongEx();

			////ws |= WS.POPUP;
			////wsex ^= WSEX.APPWINDOW;
			//wsex |= WSEX.NOACTIVATE;

			////source.Handle.SetWindowLong(ws);
			//source.Handle.SetWindowLongEx(wsex);
			//source.AddHook(WndProc);
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			switch ((WM)msg)
			{
				case WM.MOUSEACTIVATE:
					{
						handled = true;
						return new IntPtr(3);
					}

				case WM.LBUTTONDOWN:
					{
						if (!IsHitTestVisible)
							break;

						//Point ptScreen = new Point((int)lParam & 0xFFFF, ((int)lParam >> 16) & 0xFFFF);
						Point ptScreen = GetScreenPosition(lParam);
						NativeMethods.PostMessage(new WindowInteropHelper(Owner).Handle,
							(uint)WM.NCLBUTTONDOWN, (IntPtr)getHitTestValue(ptScreen),
							IntPtr.Zero);
					}
					break;

				case WM.GETMINMAXINFO:
					{
						MINMAXINFO obj = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

						if (obj.ptMaxSize.x > 0)
						{
							obj.ptMaxSize.x = obj.ptMaxSize.y =
								obj.ptMaxTrackSize.x = obj.ptMaxTrackSize.y = int.MaxValue;
							Marshal.StructureToPtr(obj, lParam, true);
						}
					}
					break;

				case WM.NCHITTEST:
					{
						//Point ptScreen = new Point((int)lParam & 0xFFFF, ((int)lParam >> 16) & 0xFFFF);
						Point ptScreen = GetScreenPosition(lParam);
						Cursor = GetCursor(PointFromScreen(ptScreen));
					}
					break;
			}

			return IntPtr.Zero;
		}

		private Point GetScreenPosition(IntPtr lParam)
		{
			int x = ((int)(short)((ushort)(((ulong)(lParam)) & 0xffff)));
			int y = ((int)(short)((ushort)((((ulong)(lParam)) >> 16) & 0xffff)));

			return new Point(x, y);
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct POINT
		{
			/// <summary>
			/// x coordinate of point.
			/// </summary>
			public int x;

			/// <summary>
			/// y coordinate of point.
			/// </summary>
			public int y;

			/// <summary>
			/// Construct a point of coordinates (x,y).
			/// </summary>
			public POINT(int x, int y)
			{
				this.x = x;
				this.y = y;
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct MINMAXINFO
		{
			public POINT ptReserved;
			public POINT ptMaxSize;
			public POINT ptMaxPosition;
			public POINT ptMinTrackSize;
			public POINT ptMaxTrackSize;
		};
	}

	public enum Direction : byte { Left, Top, Right, Bottom };
}
