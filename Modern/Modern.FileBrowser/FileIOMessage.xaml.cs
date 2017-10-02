using Daytimer.Dialogs;
using Daytimer.Fundamentals;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Modern.FileBrowser
{
	/// <summary>
	/// Interaction logic for ErrorMessage.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class FileIOMessage : OfficeWindow
	{
		public FileIOMessage(Window owner, string title, string message, MessageBoxButton buttons, MessageType type)
		{
			InitializeComponent();

			Owner = owner;
			Title = title;
			msg.Text = message;

			switch (buttons)
			{
				case MessageBoxButton.OK:
					AddButton("_OK", true, false);
					break;

				case MessageBoxButton.OKCancel:
					AddButton("_OK", true, false);
					AddButton("_Cancel", false, true);
					break;

				case MessageBoxButton.YesNo:
					AddButton("_Yes", true, false);
					AddButton("_No", false, true);
					break;

				case MessageBoxButton.YesNoCancel:
					AddButton("_Yes", true, false);
					AddButton("_No", false, false);
					AddButton("_Cancel", false, true);
					break;
			}

			Loaded += (sender, e) => { DialogHelpers.PlaySound(type); };
		}

		private void AddButton(string text, bool isDefault, bool isCancel)
		{
			Button button = new Button();
			button.Width = 75;
			button.Height = 25;
			button.Margin = new Thickness(10, 0, 0, 0);
			button.Content = text;
			button.IsDefault = isDefault;
			button.IsCancel = isCancel;

			if (isDefault)
				AccessKeyManager.Register(" ", button);

			button.Click += button_Click;

			btns.Children.Add(button);
		}

		private void button_Click(object sender, RoutedEventArgs e)
		{
			switch ((string)((Button)sender).Content)
			{
				case "_OK":
				case "_Yes":
					DialogResult = true;
					break;

				case "_No":
				case "_Cancel":
					DialogResult = false;
					break;
			}
		}
	}
}
