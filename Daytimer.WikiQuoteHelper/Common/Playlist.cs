// 
// This file is part of WikiquoteScreensaverLib.
//
// WikiquoteScreensaverLib is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// WikiquoteScreensaverLib is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with WikiquoteScreensaverLib.  If not, see <http://www.gnu.org/licenses/>.
//
// Filename: Playlist.cs
// Copyright: Christian Hanser 2008
//

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace WikiquoteScreensaverLib.Common
{
	/// <summary>
	/// A collection class intended for containing QuotePages.
	/// It provides additional data such as the total number of quotes.
	/// </summary>
	[ComVisible(false)]
	public sealed class Playlist
		: SelectablesListBase<Uri, QuotePage, QuotePageCollection>
	{
		int _quoteCount = 0;

		/// <summary>
		/// Gets or sets the quotepage item collection.
		/// </summary>
		protected internal override QuotePageCollection Items
		{
			get
			{
				return base.Items;
			}
			set
			{
				base.Items = value;
				ResetQuoteCount();
			}
		}

		/// <summary>
		/// Returns the total number of quotes, which are contained by the contained
		/// quote page items.
		/// </summary>
		public int QuoteCount
		{
			get
			{
				return _quoteCount;
			}
			private set
			{
				if (_quoteCount != value)
				{
					_quoteCount = value;
					OnPropertyChanged("QuoteCount");
				}
			}
		}

		public override QuotePage this[int index]
		{
			get
			{
				return base[index];
			}
			set
			{
				int quoteCountDelta = value.Count - base[index].Count;

				base[index] = value;
				QuoteCount += quoteCountDelta;
			}
		}

		/// <summary>
		/// Adds required event handlers to each contained quotepage instance.
		/// </summary>
		internal void AddItemEventHandlers()
		{
			QuotePageCollection items = Items;

			for (int i = 0; i < items.Count; i++)
			{
				items[i].PropertyChanged += (sender, e) =>
					{
						if (e.PropertyName == "Count")
						{
							ResetQuoteCount();
						}
					};
			}
		}

		public override void Add(QuotePage item)
		{
			base.Add(item);
			QuoteCount += item.Count;
		}

		public override bool Remove(QuotePage item)
		{
			bool returnValue = base.Remove(item);

			if (returnValue)
			{
				QuoteCount -= item.Count;
			}

			return returnValue;
		}

		public override void Insert(int index, QuotePage item)
		{
			base.Insert(index, item);
			QuoteCount += item.Count;
		}

		public override void RemoveAt(int index)
		{
			QuotePage item = base[index];

			base.RemoveAt(index);
			QuoteCount -= item.Count;
		}

		public override void Clear()
		{
			base.Clear();
			QuoteCount = 0;
		}

		/// <summary>
		/// Resets the instance's quote counter. This method should be called if the
		/// instance's internal collection has changed completely.
		/// </summary>
		private void ResetQuoteCount()
		{
			int quoteCount = 0;
			IList<QuotePage> items = Items;

			_readerWriterLock.EnterReadLock();
			for (int i = 0; i < items.Count; i++)
			{
				quoteCount += items[i].Count;
			}
			_readerWriterLock.ExitReadLock();

			QuoteCount = quoteCount;
		}
	}
}
