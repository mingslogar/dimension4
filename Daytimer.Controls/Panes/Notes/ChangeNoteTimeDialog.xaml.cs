using Daytimer.Dialogs;
using System;
using System.Windows;

namespace Daytimer.Controls.Panes.Notes
{
	/// <summary>
	/// Interaction logic for ChangeNoteTimeDialog.xaml
	/// </summary>
	public partial class ChangeNoteTimeDialog : DialogBase
	{
		public ChangeNoteTimeDialog()
		{
			InitializeComponent();
		}

		public TimeSpan SelectedTime
		{
			get { return TimeSpan.Parse(timeDropDown.TextDisplay); }
			set { timeDropDown.TextDisplay = string.Format("{0:00}:{1:00}", value.Hours, value.Minutes); }
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}
