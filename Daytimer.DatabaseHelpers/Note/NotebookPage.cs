using Daytimer.DatabaseHelpers.Recovery;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Daytimer.DatabaseHelpers.Note
{
	[Serializable]
	public class NotebookPage : DatabaseObject, INotifyPropertyChanged
	{
		#region Constructors

		public NotebookPage()
			: base()
		{

		}

		public NotebookPage(bool generateID)
			: base(generateID)
		{

		}

		public NotebookPage(NotebookPage page)
		{
			CopyFrom(page);
		}

		public NotebookPage(NotebookPage page, bool saveChangesToDisk)
		{
			CopyFrom(page);
			_saveChangesToDisk = saveChangesToDisk;
		}

		#endregion

		#region Properties

		private string _sectionID = "";
		private string _title = "";
		private bool _readOnly = false;
		private bool _private = false;
		private DateTime _created;
		private DateTime _lastModified;

		private bool _saveChangesToDisk = true;
		private FlowDocument _detailsDocument = null;

		public string SectionID
		{
			get { return _sectionID; }
			set { _sectionID = value; }
		}

		public string Title
		{
			get { return _title; }
			set
			{
				_title = value;
				OnPropertyChanged("Title");
			}
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
					return new FlowDocumentStorage(NoteDatabase.NotesAppData + "\\" + _id).DocumentValue;
				else
					return _detailsDocument;
			}
			set
			{
				if (_saveChangesToDisk)
					new FlowDocumentStorage(NoteDatabase.NotesAppData + "\\" + _id).DocumentValue = value;
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
				return await new FlowDocumentStorage(NoteDatabase.NotesAppData + "\\" + _id).GetDocumentValueAsync();
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
				await new FlowDocumentStorage(NoteDatabase.NotesAppData + "\\" + _id).SetDocumentValueAsync(value);
			else
				_detailsDocument = value;
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

		public DateTime Created
		{
			get { return _created; }
			set
			{
				_created = value;
				OnPropertyChanged("Created");
			}
		}

		public DateTime LastModified
		{
			get { return _lastModified; }
			set { _lastModified = value; }
		}

		#endregion

		#region Methods

		private void CopyFrom(NotebookPage page)
		{
			base.CopyFrom(page);

			_sectionID = page._sectionID;
			_title = page._title;
			_readOnly = page._readOnly;
			_private = page._private;
			_created = page._created;
			_lastModified = page._lastModified;
		}

		/// <summary>
		/// <para>WARNING: THIS FUNCTION IS NOT YET HOOKED UP.</para>
		/// <para>Gets if this page matches a specified query.</para>
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
		/// <para>Gets if this page exactly matches a specified query.</para>
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public bool MatchesQueryExactMatch(string query)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// <para>WARNING: THIS FUNCTION IS NOT YET HOOKED UP.</para>
		/// <para>Gets if this page matches all words in a specified query.</para>
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public bool MatchesQueryAllWords(string[] query)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// <para>WARNING: THIS FUNCTION IS NOT YET HOOKED UP.</para>
		/// <para>Gets if this page matches any word in a specified query.</para>
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public bool MatchesQueryAnyWord(string[] query)
		{
			throw (new NotImplementedException());
		}

		#endregion

		#region Graphics

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		#endregion

		#region Serialization

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue("secId", _sectionID);
			info.AddValue(NoteDatabase.TitleAttribute, _title);
			info.AddValue(NoteDatabase.ReadOnlyAttribute, _readOnly);
			info.AddValue(NoteDatabase.PrivateAttribute, _private);
			info.AddValue(NoteDatabase.CreatedAttribute, _created);
			info.AddValue(NoteDatabase.LastModifiedAttribute, _lastModified);
			info.AddValue("sav", _saveChangesToDisk);
			info.AddValue("dtl", Serializer.FlowDocumentSerialize(DetailsDocument));
		}

		protected NotebookPage(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_sectionID = info.GetString("secId");
			_title = info.GetString(NoteDatabase.TitleAttribute);
			_readOnly = info.GetBoolean(NoteDatabase.ReadOnlyAttribute);
			_private = info.GetBoolean(NoteDatabase.PrivateAttribute);
			_created = info.GetDateTime(NoteDatabase.CreatedAttribute);
			_lastModified = info.GetDateTime(NoteDatabase.LastModifiedAttribute);
			_saveChangesToDisk = info.GetBoolean("sav");
			DetailsDocument = Serializer.FlowDocumentDeserialize(info.GetString("dtl"));
		}

		#endregion
	}
}
