using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Daytimer.Toasts
{
	public static class NativeMethods
	{
		[DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
		public static extern int GetDoubleClickTime();

		[DllImport("dwmapi.dll", PreserveSig = false)]
		public static extern void DwmIsCompositionEnabled([Out, MarshalAs(UnmanagedType.Bool)] out bool pfEnabled);

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int left;
			public int top;
			public int right;
			public int bottom;

			public int Width
			{
				get { return this.right - this.left; }
			}

			public int Height
			{
				get { return this.bottom - this.top; }
			}
		}

		[DllImport("user32.dll", ExactSpelling = true)]
		public static extern IntPtr MonitorFromRect(ref RECT rect, int flags);

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
		public class MONITORINFOEX
		{
			internal int cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
			public RECT rcMonitor = new RECT();
			public RECT rcWork = new RECT();
			internal int dwFlags;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
			internal char[] szDevice = new char[0x20];
		}

		[DllImport("user32.dll", EntryPoint = "GetMonitorInfo", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetMonitorInfo(HandleRef hmonitor, [In, Out] MONITORINFOEX info);

		public const int MONITOR_DEFAULTTONEAREST = 2;

		[DllImport("user32.dll", EntryPoint = "MonitorFromWindow", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

		private const int WmPaint = 0x000F;

		[DllImport("User32.dll")]
		public static extern Int64 SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		public static void ForcePaint(Window window)
		{
			SendMessage(new WindowInteropHelper(window).Handle, WmPaint, IntPtr.Zero, IntPtr.Zero);
		}
	}
}
