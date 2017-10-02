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
// Filename: QuotePageCollection.cs
// Copyright: Christian Hanser 2008
//

using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace WikiquoteScreensaverLib.Common
{
	/// <summary>
	/// A collection class intended for containing quote pages.
	/// </summary>
	[ComVisible(false)]
	public sealed class QuotePageCollection
		: KeyedCollection<Uri, QuotePage>
	{
		protected override Uri GetKeyForItem(QuotePage item)
		{
			return item.Uri;
		}
	}
}
