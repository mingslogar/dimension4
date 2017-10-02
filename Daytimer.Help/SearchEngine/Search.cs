using System.Collections.Generic;

namespace Daytimer.Help.SearchEngine
{
	class Search
	{
		public Search(string query)
		{
			_results = GetSearchResults(query);
		}

		private List<SearchResult> _results;

		public List<SearchResult> Results
		{
			get { return _results; }
		}

		private List<SearchResult> GetSearchResults(string query)
		{
			query = Trim(query);

			List<SearchResult> results = new List<SearchResult>();
			string[] index = SearchHelpers.SearchIndex;

			foreach (string each in index)
			{
				string content = SearchHelpers.GetResourceContents(each);
				Dictionary<string, string> pageData = SearchHelpers.GetPageAttributes(content, out content);

				foreach (KeyValuePair<string, string> de in pageData)
					content = content.Replace(SearchHelpers.VariableDelimiter + de.Key, de.Value);

				if (SearchHelpers.MatchesQuery(query, content))
				{
					SearchResult result = new SearchResult(each, pageData[SearchHelpers.TitleAttribute]);
					results.Add(result);
				}
			}

			return results;
		}

		private string Trim(string data)
		{
			data = data.Trim();

			while (data.Contains("  "))
				data = data.Replace("  ", " ");

			return data;
		}
	}
}
