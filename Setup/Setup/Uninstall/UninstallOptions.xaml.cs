using System.Windows;
using System.Windows.Controls;

namespace Setup.Uninstall
{
	/// <summary>
	/// Interaction logic for UninstallOptions.xaml
	/// </summary>
	public partial class UninstallOptions : UserControl
	{
		public UninstallOptions()
		{
			InitializeComponent();
		}

		private void selectAll_Click(object sender, RoutedEventArgs e)
		{
			deleteSettings.IsChecked
				= deleteAccounts.IsChecked
				= deleteDatabase.IsChecked
				= deleteDictionary.IsChecked
				= selectAll.IsChecked;
		}

		private void optionCheckBox_Click(object sender, RoutedEventArgs e)
		{
			bool check = (sender as CheckBox).IsChecked == true;

			if (!check)
				selectAll.IsChecked = false;
			else
			{
				if (deleteSettings.IsChecked == true &&
					deleteAccounts.IsChecked == true &&
					deleteDatabase.IsChecked == true &&
					deleteDictionary.IsChecked == true)
					selectAll.IsChecked = true;
			}
		}
	}
}
