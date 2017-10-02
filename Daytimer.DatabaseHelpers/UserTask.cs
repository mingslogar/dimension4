using Daytimer.DatabaseHelpers.Recovery;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace Daytimer.DatabaseHelpers
{
	/// <summary>
	/// Called <em>UserTask</em> to differentiate from System.Threading.Tasks.Task.
	/// </summary>
	[Serializable]
	public class UserTask : DatabaseObject
	{
		#region Constructors

		public UserTask()
			: base()
		{

		}

		public UserTask(bool generateID)
			: base(generateID)
		{

		}

		public UserTask(DateTime reminder)
		{
			_reminder = reminder;
			_id = IDGenerator.GenerateID();
		}

		public UserTask(UserTask task)
		{
			CopyFrom(task);
		}

		public UserTask(UserTask task, bool saveChangesToDisk)
		{
			CopyFrom(task);
			_saveChangesToDisk = saveChangesToDisk;
		}

		public UserTask(DateTime reminder, DateTime dueDate)
		{
			_reminder = reminder;
			_dueDate = dueDate;
			_id = IDGenerator.GenerateID();
		}

		#endregion

		#region Properties

		public enum StatusPhase : byte { NotStarted, InProgress, Completed, WaitingOnSomeoneElse, Deferred };

		private string _subject = "";
		private DateTime? _startDate = DateTime.Now.Date;
		private DateTime? _dueDate = DateTime.Now.Date;
		private DateTime _reminder = DateTime.Now;
		private bool _isReminderEnabled = false;
		private StatusPhase _status = StatusPhase.NotStarted;
		private Priority _priority = Priority.Normal;
		private double _progress = 0;
		private string _categoryID = "";
		private string _owner = "";
		private bool _readOnly = false;
		private bool _private = false;
		private DateTime _lastModified;

		private bool _saveChangesToDisk = true;
		private FlowDocument _detailsDocument = null;

		public string Subject
		{
			get { return _subject; }
			set { _subject = value; }
		}

		public DateTime? StartDate
		{
			get { return _startDate; }
			set { _startDate = value; }
		}

		public DateTime? DueDate
		{
			get { return _dueDate; }
			set { _dueDate = value; }
		}

		public DateTime Reminder
		{
			get { return _reminder; }
			set { _reminder = value; }
		}

		public bool IsReminderEnabled
		{
			get { return _isReminderEnabled; }
			set { _isReminderEnabled = value; }
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
		/// Gets the details document.
		/// </summary>
		public FlowDocument DetailsDocument
		{
			get
			{
				if (_saveChangesToDisk)
					return new FlowDocumentStorage(TaskDatabase.TasksAppData + "\\" + _id).DocumentValue;
				else
					return _detailsDocument;
			}
			set
			{
				if (_saveChangesToDisk)
					new FlowDocumentStorage(TaskDatabase.TasksAppData + "\\" + _id).DocumentValue = value;
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
				return await new FlowDocumentStorage(TaskDatabase.TasksAppData + "\\" + _id).GetDocumentValueAsync();
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
				await new FlowDocumentStorage(TaskDatabase.TasksAppData + "\\" + _id).SetDocumentValueAsync(value);
			else
				_detailsDocument = value;
		}

		public StatusPhase Status
		{
			get { return _status; }
			set { _status = value; }
		}

		public Priority Priority
		{
			get { return _priority; }
			set { _priority = value; }
		}

		public double Progress
		{
			get { return _progress; }
			set { _progress = value; }
		}

		/// <summary>
		/// Get if Task is overdue.
		/// </summary>
		public bool IsOverdue
		{
			get
			{
				if (DueDate != null)
					if (((DateTime)DueDate).Subtract(DateTime.Now.Date).Days <= 0)
						return true;

				return false;
			}
		}

		public Category Category
		{
			get
			{
				Category category = TaskDatabase.GetCategory(_categoryID);

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

		public DateTime LastModified
		{
			get { return _lastModified; }
			set { _lastModified = value; }
		}

		public bool SaveChangesToDisk
		{
			get { return _saveChangesToDisk; }
			set { _saveChangesToDisk = value; }
		}

		#endregion

		#region Methods

		private void CopyFrom(UserTask task)
		{
			base.CopyFrom(task);

			_subject = task._subject;
			_startDate = task._startDate;
			_dueDate = task._dueDate;
			_reminder = task._reminder;
			_isReminderEnabled = task._isReminderEnabled;
			_status = task._status;
			_priority = task._priority;
			_progress = task._progress;
			_categoryID = task._categoryID;
			_owner = task._owner;
			_readOnly = task._readOnly;
			_private = task._private;
			_lastModified = task._lastModified;
		}

		/// <summary>
		/// <para>WARNING: THIS FUNCTION IS NOT YET HOOKED UP.</para>
		/// <para>Gets if this task matches a specified query.</para>
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
		/// <para>Gets if this task exactly matches a specified query.</para>
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public bool MatchesQueryExactMatch(string query)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// <para>WARNING: THIS FUNCTION IS NOT YET HOOKED UP.</para>
		/// <para>Gets if this task matches all words in a specified query.</para>
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public bool MatchesQueryAllWords(string[] query)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// <para>WARNING: THIS FUNCTION IS NOT YET HOOKED UP.</para>
		/// <para>Gets if this task matches any word in a specified query.</para>
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public bool MatchesQueryAnyWord(string[] query)
		{
			throw (new NotImplementedException());
		}

		#endregion

		#region Design

		/// <summary>
		/// For design purposes only, get opacity of flag if Task is overdue.
		/// </summary>
		public Double Overdue
		{
			get
			{
				if (DueDate == null || ((DateTime)DueDate).Subtract(DateTime.Now.Date).Days <= 0)
					return 1;

				return 0.4;
			}
		}

		/// <summary>
		/// For design purposes only, get visibility of flag if Task is completed.
		/// </summary>
		public Visibility ShowFlag
		{
			get { return (_status == StatusPhase.Completed ? Visibility.Collapsed : Visibility.Visible); }
		}

		/// <summary>
		/// For design purposes only, get visibility of check mark if Task is completed.
		/// </summary>
		public Visibility ShowCheck
		{
			get { return (_status == StatusPhase.Completed ? Visibility.Visible : Visibility.Collapsed); }
		}

		/// <summary>
		/// For design purposes only, get visibility of exclamation mark if Task is high priority.
		/// </summary>
		public Visibility HighPriority
		{
			get { return (_priority == Priority.High && _status != StatusPhase.Completed ? Visibility.Visible : Visibility.Collapsed); }
		}

		#endregion

		#region Serialization

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue(TaskDatabase.SubjectAttribute, _subject);
			info.AddValue(TaskDatabase.StartDateAttribute, _startDate);
			info.AddValue("due", _dueDate);
			info.AddValue(TaskDatabase.ReminderAttribute, _reminder);
			info.AddValue(TaskDatabase.IsReminderEnabledAttribute, _isReminderEnabled);
			info.AddValue(TaskDatabase.StatusAttribute, (byte)_status);
			info.AddValue(TaskDatabase.PriorityAttribute, (byte)_priority);
			info.AddValue(TaskDatabase.ProgressAttribute, _progress);
			info.AddValue(TaskDatabase.CategoryAttribute, _categoryID);
			info.AddValue(TaskDatabase.OwnerAttribute, _owner);
			info.AddValue(TaskDatabase.ReadOnlyAttribute, _readOnly);
			info.AddValue(TaskDatabase.PrivateAttribute, _private);
			info.AddValue(TaskDatabase.LastModifiedAttribute, _lastModified);
			info.AddValue("dtl", Serializer.FlowDocumentSerialize(DetailsDocument));
		}

		protected UserTask(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_saveChangesToDisk = false;

			_subject = info.GetString(TaskDatabase.SubjectAttribute);
			_startDate = (DateTime?)info.GetValue(TaskDatabase.StartDateAttribute, typeof(DateTime?));
			_dueDate = (DateTime?)info.GetValue("due", typeof(DateTime?));
			_reminder = info.GetDateTime(TaskDatabase.ReminderAttribute);
			_isReminderEnabled = info.GetBoolean(TaskDatabase.IsReminderEnabledAttribute);
			_status = (StatusPhase)info.GetByte(TaskDatabase.StatusAttribute);
			_priority = (Priority)info.GetByte(TaskDatabase.PriorityAttribute);
			_progress = info.GetDouble(TaskDatabase.ProgressAttribute);
			_categoryID = info.GetString(TaskDatabase.CategoryAttribute);
			_owner = info.GetString(TaskDatabase.OwnerAttribute);
			_readOnly = info.GetBoolean(TaskDatabase.ReadOnlyAttribute);
			_private = info.GetBoolean(TaskDatabase.PrivateAttribute);
			_lastModified = info.GetDateTime(TaskDatabase.LastModifiedAttribute);
			DetailsDocument = Serializer.FlowDocumentDeserialize(info.GetString("dtl"));
		}

		#endregion

		#region Static methods

		public static StatusPhase ParseStatus(string value)
		{
			switch (value.ToLower())
			{
				case "notstarted":
				case "not started":
				default:
					return StatusPhase.NotStarted;

				case "inprogress":
				case "in progress":
					return StatusPhase.InProgress;

				case "completed":
					return StatusPhase.Completed;

				case "waitingonsomeoneelse":
				case "waiting on someone else":
					return StatusPhase.WaitingOnSomeoneElse;

				case "deferred":
					return StatusPhase.Deferred;
			}
		}

		#endregion
	}
}
