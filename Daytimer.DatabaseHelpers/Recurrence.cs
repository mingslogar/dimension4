using Daytimer.Functions;
using System;
using System.Runtime.Serialization;

namespace Daytimer.DatabaseHelpers
{
	[Serializable]
	public class Recurrence : ISerializable
	{
		#region Constructors

		public Recurrence()
		{

		}

		public Recurrence(Recurrence data)
		{
			CopyFrom(data);
		}

		#endregion

		#region Fields

		private RepeatType _type = RepeatType.Weekly;

		private string _day = "-1";	// For RepeatType.Daily, -1 means every weekday.
		private int _week = -1;
		private int _month = -1;
		private int _year = -1;

		private RepeatEnd _end = RepeatEnd.None;

		private int _endCount = -1;
		private DateTime _endDate;

		/// <summary>
		/// We want to delete an occurrence of this appointment for
		/// certain days.
		/// </summary>
		private DateTime[] _skip = null;

		public RepeatType Type
		{
			get { return _type; }
			set { _type = value; }
		}

		/// <summary>
		/// For RepeatType.Daily, -1 means every weekday.
		/// </summary>
		public string Day
		{
			get { return _day; }
			set { _day = value; }
		}

		public int Week
		{
			get { return _week; }
			set { _week = value; }
		}

		public int Month
		{
			get { return _month; }
			set { _month = value; }
		}

		public int Year
		{
			get { return _year; }
			set { _year = value; }
		}

		public RepeatEnd End
		{
			get { return _end; }
			set { _end = value; }
		}

		public int EndCount
		{
			get { return _endCount; }
			set { _endCount = value; }
		}

		public DateTime EndDate
		{
			get { return _endDate; }
			set { _endDate = value; }
		}

		public DateTime[] Skip
		{
			get { return _skip; }
			set { _skip = value; }
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets a friendly name for the recurrence data.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			string data = "";

			switch (_type)
			{
				case RepeatType.Daily:
					data += "every ";

					switch (_day)
					{
						case "-1":
							data += "weekday";
							break;

						case "1":
							data += "day";
							break;

						case "2":
							data += "other day";
							break;

						default:
							data += _day + " days";
							break;
					}
					break;

				case RepeatType.Weekly:
					data += "every ";

					switch (_week)
					{
						case 1:
							break;

						case 2:
							data += "other ";
							break;

						default:
							data += _week.ToString() + " weeks on ";
							break;
					}

					data += Join<DayOfWeek>(Array.ConvertAll<char, DayOfWeek>(_day.ToCharArray(), StringToDayOfWeek));
					break;

				case RepeatType.Monthly:
					if (_week == -1)
						data += "day " + _day + " of every ";
					else
						data += "the " + OrdinalWeek(_week) + " " + StringToDayOfWeek(_day[0]).ToString() + " of every ";

					switch (_month)
					{
						case 1:
							data += "month";
							break;

						case 2:
							data += "other month";
							break;

						default:
							data += _month.ToString() + " months";
							break;
					}
					break;

				case RepeatType.Yearly:
					switch (_year)
					{
						case 1:
							break;

						case 2:
							data += "every other year on ";
							break;

						default:
							data += "every " + _year.ToString() + " years on ";
							break;
					}

					if (_week == -1)
						data += CalendarHelpers.Month(_month) + " " + _day;
					else
						data += "the " + OrdinalWeek(_week) + " " + StringToDayOfWeek(_day[0]).ToString() + " of " + CalendarHelpers.Month(_month);
					break;
			}

			if (_end == RepeatEnd.Count)
				data += " for " + _endCount.ToString() + " occurrences";
			else if (_end == RepeatEnd.Date)
				data += " until " + _endDate.ToString("MMMM d, yyyy");

			if (_skip != null)
				data += ", skipping " + Join<DateTime>(_skip);

			return data;
		}

		private DayOfWeek StringToDayOfWeek(char input)
		{
			return (DayOfWeek)(input - 48);
		}

		private string Join<T>(T[] data)
		{
			string result = "";

			int count = data.Length;

			if (count > 2)
			{
				for (int i = 0; i < count - 1; i++)
					result += data[i].ToString() + ", ";

				result += " and " + data[count - 1].ToString();
			}
			else if (count == 2)
				result = data[0].ToString() + " and " + data[1].ToString();
			else
				result = data[0].ToString();

			return result;
		}

		private string OrdinalWeek(int number)
		{
			switch (number)
			{
				case 0:
					return "first";

				case 1:
					return "second";

				case 2:
					return "third";

				case 3:
					return "fourth";

				case 4:
					return "last";

				default:
					throw (new ArgumentOutOfRangeException("Number must be between 0 and 4 inclusive."));
			}
		}

