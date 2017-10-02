using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Daytimer.Controls
{
	[ValueConversion(typeof(double), typeof(string))]
	public class PercentConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			double x = (double)value;
			string xstring = x.ToString();
			xstring = xstring.Split('.')[0];
			return xstring + "%";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string ptValue = (string)value;
			return double.Parse(ptValue.Remove(ptValue.Length - 1));
		}
	}

	[ValueConversion(typeof(double), typeof(double))]
	public class NegativeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return -(double)value + 1;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return -(double)value - 1;
		}
	}

	[ValueConversion(typeof(SolidColorBrush), typeof(SolidColorBrush))]
	public class FullAlphaConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Color color = ((SolidColorBrush)value).Color;
			color.A = 255;
			return new SolidColorBrush(color);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value;
		}
	}

	[ValueConversion(typeof(Uri), typeof(string))]
	public class UriToolTipConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return null;

			return ((Uri)value).ToString() + "\r\nCtrl+Click to follow link";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return new Uri(value.ToString().Split('\n')[0]);
		}
	}

	[ValueConversion(typeof(string), typeof(BitmapSource))]
	public class ImageConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return null;

			return new BitmapImage(new Uri((string)value));
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((BitmapImage)value).UriSource.ToString();
		}
	}

	[ValueConversion(typeof(string), typeof(string))]
	public class StringConcatenationConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				if (parameter == null)
					return null;
				else
					return parameter.ToString();
			else if (parameter == null)
				return value.ToString();

			return value.ToString() + parameter.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string data = value.ToString();
			string addend = parameter.ToString();

			return data.Remove(data.Length - addend.Length);
		}
	}
}
