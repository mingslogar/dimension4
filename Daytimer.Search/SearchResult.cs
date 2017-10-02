using System;

namespace Daytimer.Search
{
	public class SearchResult
	{
		public SearchResult(string id, RepresentingObject representingObject, string majorText,
			string minorText, DateTime? date, bool recurring = false)
		{
			_id = id;
			_representingObject = representingObject;
			_majorText = majorText;
			_minorText = minorText;
			_date = date;
			_recurring = recurring;
		}

		private string _id;
		private RepresentingObject _representingObject;
		private string _majorText;
		private string _minorText;
		private DateTime? _date;
		private bool _recurring;

		public string ID
		{
			get { return _id; }
			set { _id = value; }
		}

		public RepresentingObject RepresentingObject
		{
			get { return _representingObject; }
			set { _representingObject = value; }
		}

		public string MajorText
		{
			get { return _majorText; }
			set { _majorText = value; }
		}

		public string MinorText
		{
			get { return _minorText; }
			set { _minorText = value; }
		}

		public DateTime? Date
		{
			get { return _date; }
			set { _date = value; }
		}

		public bool Recurring
		{
			get { return _recurring; }
			set { _recurring = value; }
		}
	}

	public enum RepresentingObject : byte { Appointment, Contact, Task, Note };
}
