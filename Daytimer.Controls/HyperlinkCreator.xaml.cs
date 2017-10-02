using Daytimer.Dialogs;
using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for HyperlinkCreator.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class HyperlinkCreator : DialogBase
	{
		public HyperlinkCreator()
		{
			InitializeComponent();
		}

		public string DisplayText
		{
			get { return displayText.Text; }
			set { displayText.Text = value; }
		}

		public string URL
		{
			get { return url.Text; }
			set
			{
				if (!value.StartsWith("mailto:"))
				{
					url.Text = value;
					comboBox.SelectedIndex = 0;
				}
				else
				{
					url.Text = value.Substring(7);
					comboBox.SelectedIndex = 1;
				}
			}
		}

		protected override void OnContentRendered(EventArgs e)
		{
			base.OnContentRendered(e);
			displayText.Focus();
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			if (String.IsNullOrEmpty(displayText.Text))
			{
				TaskDialog dlg = new TaskDialog(this, "Empty field", "You didn't enter a value for the display text.", MessageType.Error);
				dlg.ShowDialog();
				displayText.Focus();
				return;
			}

			if (String.IsNullOrEmpty(url.Text))
			{
				TaskDialog dlg = new TaskDialog(this, "Empty field", "You didn't enter a value for the URL.", MessageType.Error);
				dlg.ShowDialog();
				url.Focus();
				return;
			}

			if (comboBox.SelectedIndex == 0)
				try
				{
					new Uri(url.Text);
				}
				catch
				{
					try
					{
						new Uri("http://" + url.Text);
						url.Text = "http://" + url.Text;
					}
					catch
					{
						TaskDialog dlg = new TaskDialog(this, "Invalid URL", "The URL you entered doesn't match a known format.", MessageType.Error);
						dlg.ShowDialog();
						url.Focus();
						return;
					}
				}
			else if (comboBox.SelectedIndex == 1)
				if (!new RegexUtilities().IsValidEmail(url.Text))
				{
					TaskDialog dlg = new TaskDialog(this, "Invalid Email", "The email address you entered doesn't match a known format.", MessageType.Error);
					dlg.ShowDialog();
					url.Focus();
					return;
				}
				else
					url.Text = "mailto:" + url.Text;

			DialogResult = true;
		}
	}
}
