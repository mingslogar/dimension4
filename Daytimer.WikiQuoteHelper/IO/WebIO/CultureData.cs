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
// Filename: CultureData.cs
// Copyright: Christian Hanser 2008
//

using System;
using System.Globalization;
using WikiquoteScreensaverLib.IO.WebIO.CollectorRules;

namespace WikiquoteScreensaverLib.IO.WebIO
{
    /// <summary>
    /// Class associating a quote collector, a wikiquote uri and the according quote collector
    /// rules with a culture.
    /// </summary>
    public sealed class CultureData
    {
        readonly CultureInfo _cultureInfo;
        readonly Uri _baseUri;
        readonly QuoteCollectorRules _rules;
 
        public CultureData(CultureInfo cultureInfo, Uri uri, QuoteCollectorRules rules)
        {
            _cultureInfo = cultureInfo;
            _baseUri = uri;
            _rules = rules;
        }
 
        public CultureInfo CultureInfo
        {
            get { return _cultureInfo; }
        }

        public Uri BaseUri
        {
            get { return _baseUri; }
        }

        public QuoteCollectorRules Rules
        {
            get { return _rules; }
        }
    }
}
