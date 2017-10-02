using System.Text.RegularExpressions;
using System.Windows.Documents;

namespace Daytimer.Functions
{
	public static class FlowDocumentSearch
	{
		/// <summary>
		/// Gets the first occurrence of a specified query starting at a specified location.
		/// </summary>
		/// <param name="query"></param>
		/// <param name="start"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static TextRange Find(this FlowDocument document, string query, TextPointer start, RegexOptions options, out TextRangeStack stack, TextRangeStack cachedStack = null)
		{
			if (cachedStack == null)
				cachedStack = new TextRangeStack(document);

			int findStart = cachedStack.GetPosition(start);

			int offset = findStart;
			Match match = Regex.Match(cachedStack.Text.Substring(findStart), query, options);

			// If we couldn't find a match, loop around to the beginning of the document.
			if (!match.Success)
			{
				offset = 0;

				int remove = findStart + query.Length;

				if (remove < cachedStack.Text.Length)
					match = Regex.Match(cachedStack.Text.Remove(remove), query, options);
				else
					match = Regex.Match(cachedStack.Text, query, options);
			}

			stack = cachedStack;

			if (match.Success)
				return cachedStack.Select(match.Index + offset, match.Index + match.Length + offset);

			return null;
		}

		/// <summary>
		/// Gets all occurrences of a specified query.
		/// </summary>
		/// <param name="query"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static TextRange[] FindAll(this FlowDocument document, string query, RegexOptions options, out TextRangeStack stack, TextRangeStack cachedStack = null)
		{
			if (cachedStack == null)
				cachedStack = new TextRangeStack(document);

			MatchCollection matches = Regex.Matches(cachedStack.Text, query, options);
			int count = matches.Count;

			TextRange[] results = new TextRange[count];

			for (int i = 0; i < count; i++)
			{
				Match match = matches[i];
				results[i] = cachedStack.Select(match.Index, match.Index + match.Length);
			}

			stack = cachedStack;

			return results;
		}
	}
}
