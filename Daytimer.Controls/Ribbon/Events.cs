using Daytimer.DatabaseHelpers;
using System;
using System.Windows.Documents;
using System.Windows.Xps;

namespace Daytimer.Controls.Ribbon
{
	public class ThemeChangedEventArgs : EventArgs
	{
		public ThemeChangedEventArgs(string theme)
		{
			_theme = theme;
		}

		private string _theme;

		public string Theme
		{
			get { return _theme; }
		}
	}

	public class BackgroundChangedEventArgs : EventArgs
	{
		public BackgroundChangedEventArgs(string background)
		{
			_background = background;
		}

		private string _background;

		public string Background
		{
			get { return _background; }
		}
	}

	public class ForceUpdateEventArgs : EventArgs
	{
		public ForceUpdateEventArgs(bool updateTheme, bool updateBackground, bool updateHours,
			bool updateTimeFormat, bool updateAutoSave, bool updateWeatherMetric)
		{
			_updateTheme = updateTheme;
			_updateBackground = updateBackground;
			_updateHours = updateHours;
			_updateTimeFormat = updateTimeFormat;
			_updateAutoSave = updateAutoSave;
			_updateWeatherMetric = updateWeatherMetric;
		}

		private bool _updateTheme;
		private bool _updateBackground;
		private bool _updateHours;
		private bool _updateTimeFormat;
		private bool _updateAutoSave;
		private bool _updateWeatherMetric;

		public bool UpdateTheme
		{
			get { return _updateTheme; }
		}

		public bool UpdateBackground
		{
			get { return _updateBackground; }
		}

		public bool UpdateHours
		{
			get { return _updateHours; }
		}

		public bool UpdateTimeFormat
		{
			get { return _updateTimeFormat; }
		}

		public bool UpdateAutoSave
		{
			get { return _updateAutoSave; }
		}

		public bool UpdateWeatherMetric
		{
			get { return _updateWeatherMetric; }
		}
	}

	public class ExportEventArgs : EventArgs
	{
		public ExportEventArgs(ExportType exportType)
		{
			_exportType = exportType;
		}

		public ExportEventArgs(ExportType exportType, EditType editType)
		{
			_exportType = exportType;
			_editType = editType;
		}

		private ExportType _exportType;
		private EditType _editType;

		public ExportType ExportType
		{
			get { return _exportType; }
		}

		public EditType EditType
		{
			get { return _editType; }
		}
	}

	public class DocumentRequestEventArgs : EventArgs
	{
		public DocumentRequestEventArgs()
		{

		}

		private DatabaseObject _databaseObject;
		private EditType _documentType;

		public DatabaseObject DatabaseObject
		{
			get { return _databaseObject; }
			set { _databaseObject = value; }
		}

		public EditType DocumentType
		{
			get { return _documentType; }
			set { _documentType = value; }
		}
	}

	public class PrintEventArgs : EventArgs
	{
		public PrintEventArgs(XpsDocumentWriter documentWriter)
		{
			XpsDocumentWriter = documentWriter;
		}

		public XpsDocumentWriter XpsDocumentWriter { get; private set; }
	}

	public class ImportEventArgs : EventArgs
	{
		public ImportEventArgs(EditType type)
		{
			Type = type;
		}

		public EditType Type { get; private set; }
	}

	public enum EditType : byte { Appointment, Contact, Task, Note };
	public enum ExportType : byte { Screenshot, Individual };
}