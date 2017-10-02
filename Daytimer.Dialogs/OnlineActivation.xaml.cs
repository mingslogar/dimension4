using Daytimer.Functions;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Daytimer.Dialogs
{
	/// <summary>
	/// Interaction logic for OnlineActivation.xaml
	/// </summary>
	public partial class OnlineActivation : DialogBase
	{
		public OnlineActivation()
		{
			InitializeComponent();
		}

		private void ActivateOnline()
		{
			message.Text = "Contacting licensing server...";
			retryButton.Visibility = Visibility.Collapsed;
			closeButton.Visibility = Visibility.Collapsed;
			changeProductKey.IsEnabled = false;

			animation.Reset();

			Thread activate = new Thread(activateOnlineWorker);
			activate.Start();
		}

		private void activateOnlineWorker()
		{
			try
			{
				Activation.ActivateOnline();
				Dispatcher.Invoke(() =>
				{
					ShowActivateResult(true, null);
				});
			}
			catch (Exception exc)
			{
				Dispatcher.Invoke(() =>
				{
					ShowActivateResult(false, exc.Message);
				});
			}
		}

		private delegate void ShowActivateResultDelegate(bool value);

		private void ShowActivateResult(bool value, string msg)
		{
			if (value)
			{
				// If on Vista or newer, we need to elevate to activate.
				if (Environment.OSVersion.Version >= OSVersions.Win_Vista)
				{
					IsEnabled = false;

					try
					{
						ProcessStartInfo pInfo = new ProcessStartInfo(Process.GetCurrentProcess().MainModule.FileName);
						pInfo.Verb = "runas";

						string key = Activation.Key;
						pInfo.Arguments = "/activate " +
							Convert.ToBase64String(Encryption.EncryptStringToBytes(key, SecurityKeys.GenerateKey(key), SecurityKeys.GenerateIV(key)));

						Process proc = Process.Start(pInfo);
						proc.EnableRaisingEvents = true;
						proc.Exited += proc_Exited;
					}
					catch (Win32Exception)
					{
						Thread deActivate = new Thread(() =>
						{
							try
							{
								Activation.Free(Activation.Key);
							}
							catch { }
						});
						deActivate.Start();

						animation.Stop();
						message.Text = "Error: Activation could not be completed due to insufficient privileges.";
						retryButton.Visibility = Visibility.Visible;
						closeButton.Visibility = Visibility.Visible;
						changeProductKey.IsEnabled = true;

						DialogResult = false;
					}
				}
				else
				{
					Activation.Activate();
					DialogResult = true;
				}
			}
			else
			{
				animation.Stop();
				message.Text = "Error: " + msg;
				retryButton.Visibility = Visibility.Visible;
				closeButton.Visibility = Visibility.Visible;
				changeProductKey.IsEnabled = true;
			}
		}

		private void proc_Exited(object sender, EventArgs e)
		{
			Dispatcher.Invoke(() =>
			{
				DialogResult = true;
			});
		}

		private void retryButton_Click(object sender, RoutedEventArgs e)
		{
			ActivateOnline();
		}

		private void continueButton_Click(object sender, RoutedEventArgs e)
		{
			continueButton.Visibility = Visibility.Collapsed;
			ActivateOnline();
		}

		private void changeProductKey_Click(object sender, RoutedEventArgs e)
		{
			ProductKey productKey = new ProductKey();
			productKey.Owner = this;

			if (productKey.ShowDialog() == true)
			{
				if (Activation.IsActivated())
					DialogResult = true;
			}
		}
	}
}
