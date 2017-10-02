using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace Daytimer.Dialogs
{
	/// <summary>
	/// Interaction logic for EditRecurring.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class EditRecurring : DialogBase
	{
		public EditRecurring(Window Owner, EditingType editType)
		{
			InitializeComponent();
			this.Owner = Owner;
			EditType = editType;

			switch (EditType)
			{
				case EditingType.Delete:
					Title = "Delete Recurring Item";
					detailsText.Text = "What do you want to delete?";
					break;

				case EditingType.Open:
					break;
			}

			AccessKeyManager.Register(" ", okButton);
		}

		public EditResult EditResult = EditResult.Single;
		public EditingType EditType = EditingType.Open;

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			EditResult = thisOne.IsChecked == true ? EditResult.Single : EditResult.All;

			DialogResult = true;

			// If the window was not shown as a dialog, the above
			// line will not do anything.
			try { Close(); }
			catch { }
		}
	}

	public enum EditResult { Single, All };
	public enum EditingType { Open, Delete };
}
