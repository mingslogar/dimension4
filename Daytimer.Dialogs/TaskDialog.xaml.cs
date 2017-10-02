using System.Media;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace Daytimer.Dialogs
{
	/// <summary>
	/// Interaction logic for TaskDialog.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class TaskDialog : DialogBase
	{
		public TaskDialog(Window Owner, string Title, string Details, MessageType MessageIcon)
		{
			InitializeComponent();

			this.Owner = Owner;
			this.Title = Title;

			detailsText.Text = Details;

			_icon = MessageIcon;

			AccessKeyManager.Register(" ", okButton);

			if (Owner == null)
				ShowAsGlobal();
		}

		public TaskDialog(Window Owner, string Title, string Details, MessageType MessageIcon, bool ShowCancel)
		{
			InitializeComponent();

			this.Owner = Owner;

			this.Title = Title;
			detailsText.Text = Details;

			_icon = MessageIcon;

			if (ShowCancel)
				cancelButton.Visibility = Visibility.Visible;

			AccessKeyManager.Register(" ", okButton);

			if (Owner == null)
				ShowAsGlobal();
		}

		public TaskDialog(Window Owner, string Title, string Details, MessageType MessageIcon, string OKText, string CancelText)
		{
			InitializeComponent();

			this.Owner = Owner;

			this.Title = Title;
			detailsText.Text = Details;

			_icon = MessageIcon;

			okButton.Content = OKText;
			cancelButton.Content = CancelText;
			cancelButton.Visibility = Visibility.Visible;

			AccessKeyManager.Register(" ", okButton);

			if (Owner == null)
				ShowAsGlobal();
		}

		private MessageType _icon;

		private void window_Loaded(object sender, RoutedEventArgs e)
		{
			DialogHelpers.PlaySound(_icon);
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;

			// If the window was not shown as a dialog, the above
			// line will not do anything.
			try { Close(); }
			catch { }
		}
	}
}