		private bool CalculateRepeatWeek(DateTime date)
		{
			int _repeatDayInt = int.Parse(_day);

			if (_repeatDayInt == 0)
			{
				// Xth Day
				if (_week < 4)
					return _week + 1 == date.Day;
				else
					return CalendarHelpers.DaysInMonth(date.Month, date.Year) == date.Day;
			}
			else if (_repeatDayInt == 1)
			{
				// Xth Weekday
				if (date.DayOfWeek != DayOfWeek.Sunday && date.DayOfWeek != DayOfWeek.Saturday)
					return date.Day == CalendarHelpers.Weekday(date.Month, date.Year, _week + 1);
			}
			else if (_repeatDayInt == 2)
			{
				// Xth Weekend Day
				if (date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday)
					return date.Day == CalendarHelpers.WeekendDay(date.Month, date.Year, _week + 1);
			}
			else
			{
				// Xth Day of Week
				int dayOfWeek = CalendarHelpers.DayOfWeekNumber(date);

				return (int)date.DayOfWeek == _repeatDayInt - 3 &&
					(dayOfWeek == _week + 1 ||
					(_week == 4 && dayOfWeek == CalendarHelpers.NumberOfWeeks(date)));
			}

			return false;
		}

		public bool OccursOnDate(DateTime date, DateTime startDate, DateTime endDate, bool allDay, out DateTime? representingDate)
		{
			int length = (int)(endDate.Date - startDate.Date).TotalDays;
			DateTime _baseDate = date;
			DateTime start = startDate.Date;

			if (allDay || endDate.TimeOfDay.TotalSeconds == 0)
				length--;

			for (int i = 0; i <= length; i++)
			{
				date = _baseDate.AddDays(-i);
				bool? occurs = occursOnDate(date, start, out representingDate);

				if (occurs.HasValue)
					return occurs.Value;
			}

			representingDate = null;
			return false;
		}

		/// <summary>
		/// Check if the current appointment recurrence pattern will overlap a certain day.
		/// </summary>
		/// <param name="date"></param>
		/// <param name="start"></param>
		/// <returns></returns>
		private bool? occursOnDate(DateTime date, DateTime start, out DateTime? representingDate)
		{
			representingDate = null;

			if (_skip != null && Array.IndexOf(_skip, date) != -1)
				return false;

			if (date < start)
				return false;

			if (_end != RepeatEnd.None)
			{
				if (date > _endDate)
					return false;
			}

			switch (_type)
			{
				case RepeatType.Daily:
					{
						if (_day == "-1")
						{
							if (date.DayOfWeek != DayOfWeek.Sunday && date.DayOfWeek != DayOfWeek.Saturday)
							{
								representingDate = date;
								return true;
							}
						}
						else
							if (date.Subtract(start).Days % int.Parse(_day) == 0)
							{
								representingDate = date;
								return true;
							}
					}
					break;

				case RepeatType.Weekly:
					{
						if (_day.Contains(((int)date.DayOfWeek).ToString()))
						{
							// Normalize to beginning of week.
							DateTime startSunday = CalendarHelpers.FirstDayOfWeek(start);
							DateTime dateSunday = CalendarHelpers.FirstDayOfWeek(date);

							if ((dateSunday.Subtract(startSunday).Days / 7) % _week == 0)
							{
								representingDate = date;
								return true;
							}
						}
					}
					break;

				case RepeatType.Monthly:
					{
						int _repeatDayInt = int.Parse(_day);

						if (_week == -1)
						{
							if (date.Day == _repeatDayInt &&
								CalendarHelpers.CalculateMonthDifference(start, date) % _month == 0)
							{
								representingDate = date;
								return true;
							}
						}
						else
						{
							if (CalendarHelpers.CalculateMonthDifference(start, date) % _month == 0)
								if (CalculateRepeatWeek(date))
								{
									representingDate = date;
									return true;
								}
						}
					}
					break;

				case RepeatType.Yearly:
					{
						if ((date.Year - start.Year) % _year == 0 && date.Month == _month + 1)
						{
							int _repeatDayInt = int.Parse(_day);

							if (_week == -1)
							{
								if (date.Day == _repeatDayInt ||

									// If a recurring appointment was set for Feb. 29, display as
									// Feb. 28 for non-leap years.
									(date.Month == 2 && date.Day == 28 && _repeatDayInt == 29 && !CalendarHelpers.IsLeapYear(date.Year)))
								{
									representingDate = date;
									return true;
								}
							}
							else
								if (CalculateRepeatWeek(date))
								{
									representingDate = date;
									return true;
								}
						}
					}
					break;
			}

			return null;
		}

