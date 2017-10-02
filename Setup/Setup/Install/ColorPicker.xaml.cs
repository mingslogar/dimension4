using Daytimer.Functions.Theme;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Setup.Install
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
			string[] themeSource = new string[]
			{
				"BlueTheme",
				"BrownTheme",
				"GreenTheme",
				"MaroonTheme",
				"PurpleTheme",
				"SilverTheme"
			};

			List<Theme> themes = new List<Theme>(themeSource.Length);

			foreach (string each in themeSource)
			{
				ResourceDictionary dictionary = new ResourceDictionary();
				dictionary.Source = new Uri("/Daytimer/Themes/" + each + ".xaml", UriKind.Relative);
				Brush sample = dictionary["Appointment"] as Brush;

				themes.Add(new Theme(each, sample));
			}

			if (themes != null)
			{
				int x = 2;
				int y = 2;

				foreach (Theme each in themes)
				{
					try
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
					catch (ResourceReferenceKeyNotFoundException)
					{
						// Invalid theme file
					}
				}
			}
		}

		public string CurrentlySelected
		{
			get { return SelectedItem != null ? (SelectedItem as ComboBoxItem).Tag as string : _selected; }
		}

		public void SetSelected(string color)
		{
			_selected = color;

			if (!IsLoaded)
				return;

			foreach (ComboBoxItem each in Items)
			{
				if (each.Tag as string == _selected)
				{
					each.IsSelected = true;
					break;
				}
			}
		}
	}
}
