using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Daytimer.Help
{
	/// <summary>
	/// Interaction logic for ViewerControl.xaml
	/// </summary>
	public partial class ViewerControl : Grid
	{
		public ViewerControl()
		{
			InitializeComponent();
			Loaded += ViewerControl_Loaded;
		}

		private void ViewerControl_Loaded(object sender, RoutedEventArgs e)
		{
			NavigationCommands.BrowseHome.Execute(null, this);
		}

		#region Global Variables

		#endregion

		#region Functions

		public void Navigate(Uri uri)
		{
			webBrowser.Navigate(uri);
		}

		#endregion

		#region Commands

		private static void ExecutedHomeCommand(object sender, ExecutedRoutedEventArgs e)
		{
			(sender as ViewerControl).Navigate(new Uri("http://localhost:" + StaticData.Port.ToString() + "/", UriKind.Absolute));
		}

		private static void ExecutedPrintCommand(object sender, ExecutedRoutedEventArgs e)
		{
			(sender as ViewerControl).webBrowser.InvokeScript("print");
		}

		static ViewerControl()
		{
			Type ownerType = typeof(ViewerControl);

			CommandBinding home = new CommandBinding(NavigationCommands.BrowseHome, ExecutedHomeCommand);
			CommandBinding print = new CommandBinding(ApplicationCommands.Print, ExecutedPrintCommand);

			CommandManager.RegisterClassCommandBinding(ownerType, home);
			CommandManager.RegisterClassCommandBinding(ownerType, print);
		}

		#endregion

		#region UI

		private void largeTextButton_Click(object sender, RoutedEventArgs e)
		{
			if (largeTextButton.IsChecked == true)
				webBrowser.Zoom = 150;
			else
				webBrowser.Zoom = 100;
		}
		
		private void searchBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				if (string.IsNullOrWhiteSpace(searchBox.Text))
					return;

				Navigate(new Uri("http://localhost:" + StaticData.Port.ToString() + "/search?s=" + searchBox.Text, UriKind.Absolute));
			}
		}

		#endregion
	}
}
