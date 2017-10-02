using Daytimer.DatabaseHelpers.Recovery;
using Daytimer.Functions;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Daytimer.DatabaseHelpers
{
	[Serializable]
	public class Appointment : SyncableDatabaseObject, ISerializable
	{
		#region Constructors

		public Appointment()
			: base()
		{

		}

		public Appointment(bool generateID)
			: base(generateID)
		{

		}

		public Appointment(TimeSpan reminder)
		{
			_reminder = reminder;
			_id = IDGenerator.GenerateID();
		}

		public Appointment(Appointment appointment)
		{
			CopyFrom(appointment);
		}

		public Appointment(Appointment appointment, bool saveChangesToDisk)
		{
			CopyFrom(appointment);
			_saveChangesToDisk = saveChangesToDisk;
		}

		#endregion

		#region Properties

		private DateTime _startDate;
		private DateTime _endDate;
		private string _subject = "";
		private string _location = "";
		private bool _allDay = true;
		private TimeSpan _reminder = TimeSpan.FromSeconds(-1);
		private Priority _priority = Priority.Normal;
		private string _categoryID = "";
		private string _owner = "";
		private string _calendarUrl = "";
		private bool _readOnly = false;
		private bool _private = false;
		private ShowAs _showAs;

		private bool _saveChangesToDisk = true;
		private FlowDocument _detailsDocument = null;

		#region Repeating

		// We want this attribute just for a quick reference for the appointment editor
		private bool _isRepeating = false;

		private Recurrence _recurrence = new Recurrence();

		/// <summary>
		/// We want to group repeating appointments by ID so that
		/// changes can be made to certain appointments which affect
		/// only that appointment, but it is still linked to the group.
		/// </summary>
		private string _repeatId = null;

		#endregion

		public DateTime StartDate
		{
			get { return _startDate; }
			set { _startDate = value; }
		}

		public DateTime EndDate
		{
			get { return _endDate; }
			set { _endDate = value; }
		}

		public string Subject
		{
			get { return _subject; }
			set { _subject = value; }
		}

		public string FormattedSubject
		{
			get
			{
				string subj = _subject;

				if (subj.Contains("%s"))
				{
					int occurrence = Occurrence();

					switch (occurrence)
					{
						case -1:
							return subj;

						case 0:
							if (subj.Contains("%s "))
								return subj.Replace("%s ", "");
							else
								return subj.Replace("%s", "");

						default:
							break;
					}

					string count = occurrence.ToString();
					char last = count[count.Length - 1];

					if (!(occurrence % 100 > 10 && occurrence % 100 < 14))
						switch (last)
						{
							case '1':
								count += "st";
								break;

							case '2':
								count += "nd";
								break;

							case '3':
								count += "rd";
								break;

							default:
								count += "th";
								break;
						}
					else
						count += "th";

					subj = subj.Replace("%s", count);
				}

				return subj;
			}
		}

		public string Location
		{
			get { return _location; }
			set { _location = value; }
		}

		public bool AllDay
		{
			get { return _allDay; }
			set { _allDay = value; }
		}

		public TimeSpan Reminder
		{
			get { return _reminder; }
			set { _reminder = value; }
		}

		/// <summary>
		/// Gets the text contents of the details document.
		/// </summary>
		public string Details
		{
			get
			{
				FlowDocument doc = DetailsDocument;

				if (doc != null)
					return new TextRange(doc.ContentStart, doc.ContentEnd).Text;
				else
					return null;
			}
		}

		/// <summary>
		/// Gets or sets the details document.
		/// </summary>
		public FlowDocument DetailsDocument
		{
			get
			{
				if (_saveChangesToDisk)
					return new FlowDocumentStorage(AppointmentDatabase.AppointmentsAppData + "\\" + _id).DocumentValue;
				else
					return _detailsDocument;
			}
			set
			{
				if (_saveChangesToDisk)
					new FlowDocumentStorage(AppointmentDatabase.AppointmentsAppData + "\\" + _id).DocumentValue = value;
				else
					_detailsDocument = value;
			}
		}

		/// <summary>
		/// Get the details document asynchronously.
		/// </summary>
		/// <returns></returns>
		public async Task<FlowDocument> GetDetailsDocumentAsync()
		{
			if (_saveChangesToDisk)
				return await new FlowDocumentStorage(AppointmentDatabase.AppointmentsAppData + "\\" + _id).GetDocumentValueAsync();
			else
				return _detailsDocument;
		}

		/// <summary>
		/// Set the details document asynchronously.
		/// </summary>
		/// <returns></returns>
		public async Task SetDetailsDocumentAsync(FlowDocument value)
		{
			if (_saveChangesToDisk)
				await new FlowDocumentStorage(AppointmentDatabase.AppointmentsAppData + "\\" + _id).SetDocumentValueAsync(value);
			else
				_detailsDocument = value;
		}

		public Priority Priority
		{
			get { return _priority; }
			set { _priority = value; }
		}

		public Category Category
		{
			get
			{
				Category category = AppointmentDatabase.GetCategory(_categoryID);

				if (category == null)
				{
					category = new Category(false);
					category.ID = _categoryID;
					category.ExistsInDatabase = false;
				}

				return category;
			}
			set { _categoryID = value.ID; }
		}

		public string CategoryID
		{
			get { return _categoryID; }
			set { _categoryID = value; }
		}

		public string Owner
		{
			get { return _owner; }
			set { _owner = value; }
		}

		public string CalendarUrl
		{
			get { return _calendarUrl; }
			set { _calendarUrl = value; }
		}

		public bool ReadOnly
		{
			get { return _readOnly; }
			set { _readOnly = value; }
		}

		public bool Private
		{
			get { return _private; }
			set { _private = value; }
		}

		public ShowAs ShowAs
		{
			get { return _showAs; }
			set { _showAs = value; }
		}

		public bool SaveChangesToDisk
		{
			get { return _saveChangesToDisk; }
			set { _saveChangesToDisk = value; }
		}

		#endregion

		#region Repeating

		public bool IsRepeating
		{
			get { return _isRepeating; }
			set { _isRepeating = value; }
		}

		public Recurrence Recurrence
		{
			get { return _recurrence; }
			set { _recurrence = value; }
		}

		public string RepeatID
		{
			get { return _repeatId; }
			set { _repeatId = value; }
		}

		/// <summary>
		/// The date this occurrence of the series is representing
		/// </summary>
		public DateTime RepresentingDate;

		/// <summary>
		/// For use by AppointmentEditor to specify if only this appointment in the
		/// series should be edited.
		/// </summary>
		public bool RepeatIsExceptionToRule = false;

		#endregion

		#region Functions

		public void CopyFrom(Appointment appointment)
		{
			base.CopyFrom(appointment);

			_allDay = appointment._allDay;
			_startDate = appointment._startDate;
			_endDate = appointment._endDate;
			_isRepeating = appointment._isRepeating;
			_location = appointment._location;
			_reminder = appointment._reminder;
			_priority = appointment._priority;
			_categoryID = appointment._categoryID;
			_owner = appointment._owner;
			_calendarUrl = appointment._calendarUrl;
			_readOnly = appointment._readOnly;
			_private = appointment._private;
			_lastModified = appointment._lastModified;
			_showAs = appointment._showAs;
			_recurrence = new Recurrence(appointment._recurrence);
			_repeatId = appointment._repeatId;
			_subject = appointment._subject;
		}

		public bool OccursOnDate(DateTime date)
		{
			DateTime? representingDate;
			bool result = _recurrence.OccursOnDate(date, _startDate, _endDate, _allDay, out representingDate);

			if (representingDate != null)
				RepresentingDate = representingDate.Value;

			return result;
		}

		public DateTime? GetPreviousRecurrence(DateTime reference, DateTime? minValue = null)
		{
			if (_repeatId != null)
				return AppointmentDatabase.GetAppointment(_repeatId).GetPreviousRecurrence(reference, minValue);

			reference = reference.Date;

			if (minValue.HasValue)
				minValue = minValue.Value.Date;

			DateTime? representingDate;
			return _recurrence.GetPreviousRecurrence(_startDate.Date, _allDay ? _endDate : _endDate.Date.AddDays(1), _allDay,
				reference, out representingDate, minValue);
		}

		public DateTime? GetNextRecurrence(DateTime reference, DateTime? maxValue = null)
		{
			if (_repeatId != null)
				return AppointmentDatabase.GetAppointment(_repeatId).GetNextRecurrence(reference, maxValue);

			reference = reference.Date;

			if (maxValue.HasValue)
				maxValue = maxValue.Value.Date.AddDays(1);

			DateTime? representingDate;
			return _recurrence.GetNextRecurrence(_startDate.Date, _allDay ? _endDate : _endDate.Date.AddDays(1), _allDay,
				reference, out representingDate, maxValue);
		}

		/// <summary>
		/// Gets the number of occurrences which will have passed by a certain date.
		/// </summary>
		/// <returns></returns>
		public int Occurrence()
		{
			return _recurrence.Occurrence(RepresentingDate, _startDate);
		}

		/// <summary>
		/// <para>WARNING: THIS FUNCTION IS NOT YET HOOKED UP.</para>
		/// <para>Gets if this appointment matches a specified query.</para>
		/// </summary>
		/// <param name="query">The query string.</param>
		/// <param name="type">The type of query to perform.</param>
		/// <returns></returns>
		public bool MatchesQuery(string query, QueryType type)
		{
			switch (type)
			{
				case QueryType.AllWords:
					return MatchesQueryAllWords(query.Split(' '));

				case QueryType.AnyWord:
					return MatchesQueryAnyWord(query.Split(' '));

				case QueryType.ExactMatch:
					return MatchesQueryExactMatch(query);

				default:
					return false;
			}
		}

		/// <summary>
		/// <para>WARNING: THIS FUNCTION IS NOT YET HOOKED UP.</para>
		/// <para>Gets if this appointment exactly matches a specified query.</para>
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public bool MatchesQueryExactMatch(string query)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// <para>WARNING: THIS FUNCTION IS NOT YET HOOKED UP.</para>
		/// <para>Gets if this appointment matches all words in a specified query.</para>
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public bool MatchesQueryAllWords(string[] query)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// <para>WARNING: THIS FUNCTION IS NOT YET HOOKED UP.</para>
		/// <para>Gets if this appointment matches any word in a specified query.</para>
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public bool MatchesQueryAnyWord(string[] query)
		{
			throw (new NotImplementedException());
		}

		public override bool Equals(object obj)
		{
			if (obj is Appointment)
				return this == (Appointment)obj;

			return false;
		}

		public static bool operator ==(Appointment a1, Appointment a2)
		{
			if ((object.Equals(a1, null) && !object.Equals(a2, null))
				|| !object.Equals(a1, null) && object.Equals(a2, null))
				return false;

			if (object.Equals(a1, null) && object.Equals(a2, null))
				return true;

			return a1._startDate == a2._startDate
				&& a1._endDate == a2._endDate
				&& a1._subject == a2._subject
				&& a1._location == a2._location
				&& a1._allDay == a2._allDay
				&& a1._reminder == a2._reminder
				&& a1._priority == a2._priority
				&& a1._categoryID == a2._categoryID
				&& a1._owner == a2._owner
				&& a1._calendarUrl == a2._calendarUrl
				&& a1._readOnly == a2._readOnly
				&& a1._private == a2._private
				&& a1._showAs == a2._showAs
				//&& a1._saveChangesToDisk == a2._saveChangesToDisk
				//&& a1._detailsDocument == a2._detailsDocument
				&& a1._isRepeating == a2._isRepeating
				&& a1._recurrence == a2._recurrence
				&& a1._repeatId == a2._repeatId;
		}

		public static bool operator !=(Appointment a1, Appointment a2)
		{
			return !(a1 == a2);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion

		#region Serialization

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue(AppointmentDatabase.AllDayAttribute, _allDay);
			info.AddValue(AppointmentDatabase.StartDateAttribute, _startDate);
			info.AddValue(AppointmentDatabase.EndDateAttribute, _endDate);
			info.AddValue("ir", _isRepeating);
			info.AddValue(AppointmentDatabase.LocationAttribute, _location);
			info.AddValue(AppointmentDatabase.ReminderAttribute, _reminder);
			info.AddValue(AppointmentDatabase.PriorityAttribute, (byte)_priority);
			info.AddValue(AppointmentDatabase.CategoryAttribute, _categoryID);
			info.AddValue("sav", _saveChangesToDisk);
			info.AddValue("dtl", Serializer.FlowDocumentSerialize(DetailsDocument));
			info.AddValue(AppointmentDatabase.OwnerAttribute, _owner);
			info.AddValue(AppointmentDatabase.CalendarUrlAttribute, _calendarUrl);
			info.AddValue(AppointmentDatabase.ReadOnlyAttribute, _readOnly);
			info.AddValue(AppointmentDatabase.PrivateAttribute, _private);
			info.AddValue(AppointmentDatabase.ShowAsAttribute, (byte)_showAs);
			info.AddValue(AppointmentDatabase.RepeatIdAttribute, _repeatId);
			info.AddValue("rec", _recurrence);
			info.AddValue(AppointmentDatabase.SubjectAttribute, _subject);
		}

		protected Appointment(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_saveChangesToDisk = false;

			_allDay = info.GetBoolean(AppointmentDatabase.AllDayAttribute);
			_startDate = info.GetDateTime(AppointmentDatabase.StartDateAttribute);
			_endDate = info.GetDateTime(AppointmentDatabase.EndDateAttribute);
			_isRepeating = info.GetBoolean("ir");
			_location = info.GetString(AppointmentDatabase.LocationAttribute);
			_reminder = (TimeSpan)info.GetValue(AppointmentDatabase.ReminderAttribute, typeof(TimeSpan));
			_priority = (Priority)info.GetByte(AppointmentDatabase.PriorityAttribute);
			_categoryID = info.GetString(AppointmentDatabase.CategoryAttribute);
			_saveChangesToDisk = info.GetBoolean("sav");
			DetailsDocument = Serializer.FlowDocumentDeserialize(info.GetString("dtl"));
			_owner = info.GetString(AppointmentDatabase.OwnerAttribute);
			_calendarUrl = info.GetString(AppointmentDatabase.CalendarUrlAttribute);
			_readOnly = info.GetBoolean(AppointmentDatabase.ReadOnlyAttribute);
			_private = info.GetBoolean(AppointmentDatabase.PrivateAttribute);
			_showAs = (ShowAs)info.GetByte(AppointmentDatabase.ShowAsAttribute);
			_recurrence = (Recurrence)info.GetValue("rec", typeof(Recurrence));
			_repeatId = info.GetString(AppointmentDatabase.RepeatIdAttribute);
			_subject = info.GetString(AppointmentDatabase.SubjectAttribute);
		}

		#endregion

		#region Designer

		public string TimeString
		{
			get
			{
				if (_allDay)
				{
					DateTime start = _startDate.Date;
					DateTime end = _endDate.Date.AddDays(-1);

					if (start == end)
						return "All day";
					else
					{
						if (start.Year == end.Year)
							if (start.Month == end.Month)
								return start.ToString("M/d") + "-" + end.Day.ToString();
							else
								return start.ToString("M/d") + "-" + end.ToString("M/d");
						else
							return start.ToString("M/d/yy") + "-" + end.ToString("M/d/yy");
					}
				}
				else
				{
					TimeSpan startTime = _startDate.TimeOfDay;
					TimeSpan endTime = _endDate.TimeOfDay;

					string start = RandomFunctions.FormatHour(startTime.Hours) + (startTime.Minutes == 0 ? "" : ":" + startTime.Minutes.ToString().PadLeft(2, '0'));
					string end = RandomFunctions.FormatHour(endTime.Hours) + (endTime.Minutes == 0 ? "" : ":" + endTime.Minutes.ToString().PadLeft(2, '0'));

					if (Settings.TimeFormat == TimeFormat.Standard)
					{
						if (startTime.Hours < 12 && endTime.Hours < 12)
						{
							end += "a";

							if (_startDate.Date != _endDate.Date)
								start += "a";
						}
						else if (startTime.Hours >= 12 && endTime.Hours >= 12)
						{
							end += "p";

							if (_startDate.Date != _endDate.Date)
								start += "p";
						}
						else
						{
							if (startTime.Hours < 12)
								start += "a";
							else
								start += "p";

							if (endTime.Hours < 12)
								end += "a";
							else
								end += "p";
						}
					}

					if (_startDate.Year == _endDate.Year)
					{
						if (_startDate.Month == _endDate.Month)
						{
							if (_startDate.Day != _endDate.Day)
							{
								start = _startDate.ToString("M/d") + " " + start;
								end = _endDate.Day.ToString() + " " + end;
							}
						}
						else
						{
							start = _startDate.ToString("M/d") + " " + start;
							end = _endDate.ToString("M/d") + " " + end;
						}
					}
					else
					{
						start = _startDate.ToString("M/d/yy");//+" " + start;
						end = _endDate.ToString("M/d/yy");//+ " " + end;
					}

					if (_startDate != _endDate)
						return start + "-" + end;
					else
						return end;
				}
			}
		}

		#endregion
	}

	#region Enums

	public enum RepeatType : byte { Daily, Weekly, Monthly, Yearly };
	public enum RepeatEnd : byte { None, Count, Date };
	public enum ShowAs : byte { Free, WorkingElsewhere, Tentative, Busy, OutOfOffice };

	#endregion
}