		public DateTime? GetPreviousRecurrence(DateTime startDate, DateTime endDate, bool allDay, DateTime reference, out DateTime? representingDate, DateTime? minValue = null)
		{
			representingDate = null;

			if (reference < startDate)
				return null;

			if (_end != RepeatEnd.None)
			{
				if (reference > _endDate)
					return null;
			}

			if (minValue.HasValue)
			{
				DateTime _min = minValue.Value;

				while (reference > startDate && reference > _min)
				{
					reference = reference.AddDays(-1);

					if (OccursOnDate(reference, startDate, endDate, allDay, out representingDate))
						return reference;
				}
			}
			else
			{
				while (reference > startDate)
				{
					reference = reference.AddDays(-1);

					if (OccursOnDate(reference, startDate, endDate, allDay, out representingDate))
						return reference;
				}
			}

			return null;
		}

		public DateTime? GetNextRecurrence(DateTime startDate, DateTime endDate, bool allDay, DateTime reference, out DateTime? representingDate, DateTime? maxValue = null)
		{
			representingDate = null;

			if (reference < startDate)
				return null;

			if (_end != RepeatEnd.None)
			{
				if (reference > _endDate)
					return null;

				if (maxValue.HasValue)
				{
					DateTime _max = maxValue.Value;

					while (reference < _endDate && reference < _max)
					{
						reference = reference.AddDays(1);

						if (OccursOnDate(reference, startDate, endDate, allDay, out representingDate))
							return reference;
					}
				}
				else
				{
					while (reference < _endDate)
					{
						reference = reference.AddDays(1);

						if (OccursOnDate(reference, startDate, endDate, allDay, out representingDate))
							return reference;
					}
				}
			}

			DateTime max = DateTime.MaxValue.Date;

			while (reference < max)
			{
				reference = reference.AddDays(1);

				if (OccursOnDate(reference, startDate, endDate, allDay, out representingDate))
					return reference;
			}

			return null;
		}

		/// <summary>
		/// Gets the number of occurrences which will have passed by a certain date.
		/// </summary>
		/// <returns></returns>
		public int Occurrence(DateTime representingDate, DateTime startDate)
		{
			if (_type != RepeatType.Yearly)
				return -1;

			return (representingDate.Year - startDate.Year) / _year;
		}

		public override bool Equals(object obj)
		{
			return this == (Recurrence)obj;
		}

		public static bool operator ==(Recurrence r1, Recurrence r2)
		{
			if ((object.Equals(r1, null) && !object.Equals(r2, null))
				|| !object.Equals(r1, null) && object.Equals(r2, null))
				return false;

			if (object.Equals(r1, null) && object.Equals(r2, null))
				return true;

			return r1._type == r2._type
				&& r1._day == r2._day
				&& r1._week == r2._week
				&& r1._month == r2._month
				&& r1._year == r2._year
				&& r1._end == r2._end
				&& r1._endCount == r2._endCount
				&& r1._endDate == r2._endDate
				&& r1._skip == r2._skip;
		}

		public static bool operator !=(Recurrence r1, Recurrence r2)
		{
			return !(r1 == r2);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion

		#region Private Methods

		private void CopyFrom(Recurrence data)
		{
			_type = data._type;
			_day = data._day;
			_week = data._week;
			_month = data._month;
			_year = data._year;
			_end = data._end;
			_endCount = data._endCount;
			_skip = data._skip;
		}

		#endregion

		#region Serialization

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(AppointmentDatabase.RepeatDayAttribute, _day);
			info.AddValue(AppointmentDatabase.RepeatEndAttribute, (byte)_end);
			info.AddValue(AppointmentDatabase.RepeatEndCountAttribute, _endCount);
			info.AddValue(AppointmentDatabase.RepeatEndDateAttribute, _endDate);
			info.AddValue(AppointmentDatabase.RepeatMonthAttribute, _month);
			info.AddValue(AppointmentDatabase.RepeatTypeAttribute, (byte)_type);
			info.AddValue(AppointmentDatabase.RepeatWeekAttribute, _week);
			info.AddValue(AppointmentDatabase.RepeatYearAttribute, _year);
			info.AddValue(AppointmentDatabase.RepeatSkipAttribute, _skip);
		}

		protected Recurrence(SerializationInfo info, StreamingContext context)
		{
			_day = info.GetString(AppointmentDatabase.RepeatDayAttribute);
			_end = (RepeatEnd)info.GetByte(AppointmentDatabase.RepeatEndAttribute);
			_endCount = info.GetInt32(AppointmentDatabase.RepeatEndCountAttribute);
			_endDate = info.GetDateTime(AppointmentDatabase.RepeatEndDateAttribute);
			_month = info.GetInt32(AppointmentDatabase.RepeatMonthAttribute);
			_type = (RepeatType)info.GetByte(AppointmentDatabase.RepeatTypeAttribute);
			_week = info.GetInt32(AppointmentDatabase.RepeatWeekAttribute);
			_year = info.GetInt32(AppointmentDatabase.RepeatYearAttribute);
			_skip = (DateTime[])info.GetValue(AppointmentDatabase.RepeatSkipAttribute, typeof(DateTime[]));
		}

		#endregion
	}
}
