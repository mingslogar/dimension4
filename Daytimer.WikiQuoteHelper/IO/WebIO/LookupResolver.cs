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
// Filename: LookupResolver.cs
// Copyright: Christian Hanser 2008
//

using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using WikiquoteScreensaverLib.Common.ErrorHandling;

namespace WikiquoteScreensaverLib.IO.WebIO
{
    public sealed class LookupResolver
    {
        public const string WikiquoteHost = "wikiquote.org";
        public const string UriDefaultWikiPath = "/wiki/";
        public const string UriMediaWikiPath = "/w/api.php";

        const char UriSpaceReplacementCharacter = '_';
        const string UriMediaWikiApiNormalizationQuery = "action=query&titles={0}&redirects&format=xml";

        delegate string NormalizeTopicDelegate(string topic, CultureInfo culture);

        readonly CultureMapper _cultureMapper;
        readonly NormalizeTopicDelegate _normalizeTopicDelegate;

        public LookupResolver(CultureMapper cultureMapper)
        {
            if (cultureMapper == null)
            {
                throw new ArgumentNullException();
            }

            _cultureMapper = cultureMapper;
            _normalizeTopicDelegate = NormalizeTopicOmitChecks;
        }

        private static string NormalizeTopicHelper(Uri baseUri, string topic)
        {
            string returnValue = null;

            UriBuilder uriBuilder = new UriBuilder(baseUri)
                {
                    Path = UriMediaWikiPath,
                    Query = String.Format(UriMediaWikiApiNormalizationQuery, topic)
                };

            XDocument metaDataPage = Util.FetchXmlPage(uriBuilder.Uri);

            if (metaDataPage != null)
            {
                var pageTag = (from items in metaDataPage.Element(MediaWikiApiSymbols.ApiName).Element(MediaWikiApiSymbols.QueryName).Element(MediaWikiApiSymbols.PagesName).Descendants(MediaWikiApiSymbols.PageName)
                               select items).First();

                // check whether the topic name was found
                if (pageTag.Attribute(MediaWikiApiSymbols.MissingName) == null)
                {
                    returnValue = pageTag.Attribute(MediaWikiApiSymbols.TitleName).Value;
                }
            }

            return returnValue;
        }

        internal IAsyncResult BeginNormalizeTopicOmitChecks(string topic, CultureInfo culture, AsyncCallback callback, object state)
        {
            return _normalizeTopicDelegate.BeginInvoke(topic, culture, callback, state);
        }

        internal string NormalizeTopicOmitChecks(string topic, CultureInfo culture)
        {
            string returnValue = null;
            Uri baseUri = _cultureMapper[culture].BaseUri;

            try
            {
                returnValue = NormalizeTopicHelper(baseUri, topic);

                if (returnValue == null)
                {
                    returnValue = NormalizeTopicHelper(baseUri, topic.Capitalize(culture));
                }
            }
            catch
            {
                throw new QuotePageNotFoundException(topic);
            }

            if (returnValue == null)
            {
                throw new QuotePageNotFoundException(topic);
            }

            return returnValue;
        }

        public Uri GetUri(string topic, CultureInfo culture)
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

            UriBuilder uriBuilder = new UriBuilder(_cultureMapper[culture].BaseUri) { Path = UriDefaultWikiPath + topic.Replace(' ', '_') };

            return uriBuilder.Uri;
        }

        public IAsyncResult BeginNormalizeTopic(string topic, CultureInfo culture, AsyncCallback callback, object state)
        {
            // precondition checking
            if (culture == null || topic == null)
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

            return _normalizeTopicDelegate.BeginInvoke(topic, culture, callback, state);
        }

        public string EndNormalizeTopic(IAsyncResult result)
        {
            return _normalizeTopicDelegate.EndInvoke(result);
        }

        public string NormalizeTopic(string topic, CultureInfo culture)
        {
            // precondition checking
            if (culture == null || topic == null)
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

            return NormalizeTopicOmitChecks(topic, culture);
        }

        public bool CanHandleUri(Uri uri, out CultureInfo culture)
        {
            // precondition checking
            if (uri == null)
            {
                throw new ArgumentNullException();
            }

            bool returnValue = false;
            culture = null;

            if (uri.Host.EndsWith(WikiquoteHost) && uri.PathAndQuery.StartsWith(UriDefaultWikiPath))
            {
                try
                {
                    CultureInfo helper = new CultureInfo(uri.Host.Substring(0, uri.Host.IndexOf('.')));

                    if (_cultureMapper.Contains(helper))
                    {
                        culture = helper;
                        returnValue = true;
                    }
                }
                catch
                {
                    // no action, returnValue is already false
                }
            }

            return returnValue;
        }
    }
}
