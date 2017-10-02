using System.Collections.Generic;

namespace Daytimer.Help.SearchEngine
{
	class HtmlGenerator
	{
		public HtmlGenerator(List<SearchResult> results, string query)
		{
			bool hasResults = results.Count > 0;

			if (hasResults)
				foreach (SearchResult each in results)
					_html += SearchResultHtml(each);
			else
				_html = "<p>It doesn&#39;t look like we have any help for that here. You might want to broaden your search terms.</p>";

			Finalize(hasResults, query);
		}

		private void Finalize(bool hasResults, string query)
		{
			string header = SearchHelpers.GetResourceContents("include/header.html");

			if (!string.IsNullOrWhiteSpace(query))
			{
				header = header.Replace(SearchHelpers.VariableDelimiter + SearchHelpers.TitleAttribute, "Search results for " + query);

				if (hasResults)
					header += "<h1>Search results for &quot;" + query + "&quot;</h1>";
				else
					header += "<h1>We didn&#39;t find anything</h1>";
			}
			else
			{
				header = header.Replace(SearchHelpers.VariableDelimiter + SearchHelpers.TitleAttribute, "Help Index")
					+ "<h1>Help Index</h1>";
			}

			string footer = SearchHelpers.GetResourceContents("include/footer.html");

			_html = header + _html + footer;
		}

		private string _html = "";

		public string HTML
		{
			get { return _html; }
		}

		private string SearchResultHtml(SearchResult result)
		{
			return "<p><a href=\"" + result.Link + "\">" + result.DisplayText + "</a></p>";
		}
	}
}
