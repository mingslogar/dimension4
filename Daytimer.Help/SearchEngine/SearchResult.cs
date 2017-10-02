namespace Daytimer.Help.SearchEngine
{
	class SearchResult
	{
		public SearchResult()
		{

		}

		public SearchResult(string link, string displayText)
		{
			_link = link;
			_displayText = displayText;
		}

		private string _link;
		private string _displayText;

		public string Link
		{
			get { return _link; }
			set { _link = value; }
		}

		public string DisplayText
		{
			get { return _displayText; }
			set { _displayText = value; }
		}
	}
}
