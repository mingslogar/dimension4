using Daytimer.DatabaseHelpers.Sync;
using Daytimer.GoogleCalendarHelpers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Daytimer.Controls.Ribbon
{
	/// <summary>
	/// Interaction logic for AccountDisplay.xaml
	/// </summary>
	public partial class AccountDisplay : Button
	{
		public AccountDisplay(ImageSource image, string group, string email)
		{
			InitializeComponent();

			this.image.Source = image;
			title.Text = group;
			detail.Text = email;

			_group = group;
			_email = email;
		}

		private string _group;
		private string _email;

		protected override void OnClick()
		{
			base.OnClick();

			switch (_group)
			{
				case "Google":
					GoogleSignIn gSignIn = new GoogleSignIn(true);
					gSignIn.Owner = Window.GetWindow(this);
					gSignIn.Email = _email;
					gSignIn.EmailReadOnly = true;
					gSignIn.ShowDialog();
					break;

				default:
					break;
			}

			//AccountEdit edit = new AccountEdit(title.Text, detail.Text);
			//edit.Owner = Window.GetWindow(this);

			//if (edit.ShowDialog() == true)
			//{
			//	if (edit.Deleted)
			//	{
			//		(Parent as Panel).Children.Remove(this);
			//		SyncDatabase.Delete(detail.Text);
			//	}
			//}
		}

		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);

			DoubleAnimation opacityAnim = new DoubleAnimation(1, new Duration(TimeSpan.FromMilliseconds(200)));
			deleteAccountButton.BeginAnimation(OpacityProperty, opacityAnim);
		}

		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);

			DoubleAnimation opacityAnim = new DoubleAnimation(0, new Duration(TimeSpan.FromMilliseconds(500)));
			deleteAccountButton.BeginAnimation(OpacityProperty, opacityAnim);
		}

		private void deleteAccountButton_Click(object sender, RoutedEventArgs e)
		{
			SecureStorage secure = new SecureStorage(_group, _email);
			secure.Delete();

			SyncDatabase.Delete(_email);

			(Parent as Panel).Children.Remove(this);
		}
	}
}
