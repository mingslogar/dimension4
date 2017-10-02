using Daytimer.Dialogs;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Daytimer.GoogleCalendarHelpers
{
	/// <summary>
	/// Interaction logic for GoogleSignIn.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class GoogleSignIn : DialogBase
	{
		public GoogleSignIn()
		{
			InitializeComponent();
		}

		public GoogleSignIn(bool updateMode)
		{
			InitializeComponent();

			_updateMode = updateMode;
		}

		private bool _updateMode = false;

		public string Email
		{
			get { return emailBox.Text; }
			set
			{
				emailBox.Text = value;

				if (_updateMode)
				{
					SecureStorage secure = new SecureStorage("Google", value);
					secure.Load();
					passwordBox.Password = secure.Password;
				}
			}
		}

		public string Password
		{
			get { return passwordBox.Password; }
		}

		public bool EmailReadOnly
		{
			get { return emailBox.IsReadOnly; }
			set { emailBox.IsReadOnly = value; }
		}

		protected override void OnContentRendered(EventArgs e)
		{
			base.OnContentRendered(e);
			emailBox.Focus();
		}

		private void signInButton_Click(object sender, RoutedEventArgs e)
		{
			if (!new RegexUtilities().IsValidEmail(emailBox.Text))
			{
				TaskDialog td = new TaskDialog(this, "Invalid Email", "The email address you provided is invalid.", MessageType.Error);
				td.ShowDialog();
				emailBox.Focus();
				return;
			}

			if (string.IsNullOrEmpty(passwordBox.Password))
			{
				TaskDialog td = new TaskDialog(this, "Invalid Password", "The password field must be filled out.", MessageType.Error);
				td.ShowDialog();
				passwordBox.Focus();
				return;
			}

			signInButton.Visibility = Visibility.Hidden;
			SignIn();
		}

		private void retryButton_Click(object sender, RoutedEventArgs e)
		{
			SignIn();
		}

		private void emailBox_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter || e.Key == Key.Tab)
				passwordBox.Focus();
		}

		private void SignIn()
		{
			message.Text = "Checking your credentials...";

			retryButton.IsEnabled = false;

			animation.Start();

			userDataGrid.Visibility = Visibility.Collapsed;

			string email = emailBox.Text;
			string password = passwordBox.Password;

			Thread thread = new Thread(() =>
			{
				try
				{
					if (!CalendarHelper.Verify(email, password))
					{
						Dispatcher.BeginInvoke(() =>
						{
							if (!IsLoaded)
								return;

							TaskDialog td = new TaskDialog(this, "Credentials",
								"It doesn't look like these credentials work. Try and enter them again.",
								MessageType.Error);
							td.ShowDialog();
							passwordBox.SelectAll();
							signInButton.Visibility = Visibility.Visible;

							animation.Stop();
							userDataGrid.Visibility = Visibility.Visible;

							message.Text = "Enter your Google account credentials below.";
						});

						return;
					}

					SecureStorage secure = new SecureStorage("Google", email);

					if (!_updateMode)
					{
						secure.Load();

						if (secure.Username != null || secure.Password != null)
						{
							Dispatcher.BeginInvoke(() =>
							{
								if (!IsLoaded)
									return;

								TaskDialog td = new TaskDialog(this, "Account Exists",
									"You already added this account to " + GlobalAssemblyInfo.AssemblyName + ".", MessageType.Error);
								td.ShowDialog();
								emailBox.SelectAll();
								signInButton.Visibility = Visibility.Visible;

								animation.Stop();
								userDataGrid.Visibility = Visibility.Visible;
							});

							return;
						}
					}

					secure.Username = email;
					secure.Password = password;
					secure.Save();

					Dispatcher.BeginInvoke(() =>
					{
						if (!IsLoaded)
							return;

						DialogResult = true;
					});
				}
				catch
				{
					Dispatcher.BeginInvoke(() =>
					{
						if (!IsLoaded)
							return;

						TaskDialog td = new TaskDialog(this, "Sign In",
							"We're having trouble signing into your Google account. Check your Internet connection and try again.",
							MessageType.Error);
						td.ShowDialog();

						retryButton.IsEnabled = true;
						retryButton.Visibility = Visibility.Visible;
						closeButton.Visibility = Visibility.Visible;

						animation.Stop();
						userDataGrid.Visibility = Visibility.Visible;
						message.Text = "Enter your Google account credentials below.";
					});
				}
			});
			thread.Start();
		}

		private void passwordBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			ShowCapsLockMessage();
		}

		private void passwordBox_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			ShowCapsLockMessage();
		}

		private void passwordBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			capsLockMsg.IsOpen = false;
		}

		private void ShowCapsLockMessage()
		{
			capsLockMsg.IsOpen = Keyboard.IsKeyToggled(Key.CapsLock);
		}
	}
}
