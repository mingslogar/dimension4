using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows.Media;
using System.Xml;

namespace Daytimer.DatabaseHelpers.Note
{
	[Serializable]
	public class NotebookSection : DatabaseObject, INotifyPropertyChanged
	{
		#region Constructors

		public NotebookSection()
			: base()
		{

		}

		public NotebookSection(bool generateID)
			: base(generateID)
		{

		}

		public NotebookSection(NotebookSection section)
		{
			CopyFrom(section);
		}

		#endregion

		#region Properties

		private string _notebookID = "";
		private string _title = "";
		private Color _color = Colors.Transparent;
		private bool _readOnly = false;
		private bool _private = false;
		private DateTime _lastModified;
		private string _lastSelectedPageID = "";

		public string NotebookID
		{
			get { return _notebookID; }
			set { _notebookID = value; }
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

		public Color Color
		{
			get { return _color; }
			set
			{
				_color = value;
				OnPropertyChanged("Color");
			}
		}

		public IEnumerable<NotebookPage> Pages
		{
			get
			{
				XmlNode node = NoteDatabase.Database.Doc.GetElementById(_id);

				foreach (XmlNode each in node.ChildNodes)
					yield return NoteDatabase.GetPage(each);
			}
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
			set
			{
				_lastModified = value;
				OnPropertyChanged("LastModified");
			}
		}

		public string LastSelectedPageID
		{
			get { return _lastSelectedPageID; }
			set { _lastSelectedPageID = value; }
		}

		#endregion

		#region Methods

		private void CopyFrom(NotebookSection section)
		{
			_id = section._id;
			_notebookID = section._notebookID;
			_title = section._title;
			_color = section._color;
			_readOnly = section._readOnly;
			_private = section._private;
			_lastModified = section._lastModified;
			_lastSelectedPageID = section._lastSelectedPageID;
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

			info.AddValue("ntbId", _notebookID);
			info.AddValue(NoteDatabase.TitleAttribute, _title);
			info.AddValue(NoteDatabase.ColorAttribute, _color);
			info.AddValue(NoteDatabase.ReadOnlyAttribute, _readOnly);
			info.AddValue(NoteDatabase.PrivateAttribute, _private);
			info.AddValue(NoteDatabase.LastModifiedAttribute, _lastModified);
			info.AddValue(NoteDatabase.LastSelectedAttribute, _lastSelectedPageID);
		}

		protected NotebookSection(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_notebookID = info.GetString("ntbId");
			_title = info.GetString(NoteDatabase.TitleAttribute);
			_color = (Color)info.GetValue(NoteDatabase.ColorAttribute, typeof(Color));
			_readOnly = info.GetBoolean(NoteDatabase.ReadOnlyAttribute);
			_private = info.GetBoolean(NoteDatabase.PrivateAttribute);
			_lastModified = info.GetDateTime(NoteDatabase.LastModifiedAttribute);
			_lastSelectedPageID = info.GetString(NoteDatabase.LastSelectedAttribute);
		}

		#endregion
	}
}
