using System;

namespace Daytimer.Functions
{
	public class CalendarHelpers
	{
		public static string DayOfWeek(int day)
		{
			return ((System.DayOfWeek)(day - 1)).ToString();
		}

		public static int GetMonth(string _month)
		{
			switch (_month)
			{
				case "Jan":
					return 1;

				case "Feb":
					return 2;

				case "Mar":
					return 3;

				case "Apr":
					return 4;

				case "May":
					return 5;

				case "Jun":
					return 6;

				case "Jul":
					return 7;

				case "Aug":
					return 8;

				case "Sep":
					return 9;

				case "Oct":
					return 10;

				case "Nov":
					return 11;

				case "Dec":
					return 12;

				default:
					return -1;
			}
		}

		/// <summary>
		/// Gets a one-based day of week starting with Sunday.
		/// </summary>
		public static int DayOfWeek(DayOfWeek day)
		{
			return (int)day + 1;
		}

		/// <summary>
		/// Gets the name of a given month
		/// </summary>
		/// <param name="month">the one-based month</param>
		public static string Month(int month)
		{
			return ((Month)month).ToString();
		}

		/// <summary>
		/// Gets the name of a given month, abbreviated to 3 letters.
		/// </summary>
		/// <param name="month"></param>
		/// <returns></returns>
		public static string AbbrevMonth(int month)
		{
			return Month(month).Remove(3);
		}

		/// <summary>
		/// Gets the number of days in a given month.
		/// </summary>
		/// <param name="month">the one-based month index</param>
		/// <returns></returns>
		public static int DaysInMonth(int month, int year)
		{
			switch (month)
			{
				case 4:
				case 6:
				case 9:
				case 11:
					return 30;

				case 1:
				case 3:
				case 5:
				case 7:
				case 8:
				case 10:
				case 12:
					return 31;

				case 2:
					if (IsLeapYear(year))
						return 29;
					else
						return 28;

				default:
					return -1;
			}
		}

