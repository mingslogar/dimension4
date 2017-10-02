using System;
using System.Runtime.InteropServices;
using System.Security;

namespace MahApps.Metro.Native
{
	/// <devdoc>http://msdn.microsoft.com/en-us/library/ms182161.aspx</devdoc>
	[SuppressUnmanagedCodeSecurity]
	public static class UnsafeNativeMethods
	{
		/// <devdoc>http://msdn.microsoft.com/en-us/library/dd144901%28v=VS.85%29.aspx</devdoc>
		[DllImport("user32", EntryPoint = "GetMonitorInfoW", ExactSpelling = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetMonitorInfo([In] IntPtr hMonitor, [Out] MONITORINFO lpmi);

		/// <devdoc>http://msdn.microsoft.com/en-us/library/dd145064%28v=VS.85%29.aspx</devdoc>
		[DllImport("user32")]
		public static extern IntPtr MonitorFromWindow([In] IntPtr handle, [In] int flags);

		/// <devdoc>http://msdn.microsoft.com/en-us/library/windows/desktop/ms633545(v=vs.85).aspx</devdoc>
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
	}
}