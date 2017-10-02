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
// Filename: WebIOEventArgs.cs
// Copyright: Christian Hanser 2008
//

using System;
using System.Globalization;

namespace WikiquoteScreensaverLib.IO.WebIO
{
    public abstract class WebIOEventArgs 
        : EventArgs
    {
        readonly string _topic;
        readonly CultureInfo _culture;

        protected WebIOEventArgs(string topic, CultureInfo culture)
        {
            _topic = topic;
            _culture = culture;
        }

        public string Topic
        {
            get { return _topic; }
        }

        public CultureInfo Culture
        {
            get { return _culture; }
        }
    }
}