		public static bool IsLeapYear(int year)
		{
			if (year % 100 == 0)
				if (year % 400 == 0)
					return true;
				else
					return false;
			else if (year % 4 == 0)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Gets the 1-based day of week.
		/// </summary>
		/// <param name="y">the year</param>
		/// <param name="m">the month</param>
		/// <param name="d">the day</param>
		/// <returns></returns>
		public static int DayOfWeek(int y, int m, int d)
		{
			int[] t = new int[] { 0, 3, 2, 5, 0, 3, 5, 1, 4, 6, 2, 4 };
			y -= (m < 3 ? 1 : 0);
			return (y + y / 4 - y / 100 + y / 400 + t[m - 1] + d) % 7 + 1;
		}

		/// <summary>
		/// Gets a 7x5 or 7x6 display of the days of the specified month and year.
		/// </summary>
		public static string[] MonthDisplay(int month, int year, int beginning)
		{
			string[] days = new string[42];

			int daysLastMonth = DaysInMonth(month > 1 ? month - 1 : 12, (month > 1 ? year : year - 1));
			int daysThisMonth = DaysInMonth(month, year);

			int counter = 0;

			int beginningM2 = beginning - 2;

			for (int i = beginningM2; i >= 0; i--)
			{
				days[counter] = (daysLastMonth - i).ToString();
				counter++;
			}

			int counter2 = 1;

			while (counter2 <= daysThisMonth)
			{
				days[counter++] = (counter2++).ToString();
			}

			counter2 = 1;

			while (counter < 42)
			{
				days[counter++] = (counter2++).ToString();
			}

			string thisMonth = (Month(month));

			if (thisMonth.Length > 3)
				days[beginning - 1] = thisMonth.Remove(3) + " " + days[beginning - 1];
			else
				days[beginning - 1] = thisMonth + " " + days[beginning - 1];

			string nextMonth = (Month(month < 12 ? month + 1 : 1));

			if (days[0].Length <= 2)
			{
				string lastMonth = Month(month > 1 ? month - 1 : 12);

				if (lastMonth.Length > 3)
					days[0] = lastMonth.Remove(3) + " " + days[0];
				else
					days[0] = lastMonth + " " + days[0];
			}

			if (nextMonth.Length > 3)
				days[beginning + daysThisMonth - 1] = nextMonth.Remove(3) + " " + days[beginning + daysThisMonth - 1];
			else
				days[beginning + daysThisMonth - 1] = nextMonth + " " + days[beginning + daysThisMonth - 1];

			if (month == 12)
			{
				if (year >= 1000)
					days[beginning + daysThisMonth - 1] = days[beginning + daysThisMonth - 1] + ", " + (year + 1).ToString().Substring(2);
				else if (year >= 100)
					days[beginning + daysThisMonth - 1] = days[beginning + daysThisMonth - 1] + ", " + (year + 1).ToString().Substring(1);
				else
					days[beginning + daysThisMonth - 1] = days[beginning + daysThisMonth - 1] + ", " + (year + 1).ToString();
			}
			else if (month == 1)
			{
				if (year >= 1000)
					days[beginning - 1] = days[beginning - 1] + ", " + year.ToString().Substring(2);
				else if (year >= 100)
					days[beginning - 1] = days[beginning - 1] + ", " + year.ToString().Substring(1);
				else
					days[beginning - 1] = days[beginning - 1] + ", " + year.ToString();
			}

			bool isLastRowUsed = false;

			for (int i = 36; i < 42; i++)
			{
				if (days[i].Length > 2)
				{
					isLastRowUsed = true;
					break;
				}
			}

			if (!isLastRowUsed)
				days[35] = null;

			return days;
		}

		///// <summary>
		///// Gets a 7x5 or 7x6 display of the days of the specified month and year.
		///// </summary>
		//public static DateTime[] MonthDisplay(int month, int year, int beginning)
		//{
		//	DateTime[] days = new DateTime[42];

		//	int lastMonthYear = month > 1 ? year : year - 1;
		//	int lastMonth = month > 1 ? month - 1 : 12;
		//	int daysLastMonth = DaysInMonth(lastMonth, lastMonthYear);
		//	int daysThisMonth = DaysInMonth(month, year);

		//	int nextMonthYear = month < 12 ? year : year+1;
		//	int nextMonth = month < 12? month+1:1;

		//	int counter = 0;

		//	//
		//	// Last month
		//	//
		//	int beginningM2 = beginning - 2;

		//	for (int i = beginningM2; i >= 0; i--)
		//	{
		//		days[counter] = new DateTime(lastMonthYear, lastMonth, daysLastMonth - i);
		//		counter++;
		//	}

		//	//
		//	// This month
		//	//
		//	int counter2 = 1;

		//	while (counter2 <= daysThisMonth)
		//	{
		//		days[counter++] = new DateTime(year, month, counter2++);
		//	}

		//	//
		//	// Next month
		//	//
		//	counter2 = 1;

		//	while (counter < 42)
		//	{
		//		days[counter++] = new DateTime(nextMonthYear, nextMonth, counter2++);
		//	}

		//	//
		//	// Is last row used
		//	//
		//	if (days[35].Month != month)
		//		days[35].AddSeconds(1);

		//	return days;
		//}

		/// <summary>
		/// Gets the DateTime of the Sunday of the specified <see cref="System.DateTime"/>.
		/// </summary>
		public static DateTime FirstDayOfWeek(DateTime date)
		{
			return FirstDayOfWeek(date.Month, date.Day, date.Year);
		}

		/// <summary>
		/// Gets the DateTime of the Sunday of the specified day, month and year
		/// </summary>
		public static DateTime FirstDayOfWeek(int month, int day, int year)
		{
			int start = DayOfWeek(year, month, day);
			int sunday = day - start + 1;

			if (sunday < 1)
			{
				if (month > 1)
					month--;
				else
				{
					month = 12;
					year--;
				}

				sunday += DaysInMonth(month, year);
			}

			return new DateTime(year, month, sunday);
		}

		/// <summary>
		/// For the default setting, returns if the string has the requested day in it.
		/// </summary>
		public static bool IsDayWorkDay(string setting, DayOfWeek day)
		{
			return setting[(int)day] == '1';
		}

		/// <summary>
		/// Gets a rounded time based on a zoom level
		/// </summary>
		/// <param name="hour"></param>
		/// <param name="zoom"></param>
		/// <returns></returns>
		public static double SnappedHour(double hour, double zoom)
		{
			if (zoom < 1)
			{
				// Snap to 30 minutes
				hour = hour - (hour * 100 % 50) / 100;
			}
			else if (zoom < 3)
			{
				// Snap to 15 minutes
				hour = hour - (hour * 100 % 25) / 100;
			}
			else if (zoom < 5)
			{
				// Snap to 5 minutes
				hour = hour - (hour * 100 % (100d / 12d)) / 100d;
			}

			return hour;
		}

		public static DateTime DayOccurrence(int year, int month, DayOfWeek day, int occurrenceNumber)
		{
			DateTime start = new DateTime(year, month, 1);
			DateTime first = start.AddDays((7 - ((int)start.DayOfWeek - (int)day)) % 7);

			return first.AddDays(7 * (occurrenceNumber - 1));
		}

		/// <summary>
		/// Gets the number of the week (1, 2, 3, 4, 5) a date is in.
		/// </summary>
		/// <param name="date">the date to check</param>
		/// <returns></returns>
		public static int Week(DateTime date)
		{
			DateTime firstDOW = DayOccurrence(date.Year, date.Month, date.DayOfWeek, 1);
			return (date - firstDOW).Days / 7 + 1;
		}

		/// <summary>
		/// Gets the month difference between DateTime objects
		/// </summary>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <returns></returns>
		public static int CalculateMonthDifference(DateTime startDate, DateTime endDate)
		{
			int monthsApart = 12 * (startDate.Year - endDate.Year) + startDate.Month - endDate.Month;
			return Math.Abs(monthsApart);
		}

		/// <summary>
		/// Gets the number (1,2,3,4,5) of a certain day (4th Sunday)
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public static int DayOfWeekNumber(DateTime date)
		{
			int firstDOW = CalendarHelpers.DayOfWeek(date.Year, date.Month, 1);
			int weeks = (int)Math.Ceiling(((double)date.Day + (double)firstDOW) / 7d);

			if ((int)date.DayOfWeek + 1 < firstDOW)
				weeks--;

			return weeks;
		}

		/// <summary>
		/// Gets the number of weeks in a month defined by a DateTime.
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public static int NumberOfWeeks(DateTime date)
		{
			DateTime lastDay = new DateTime(date.Year, date.Month, CalendarHelpers.DaysInMonth(date.Month, date.Year));
			lastDay = lastDay.AddDays(-(lastDay.DayOfWeek - date.DayOfWeek));

			if (lastDay.Month != date.Month)
				lastDay = lastDay.AddDays(-7);

			DateTime firstDay = new DateTime(date.Year, date.Month, 1);
			firstDay = firstDay.AddDays(-(firstDay.DayOfWeek - date.DayOfWeek));

			if (firstDay.Month != date.Month)
				firstDay = firstDay.AddDays(7);

			return (lastDay.Day - firstDay.Day) / 7 + 1;
		}

		/// <summary>
		/// Gets the xth weekday
		/// </summary>
		/// <param name="count"></param>
		/// <returns></returns>
		public static int Weekday(int month, int year, int count)
		{
			if (count != 5)
			{
				DateTime firstDay = new DateTime(year, month, 1);

				// Get the first weekday
				firstDay = JumpToWeekday(firstDay);

				// Add the number of days
				firstDay = firstDay.AddDays(count - 1);

				firstDay = JumpToWeekday(firstDay);

				return firstDay.Day;
			}
			else
			{
				DateTime lastDay = new DateTime(year, month, CalendarHelpers.DaysInMonth(month, year));

				if (lastDay.DayOfWeek == System.DayOfWeek.Sunday)
					lastDay = lastDay.AddDays(-2);
				else if (lastDay.DayOfWeek == System.DayOfWeek.Saturday)
					lastDay = lastDay.AddDays(-1);

				return lastDay.Day;
			}
		}

		private static DateTime JumpToWeekday(DateTime date)
		{
			if (date.DayOfWeek == System.DayOfWeek.Saturday)
				date = date.AddDays(2);
			else if (date.DayOfWeek == System.DayOfWeek.Sunday)
				date = date.AddDays(1);

			return date;
		}

		/// <summary>
		/// Gets the xth weekend day
		/// </summary>
		/// <param name="count"></param>
		/// <returns></returns>
		public static int WeekendDay(int month, int year, int count)
		{
			if (count != 5)
			{
				DateTime firstDay = new DateTime(year, month, 1);

				// Get the first weekday
				firstDay = JumpToWeekend(firstDay);

				// Add the number of days
				for (int i = 0; i < count - 1; i++)
				{
					firstDay = firstDay.AddDays(1);
					firstDay = JumpToWeekend(firstDay);
				}

				return firstDay.Day;
			}
			else
			{
				DateTime lastDay = new DateTime(year, month, CalendarHelpers.DaysInMonth(month, year));

				while (lastDay.DayOfWeek != System.DayOfWeek.Saturday && lastDay.DayOfWeek != System.DayOfWeek.Sunday)
					lastDay = lastDay.AddDays(-1);

				return lastDay.Day;
			}
		}

		private static DateTime JumpToWeekend(DateTime date)
		{
			while (date.DayOfWeek != System.DayOfWeek.Saturday && date.DayOfWeek != System.DayOfWeek.Sunday)
				date = date.AddDays(1);

			return date;
		}

		/// <summary>
		/// Gets if a given DateTime is under DST.
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static bool IsDst(DateTime dt)
		{
			return TimeZone.CurrentTimeZone.IsDaylightSavingTime(dt);
		}

		/// <summary>
		/// Gets the date for Easter Sunday based on the year.
		/// http://aa.usno.navy.mil/faq/docs/easter.php
		/// </summary>
		/// <param name="y">The year.</param>
		/// <returns></returns>
		public static DateTime Easter(int y)
		{
			int c = y / 100;
			int n = y - 19 * (y / 19);
			int k = (c - 17) / 25;
			int i = c - c / 4 - (c - k) / 3 + 19 * n + 15;
			i = i - 30 * (i / 30);
			i = i - (i / 28) * (1 - (i / 28) * (29 / (i + 1)) * ((21 - n) / 11));
			int j = y + y / 4 + i + 2 - c + c / 4;
			j = j - 7 * (j / 7);
			int l = i - j;
			int m = 3 + (l + 40) / 44;
			int d = l + 28 - 31 * (m / 4);

			return new DateTime(y, m, d);
		}
		
		/// <summary>
		/// This will return the date for items such as "second Sunday," "last Monday,"
		/// or "third Friday."
		/// </summary>
		/// <param name="year">The year (1 to 9999 inclusive).</param>
		/// <param name="month">The 1-based month (1 to 12 inclusive).</param>
		/// <param name="week">The 1-based week (1 to 5 inclusive).</param>
		/// <param name="day">The day of the week.</param>
		/// <returns></returns>
		public static DateTime GetDateOfOrdinalWeek(int year, int month, int week, DayOfWeek day)
		{
			if (year < 1 || year > 9999)
				throw new ArgumentOutOfRangeException("year");

			if (month < 1 || month > 12)
				throw new ArgumentOutOfRangeException("month");

			if (week < 1 || week > 5)
				throw new ArgumentOutOfRangeException("week");

			int firstDoW = DayOfWeek(year, month, 1) - 1;

			int d = 1 + (week - 1) * 7 + ((int)day - firstDoW) % 7;

			int maxDay = DaysInMonth(month, year);

			if (d > maxDay)
			{
				// There is no fifth DayOfWeek; run the numbers for the fourth week.
				d = 1 + 3 * 7 + ((int)day - firstDoW) % 7;
			}

			return new DateTime(year, month, d);
		}
	}
}
