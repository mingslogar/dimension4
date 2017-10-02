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
// Filename: CultureMapper.cs
// Copyright: Christian Hanser 2008
//

using System;
using System.Collections.Generic;
using System.Globalization;
using WikiquoteScreensaverLib.Common;
using WikiquoteScreensaverLib.Common.ErrorHandling;
using WikiquoteScreensaverLib.IO.WebIO.CollectorRules;

namespace WikiquoteScreensaverLib.IO.WebIO
{
    /// <summary>
    /// Class for managing different quote collector rules and different cultures.
    /// It provides methods for registering culture dependend quote collector rules.
    /// </summary>
    public sealed class CultureMapper 
        : IEnumerable<CultureInfo>
    {
        #region Nested Classes
        private sealed class CultureDataKeyedCollection : ExtendedKeyedCollection<CultureInfo, CultureData>
        {
            const int DefaultDictionaryCreationThreshold = 0;

            public CultureDataKeyedCollection()
                : base(DefaultDictionaryCreationThreshold)
            {
            }

            protected override CultureInfo GetKeyForItem(CultureData item)
            {
                return item.CultureInfo;
            }

            public ICollection<CultureInfo> Keys
            {
                get { return Dictionary.Keys; }
            }
        }
        #endregion

        const string WikiquoteBaseUrl = "http://{0}." + LookupResolver.WikiquoteHost;
        CultureDataKeyedCollection _dataCollection = new CultureDataKeyedCollection();

        public void Add(QuoteCollectorRules rules)
        {
            // checking preconditions
            if (rules == null)
            {
                throw new ArgumentNullException();
            }

            Add(new Uri(String.Format(WikiquoteBaseUrl, rules.Culture.TwoLetterISOLanguageName)), rules);
        }

        public void Add(Uri baseUri, QuoteCollectorRules rules)
        {
            // checking preconditions
            if (baseUri == null || rules == null)
            {
                throw new ArgumentNullException();
            }

            _dataCollection.Add(new CultureData(rules.Culture, baseUri, rules));
        }

        public CultureData this[CultureInfo culture]
        {
            get
            {
                // checking preconditions
                if (culture == null)
                {
                    throw new ArgumentNullException();
                }

                CultureData returnValue;

                if (_dataCollection != null)
                {
                    if (!_dataCollection.TryGetValue(culture, out returnValue))
                    {
                        throw new CultureNotSupportedException(culture);
                    }
                }
                else
                {
                    throw new NotSupportedException();
                }

                return returnValue;
            }
        }

        public bool Contains(CultureInfo culture)
        {
            return culture != null && _dataCollection.Contains(culture);
        } 
 
        #region IEnumerable<CultureInfo> Members

        public IEnumerator<CultureInfo> GetEnumerator()
        {
            return _dataCollection.Keys.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
