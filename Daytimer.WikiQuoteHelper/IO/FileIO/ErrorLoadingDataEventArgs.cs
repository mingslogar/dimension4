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
// Filename: ErrorLoadingDataEventArgs.cs
// Copyright: Christian Hanser 2008
//

using System;

namespace WikiquoteScreensaverLib.IO.FileIO
{
    public sealed class ErrorLoadingDataEventArgs 
        : EventArgs
    {
        readonly string _filepath;
        readonly Exception _exception;

        public ErrorLoadingDataEventArgs(string filepath, Exception exception)
        {
            _filepath = filepath;
            _exception = exception;
        }

        public string Filename
        {
            get { return _filepath; }
        } 

        public Exception Error
        {
            get { return _exception; }
        }
    }
}
