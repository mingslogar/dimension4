using Daytimer.DatabaseHelpers.Note;
using Daytimer.Fundamentals;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Daytimer.Controls.Panes.Notes
{
	/// <summary>
	/// Interaction logic for HiddenTabsFlyout.xaml
	/// </summary>
	public partial class HiddenTabsFlyout : BalloonTip
	{
		public HiddenTabsFlyout(UIElement ownerControl, IEnumerable tabsSource)
			: base(ownerControl)
		{
			InitializeComponent();

			PositionOrder = new PositionOrder(Location.Bottom, Location.Left, Location.Top, Location.Right, Location.Right);
			Owner = Window.GetWindow(ownerControl);

			ownerControl.MouseEnter += ownerControl_MouseEnter;
			ownerControl.MouseLeave += ownerControl_MouseLeave;

			listBox.ItemsSource = tabsSource;
			Show();
		}

		private void ownerControl_MouseEnter(object sender, MouseEventArgs e)
		{
			ResetTimer();
		}

		private void ownerControl_MouseLeave(object sender, MouseEventArgs e)
		{
			Close();
		}

		private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			RaiseNavigateEvent();
			FastClose();
		}

		public NotebookSection SelectedSection
		{
			get { return (NotebookSection)listBox.SelectedItem; }
		}

		#region RoutedEvents

		public static readonly RoutedEvent NavigateEvent = EventManager.RegisterRoutedEvent(
			"Navigate", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HiddenTabsFlyout));

		public event RoutedEventHandler Navigate
		{
			add { AddHandler(NavigateEvent, value); }
			remove { RemoveHandler(NavigateEvent, value); }
		}

		private void RaiseNavigateEvent()
		{
			RaiseEvent(new RoutedEventArgs(NavigateEvent));
		}

		#endregion
	}
}
