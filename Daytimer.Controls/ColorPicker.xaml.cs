using Daytimer.Functions.Theme;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for ColorPicker.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class ColorPicker : ComboBox
	{
		public ColorPicker()
		{
			InitializeComponent();
			Loaded += ColorPicker_Loaded;
		}

		private bool _isFirstTime = true;
		private string _selected = null;

		private void ColorPicker_Loaded(object sender, RoutedEventArgs e)
		{
			if (_isFirstTime)
			{
				_isFirstTime = false;
				LoadThemes();
				SetSelected(_selected);
			}
		}

		private void LoadThemes()
		{
			try
			{
				List<Theme> themes = ThemeColors.GetInstalledThemes();

				if (themes != null)
				{
					int x = 2;
					int y = 2;

					foreach (Theme each in themes)
					{
						ComboBoxItem item = new ComboBoxItem();
						Border border = new Border();
						border.Background = each.Brush;
						border.Style = FindResource("Color") as Style;
						item.Content = border;
						item.Tag = each.Name;

						//
						// Create tooltip
						//
						string tooltip = "";
						string filename = each.Name;

						foreach (char c in filename)
						{
							if (char.IsUpper(c))
								tooltip += " " + c;
							else
								tooltip += c;
						}

						tooltip = tooltip.TrimStart();

						int lIndex = tooltip.LastIndexOf(' ');

						if (lIndex != -1)
							tooltip = tooltip.Remove(lIndex);

						item.ToolTip = tooltip;

						item.Margin = new Thickness(x, y, 2, 2);

						if (x == 2)
							y -= 25;

						if (x < 50)
							x += 25;
						else
						{
							x = 2;
							y = 2;
						}

						Items.Add(item);
					}
				}
			}
			catch (ResourceReferenceKeyNotFoundException)
			{
				// Invalid theme file
			}
		}

		public string CurrentlySelected
		{
			get { return SelectedItem != null ? (string)((FrameworkElement)SelectedItem).Tag : _selected; }
		}

		public void SetSelected(string color)
		{
			_selected = color;

			if (!IsLoaded)
				return;

			foreach (ComboBoxItem each in Items)
			{
				if ((string)each.Tag == _selected)
				{
					each.IsSelected = true;
					break;
				}
			}
		}

		//public void SetSelected(string color)
		//{
		//	if (!IsLoaded)
		//	{
		//		_selected = color;
		//		return;
		//	}

		//	foreach (ComboBoxItem each in Items)
		//	{
		//		if (each.Tag as string == color)
		//		{
		//			each.IsSelected = true;
		//			break;
		//		}
		//	}
		//}
	}
}
