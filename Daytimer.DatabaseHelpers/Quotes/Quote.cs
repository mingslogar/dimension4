using System;

namespace Daytimer.DatabaseHelpers.Quotes
{
	public class Quote : DatabaseObject
	{
		#region Constructors

		public Quote(DateTime date, string title, string content, string author)
		{
			_id = FormatHelpers.DateTimeToShortString(date);
			Date = date;
			Title = title;
			Content = content;
			Author = author;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the date which this quote is applicable to.
		/// </summary>
		public DateTime Date { get; private set; }

		/// <summary>
		/// Gets the title of the quote.
		/// </summary>
		public string Title { get; private set; }

		/// <summary>
		/// Gets the content of the quote.
		/// </summary>
		public string Content { get; private set; }

		/// <summary>
		/// Gets the author of the quote.
		/// </summary>
		public string Author { get; private set; }

		#endregion
	}
}
