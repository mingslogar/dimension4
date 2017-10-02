using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Daytimer.Functions;

namespace Modern.FileBrowser
{
	[ValueConversion(typeof(double), typeof(string))]
	public class FileSizeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return null;

			return Convert((long)value);
		}

		public static string Convert(long value)
		{
			decimal size = (decimal)value;

			if (size < 1024)
				return size.ToString() + " byte" + (size != 1 ? "s" : "");
			else if (size < 1048576)
				return round(size / 1024M) + " KB";
			else if (size < 1073741824)
				return round(size / 1048576M) + " MB";
			else if (size < 1099511627776)
				return round(size / 1073741824M) + " GB";
			else
				return round(size / 1099511627776M) + " TB";
		}

		private static string round(decimal value)
		{
			double modified;

			if (value >= 100)
				modified = (double)Math.Round(value);
			else
				modified = ((double)value).RoundToSignificantDigits(3);

			if (modified != Math.Round(modified))
			{
				if (modified < 100)
					return modified.ToString().PadRight(4, '0');
			}
			else
			{
				if (modified < 100)
					return (modified.ToString() + ".").PadRight(4, '0');
			}

			return modified.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return null;

			string[] split = ((string)value).Split(' ');
			float size = float.Parse(split[0]);
			string modifier = split[1];

			switch (modifier)
			{
				case "byte":
				case "bytes":
				default:
					return size;

				case "KB":
					return size * 1024F;

				case "MB":
					return size * 1073741824F;

				case "GB":
					return size * 1073741824F;

				case "TB":
					return size * 1099511627776F;
			}
		}
	}

	[ValueConversion(typeof(double), typeof(string))]
	public class DateTimeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value != null ? ((DateTime)value).ToString("g", DateTimeFormatInfo.CurrentInfo) : null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return DateTime.Parse((string)value);
		}
	}

	/// <summary>
	/// Convert between !boolean and visibility
	/// </summary>
	[Localizability(LocalizationCategory.NeverLocalize)]
	public sealed class InverseBooleanToVisibilityConverter : IValueConverter
	{
		/// <summary>
		/// Convert bool or Nullable&lt;bool&gt; to Visibility
		/// </summary>
		/// <param name="value">bool or Nullable&lt;bool&gt;</param>
		/// <param name="targetType">Visibility</param>
		/// <param name="parameter">null</param>
		/// <param name="culture">null</param>
		/// <returns>Visible or Collapsed</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool bValue = false;
			if (value is bool)
			{
				bValue = (bool)value;
			}
			else if (value is Nullable<bool>)
			{
				Nullable<bool> tmp = (Nullable<bool>)value;
				bValue = tmp.HasValue ? tmp.Value : false;
			}
			return (bValue) ? Visibility.Collapsed : Visibility.Visible;
		}

		/// <summary>
		/// Convert Visibility to boolean
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Visibility)
			{
				return (Visibility)value == Visibility.Collapsed;
			}
			else
			{
				return false;
			}
		}
	}
}
