using Daytimer.DatabaseHelpers;
using Daytimer.Functions;
using DDay.iCal;
using RecurrenceGenerator;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Daytimer.ICalendarHelpers
{
	public static class Extensions
	{
		#region Constants

		/// <summary>
		/// yyyyMMdd
		/// </summary>
		private const string DateFormat = "yyyyMMdd";

		/// <summary>
		/// yyyyMMddTHHmmss
		/// </summary>
		private const string DateTimeLocalFormat = DateFormat + "THHmmss";

		/// <summary>
		/// yyyyMMddTHHmmssZ
		/// </summary>
		private const string DateTimeZuluFormat = DateTimeLocalFormat + "Z";

		/// <summary>
		/// DTSTART
		/// </summary>
		private const string DTSTART = "DTSTART";

		/// <summary>
		/// DTEND
		/// </summary>
		private const string DTEND = "DTEND";

		/// <summary>
		/// DTSTART;VALUE=DATE
		/// </summary>
		private const string DTSTART_VALUE_DATE = DTSTART + ";VALUE=DATE";

		/// <summary>
		/// DTEND;VALUE=DATE
		/// </summary>
		private const string DTEND_VALUE_DATE = DTEND + ";VALUE=DATE";

		/// <summary>
		/// TZID
		/// </summary>
		private const string TZID = "TZID";

		/// <summary>
		/// DTSTART;TZID=
		/// </summary>
		private const string DTSTART_TZID_PREFIX = DTSTART + ";" + TZID + "=";

		/// <summary>
		/// DTEND;TZID=
		/// </summary>
		private const string DTEND_TZID_PREFIX = DTEND + ";" + TZID + "=";

		/// <summary>
		/// TZOFFSETFROM
		/// </summary>
		private const string TZOFFSETFROM = "TZOFFSETFROM";

		/// <summary>
		/// TZOFFSETTO
		/// </summary>
		private const string TZOFFSETTO = "TZOFFSETTO";

		/// <summary>
		/// RRULE
		/// </summary>
		private const string RRULE = "RRULE";

		/// <summary>
		/// TZNAME
		/// </summary>
		private const string TZNAME = "TZNAME";

		#endregion

		/// <summary>
		/// Set the recurrence on an appointment based on an RFC2445-formatted string.
		/// </summary>
		/// <param name="appt"></param>
		/// <param name="rValue"></param>
		public static void SetRecurrenceValues(this Appointment appt, string rValue)
		{
			if (string.IsNullOrWhiteSpace(rValue))
			{
				appt.IsRepeating = false;
				return;
			}

			if (rValue == null || rValue.Length == 0)
			{
				appt.IsRepeating = false;
				return;
			}

			try
			{
				Dictionary<string, string> fields = Parser.Fields(rValue);

				if (fields.ContainsKey(DTSTART_VALUE_DATE))
				{
					appt.StartDate = DateTime.ParseExact(
						fields[DTSTART_VALUE_DATE],
						DateFormat,
						CultureInfo.InvariantCulture
					);

					appt.EndDate = DateTime.ParseExact(
						fields[DTEND_VALUE_DATE],
						DateFormat,
						CultureInfo.InvariantCulture
					);

					appt.AllDay = true;
				}
				else
				{
					string tzid = fields[TZID];

					if (fields.ContainsKey(DTSTART_TZID_PREFIX + tzid))
					{
						TimeSpan offset = SplitTimeString(
							fields[CalendarHelpers.IsDst(DateTime.Now) ? TZOFFSETTO : TZOFFSETFROM]
						);

						appt.StartDate = DateTime.ParseExact(
							fields[DTSTART_TZID_PREFIX + tzid],
							DateTimeLocalFormat,
							CultureInfo.InvariantCulture
						).Subtract(offset).ToLocalTime();

						appt.EndDate = DateTime.ParseExact(
							fields[DTEND_TZID_PREFIX + tzid],
							DateTimeLocalFormat,
							CultureInfo.InvariantCulture
						).Subtract(offset).ToLocalTime();
					}
					else
					{
						appt.StartDate = Parse(fields[DTSTART]);
						appt.EndDate = Parse(fields[DTEND]);
					}

					appt.AllDay = false;
				}

				string rrule = fields[RRULE];

				RecurrencePattern pattern = new RecurrencePattern(rrule);
				appt.IsRepeating = true;

				Recurrence recur = appt.Recurrence;

				#region RepeatType

				switch (pattern.Frequency)
				{
					case FrequencyType.Daily:
						recur.Type = RepeatType.Daily;
						recur.Day = pattern.Interval.ToString();
						break;

					case FrequencyType.Weekly:
						recur.Type = RepeatType.Weekly;
						recur.Week = pattern.Interval;

						if (pattern.ByDay == null)
							recur.Day = "0123456";
						else
						{
							string r = "";

							foreach (IWeekDay day in pattern.ByDay)
								r += ((int)day.DayOfWeek).ToString();

							recur.Day = r;
						}
						break;

					case FrequencyType.Monthly:
						recur.Type = RepeatType.Monthly;
						recur.Month = pattern.Interval;

						if (pattern.ByMonthDay != null)
						{
							recur.Week = -1;
							recur.Day = pattern.ByMonthDay[0].ToString();
						}
						else
						{
							recur.Week = pattern.ByDay[0].Offset;
							recur.Day = ((int)pattern.ByDay[0].DayOfWeek).ToString();
						}
						break;

					case FrequencyType.Yearly:
						recur.Type = RepeatType.Yearly;
						recur.Year = pattern.Interval;

						if (pattern.ByDay == null)
						{
							recur.Week = -1;
							recur.Day = appt.StartDate.Day.ToString();
							recur.Month = pattern.ByMonth != null ? pattern.ByMonth[0] : appt.StartDate.Month;
						}
						else
						{
							recur.Week = pattern.ByDay[0].Offset;
							recur.Day = ((int)pattern.ByDay[0].DayOfWeek).ToString();
							recur.Month = pattern.ByMonth != null ? pattern.ByMonth[0] : appt.StartDate.Month;
						}
						break;

					default:
						// Unsupported recurrence type
						throw (new ArgumentException("Frequency of recurring appointment is not supported."));
				}

				#endregion

				#region RepeatEnd

				if (pattern.Count.HasValue)
				{
					recur.End = RepeatEnd.Count;
					recur.EndCount = pattern.Count.Value;
					appt.CalculateRecurrenceEnd();
				}
				else if (pattern.Until.HasValue)
				{
					recur.End = RepeatEnd.Date;
					recur.EndDate = pattern.Until.Value;
				}
				else
					recur.End = RepeatEnd.None;

				#endregion
			}
			catch
			{
				// Invalid recurrence pattern
				appt.IsRepeating = false;
			}
		}

		//private static DateTime SplitDateString(string yyyymmdd)
		//{
		//	return new DateTime(
		//		int.Parse(yyyymmdd.Substring(0, 4)),
		//		int.Parse(yyyymmdd.Substring(4, 2)),
		//		int.Parse(yyyymmdd.Substring(6, 2))
		//	);
		//}

		private static TimeSpan SplitTimeString(string hhmm)
		{
			if (hhmm[0] == '-')
				return new TimeSpan(
					-int.Parse(hhmm.Substring(1, 2)),
					-int.Parse(hhmm.Substring(3, 2)),
					0
				);
			else
				return new TimeSpan(
					int.Parse(hhmm.Substring(0, 2)),
					int.Parse(hhmm.Substring(2, 2)),
					0
				);
		}

		private static string JoinTimeSpan(TimeSpan ts)
		{
			return ((ts < TimeSpan.Zero) ? "-" : "") + ts.ToString("hhmm");
		}

		private static void CalculateRecurrenceEnd(this Appointment appt)
		{
			Recurrence recur = appt.Recurrence;

			int occurrences = recur.EndCount;
			DateTime start = appt.StartDate;

			RecurrenceValues values = null;

			if (recur.Type == RepeatType.Daily)
			{
				DailyRecurrenceSettings daily = new DailyRecurrenceSettings(start, occurrences);

				if (recur.Day != "-1")
					values = daily.GetValues(int.Parse(recur.Day), DailyRegenType.OnEveryXDays);
				else
					values = daily.GetValues(1, DailyRegenType.OnEveryWeekday);

				recur.EndCount = daily.NumberOfOccurrences;
			}
			else if (recur.Type == RepeatType.Weekly)
			{
				WeeklyRecurrenceSettings weekly = new WeeklyRecurrenceSettings(start, occurrences);

				SelectedDayOfWeekValues sdwv = new SelectedDayOfWeekValues();
				sdwv.Sunday = recur.Day.IndexOf('0') != -1;
				sdwv.Monday = recur.Day.IndexOf('1') != -1;
				sdwv.Tuesday = recur.Day.IndexOf('2') != -1;
				sdwv.Wednesday = recur.Day.IndexOf('3') != -1;
				sdwv.Thursday = recur.Day.IndexOf('4') != -1;
				sdwv.Friday = recur.Day.IndexOf('5') != -1;
				sdwv.Saturday = recur.Day.IndexOf('6') != -1;

				values = weekly.GetValues(recur.Week, sdwv);

				recur.EndCount = weekly.NumberOfOccurrences;
			}
			else if (recur.Type == RepeatType.Monthly)
			{
				MonthlyRecurrenceSettings monthly = new MonthlyRecurrenceSettings(start, occurrences);

				if (recur.Week == -1)
					values = monthly.GetValues(int.Parse(recur.Day), recur.Month);
				else
					values = monthly.GetValues((MonthlySpecificDatePartOne)recur.Week, (MonthlySpecificDatePartTwo)int.Parse(recur.Day), recur.Month);

				recur.EndCount = monthly.NumberOfOccurrences;
			}
			else if (recur.Type == RepeatType.Yearly)
			{
				YearlyRecurrenceSettings yearly = new YearlyRecurrenceSettings(start, occurrences);

				if (recur.Week == -1)
					values = yearly.GetValues(int.Parse(recur.Day), recur.Month + 1, recur.Year);
				else
					values = yearly.GetValues((YearlySpecificDatePartOne)recur.Week, (YearlySpecificDatePartTwo)int.Parse(recur.Day), (YearlySpecificDatePartThree)(recur.Month + 1), recur.Year);

				recur.EndCount = yearly.NumberOfOccurrences;
			}

			recur.EndDate = values.EndDate;
		}

		/// <summary>
		/// Return an RFC2445 representation of the recurrence value of an appointment.
		/// </summary>
		/// <param name="appt">The appointment which is to be parsed.</param>
		/// <returns></returns>
		public static string[] GetRecurrenceValues(this Appointment appt)
		{
			if (!appt.IsRepeating)
				return null;

			RecurrencePattern pattern = new RecurrencePattern();
			Recurrence recur = appt.Recurrence;

			if (recur.Type == RepeatType.Daily)
			{
				if (recur.Day != "-1")
				{
					pattern.Frequency = FrequencyType.Daily;
					pattern.Interval = int.Parse(recur.Day);
				}
				else
				{
					pattern.Frequency = FrequencyType.Weekly;
					pattern.ByDay = new List<IWeekDay>(new IWeekDay[]{
						new WeekDay(DayOfWeek.Monday),
						new WeekDay(DayOfWeek.Tuesday),
						new WeekDay(DayOfWeek.Wednesday),
						new WeekDay(DayOfWeek.Thursday),
						new WeekDay(DayOfWeek.Friday)
					});
				}
			}
			else if (recur.Type == RepeatType.Weekly)
			{
				pattern.Frequency = FrequencyType.Weekly;
				pattern.Interval = recur.Week;

				pattern.ByDay = new List<IWeekDay>();

				foreach (char each in recur.Day)
					pattern.ByDay.Add(new WeekDay((DayOfWeek)int.Parse(each.ToString())));
			}
			else if (recur.Type == RepeatType.Monthly)
			{
				pattern.Frequency = FrequencyType.Monthly;
				pattern.Interval = recur.Month;

				if (recur.Week == -1)
					pattern.ByMonthDay = new List<int>(new int[] { int.Parse(recur.Day) });
				else
					pattern.ByDay = new List<IWeekDay>(new WeekDay[] {
						new WeekDay((DayOfWeek)int.Parse(recur.Day), recur.Week)
					});
			}
			else if (recur.Type == RepeatType.Yearly)
			{
				pattern.Frequency = FrequencyType.Yearly;
				pattern.Interval = recur.Year;

				if (recur.Week != -1)
					pattern.ByDay = new List<IWeekDay>(new WeekDay[] {
						new WeekDay((DayOfWeek)int.Parse(recur.Day), recur.Week)
					});

				pattern.ByMonth = new List<int>(new int[] { recur.Month });
			}

			if (appt.AllDay)
				return AllDayPattern(appt, pattern);
			else
				return PartialDayPattern(appt, pattern);
		}

		//private static string AllDayPattern(Appointment appt, RecurrencePattern pattern)
		//{
		//	return DTSTART_VALUE_DATE + ":" + appt.StartDate.Date.ToString(DateFormat) + "\r\n"
		//		+ DTEND_VALUE_DATE + ":" + appt.EndDate.Date.ToString(DateFormat) + "\r\n"
		//		+ RRULE + ":" + pattern.ToString();
		//}

		private static string[] AllDayPattern(Appointment appt, RecurrencePattern pattern)
		{
			return new string[]
			{
				DTSTART_VALUE_DATE + ":" + appt.StartDate.Date.ToString(DateFormat),
				DTEND_VALUE_DATE + ":" + appt.EndDate.Date.ToString(DateFormat),
				RRULE + ":" + pattern.ToString()
			};
		}

		//private static string PartialDayPattern(Appointment appt, RecurrencePattern pattern)
		//{
		//	TimeZoneInfo local = TimeZoneInfo.Local;
		//	string olson = OlsonConverter.TimeZoneInfoToOlsonTimeZone(local);

		//	return DTSTART_TZID_PREFIX + olson + ":" + appt.StartDate.ToString(DateTimeLocalFormat) + "\r\n"
		//		+ DTEND_TZID_PREFIX + olson + ":" + appt.EndDate.ToString(DateTimeLocalFormat) + "\r\n"
		//		+ RRULE + ":" + pattern.ToString() + "\r\n"
		//		+ "BEGIN:VTIMEZONE" + "\r\n"
		//		+ TZID + ":" + olson + "\r\n"
		//		+ "X-LIC-LOCATION:" + olson + "\r\n"
		//		+ "BEGIN:DAYLIGHT\r\n"
		//		+ TZOFFSETFROM + ":" + JoinTimeSpan(local.BaseUtcOffset) + "\r\n"
		//		+ TZOFFSETTO + ":" + JoinTimeSpan(local.BaseUtcOffset.Add(TimeSpan.FromHours(1))) + "\r\n"
		//		+ TZNAME + ":" + local.DaylightName.Acronym() + "\r\n"
		//		+ DTSTART + ":19700308T020000\r\n"
		//		+ RRULE + ":FREQ=YEARLY;BYMONTH=3;BYDAY=2SU\r\n"
		//		+ "END:DAYLIGHT\r\n"
		//		+ "BEGIN:STANDARD\r\n"
		//		+ TZOFFSETFROM + ":" + JoinTimeSpan(local.BaseUtcOffset.Add(TimeSpan.FromHours(1))) + "\r\n"
		//		+ TZOFFSETTO + ":" + JoinTimeSpan(local.BaseUtcOffset) + "\r\n"
		//		+ TZNAME + ":" + local.StandardName.Acronym() + "\r\n"
		//		+ DTSTART + ":19701101T020000\r\n"
		//		+ RRULE + ":FREQ=YEARLY;BYMONTH=11;BYDAY=1SU\r\n"
		//		+ "END:STANDARD\r\n"
		//		+ "END:VTIMEZONE";
		//}

		private static string[] PartialDayPattern(Appointment appt, RecurrencePattern pattern)
		{
			TimeZoneInfo local = TimeZoneInfo.Local;
			string olson = OlsonConverter.TimeZoneInfoToOlsonTimeZone(local);

			return new string[]
			{
				DTSTART_TZID_PREFIX + olson + ":" + appt.StartDate.ToString(DateTimeLocalFormat),
				DTEND_TZID_PREFIX + olson + ":" + appt.EndDate.ToString(DateTimeLocalFormat),
				RRULE + ":" + pattern.ToString(),
				"BEGIN:VTIMEZONE" , TZID + ":" + olson,
				"X-LIC-LOCATION:" + olson,
				"BEGIN:DAYLIGHT",
				TZOFFSETFROM + ":" + JoinTimeSpan(local.BaseUtcOffset),
				TZOFFSETTO + ":" + JoinTimeSpan(local.BaseUtcOffset.Add(TimeSpan.FromHours(1))),
				TZNAME + ":" + local.DaylightName.Acronym(),
				DTSTART + ":19700308T020000",
				RRULE + ":FREQ=YEARLY;BYMONTH=3;BYDAY=2SU",
				"END:DAYLIGHT",
				"BEGIN:STANDARD",
				TZOFFSETFROM + ":" + JoinTimeSpan(local.BaseUtcOffset.Add(TimeSpan.FromHours(1))),
				TZOFFSETTO + ":" + JoinTimeSpan(local.BaseUtcOffset) ,
				TZNAME + ":" + local.StandardName.Acronym(),
				DTSTART + ":19701101T020000",
				RRULE + ":FREQ=YEARLY;BYMONTH=11;BYDAY=1SU",
				"END:STANDARD",
				"END:VTIMEZONE"
			};
		}

		private static DateTime Parse(string dateString)
		{
			if (dateString.EndsWith("Z"))
				return DateTime.ParseExact(dateString, DateTimeZuluFormat, CultureInfo.InvariantCulture).ToLocalTime();
			else
				return DateTime.ParseExact(dateString, DateTimeLocalFormat, CultureInfo.InvariantCulture);
		}
	}
}