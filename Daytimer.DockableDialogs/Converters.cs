using System;
using System.Globalization;
using System.Windows.Data;

namespace Daytimer.DockableDialogs
{
	[ValueConversion(typeof(double), typeof(double))]
	public class DockTargetSizeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (double)value - 250;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (double)value + 250;
		}
	}
}
