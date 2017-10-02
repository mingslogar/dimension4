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
// Filename: WikiQuoteScreenSaverException.cs
// Copyright: Christian Hanser 2008
//

using System;
using System.Runtime.Serialization;

namespace WikiquoteScreensaverLib.Common.ErrorHandling
{
    /// <summary>
    /// Base class for all other library exceptions.
    /// </summary>
    [Serializable]
    public abstract class WikiquoteScreensaverLibException 
        : Exception
    {
        protected WikiquoteScreensaverLibException()
            : base()
        {
        }

        protected WikiquoteScreensaverLibException(string message)
            : base(message)
        {
        }

        protected WikiquoteScreensaverLibException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected WikiquoteScreensaverLibException(Exception innerException)
            : base(null, innerException)
        {
        }

        protected WikiquoteScreensaverLibException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
