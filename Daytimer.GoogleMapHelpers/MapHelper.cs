using System.Diagnostics;

namespace Daytimer.GoogleMapHelpers
{
	public class MapHelper
	{
		public static void ShowDirections(string destination)
		{
			OpenBrowser("http://maps.google.com/?daddr=" + destination.Trim());
		}

		public static void ShowDirections(string destination, string origin)
		{
			OpenBrowser("http://maps.google.com/?saddr=" + origin.Trim() + "&daddr=" + destination.Trim());
		}

		private static void OpenBrowser(string url)
		{
			Process.Start(url);
		}
	}
}
