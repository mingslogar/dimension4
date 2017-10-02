using Daytimer.DatabaseHelpers.Sync;
using Daytimer.Dialogs;
using Daytimer.GoogleCalendarHelpers;
using Microsoft.Win32;
using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Daytimer.Controls.Ribbon
{
	/// <summary>
	/// Interaction logic for AccountsControl.xaml
	/// </summary>
	public partial class AccountsControl : Grid
	{
		public AccountsControl()
		{
			InitializeComponent();

			if (!NetworkInterface.GetIsNetworkAvailable())
			{
				noInternetMsg.Visibility = Visibility.Visible;
				servicesPanel.IsEnabled = addServiceButton.IsEnabled = false;
			}

			NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;

			Loaded += AccountsControl_Loaded;
		}

		private bool _isFirstTime = true;

		private void AccountsControl_Loaded(object sender, RoutedEventArgs e)
		{
			if (!_isFirstTime)
				return;

			_isFirstTime = false;

			Load();
		}

		private void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
		{
			Dispatcher.Invoke(() =>
			{
				if (!e.IsAvailable)
				{
					noInternetMsg.Visibility = Visibility.Visible;
					servicesPanel.IsEnabled = addServiceButton.IsEnabled = false;
				}
				else
				{
					noInternetMsg.Visibility = Visibility.Collapsed;
					servicesPanel.IsEnabled = addServiceButton.IsEnabled = true;
				}
			});
		}

		private void Load()
		{
			Task.Factory.StartNew(() =>
			{
				Thread.Sleep(500);

				try
				{
					RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\" + GlobalAssemblyInfo.AssemblyName + @"\Accounts");
					string[] accounts = key.GetSubKeyNames();

					foreach (string each in accounts)
					{
						string[] subAccounts = key.OpenSubKey(each).GetSubKeyNames();

						foreach (string sub in subAccounts)
						{
							Dispatcher.Invoke(() =>
							{
								AccountDisplay display = new AccountDisplay(new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/google.png", UriKind.Absolute)), each, sub);
								servicesPanel.Children.Add(display);
							});
						}
					}
				}
				catch { }
			});
		}

		private void addServiceButton_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			TaskDialog dlg = new TaskDialog(Window.GetWindow(this), "Add Service",
				"This feature is still in beta. Data stored on both local and remote servers, including data not associated with Dimension 4, may become corrupted or otherwise unusable. Continue at your own risk.",
				MessageType.Exclamation, true);

			if (dlg.ShowDialog() == false)
				return;

			if (e.AddedItems[0] == googleService)
				AddGoogleService();
		}

		private void AddGoogleService()
		{
			GoogleSignIn gSignIn = new GoogleSignIn();
			gSignIn.Owner = Window.GetWindow(this);

			if (gSignIn.ShowDialog() == true)
			{
				AccountDisplay display = new AccountDisplay(new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/google.png", UriKind.Absolute)), "Google", gSignIn.Email);
				servicesPanel.Children.Add(display);

				if (SyncDatabase.GetSyncObject(gSignIn.Email) == null)
					SyncDatabase.Add(new SyncObject(gSignIn.Email, DateTime.Now, SyncType.EntireAccount));
			}
		}
	}
}
