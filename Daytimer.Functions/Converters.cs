using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Daytimer.Functions
{
	[ValueConversion(typeof(double), typeof(Thickness))]
	public class HeightToThicknessConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			double y = (double)value;
			return new Thickness(0, -y, 0, y);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Thickness Value = (Thickness)value;
			return -Value.Top;
		}
	}

	[ValueConversion(typeof(double), typeof(Thickness))]
	public class NegativeHeightToThicknessConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			double y = (double)value;
			return new Thickness(0, y, 0, -y);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Thickness Value = (Thickness)value;
			return Value.Top;
		}
	}

	public class Converter
	{
		public static double PixelToPoint(double value)
		{
			return value * 96d / 72d;
		}

		public static double PointToPixel(double value)
		{
			return value * 72d / 96d;
		}
	}
}
