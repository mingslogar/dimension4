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

using System;
using System.Collections.Generic;
using System.Globalization;
using WikiquoteScreensaverLib.IO.WebIO;

namespace WikiquoteScreensaverLib.Common
{
    public static class Util
    {
        /// <summary> 
        /// Extension method returning a value indicating whether the specified String object occurs 
        /// within this string with respect to a specific string comparison method.
        /// </summary>
        /// <param name="value">The String object to seek.</param>
        /// <param name="comparison">The string comparison method to use.</param>
        /// <returns>
        /// true if the value parameter occurs within this string, or if value is the empty string (""); 
        /// otherwise, false.
        /// </returns>
        public static bool Contains(this string str, string value, StringComparison comparison)
        {
            return (str != null) && str.IndexOf(value, 0, comparison) >= 0;
        }
    }
}
