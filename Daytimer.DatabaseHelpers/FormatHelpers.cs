using System;
using System.Globalization;
using System.Threading;

namespace Daytimer.DatabaseHelpers
{
	public class FormatHelpers
	{
		public static string FormatDate(DateTime date)
		{
			return date.ToString("dddd, MMMM d, yyyy");// date.DayOfWeek.ToString() + ", " + CalendarHelpers.Month(date.Month) + " " + date.Day.ToString() + ", " + date.Year.ToString();
		}

		private const string DateTagFormat = "\\dyyyyMMdd";

		/// <summary>
		/// Gets a DateTime from a formatted date string "d"yyyymmdd.
		/// </summary>
		/// <param name="datestring"></param>
		/// <returns></returns>
		public static DateTime? SplitDateString(string datestring)
		{
			DateTime value;

			if (DateTime.TryParseExact(datestring, DateTagFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out value))
				return value;

			return null;

			//if (datestring.Length >= 9)
			//	try
			//	{
			//		DateTime date = new DateTime(
			//			int.Parse(datestring.Substring(1, 4)),
			//			int.Parse(datestring.Substring(5, 2)),
			//			int.Parse(datestring.Substring(7))
			//		);

			//		return date;
			//	}
			//	catch { }

			//return null;
		}

		/// <summary>
		/// Get a formatted date string "d"yyyymmdd.
		/// </summary>
		/// <returns></returns>
		public static string DateString(DateTime date)
		{
			return date.ToString(DateTagFormat);
		}

		/// <summary>
		/// Get a formatted date string "d"mmdd.
		/// </summary>
		/// <param name="month">the month</param>
		/// <param name="day">the day</param>
		/// <returns></returns>
		public static string DateString(int month, int day)
		{
			return "d" + string.Format("{0:00}{1:00}", month, day);
		}

		/// <summary>
		/// Get a formatted date string "d"dd.
		/// </summary>
		/// <param name="day">the day</param>
		/// <returns></returns>
		public static string DateString(int day)
		{
			return "d" + string.Format("{0:00}", day);
		}

		/// <summary>
		/// Get a formatted date string "w"d.
		/// </summary>
		/// <param name="dayofweek">the one-based day of week</param>
		/// <returns></returns>
		public static string DayOfWeekString(int dayofweek)
		{
			return "w" + dayofweek.ToString();
		}

		/// <summary>
		/// For a repeating appointment, gets a string representation of the week array
		/// </summary>
		/// <param name="weeks"></param>
		/// <returns></returns>
		public static string WeekArrayString(int[] weeks)
		{
			string w = "";

			foreach (int each in weeks)
				w += each.ToString() + " ";

			return w.Trim();
		}

		/// <summary>
		/// For a repeating appointment, gets an int array representation of the week string
		/// </summary>
		/// <param name="weeks"></param>
		/// <returns></returns>
		public static int[] WeekArrayInt(string weeks)
		{
			string[] split = weeks.Split(' ');
			int length = split.Length;

			int[] w = new int[length];

			for (int i = 0; i < length; i++)
				w[i] = int.Parse(split[i]);

			return w;
		}

		/// <summary>
		/// Get a string representation of a DateTime array.
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static string DateTimeArray(DateTime[] dt)
		{
			if (dt == null)
				return null;

			string str = "";

			foreach (DateTime each in dt)
				str += each.ToShortDateString() + "|";

			str = str.TrimEnd('|');

			return str;
		}

		/// <summary>
		/// Get a DateTime array from a string representation.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static DateTime[] DateTimeArray(string str)
		{
			if (str == null)
				return null;

			string[] split = str.Split('|');
			int length = split.Length;

			DateTime[] dt = new DateTime[length];

			for (int i = 0; i < length; i++)
				dt[i] = DateTime.Parse(split[i]);

			return dt;
		}

		public static string BoolToString(bool val)
		{
			return val ? "1" : "0";
		}

		public static bool ParseBool(string str)
		{
			return str == "1" ? true : false;
		}

		private const string DateTimeFormat = "yyyyMMddHHmmss";
		private const string DateTimeShortFormat = "yyyyMMdd";

		/// <summary>
		/// Converts a <see cref="DateTime"/> to a string yyyyMMddHHmmss.
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static string DateTimeToString(DateTime val)
		{
			return val.ToString(DateTimeFormat);
		}

		/// <summary>
		/// Converts a string yyyyMMddHHmmss to a <see cref="DateTime"/>.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static DateTime ParseDateTime(string str)
		{
			try
			{
				return DateTime.ParseExact(str, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
			}
			catch (ThreadAbortException)
			{
				throw;
			}
			catch
			{
				// Compatibility with older versions.
				return DateTime.Parse(str);
			}
		}

		/// <summary>
		/// Converts a <see cref="DateTime"/> to a string yyyyMMdd.
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static string DateTimeToShortString(DateTime val)
		{
			return val.ToString(DateTimeShortFormat);
		}

		/// <summary>
		/// Converts a string yyyyMMdd to a <see cref="DateTime"/>.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static DateTime ParseShortDateTime(string str)
		{
			try
			{
				return DateTime.ParseExact(str, DateTimeShortFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
			}
			catch (ThreadAbortException)
			{
				throw;
			}
			catch
			{
				// Compatibility with older versions.
				return DateTime.Parse(str);
			}
		}

		public static string Capitalize(string text)
		{
			string result = "";
			string[] split = text.Split(' ');

			foreach (string each in split)
				result += char.ToUpper(each[0]).ToString() + each.Substring(1).ToLower() + " ";

			return result.TrimEnd();
		}
	}
}
