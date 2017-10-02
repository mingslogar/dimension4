using Daytimer.DatabaseHelpers.Quotes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WikiquoteScreensaverLib.Common;

namespace Daytimer.WikiQuoteHelper
{
	/// <summary>
	/// Facilitates transferring between WikiQuote content to usable date-based content.
	/// </summary>
	class Database
	{
		/// <summary>
		/// Adds a new quote page to the database.
		/// </summary>
		/// <param name="quotePage"></param>
		public static async Task AddToDatabase(QuotePage quotePage)
		{
			//DateTime? lastDate = QuoteDatabase.GetLastDate();
			//DateTime date = lastDate.HasValue ? lastDate.Value.AddDays(1) : DateTime.Now.Date;

			//foreach (WikiquoteScreensaverLib.Common.Quote each in quotePage)
			//{
			//	QuoteDatabase.AddQuote(
			//		new DatabaseHelpers.Quotes.Quote(date, null, each.Text, each.AdditionalInformation)
			//	);

			//	date = date.AddDays(1);
			//}

			await Task.Factory.StartNew(() =>
			{
				//
				// We have to loop through every date from now until the future to make
				// sure we fill up any holes in the database.
				//
				DateTime? date = DateTime.Now.Date;

				foreach (WikiquoteScreensaverLib.Common.Quote each in quotePage)
				{
					date = QuoteDatabase.NextFreeDate(date.Value);

					if (!date.HasValue)
						break;

					AddToDatabase(each, date.Value, quotePage.Topic);
					date = date.Value.AddDays(1);
				}
			});
		}

		/// <summary>
		/// Adds a list of new quote pages to the database, randomizing
		/// the order of the quotes.
		/// </summary>
		/// <param name="quotePage"></param>
		public static async Task AddToDatabase(IList<QuotePage> quotePage)
		{
			await Task.Factory.StartNew(() =>
			{
				int pagesCount = quotePage.Count;
				Random random = new Random((int)DateTime.Now.TimeOfDay.Ticks);

				// We have to loop through every date from now until the future to make
				// sure we fill up any holes in the database.
				DateTime? date = DateTime.Now.Date;

				while (pagesCount > 0)
				{
					QuotePage page = quotePage[random.Next(0, pagesCount)];

					if (page.Count == 0)
					{
						quotePage.Remove(page);
						pagesCount--;
						continue;
					}

					date = QuoteDatabase.NextFreeDate(date.Value);

					if (!date.HasValue)
						break;

					AddToDatabase(page.Items[0], date.Value, page.Topic);

					date = date.Value.AddDays(1);
					page.Items.RemoveAt(0);
				}
			});
		}

		/// <summary>
		/// Delete all the quotes in a quote page from the database.
		/// </summary>
		/// <param name="quotePage"></param>
		public static async Task RemoveFromDatabase(QuotePage quotePage)
		{
			List<DatabaseHelpers.Quotes.Quote> delete = new List<DatabaseHelpers.Quotes.Quote>();

			await Task.Factory.StartNew(() =>
			{
				IEnumerable<DatabaseHelpers.Quotes.Quote> list = QuoteDatabase.AllQuotes();

				foreach (WikiquoteScreensaverLib.Common.Quote wikiQuote in quotePage)
				{
					foreach (DatabaseHelpers.Quotes.Quote dbQuote in list)
					{
						if (dbQuote.Content == wikiQuote.Text)
						{
							delete.Add(dbQuote);
							break;
						}
					}
				}
			});

			await Task.Factory.StartNew(() =>
			{
				foreach (DatabaseHelpers.Quotes.Quote dbQuote in delete)
					QuoteDatabase.DeleteQuote(dbQuote);
			});
		}

		/// <summary>
		/// Rebuild entire database.
		/// </summary>
		/// <param name="playlist"></param>
		public static async Task RebuildDatabase(Playlist playlist)
		{
			QuoteDatabase.Clear();
			await AddToDatabase(playlist.Items);
		}

		private static void AddToDatabase(WikiquoteScreensaverLib.Common.Quote quote, DateTime date, string title)
		{
			QuoteDatabase.AddQuote(
				new DatabaseHelpers.Quotes.Quote(date, title, quote.Text, quote.AdditionalInformation)
			);
		}
	}
}
