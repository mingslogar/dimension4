using System.Text;
using System.Text.RegularExpressions;

namespace Modern.FileBrowser
{
	static class StringExtensions
	{
		/// <summary>
		/// Case-insensitive string replacement.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="query"></param>
		/// <param name="replace"></param>
		/// <returns></returns>
		public static string Replace(this string value, string[] query, string replace)
		{
			StringBuilder expression = new StringBuilder();
			expression.Append('(');

			int queryLength = query.Length;

			for (int i = 0; i < queryLength; i++)
			{
				expression.Append(Regex.Escape(query[i]));

				if (i < queryLength - 1)
					expression.Append('|');
			}

			expression.Append(')');

			return Regex.Replace(value, expression.ToString(), replace, RegexOptions.IgnoreCase);
		}
	}
}
