using Daytimer.DatabaseHelpers;
using Daytimer.Dialogs;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for CategoryEditor.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class CategoryEditor : DialogBase
	{
		public CategoryEditor(Func<IEnumerable> load, Func<Category, bool> add, Action<Category> update, Action<Category> delete)
		{
			InitializeComponent();
			loadCategory = load;
			addCategory = add;
			updateCategory = update;
			deleteCategory = delete;
			Load();
		}

		private void Load()
		{
			listBox.ItemsSource = loadCategory(); // AppointmentDatabase.GetCategories();
		}

		private Func<IEnumerable> loadCategory;
		private Func<Category, bool> addCategory;
		private Action<Category> updateCategory;
		private Action<Category> deleteCategory;

		private void newButton_Click(object sender, RoutedEventArgs e)
		{
			NewCategory category = new NewCategory();
			category.Owner = this;

			if (category.ShowDialog() == true)
			{
				Category c = new Category();
				c.Name = category.nameTextBox.Text;
				c.Color = ((SolidColorBrush)category.colorPicker.SelectedColor).Color;

				addCategory(c);
				Load();
			}
		}

		private void ListBoxItem_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			Category _sender = (Category)((ContentControl)sender).Content;
			NewCategory newCategory = new NewCategory(_sender);
			newCategory.Owner = this;

			if (newCategory.ShowDialog() == true)
				if (newCategory.Action == Action.Delete)
					deleteCategory(_sender);
				else if (newCategory.Action == Action.OK)
				{
					_sender.Name = newCategory.nameTextBox.Text;
					_sender.Color = ((SolidColorBrush)newCategory.colorPicker.SelectedColor).Color;
					updateCategory(_sender);
				}

			if (newCategory.Action != Action.Cancel)
				Load();
		}
	}
}
