using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace Daytimer.Controls.Panes.Calendar
{
	public delegate void ReminderChangedEventHandler(object sender, ReminderChangedEventArgs e);

	[ComVisible(false)]
	public class ReminderChangedEventArgs : RoutedEventArgs
	{
		public ReminderChangedEventArgs(RoutedEvent routedEvent, TimeSpan? reminder)
			: base(routedEvent)
		{
			Reminder = reminder;
		}

		public TimeSpan? Reminder
		{
			get;
			private set;
		}
	}
}
