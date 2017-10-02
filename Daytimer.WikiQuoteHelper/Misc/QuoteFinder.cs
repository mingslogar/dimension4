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
// Filename: QuoteFinder.cs
// Copyright: Christian Hanser 2008
//

#if QUOTE_FINDER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using MultithreadBinding;
using WikiquoteScreensaverLib.Common;
using WikiquoteScreensaverLib.Common.Util;

namespace WikiquoteScreensaverLib.Misc
{
    public sealed class QuoteFinder
    {
        #region Nested Classes
        private sealed class SearchCacheKey
        {
            readonly string _searchText;
            readonly SearchTargets _searchMode;

            public SearchCacheKey(string searchText, SearchTargets searchMode)
            {
                _searchText = searchText;
                _searchMode = searchMode;
            }

            public string SearchText
            {
                get { return _searchText; }
            }

            public SearchTargets SearchTarget
            {
                get { return _searchMode; }
            }
        }

        private sealed class SearchCacheKeyEqualityComparer : IEqualityComparer<SearchCacheKey>
        {
            const string SEARCH_MODES_STRING_FORMAT = "d";
            readonly StringComparison _stringComparison;
            readonly StringComparer _stringComparer;

            public SearchCacheKeyEqualityComparer(StringComparison stringComparison, StringComparer stringComparer)
            {
                _stringComparison = stringComparison;
                _stringComparer = stringComparer;
            }

            #region IEqualityComparer<SearchCacheKey> Members

            public bool Equals(SearchCacheKey x, SearchCacheKey y)
            {
                return (_stringComparer.Compare(x.SearchText, y.SearchText) == 0 && 
                        x.SearchTarget == y.SearchTarget);
            }

            public int GetHashCode(SearchCacheKey obj)
            {
                string searchText;

                switch (_stringComparison)
                {
                    case StringComparison.CurrentCultureIgnoreCase :
                        searchText = obj.SearchText.ToLower();
                        break;
                    case StringComparison.InvariantCultureIgnoreCase :
                        searchText = obj.SearchText.ToLowerInvariant();
                        break;
                    default :
                        searchText = obj.SearchText;
                        break;
                }

                return (searchText + obj.SearchTarget.ToString(SEARCH_MODES_STRING_FORMAT)).GetHashCode();
            }

            #endregion
        }
        #endregion

        #region Enumerations
        public enum SearchTargets 
            { 
                QuoteTexts, 
                AdditionalInformation, 
                Everything 
            };
        #endregion

        #region Constants
        const int CREATE_CACHE_THRESHOLD = 80;
        #endregion

        #region Member Variables
        readonly StringComparer _stringComparer;
        readonly StringComparison _stringComparison;
        readonly SearchCacheKeyEqualityComparer _searchCacheKeyEqualityComparer;
        readonly int _cacheThreshold;
        readonly Dispatcher _dispatcher;
        #endregion

        #region Member Variables
        QuotePage _currentQuotePage;
        Dictionary<SearchCacheKey, SelectableQuote[]> _searchResults;
        Dictionary<QuotePage, WeakReference> _oldSearchResults;
        string _oldSearchText = String.Empty;
        SearchTargets _oldSearchTarget = SearchTargets.QuoteTexts;
        #endregion

        #region Constructors
        public QuoteFinder(StringComparison stringComparison)
            : this(stringComparison, null, CREATE_CACHE_THRESHOLD)
        {
        }

        public QuoteFinder(StringComparison stringComparison, int cacheThreshold)
            : this(stringComparison, null, cacheThreshold)
        {
        }

        public QuoteFinder(StringComparison stringComparison, Dispatcher dispatcher)
            : this(stringComparison, dispatcher, CREATE_CACHE_THRESHOLD)
        {
        }

