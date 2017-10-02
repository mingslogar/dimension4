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
// Filename: QuoteCollectorEventArgs.cs
// Copyright: Christian Hanser 2008
//

using System;
using System.Collections.Generic;
using System.Globalization;
using WikiquoteScreensaverLib.Common;

namespace WikiquoteScreensaverLib.IO.WebIO
{
    public abstract class QuoteCollectorEventArgs 
        : WebIOEventArgs
    {
        readonly Uri _uri;

        protected QuoteCollectorEventArgs(string topic, CultureInfo culture, Uri uri)
            : base(topic, culture)
        {
            _uri = uri;
        }

        public Uri Uri
        {
            get { return _uri; }
        }
    }

    public sealed class QuotesCollectingCompletedEventArgs 
        : QuoteCollectorEventArgs
    {
        readonly QuotePage _page;
        readonly bool _update;

        internal QuotesCollectingCompletedEventArgs(QuotePage page, bool update)
            : base(page.Topic, page.Culture, page.Uri)
        {
            _page = page;
            _update = update;
        }

        public QuotePage QuotePage
        {
            get { return _page; }
        }

        public bool Update
        {
            get { return _update; }
        } 
    }

    public sealed class ErrorCollectingQuotesEventArgs 
        : QuoteCollectorEventArgs
    {
        readonly Exception _error;

        internal ErrorCollectingQuotesEventArgs(string topic, CultureInfo culture, Uri uri, Exception ex)
            : base(topic, culture, uri)
        {
            _error = ex;
        }

        public Exception Error
        {
            get { return _error; }
        }
    }

    public sealed class NoQuotesCollectedEventArgs 
        : QuoteCollectorEventArgs
    {
        internal NoQuotesCollectedEventArgs(string topic, CultureInfo culture, Uri uri)
            : base(topic, culture, uri)
        {
        }
    }

    public sealed class TopicAmbiguousEventArgs 
        : QuoteCollectorEventArgs
    {
        readonly IEnumerable<TopicChoice> _topicChoices;

        internal TopicAmbiguousEventArgs(string topic, CultureInfo culture, Uri uri, IEnumerable<TopicChoice> topicChoices)
            : base(topic, culture, uri)
        {
            _topicChoices = topicChoices;
        }

        public IEnumerable<TopicChoice> TopicChoices
        {
            get { return _topicChoices; }
        }
    }
}
