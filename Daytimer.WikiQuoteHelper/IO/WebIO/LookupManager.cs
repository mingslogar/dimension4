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
// Filename: LookupManager.cs
// Copyright: Christian Hanser 2008
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using WikiquoteScreensaverLib.Common;
using WikiquoteScreensaverLib.Common.ErrorHandling;
using Wintellect.Threading.AsyncProgModel;

namespace WikiquoteScreensaverLib.IO.WebIO
{
    public sealed class LookupManager 
        : INotifyPropertyChanged
    {
        const int AsyncEnumeratorDiscardGroup = 0;

        readonly static object _busyLock = new object();
        readonly static object _lookupResolverLock = new object();

        readonly HashSet<AsyncEnumerator> _asyncEnumerators = new HashSet<AsyncEnumerator>();
        readonly HashSet<Uri> _lookupsInProgress = new HashSet<Uri>();
        readonly Playlist _playlist;
        readonly CultureMapper _cultureMapper;
        readonly QuoteCollector _quoteCollector;
        LookupResolver _lookupResolver;

        int _cancelTimeout = 0;
        bool _busy = false;

        public event EventHandler<TopicNotFoundEventArgs> TopicNotFound;
        public event EventHandler<TopicAlreadyExistsEventArgs> TopicAlreadyExists;

        public LookupManager(Playlist playlist, CultureMapper cultureMapper)
        {
            // precondition checking
            if (playlist == null || cultureMapper == null)
            {
                throw new ArgumentNullException();
            }

            _cultureMapper = cultureMapper;
            _playlist = playlist;
            _quoteCollector = new QuoteCollector(playlist, cultureMapper);

            RegisterQuoteCollectorEventHandler();
        }

        public QuoteCollector QuoteCollector
        {
            get { return _quoteCollector; }
        } 

        public LookupResolver LookupResolver
        {
            get 
            {
                lock (_lookupResolverLock)
                {
                    if (_lookupResolver == null)
                    {
                        _lookupResolver = new LookupResolver(_cultureMapper);
                    }

                    return _lookupResolver;
                }
            }
        } 

        public int CancelTimeout 
        {
            get
            {
                return _cancelTimeout;
            }
            set
            {
                // precondition checking
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _cancelTimeout = value;
            }
        }

        public bool IsBusy
        {
            get
            {
                lock (_busyLock)
                {
                    return _busy;
                }
            }
            private set
            {
                lock (_busyLock)
                {
                    if (_busy != value)
                    {
                        _busy = value;
                        OnPropertyChanged("IsBusy");
                    }
                }
            }
        }
 
        public void AddLookup(QuotePage page)
        {
            AddLookups(new QuotePage[] { page });
        }

        public void AddLookups(QuotePage[] pages)
        {
            // precondition checking
            if (pages == null)
            {
                throw new ArgumentNullException();
            }
            else if (pages.Length == 0)
            {
                throw new ArgumentException();
            }

            foreach (QuotePage page in pages)
            {
                if (page == null)
                {
                    throw new ArgumentException();
                }
                else if (!_cultureMapper.Contains(page.Culture))
                {
                    throw new CultureNotSupportedException(page.Culture);
                }
            }

            IsBusy = true;

            AsyncEnumerator asyncEnumerator = GetAsyncEnumerator();
            asyncEnumerator.BeginExecute(ProcessUpdateLookups(asyncEnumerator, pages), asyncEnumerator.EndExecute);
        }

        public void AddLookup(string topic, CultureInfo culture)
        {
            // precondition checking
            if (topic == null || culture == null)
            {
                throw new ArgumentNullException();
            }
            else if (topic.Length == 0)
            {
                throw new ArgumentException();
            }
            else if (!_cultureMapper.Contains(culture))
            {
                throw new CultureNotSupportedException(culture);
            }

            IsBusy = true;

            AsyncEnumerator asyncEnumerator = GetAsyncEnumerator();
            asyncEnumerator.BeginExecute(ProcessLookup(asyncEnumerator, topic, culture), asyncEnumerator.EndExecute);
        }

        public void CancelAllPendingLookups()
        {
            QuoteCollector.CancelAllPendingLookups();

            lock (_asyncEnumerators)
            {
                foreach (AsyncEnumerator asyncEnumerator in _asyncEnumerators)
                {
                    asyncEnumerator.Cancel(null);
                    asyncEnumerator.DiscardGroup(AsyncEnumeratorDiscardGroup);
                }

                _asyncEnumerators.Clear();
            }

            lock (_lookupsInProgress)
            {
                _lookupsInProgress.Clear();
            }

            IsBusy = false;
        }

        private IEnumerator<int> ProcessLookup(AsyncEnumerator asyncEnumerator, string topic, CultureInfo culture)
        {
            LookupResolver.BeginNormalizeTopicOmitChecks(topic, culture, 
                asyncEnumerator.End(AsyncEnumeratorDiscardGroup, LookupResolver.EndNormalizeTopic), null);

            yield return 1;
            if (asyncEnumerator.IsCanceled())
            {
                yield break;
            }

            try
            {
                topic = LookupResolver.EndNormalizeTopic(asyncEnumerator.DequeueAsyncResult());
            }
            catch
            {
                OnTopicNotFound(new TopicNotFoundEventArgs(topic, culture));
                yield break;
            }

            Uri uri = LookupResolver.GetUri(topic, culture);
            bool collectQuotes = false;

            lock (_playlist)
            {
                if (_playlist.Contains(uri))
                {
                    OnTopicAlreadyExists(new TopicAlreadyExistsEventArgs(topic, culture));
                    yield break;
                }
            }

            lock (_lookupsInProgress)
            {
                if (!_lookupsInProgress.Contains(uri))
                {
                    _lookupsInProgress.Add(uri);
                    collectQuotes = true;
                }
            }

            if (collectQuotes)
            {
                QuoteCollector.CollectQuotesAsync(topic, culture, uri);
            }

            lock (_asyncEnumerators)
            {
                _asyncEnumerators.Remove(asyncEnumerator);
            }
        }

        private IEnumerator<int> ProcessUpdateLookups(AsyncEnumerator asyncEnumerator, QuotePage[] pages)
        {
            QuotePage page;

            for (int i = 0; i < pages.Length; i++)
            {
                page = pages[i];
                LookupResolver.BeginNormalizeTopicOmitChecks(page.Topic, page.Culture,
                    asyncEnumerator.End(AsyncEnumeratorDiscardGroup, LookupResolver.EndNormalizeTopic), page);
            }

            IAsyncResult result;
            bool updateQuotes = false;

            for (int i = 0; i < pages.Length; i++)
            {
                yield return 1;
                if (asyncEnumerator.IsCanceled())
                {
                    yield break;
                }

                result = asyncEnumerator.DequeueAsyncResult();
                page = result.AsyncState as QuotePage;

                try
                {
                    page.Topic = LookupResolver.EndNormalizeTopic(result);
                    page.Uri = LookupResolver.GetUri(page.Topic, page.Culture);
                }
                catch
                {
                    OnTopicNotFound(new TopicNotFoundEventArgs(page.Topic, page.Culture));
                    yield break;
                }

                lock (_lookupsInProgress)
                {
                    if (!_lookupsInProgress.Contains(page.Uri))
                    {
                        _lookupsInProgress.Add(page.Uri);
                        updateQuotes = true;
                    }
                }

                if (updateQuotes)
                {
                    QuoteCollector.UpdateQuotesAsync(page);
                }
            }

            lock (_asyncEnumerators)
            {
                _asyncEnumerators.Remove(asyncEnumerator);
            }
        }

        private void OnTopicAlreadyExists(TopicAlreadyExistsEventArgs e)
        {
            SetIsBusy();

            EventHandler<TopicAlreadyExistsEventArgs> handler = TopicAlreadyExists;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnTopicNotFound(TopicNotFoundEventArgs e)
        {
            SetIsBusy();

            EventHandler<TopicNotFoundEventArgs> handler = TopicNotFound;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private AsyncEnumerator GetAsyncEnumerator()
        {
            AsyncEnumerator asyncEnumerator = new AsyncEnumerator();

            if (CancelTimeout > 0)
            {
                asyncEnumerator.SetCancelTimeout(CancelTimeout, null);
            }

            lock (_asyncEnumerators)
            {
                _asyncEnumerators.Add(asyncEnumerator);
            }

            return asyncEnumerator;
        }

        private void RegisterQuoteCollectorEventHandler()
        {
            QuoteCollector.QuoteCollectingCompleted += QuoteCollectorEventArgsHandler<QuotesCollectingCompletedEventArgs>;
            QuoteCollector.NoQuotesCollected += QuoteCollectorEventArgsHandler<NoQuotesCollectedEventArgs>;
            QuoteCollector.TopicAmbiguous += QuoteCollectorEventArgsHandler<TopicAmbiguousEventArgs>;
            QuoteCollector.ErrorCollectingQuotes += QuoteCollectorEventArgsHandler<ErrorCollectingQuotesEventArgs>;
        }

        private void QuoteCollectorEventArgsHandler<T>(object sender, T e)
            where T : QuoteCollectorEventArgs
        {
            lock (_lookupsInProgress)
            {
                _lookupsInProgress.Remove(e.Uri);
            }

            SetIsBusy();
        }

        private void SetIsBusy()
        {
            bool busy;

            lock (_lookupsInProgress)
            {
                busy = (_lookupsInProgress.Count != 0);
            }

            IsBusy = busy;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
