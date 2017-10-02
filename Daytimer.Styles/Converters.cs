using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Daytimer.Styles
{
	[ValueConversion(typeof(double), typeof(double))]
	public class WidthConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (double)value - 2;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (double)value + 2;
		}
	}

	[ValueConversion(typeof(double), typeof(double))]
	public class HeightConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (double)value - 32;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (double)value + 32;
		}
	}

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

	[ValueConversion(typeof(string), typeof(string))]
	public class CapitalsConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value.ToString().ToUpper();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value.ToString().ToLower();
		}
	}

	[ValueConversion(typeof(double), typeof(double))]
	public class NegativeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return -(double)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return -(double)value;
		}
	}
}
