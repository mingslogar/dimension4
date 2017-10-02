using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace Daytimer.Controls
{
	public static partial class Extensions
	{
		/// <summary>
		/// Fill the ComboBox with system fonts.
		/// </summary>
		/// <param name="fontBox"></param>
		public static async Task PopulateFonts(this ComboBox fontBox)
		{
			fontBox.ItemsSource = null;
			fontBox.Items.Clear();

			ComboBoxItem item = new ComboBoxItem();
			item.Content = "Loading...";
			item.IsEnabled = false;
			item.IsHitTestVisible = false;
			//item.FontSize = 12;
			fontBox.Items.Add(item);

			await CreateFontsArray(fontBox);
		}

		private static async Task CreateFontsArray(ComboBox box)
		{
			List<string> allFonts = new List<string>();

			await Task.Factory.StartNew(() =>
			{
				XmlLanguage enUS = XmlLanguage.GetLanguage("en-us");

				foreach (FontFamily fontFamily in Fonts.SystemFontFamilies)
				{
					FamilyTypefaceCollection typefaces = fontFamily.FamilyTypefaces;

					string baseName = fontFamily.Source;

					if (typefaces.Count == 1)
						allFonts.Add(baseName);
					else
					{
						// Fonts from this group which are going to be shown
						List<string> list = new List<string>(typefaces.Count);
						
						foreach (FamilyTypeface variation in typefaces)
						{
							if (variation.Style == FontStyles.Normal)
							{
								string faceName = variation.AdjustedFaceNames[enUS];

								if (faceName == "Regular")
									list.Add(baseName);
								else if (faceName != "Bold" && !faceName.Contains(" Bold"))
									list.Add(baseName + " " + faceName);
							}
						}

						if (list.Count == 1)
							allFonts.Add(baseName);
						else
							allFonts.AddRange(list);
					}
				}

				allFonts.Sort(string.Compare);

				//	int count = allFonts.Count - 1;

				//	// Sort
				//	bool swapHasBeenMade = true;

				//	while (swapHasBeenMade)
				//	{
				//		swapHasBeenMade = false;

				//		// run one pass for each item
				//		for (int i = 0; i < count; i++)
				//		{
				//			string s1 = allFonts[i];
				//			string s2 = allFonts[i + 1];

				//			if (string.Compare(s1, s2) > 0)
				//			{
				//				allFonts[i + 1] = s1;
				//				allFonts[i] = s2;

				//				swapHasBeenMade = true;
				//			}
				//		}
				//	}
			});

			box.Items.Clear();
			box.ItemsSource = allFonts;
		}
	}
}
