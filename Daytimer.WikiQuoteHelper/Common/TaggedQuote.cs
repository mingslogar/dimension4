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
// Filename: TaggedQuote.cs
// Copyright: Christian Hanser 2008
//

using System;
using System.Globalization;

namespace WikiquoteScreensaverLib.Common
{
    /// <summary>
    /// A quote class containing additional data that can be used for the screensaver
    /// presentation mode.
    /// </summary>
    [Serializable]
    public sealed class TaggedQuote 
        : Quote
    {
        readonly string _topic;
        readonly Uri _uri;
        readonly CultureInfo _culture; 
 
        /// <summary>
        /// Initializes a new TaggedQuote instance.
        /// </summary>
        /// <param name="page">The quote page that contains the original quote.</param>
        /// <param name="quote">The original quote.</param>
        internal TaggedQuote(QuotePage page, Quote quote)
            : base(quote.Text, quote.AdditionalInformation)
        {
            _topic = page.Topic;
            _uri = page.Uri;
            _culture = page.Culture;
        } 
 
        /// <summary>
        /// The wikiquote topic to which the quote belongs.
        /// </summary>
        public string Topic
        {
            get { return _topic; }
        }

        /// <summary>
        /// The wikiquote uri wherefrom the quote was taken.
        /// </summary>
        public Uri Uri
        {
            get { return _uri; }
        }

        /// <summary>
        /// The culture corresponding to the quote's language.
        /// </summary>
        public CultureInfo Culture
        {
            get { return _culture; }
        }  
    }
}
