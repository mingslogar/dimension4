using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace Demo.Main
{
	/// <summary>
	/// Interaction logic for Message.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class Message : Grid
	{
		public Message(string title, string message, string buttonContent)
		{
			InitializeComponent();
			this.title.Text = title;
			this.message.Text = message;
			this.nextButton.Content = buttonContent;
		}

		private void nextButton_Click(object sender, RoutedEventArgs e)
		{
			RaiseNextEvent();
		}

		private void closeButton_Click(object sender, RoutedEventArgs e)
		{
			Driver.Exit();
		}

		#region Routed Events

		public static readonly RoutedEvent NextEvent = EventManager.RegisterRoutedEvent(
			"Next", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Message));

		public event RoutedEventHandler Next
		{
			add { AddHandler(NextEvent, value); }
			remove { RemoveHandler(NextEvent, value); }
		}

		private void RaiseNextEvent()
		{
			RaiseEvent(new RoutedEventArgs(NextEvent));
		}

		#endregion
	}
}
