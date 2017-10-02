using Daytimer.Dialogs;
using Daytimer.GoogleCalendarHelpers;
using System.Windows;
using System.Windows.Input;

namespace Daytimer.Controls.Ribbon
{
	/// <summary>
	/// Interaction logic for AccountEdit.xaml
	/// </summary>
	public partial class AccountEdit : DialogBase
	{
		public AccountEdit(string group, string email)
		{
			InitializeComponent();

			_group = group;
			emailBox.Text = email;

			SecureStorage secure = new SecureStorage(_group, email);
			secure.Load();
			passwordBox.Password = secure.Password;
		}

		private string _group;

		private bool _deleted = false;

		public bool Deleted
		{
			get { return _deleted; }
		}

		private void emailBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Tab)
				passwordBox.Focus();
		}

		private void deleteButton_Click(object sender, RoutedEventArgs e)
		{
			SecureStorage secure = new SecureStorage(_group, emailBox.Text);
			secure.Delete();

			_deleted = true;
			DialogResult = true;
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			SecureStorage secure = new SecureStorage(_group, emailBox.Text);

			secure.Username = emailBox.Text;
			secure.Password = passwordBox.Password;
			secure.Save();

			DialogResult = true;
		}
	}
}
