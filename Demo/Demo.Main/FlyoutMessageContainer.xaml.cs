using Daytimer.Fundamentals;
using System.Runtime.InteropServices;
using System.Windows;

namespace Demo.Main
{
	/// <summary>
	/// Interaction logic for FlyoutMessageContainer.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class FlyoutMessageContainer : BalloonTip
	{
		public FlyoutMessageContainer(UIElement ownerControl, string title, string message, string buttonContent)
			: base(ownerControl)
		{
			InitializeComponent();

			Message msg = new Message(title, message, buttonContent);
			msg.Next += (sender, e) => { RaiseNextEvent(); };
			Content = msg;
		}

		#region Routed Events

		public static readonly RoutedEvent NextEvent = EventManager.RegisterRoutedEvent(
			"Next", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FlyoutMessageContainer));

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
