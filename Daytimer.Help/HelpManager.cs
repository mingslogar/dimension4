using Bend.Util;
using Daytimer.Help.Server;
using System;
using System.Threading;
using System.Windows;

namespace Daytimer.Help
{
	public class HelpManager
	{
		private static HttpServer httpServer;
		private static Thread serveThread;

		public static void ShowHelp()
		{
			if (httpServer == null)
			{
				httpServer = new HttpResourceServer();
				serveThread = new Thread(httpServer.Listen);
				serveThread.IsBackground = true;
				serveThread.Start();
			}

			if (HelpWindow == null)
			{
				HelpViewer hlp = new HelpViewer();
				HelpWindow = hlp;

				hlp.Closed += hlp_Closed;
				hlp.Show();
			}
			else
			{
				Window _helpWindow = HelpWindow as Window;

				if (_helpWindow.WindowState == WindowState.Minimized)
					_helpWindow.WindowState = WindowState.Normal;

				_helpWindow.Activate();
			}
		}

		private static void hlp_Closed(object sender, EventArgs e)
		{
			(sender as Window).Closed -= hlp_Closed;
			serveThread.Abort();
			serveThread = null;
			httpServer.Shutdown();
			httpServer = null;
			HelpWindow = null;
		}

		private static object HelpWindow = null;
	}
}
