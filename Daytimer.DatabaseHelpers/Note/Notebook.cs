using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows.Media;
using System.Xml;

namespace Daytimer.DatabaseHelpers.Note
{
	[Serializable]
	public class Notebook : DatabaseObject, INotifyPropertyChanged
	{
		#region Constructors

		public Notebook()
			: base()
		{

		}

		public Notebook(bool generateID)
			: base(generateID)
		{

		}

		public Notebook(Notebook notebook)
		{
			CopyFrom(notebook);
		}

		#endregion

		#region Properties

		private string _title = "";
		private Color _color = Colors.Transparent;
		private bool _readOnly = false;
		private bool _private = false;
		private DateTime _lastModified;
		private string _lastSelectedSectionID = "";

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

		public IEnumerable<NotebookSection> Sections
		{
			get
			{
				XmlNode node = NoteDatabase.Database.Doc.GetElementById(_id);

				foreach (XmlNode each in node.ChildNodes)
					yield return NoteDatabase.GetSection(each);
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

		public string LastSelectedSectionID
		{
			get { return _lastSelectedSectionID; }
			set { _lastSelectedSectionID = value; }
		}

		#endregion

		#region Methods

		private void CopyFrom(Notebook notebook)
		{
			_id = notebook._id;
			_title = notebook._title;
			_readOnly = notebook._readOnly;
			_private = notebook._private;
			_lastModified = notebook._lastModified;
			_lastSelectedSectionID = notebook._lastSelectedSectionID;
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

			info.AddValue(NoteDatabase.TitleAttribute, _title);
			info.AddValue(NoteDatabase.ColorAttribute, _color);
			info.AddValue(NoteDatabase.ReadOnlyAttribute, _readOnly);
			info.AddValue(NoteDatabase.PrivateAttribute, _private);
			info.AddValue(NoteDatabase.LastModifiedAttribute, _lastModified);
			info.AddValue(NoteDatabase.LastSelectedAttribute, _lastSelectedSectionID);
		}

		protected Notebook(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_title = info.GetString(NoteDatabase.TitleAttribute);
			_color = (Color)info.GetValue(NoteDatabase.ColorAttribute, typeof(Color));
			_readOnly = info.GetBoolean(NoteDatabase.ReadOnlyAttribute);
			_private = info.GetBoolean(NoteDatabase.PrivateAttribute);
			_lastModified = info.GetDateTime(NoteDatabase.LastModifiedAttribute);
			_lastSelectedSectionID = info.GetString(NoteDatabase.LastSelectedAttribute);
		}

		#endregion
	}
}
