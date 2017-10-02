using Daytimer.DatabaseHelpers;
using Daytimer.Dialogs;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for NewCategory.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class NewCategory : DialogBase
	{
		public NewCategory()
		{
			InitializeComponent();
		}

		public NewCategory(Category category)
		{
			InitializeComponent();

			Title = "Edit Category";
			nameTextBox.Text = category.Name;
			colorPicker.SelectedColor = new SolidColorBrush(category.Color);
			nameTextBox.IsReadOnly = category.ReadOnly;
			deleteButton.IsEnabled = !category.ReadOnly;
		}

		protected override void OnContentRendered(EventArgs e)
		{
			base.OnContentRendered(e);
			nameTextBox.Focus();
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			Action = Action.OK;
			DialogResult = true;
		}

		private void deleteButton_Click(object sender, RoutedEventArgs e)
		{
			Action = Action.Delete;
			DialogResult = true;
		}

		public Action Action = Action.Cancel;
	}

	public enum Action : byte { OK, Delete, Cancel };
}
