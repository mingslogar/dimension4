using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Daytimer.Functions
{
	public class MonitorHelper
	{
		/// <summary>
		/// Get the size and location of the monitor working area containing
		/// the largest portion of a specified window.
		/// </summary>
		/// <param name="window">The window to find a monitor for.</param>
		/// <exception cref="System.InvalidOperationException" />
		/// <returns></returns>
		public static Rect MonitorWorkingAreaFromWindow(Window window)
		{
			IntPtr monitorPtr = NativeMethods.MonitorFromWindow((new WindowInteropHelper(window)).Handle, NativeMethods.MONITOR_DEFAULTTONEAREST);
			if (monitorPtr != IntPtr.Zero)
			{
				NativeMethods.MONITORINFOEX monitorInfo = new NativeMethods.MONITORINFOEX();

				NativeMethods.GetMonitorInfo(new HandleRef(null, monitorPtr), monitorInfo);
				NativeMethods.RECT rect = monitorInfo.rcWork;

				return new Rect(rect.left, rect.top, rect.Width, rect.Height);
			}
			else
				throw (new InvalidOperationException("Window does not have an associated monitor."));
		}

		/// <summary>
		/// Get the size and location of the monitor working area containing
		/// the largest portion of a specified <see cref="System.Windows.Window"/>.
		/// </summary>
		/// <param name="window">The window to find a monitor for.</param>
		/// <exception cref="System.InvalidOperationException" />
		/// <returns></returns>
		public static Rect MonitorFromWindow(Window window)
		{
			IntPtr monitorPtr = NativeMethods.MonitorFromWindow((new WindowInteropHelper(window)).Handle, NativeMethods.MONITOR_DEFAULTTONEAREST);
			if (monitorPtr != IntPtr.Zero)
			{
				NativeMethods.MONITORINFOEX monitorInfo = new NativeMethods.MONITORINFOEX();

				NativeMethods.GetMonitorInfo(new HandleRef(null, monitorPtr), monitorInfo);
				NativeMethods.RECT rect = monitorInfo.rcMonitor;

				return new Rect(rect.left, rect.top, rect.Width, rect.Height);
			}
			else
				throw (new InvalidOperationException("Window does not have an associated monitor."));
		}

		/// <summary>
		/// Get the size and location of the monitor working area containing
		/// the largest portion of a specified <see cref="System.Windows.Rect"/>.
		/// </summary>
		/// <param name="window">The window to find a monitor for.</param>
		/// <exception cref="System.InvalidOperationException" />
		/// <returns></returns>
		public static Rect MonitorWorkingAreaFromRect(Rect r)
		{
			NativeMethods.RECT rc = new NativeMethods.RECT();
			rc.left = (int)r.Left;
			rc.top = (int)r.Top;
			rc.right = (int)r.Right;
			rc.bottom = (int)r.Bottom;

			IntPtr monitorPtr = NativeMethods.MonitorFromRect(ref rc, NativeMethods.MONITOR_DEFAULTTONEAREST);
			if (monitorPtr != IntPtr.Zero)
			{
				NativeMethods.MONITORINFOEX monitorInfo = new NativeMethods.MONITORINFOEX();

				NativeMethods.GetMonitorInfo(new HandleRef(null, monitorPtr), monitorInfo);
				NativeMethods.RECT rect = monitorInfo.rcWork;

				return new Rect(rect.left, rect.top, rect.Width, rect.Height);
			}
			else
				throw (new InvalidOperationException("Rect does not have an associated monitor."));
		}

		/// <summary>
		/// Get the size and location of the primary monitor working area.
		/// </summary>
		/// <exception cref="System.InvalidOperationException" />
		/// <returns></returns>
		public static Rect PrimaryMonitorWorkingArea()
		{
			return MonitorWorkingAreaFromRect(new Rect(0, 0, 0, 0));
		}
	}
}
