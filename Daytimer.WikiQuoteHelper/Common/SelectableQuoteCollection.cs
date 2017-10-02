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
// Filename: SelectableQuoteCollection.cs
// Copyright: Christian Hanser 2008
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

namespace WikiquoteScreensaverLib.Common
{
	/// <summary>
	/// A collection class intended for containing selectable quotes.
	/// </summary>
	[ComVisible(false)]
	public sealed class SelectableQuoteCollection
		: SortedKeyedCollection<string, SelectableQuote>
	{
		IComparer<string> _keyComparer;

		public SelectableQuoteCollection()
			: base(true)
		{
		}

		public CultureInfo Culture { get; set; }

		protected override IComparer<string> KeyComparer
		{
			get
			{
				if (_keyComparer == null)
				{
					_keyComparer = (Culture != null) ?
						StringComparer.Create(Culture, false) :
						base.KeyComparer;
				}

				return _keyComparer;
			}
		}

		protected override string GetKeyForItem(SelectableQuote item)
		{
			return item.Text;
		}
	}
}
