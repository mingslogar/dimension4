using Daytimer.Functions;
using Setup;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace FirstRun
{
	/// <summary>
	/// Interaction logic for Personalize.xaml
	/// </summary>
	public partial class Personalize : UserControl
	{
		public Personalize()
		{
			InitializeComponent();

			bgImgCombo.Text = Settings.BackgroundImage;
			themeCombo.SetSelected(Settings.ThemeColor);
		}

		private void bgImgCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			string img = (bgImgCombo.SelectedItem as ComboBoxItem).Content as string;
			Settings.BackgroundImage = img;

			bgImgCopy.Source = bgImg.Source;
			bgImgCopy.Opacity = 1;
			bgImgCopy.Visibility = Visibility.Visible;

			bgImg.Opacity = 0;
			bgImg.Source = new BitmapImage(new Uri("Backgrounds/" + img.Replace(" ", "") + ".png", UriKind.Relative));

			if (IsLoaded)
			{
				bgImgCopy.FadeOut();
				bgImg.FadeIn();
			}
			else
			{
				bgImg.Opacity = 1;
				bgImgCopy.Opacity = 0;
			}
		}

		private void themeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Settings.ThemeColor = themeCombo.CurrentlySelected;
			ThemeHelpers.UpdateTheme(false);
		}
	}
}
