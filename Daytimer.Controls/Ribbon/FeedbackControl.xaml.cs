using System.Windows;
using System.Windows.Controls;

namespace Daytimer.Controls.Ribbon
{
	/// <summary>
	/// Interaction logic for FeedbackControl.xaml
	/// </summary>
	public partial class FeedbackControl : Grid
	{
		public FeedbackControl()
		{
			InitializeComponent();
		}

		private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			CommonActions.TabControl_SelectionChanged(sender, e);
		}

		private void sendSmile_Click(object sender, RoutedEventArgs e)
		{
			Feedback feedback = new Feedback(FeedbackType.Smile);
			feedback.Owner = Application.Current.MainWindow;
			feedback.ShowDialog();
		}

		private void sendFrown_Click(object sender, RoutedEventArgs e)
		{
			Feedback feedback = new Feedback(FeedbackType.Frown);
			feedback.Owner = Application.Current.MainWindow;
			feedback.ShowDialog();
		}
	}
}
