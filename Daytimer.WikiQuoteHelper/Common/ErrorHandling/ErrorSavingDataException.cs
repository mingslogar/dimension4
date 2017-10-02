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
// Filename: ErrorSavingDataException.cs
// Copyright: Christian Hanser 2008
//

using System;
using System.Runtime.Serialization;

namespace WikiquoteScreensaverLib.Common.ErrorHandling
{
    /// <summary>
    /// Exception class used to indicate that an error occured while saving data.
    /// </summary>
    [Serializable]
    public sealed class ErrorSavingDataException 
        : WikiquoteScreensaverLibException
    {
        readonly string _filename;

        internal ErrorSavingDataException(string filename, Exception innerException)
            : base(innerException)
        {
            _filename = filename;
        }

        internal ErrorSavingDataException(string filename)
            : this(filename, null)
        {
        }

        internal ErrorSavingDataException()
            : base()
        {
        }

        private ErrorSavingDataException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string Filename
        {
            get { return _filename; }
        } 
    }
}
