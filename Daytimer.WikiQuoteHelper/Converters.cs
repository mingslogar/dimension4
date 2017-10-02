using System;
using System.Globalization;
using System.Windows.Data;

namespace Daytimer.WikiQuoteHelper
{
	[ValueConversion(typeof(DateTime), typeof(string))]
	public class DateTimeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((DateTime)value).ToString("MMMM d, yyyy h:mm:ss tt");
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return DateTime.Parse((string)value);
		}
	}
}
