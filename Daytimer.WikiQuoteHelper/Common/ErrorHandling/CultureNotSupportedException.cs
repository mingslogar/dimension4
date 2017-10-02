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
// Filename: CultureNotSupportedException.cs
// Copyright: Christian Hanser 2008
//

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace WikiquoteScreensaverLib.Common.ErrorHandling
{
    /// <summary>
    /// Exception class used to indicate that a specific culture is not supported
    /// by the library.
    /// </summary>
    [Serializable]
    public sealed class CultureNotSupportedException 
        : WikiquoteScreensaverLibException
    {
        readonly CultureInfo _culture;

        internal CultureNotSupportedException(CultureInfo culture, Exception innerException)
            : base(innerException)
        {
            _culture = culture;
        }

        internal CultureNotSupportedException(CultureInfo culture)
            : this(culture, null)
        {
        }

        internal CultureNotSupportedException()
            : base()
        {
        }

        private CultureNotSupportedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public CultureInfo Culture
        {
            get { return _culture; }
        } 
    }
}
