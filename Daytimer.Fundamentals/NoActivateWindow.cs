using Daytimer.Fundamentals.Win32;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Daytimer.Fundamentals
{
	[ComVisible(false)]
	public class NoActivateWindow : Window
	{
		public NoActivateWindow()
		{

		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
			//HwndSource.FromHwnd((new WindowInteropHelper(this)).Handle).AddHook(new HwndSourceHook(WndProc));

			HwndSource source = (HwndSource)PresentationSource.FromVisual(this);
			WS ws = source.Handle.GetWindowLong();
			WSEX wsex = source.Handle.GetWindowLongEx();

			ws |= WS.POPUP;
			//wsex ^= WSEX.APPWINDOW;
			wsex |= WSEX.NOACTIVATE;

			source.Handle.SetWindowLong(ws);
			source.Handle.SetWindowLongEx(wsex);
			source.AddHook(WndProc);
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			switch (msg)
			{
				case (int)WM.ACTIVATE:
				case (int)WM.NCACTIVATE:
					handled = true;
					return new IntPtr(3);

				case (int)WM.MOUSEACTIVATE:
					handled = true;
					return new IntPtr(3);
			}

			return IntPtr.Zero;
		}
	}
}
