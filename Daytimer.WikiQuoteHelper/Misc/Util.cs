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
// Filename: Util.cs
// Copyright: Christian Hanser 2008
//

#if QUOTE_FINDER
using System;

namespace WikiquoteScreensaverLib.Common.Util
{
    internal static class Util
    {
        /// <summary>
        /// Extension method returning a StringComparer instance belonging to a StringComparison value.
        /// </summary>
        /// <returns>The StringComparer instance belonging to <paramref name="comparison"/></returns>
        internal static StringComparer GetStringComparer(this StringComparison comparison)
        {
            StringComparer returnValue = null;

            switch (comparison)
            {
                case StringComparison.CurrentCulture :
                    returnValue = StringComparer.CurrentCulture;
                    break;
                case StringComparison.CurrentCultureIgnoreCase :
                    returnValue = StringComparer.CurrentCultureIgnoreCase;
                    break;
                case StringComparison.InvariantCulture :
                    returnValue = StringComparer.InvariantCulture;
                    break;
                case StringComparison.InvariantCultureIgnoreCase :
                    returnValue = StringComparer.InvariantCultureIgnoreCase;
                    break;
                case StringComparison.Ordinal :
                    returnValue = StringComparer.Ordinal;
                    break;
                case StringComparison.OrdinalIgnoreCase :
                    returnValue = StringComparer.OrdinalIgnoreCase;
                    break;
            };

            return returnValue;
        }
    }
}
#endif