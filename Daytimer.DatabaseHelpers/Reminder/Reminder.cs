using System;

namespace Daytimer.DatabaseHelpers.Reminder
{
	public class Reminder
	{
		public Reminder()
		{
		}

		private string _id;
		private DateTime _alertStartTime;
		private DateTime? _alertEndTime = null;
		private DateTime? _eventStartDate = null;
		private DateTime? _eventEndDate = null;
		private ReminderType _type;

		public string ID
		{
			get { return _id; }
			set { _id = value; }
		}

		/// <summary>
		/// The start time of the alert.
		/// </summary>
		public DateTime AlertStartTime
		{
			get { return _alertStartTime; }
			set { _alertStartTime = value; }
		}

		/// <summary>
		/// The end time of the alert.
		/// </summary>
		public DateTime? AlertEndTime
		{
			get { return _alertEndTime; }
			set { _alertEndTime = value; }
		}

		/// <summary>
		/// The start date of the event.
		/// </summary>
		public DateTime? EventStartDate
		{
			get { return _eventStartDate; }
			set { _eventStartDate = value; }
		}

		/// <summary>
		/// The end date of the event.
		/// </summary>
		public DateTime? EventEndDate
		{
			get { return _eventEndDate; }
			set { _eventEndDate = value; }
		}

		public ReminderType ReminderType
		{
			get { return _type; }
			set { _type = value; }
		}
	}

	public enum ReminderType { Appointment, Task };
}
