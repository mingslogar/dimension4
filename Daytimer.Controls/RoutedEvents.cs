using System.Runtime.InteropServices;
using System.Windows;

namespace Daytimer.Controls
{
	public delegate void UpdateStatusEventHandler(object sender, UpdateStatusEventArgs e);

	[ComVisible(false)]
	public class UpdateStatusEventArgs : RoutedEventArgs
	{
		public UpdateStatusEventArgs(RoutedEvent re, string status)
			: base(re)
		{
			_status = status;
		}

		private string _status;

		public string Status
		{
			get { return _status; }
		}
	}
}
