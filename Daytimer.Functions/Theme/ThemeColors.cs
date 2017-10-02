using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace Daytimer.Functions.Theme
{
	public class ThemeColors
	{
		/// <summary>
		/// Get all XAML theme files in /Themes.
		/// </summary>
		/// <returns></returns>
		public static List<Theme> GetInstalledThemes()
		{
			string themedir = Process.GetCurrentProcess().MainModule.FileName;
			themedir = themedir.Remove(themedir.LastIndexOf('\\')) + "\\Themes";

			if (Directory.Exists(themedir))
			{
				string[] files = Directory.GetFiles(themedir);
				List<Theme> samples = new List<Theme>(6);

				foreach (string each in files)
				{
					if (each.EndsWith(".xaml", StringComparison.InvariantCultureIgnoreCase))
					{
						ResourceDictionary dictionary = new ResourceDictionary();
						dictionary.Source = new Uri(each, UriKind.Absolute);
						Brush sample = dictionary["Appointment"] as Brush;

						string strippedFilename = (new FileInfo(each)).Name;
						strippedFilename = strippedFilename.Remove(strippedFilename.LastIndexOf('.'));
						samples.Add(new Theme(strippedFilename, sample));
					}
				}

				return samples;
			}
			else
				return null;
		}
	}
}
