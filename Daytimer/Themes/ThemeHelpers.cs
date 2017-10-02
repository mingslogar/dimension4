using Daytimer.Functions;
using System;
using System.Windows;

namespace Daytimer.Themes
{
	class ThemeHelpers
	{
		public static void UpdateTheme(bool firstTime)
		{
			string current = Settings.ThemeColor;
			ResourceDictionary dictionary = new ResourceDictionary();

			try
			{
				dictionary.Source = BuildThemeSource(current);
			}
			catch
			{
				// Invalid theme
				Settings.ThemeColor = Settings.themeColorDefault;
				dictionary.Source = BuildThemeSource(Settings.themeColorDefault);
			}

			(Application.Current as App).Add(dictionary, firstTime);

			if (Application.Current.MainWindow != null)
				(Application.Current.MainWindow as MainWindow).UpdateTheme();
		}

		public static void UpdateTheme(string theme)
		{
			ResourceDictionary dictionary = new ResourceDictionary();

			try
			{
				dictionary.Source = BuildThemeSource(theme);
			}
			catch
			{
				// Invalid theme
				Settings.ThemeColor = Settings.themeColorDefault;
				dictionary.Source = BuildThemeSource(Settings.themeColorDefault);
			}

			(Application.Current as App).Add(dictionary, false);

			if (Application.Current.MainWindow != null)
				(Application.Current.MainWindow as MainWindow).UpdateTheme();
		}

		private static Uri BuildThemeSource(string value)
		{
			return new Uri("pack://siteoforigin:,,,/Themes/" + value + ".xaml", UriKind.Absolute);
		}
	}
}
