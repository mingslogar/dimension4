using Daytimer.Functions;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Daytimer.Fundamentals
{
	[ValueConversion(typeof(SolidColorBrush), typeof(Color))]
	public class SolidColorBrushConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((SolidColorBrush)value).Color;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return new SolidColorBrush((Color)value);
		}
	}

	/// <summary>
	/// For use by BalloonTip class.
	/// </summary>
	[ValueConversion(typeof(Thickness), typeof(Thickness))]
	public class MarginConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Thickness t1 = (Thickness)value;
			Thickness t2 = ThicknessFromString(parameter.ToString());

			return new Thickness(t1.Left + t2.Left, t1.Top + t2.Top, t1.Right + t2.Right, t1.Bottom + t2.Bottom);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Thickness t1 = (Thickness)value;
			Thickness t2 = ThicknessFromString(parameter.ToString());

			return new Thickness(t1.Left - t2.Left, t1.Top - t2.Top, t1.Right - t2.Right, t1.Bottom - t2.Bottom);
		}

		private Thickness ThicknessFromString(string value)
		{
			string[] split = value.Split(',');

			if (split.Length == 1)
				return new Thickness(double.Parse(split[0].Trim()));

			if (split.Length == 2)
			{
				double val1 = double.Parse(split[0].Trim());
				double val2 = double.Parse(split[1].Trim());

				return new Thickness(val1, val2, val1, val2);
			}

			return new Thickness(double.Parse(split[0].Trim()), double.Parse(split[1].Trim()), double.Parse(split[2].Trim()), double.Parse(split[3].Trim()));
		}
	}

	/// <summary>
	/// Extract a 16x16 image from a .ico file.
	/// </summary>
	[ValueConversion(typeof(ImageSource), typeof(string))]
	public class WindowIconConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			try
			{
				if (value == null)
					return null;

				// The icon decoder does not work on XP.
				if (Environment.OSVersion.Version <= OSVersions.Win_XP_64)
					return null;

				string source = value.ToString();
				IconBitmapDecoder decoder = new IconBitmapDecoder(new Uri(source, UriKind.Absolute),
					BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnDemand);

				foreach (BitmapFrame each in decoder.Frames)
				{
					if (each.Width == 16 && each.Height == 16)
						return each;
				}

				return decoder.Frames[0];
			}
			catch
			{
				return null;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
