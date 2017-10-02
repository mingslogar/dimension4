using System.Runtime.InteropServices;
using System.Windows;

namespace Modern.FileBrowser
{
	public delegate void NavigateEventHandler(object sender, NavigateEventArgs e);

	[ComVisible(false)]
	public class NavigateEventArgs : RoutedEventArgs
	{
		public NavigateEventArgs(RoutedEvent routedEvent, string location)
			: base(routedEvent)
		{
			Location = location;
		}

		public string Location
		{
			get;
			private set;
		}
	}
}
