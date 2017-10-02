using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace Demo.Main
{
	/// <summary>
	/// Interaction logic for Message.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class FixedMessageContainer : Border
	{
		public FixedMessageContainer(string title, string message, string buttonContent)
		{
			InitializeComponent();

			Message msg = new Message(title, message, buttonContent);
			msg.Next += (sender, e) => { RaiseNextEvent(); };
			innerBorder.Child = msg;
		}

		#region Routed Events

		public static readonly RoutedEvent NextEvent = EventManager.RegisterRoutedEvent(
			"Next", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FixedMessageContainer));

		public event RoutedEventHandler Next
		{
			add { AddHandler(NextEvent, value); }
			remove { RemoveHandler(NextEvent, value); }
		}

		internal void RaiseNextEvent()
		{
			RaiseEvent(new RoutedEventArgs(NextEvent));
		}

		#endregion
	}
}