        public QuoteFinder(StringComparison stringComparison, Dispatcher dispatcher, int cacheThreshold)
        {
            // checking preconditions
            if (stringComparison == StringComparison.Ordinal ||
                stringComparison == StringComparison.OrdinalIgnoreCase)
            {
                throw new NotSupportedException();
            }
            else if (cacheThreshold < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            _cacheThreshold = cacheThreshold;
            _stringComparison = stringComparison;
            _stringComparer = stringComparison.GetStringComparer();
            _searchCacheKeyEqualityComparer = new SearchCacheKeyEqualityComparer(stringComparison, _stringComparer);
            _dispatcher = dispatcher;
        }
        #endregion

        #region Properties
        private Dictionary<QuotePage, WeakReference> OldSearchResults
        {
            get 
            {
                if (_oldSearchResults == null)
                {
                    _oldSearchResults = new Dictionary<QuotePage, WeakReference>();
                }

                return _oldSearchResults; 
            }
        }

        public QuotePage CurrentQuotePage 
        {
            set
            {
                _searchResults = null;
                _currentQuotePage = value;
            }
        }
        #endregion

        #region Public Methods
        public void RemoveResultsFor(QuotePage page)
        {
            if (_currentQuotePage == page)
            {
                _searchResults = null;
            }

            if (_oldSearchResults != null)
            {
                _oldSearchResults.Remove(page);
            }
        }

        public void Reset()
        {
            CurrentQuotePage = null;
            _searchResults = null;
            _oldSearchResults = null;
            _oldSearchText = String.Empty;
            _oldSearchTarget = SearchTargets.QuoteTexts;
        }

        public IList<SelectableQuote> FindQuotes(string searchText, SearchTargets searchTarget)
        {
            if (searchText == null)
            {
                throw new ArgumentNullException();
            }

            IList<SelectableQuote> returnValue;

            if (searchText.Length > 0)
            {
                // only start caching if number of quotes exceeds the value of _cacheThreshold
                if (_currentQuotePage.Count >= _cacheThreshold)
                {
                    if (_searchResults == null)
                    {
                        WeakReference oldReference;

                        // try to get old search results belonging to the current QuotePage
                        if (OldSearchResults.TryGetValue(_currentQuotePage, out oldReference))
                        {
                            _searchResults = oldReference.Target as Dictionary<SearchCacheKey, SelectableQuote[]>;

                            // if not possible create it newly
                            if (_searchResults == null)
                            {
                                _searchResults = new Dictionary<SearchCacheKey, SelectableQuote[]>(_searchCacheKeyEqualityComparer);
                                OldSearchResults[_currentQuotePage] = new WeakReference(_searchResults);
                            }
                        }
                        else
                        {
                            _searchResults = new Dictionary<SearchCacheKey, SelectableQuote[]>(_searchCacheKeyEqualityComparer);
                            OldSearchResults.Add(_currentQuotePage, new WeakReference(_searchResults));
                        }
                    }

                    SelectableQuote[] result;
                    SearchCacheKey newKey = new SearchCacheKey(searchText, searchTarget);

                    // does a matching entry already exist?
                    if (_searchResults.TryGetValue(newKey, out result))
                    {
                        returnValue = result;
                    }
                    // is there an entry that can be reused for searching?
                    else if (searchText.Contains(_oldSearchText, _stringComparison) &&
                             ((_oldSearchTarget == searchTarget &&
                               _searchResults.TryGetValue(new SearchCacheKey(_oldSearchText, _oldSearchTarget), out result)) ||
                              _searchResults.TryGetValue(new SearchCacheKey(_oldSearchText, SearchTargets.Everything), out result)))
                    {
                        returnValue = SearchQuotesCached(result, newKey);
                    }
                    // start search using the whole QuotePage
                    else
                    {
                        returnValue = SearchQuotesCached(_currentQuotePage, newKey);
                    }
                }
                // perform uncached search
                else
                {
                    returnValue = SearchQuotes(_currentQuotePage, searchText, searchTarget);
                }
            }
            else
            {
                returnValue = (_dispatcher != null) ?
                    new DispatchingList<QuotePage, SelectableQuote>(_currentQuotePage, _dispatcher) as IList<SelectableQuote> :
                    _currentQuotePage as IList<SelectableQuote>;
            }

            return returnValue;
        }
        #endregion

        #region Private Methods
        private SelectableQuote[] SearchQuotes(IEnumerable<SelectableQuote> searchSource, string searchText, SearchTargets searchTarget)
        {
            IEnumerable<SelectableQuote> quotes;

            // get proper linq expression
            switch (searchTarget)
            {
                case SearchTargets.QuoteTexts:
                    quotes = from quote in searchSource
                             where quote.Text.Contains(searchText, _stringComparison)
                             select quote;
                    break;
                case SearchTargets.AdditionalInformation:
                    quotes = from quote in searchSource
                             where quote.AdditionalInformation.Contains(searchText, _stringComparison)
                             select quote;
                    break;
                default:
                    quotes = from quote in searchSource
                             where quote.Text.Contains(searchText, _stringComparison) ||
                                   quote.AdditionalInformation.Contains(searchText, _stringComparison)
                             select quote;
                    break;
            }

            return quotes.ToArray();
        }

        private SelectableQuote[] SearchQuotesCached(IEnumerable<SelectableQuote> searchSource, SearchCacheKey key)
        {
            SelectableQuote[] returnValue = SearchQuotes(searchSource, key.SearchText, key.SearchTarget);

            // add caching information
            _oldSearchText = key.SearchText;
            _oldSearchTarget = key.SearchTarget;
            _searchResults.Add(key, returnValue);

            return returnValue;
        }  
        #endregion
    }
}
#endif