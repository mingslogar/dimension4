using Daytimer.Fundamentals;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using WikiquoteScreensaverLib.IO.WebIO;

namespace Daytimer.WikiQuoteHelper
{
	/// <summary>
	/// Interaction logic for DisambiguationWindow.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class DisambiguationWindow : OfficeWindow
	{
		public DisambiguationWindow(IEnumerable<TopicChoice> topicChoices, string topic)
		{
			InitializeComponent();

			listBox.ItemsSource = topicChoices;
			title.Text = "The topic you requested, " + topic.ToString() + ", is a little ambiguous. Choose one of the topics listed below.";
		}

		public TopicChoice SelectedTopic
		{
			get { return (TopicChoice)listBox.SelectedItem; }
		}

		private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			DialogResult = true;
		}

		private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
		{
			okButton.IsEnabled = true;
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}
