using Daytimer.Dialogs;
using System;
using System.Windows;

namespace Daytimer.Controls.Panes.Notes
{
	/// <summary>
	/// Interaction logic for ChangeNoteDateDialog.xaml
	/// </summary>
	public partial class ChangeNoteDateDialog : DialogBase
	{
		public ChangeNoteDateDialog()
		{
			InitializeComponent();
		}

		public DateTime SelectedDate
		{
			get { return dateDropDown.SelectedDate.Value; }
			set { dateDropDown.SelectedDate = value.Date; }
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}
