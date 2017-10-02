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
// Filename: QuoteCollector.cs
// Copyright: Christian Hanser 2008
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using WikiquoteScreensaverLib.Common;
using WikiquoteScreensaverLib.Common.ErrorHandling;
using WikiquoteScreensaverLib.IO.WebIO.CollectorRules;
using Wintellect.Threading.AsyncProgModel;

namespace WikiquoteScreensaverLib.IO.WebIO
{
    public sealed class QuoteCollector
    {
        internal const string TopicNameRegexGroup = "topicName";
        internal const string TopicIdRegexGroup = "topicId";
        internal const string TopicDescriptionRegexGroup = "topicDescription";
        internal const string QuoteRegexGroup = "quote";
        internal const string AdditionalInformationRegexGroup = "additionalInformation";
        const string WikiLinkRegexGroup = "wikiLink";
        const string WikiLinkDescriptionRegexGroup = "wikiLinkDescription";
        const string ExternalLinkRegexGroup = "externalLink";
        const string ExternalLinkDescriptionRegexGroup = "externalLinkDescription";
        const string ReplaceBySpaceRegexGroup = "replaceBySpace";
        const string XhtmlTagContentsRegexGroup = "xhtmlTagContents";
        const string UnimportantRegexGroup = "unimportant";
        const string CultureRegexGroup = "culture";
        const string TopicTranslationRegexGroup = "topicTranslation";

        const string UriMediaWikiQuotePageQuery = "action=query&titles={0}&prop=revisions&rvprop=content&format=xml&redirects";

        const string TopicTranslationRegexPattern = @"[\n]\ *\[\[(?'" + CultureRegexGroup + @"'{0}):(?'" + TopicTranslationRegexGroup + @"'.+?)\]\]";
        const string XhtmlTagRegexPattern = @"<(?'tag'[a-z][a-z0-9]*)\b[^>]*>(?'" + XhtmlTagContentsRegexGroup + @"'.*?)</\k'tag'>"; // matches xhtml tags
        const string MiscSequencesRegexPattern = @"(?'" + ReplaceBySpaceRegexGroup + @"'[\n]\ *:|(\ ?[\t]\ ?)+|\ {2,})|" + // matches : at the beginning of a line, tabs and space sequences
                                                 @"(?'" + UnimportantRegexGroup + @"'(?!<nowiki>)('{2,3}|'{5})(?!</nowiki>)|" + // matches '', ''', ''''' commands that are not surrounded by <nowiki> tags
                                                 @"\[\[[^\[]+?:[^[|]+?\]\]|" + // matches [[...:...]] but not [[...:...|...]] (the latter will be replaced later on by its description)
                                                 @"(?'headline'={2,4}).+?=\k'headline'|" + // matches wiki headlines
                                                 @"{{[^{|]+?}}|" + // matches {{...}} but not {{...|...}}
                                                 @"<!--.+?-->|<([a-z][a-z0-9]*)\b[^>]*/>|" + // matches html comments or single html tags
                                                 @"[\n]\*[^*]?\ *\[\[[^[]+?(\|.+?)*?\]\]\ *[\n]|" + // matches a line that only consists of a wiki link
                                                 @"[\n][^*]?\ *\(.+?\)\ *[\n]|" + // matches a line that only comprises (...)
                                                 @"mailto:[^ ]+@[^ ]+\.[^ ]+)|" + // matches a mailto expression
                                                 @"\[\[(?'" + WikiLinkRegexGroup + @"'[^[]+?)(\|(?'" + WikiLinkDescriptionRegexGroup + @"'[^[]+?))?\]\]|" +  // matches wiki links
                                                 @"\[(?'" + ExternalLinkRegexGroup + @"'(http[s]?|ftp|news|irc)://.+?)(\ (?'" +
                                                   ExternalLinkDescriptionRegexGroup + @"'[^[]+?))?\]"; // matches external links

        const string WikipediaLinkPrefix = "w:";
        const int AsyncEnumeratorDiscardGroup = 0;

        static Regex _miscSequencesRegex;
        static Regex _xhtmlTagRegex;

        readonly Regex _topicTranslationsRegex;

        readonly static object _miscSequencesRegexLock = new object();
        readonly static object _xhtmlTagRegexLock = new object();

        delegate SelectableQuoteCollection ParsePageContentsAndExtractQuotesDelegate(string topic, CultureInfo culture, string contents, SelectableQuoteCollection oldQuotes);
        delegate TopicTranslation[] ExtractTopicTranslationsDelegate(string contents);
        delegate void AssignQuotePageDataDelegate(SelectableQuoteCollection quotes, TopicTranslation[] topicTranslations);

        readonly HashSet<AsyncEnumerator> _asyncEnumerators = new HashSet<AsyncEnumerator>();
        readonly Playlist _playlist;
        readonly CultureMapper _cultureMapper;

        readonly ParsePageContentsAndExtractQuotesDelegate _parsePageContentsAndExtractQuotes;
        readonly ExtractTopicTranslationsDelegate _extractTopicTranslationsDelegate;

        public event EventHandler<QuotesCollectingCompletedEventArgs> QuoteCollectingCompleted;
        public event EventHandler<ErrorCollectingQuotesEventArgs> ErrorCollectingQuotes;
        public event EventHandler<NoQuotesCollectedEventArgs> NoQuotesCollected;
        public event EventHandler<TopicAmbiguousEventArgs> TopicAmbiguous;

        public QuoteCollector(Playlist playlist, CultureMapper cultureMapper)
        {
            // checking preconditions
            if (playlist == null || cultureMapper == null)
            {
                throw new ArgumentNullException();
            }

            _playlist = playlist;
            _cultureMapper = cultureMapper;
            _parsePageContentsAndExtractQuotes = ParsePageContentsAndExtractQuotes;
            _extractTopicTranslationsDelegate = ExtractTopicTranslations;
            _topicTranslationsRegex = CompileRegex(cultureMapper.ToList(), TopicTranslationRegexPattern, RegexOptions.ExplicitCapture);
        }

        private static Regex MiscSequencesRegex
        {
            get
            {
                lock (_miscSequencesRegexLock)
                {
                    if (_miscSequencesRegex == null)
                    {
                        _miscSequencesRegex = new Regex(MiscSequencesRegexPattern, RegexOptions.ExplicitCapture);
                    }

                    return _miscSequencesRegex;
                }
            }
        }

        private static Regex XhtmlTagRegex
        {
            get
            {
                lock (_xhtmlTagRegexLock)
                {
                    if (_xhtmlTagRegex == null)
                    {
                        _xhtmlTagRegex = new Regex(XhtmlTagRegexPattern, RegexOptions.ExplicitCapture);
                    }

                    return _xhtmlTagRegex;
                }
            }
        }

        private Uri GetQuoteSourceUri(CultureInfo culture, string topic)
        {
            UriBuilder uriBuilder = new UriBuilder(_cultureMapper[culture].BaseUri)
                {
                    Path = LookupResolver.UriMediaWikiPath,
                    Query = String.Format(UriMediaWikiQuotePageQuery, topic.Trim())
                };

            return uriBuilder.Uri;
        }

        private SelectableQuoteCollection ParsePageContentsAndExtractQuotes(string topic, CultureInfo culture, string contents, SelectableQuoteCollection oldQuotes)
        {
            QuoteCollectorRules rules = _cultureMapper[culture].Rules as QuoteCollectorRules;

            if (rules.DisambiguationTemplateIdentifiers != null && rules.DisambiguationTemplateRegex != null)
            {
                for (int i = 0; i < rules.DisambiguationTemplateIdentifiers.Length; i++)
                {
                    if (contents.Contains(rules.DisambiguationTemplateIdentifiers[i]))
                    {
                        throw new TopicAmbiguousException(topic, GetTopicChoices(contents, rules));
                    }
                }
            }

            // removing sections that do not contain quotes (only if required, i.e. quotes have to be determined heuristically)
            if (rules.WikiSectionsToSkipRegex != null)
            {
                contents = rules.WikiSectionsToSkipRegex.Replace(contents, String.Empty);
            }

            // replacing xhtml tags by their contents
            contents = ReplaceXhtmlTags(contents);

            // various string replacements
            contents = HttpUtility.HtmlDecode(ReplaceMiscSequences(contents));

            // extracting quotes
            MatchCollection quoteMatches = rules.QuoteRegex.Matches(contents);

            var quotes = from Match match in quoteMatches
                         where match.Groups[QuoteRegexGroup].Success
                         let quoteHelper = match.Groups[QuoteRegexGroup].Value
                         let quote = (rules.NewlineRegex != null) ?
                                     rules.NewlineRegex.Replace(quoteHelper, "\n") :
                                     quoteHelper
                         let additionalInformation = match.Groups[AdditionalInformationRegexGroup].Success ?
                                                     rules.NewlineRegex.Replace(match.Groups[AdditionalInformationRegexGroup].Value, " ") :
                                                     String.Empty
                         select GetQuote(oldQuotes, quote.TrimEnd(), additionalInformation.TrimEnd());

            return quotes.ToSelectableQuoteCollection(culture);
        }

        private TopicTranslation[] ExtractTopicTranslations(string contents)
        {
            TopicTranslation[] returnValue = null;

            if (_topicTranslationsRegex != null)
            {
                MatchCollection topicTranslationMatches = _topicTranslationsRegex.Matches(contents);

                if (topicTranslationMatches.Count > 0)
                {
                    string culture;
                    string topicTranslation;
                    returnValue = new TopicTranslation[topicTranslationMatches.Count];

                    for (int i = 0; i < topicTranslationMatches.Count; i++)
                    {
                        if (topicTranslationMatches[i].Groups[CultureRegexGroup].Success &&
                            topicTranslationMatches[i].Groups[TopicTranslationRegexGroup].Success)
                        {
                            culture = topicTranslationMatches[i].Groups[CultureRegexGroup].Value;
                            topicTranslation = topicTranslationMatches[i].Groups[TopicTranslationRegexGroup].Value;

                            returnValue[i] = new TopicTranslation(new CultureInfo(culture), topicTranslation);
                        }
                    }
                }
            }

            return returnValue;
        }

        internal void CancelAllPendingLookups()
        {
            lock (_asyncEnumerators)
            {
                foreach (AsyncEnumerator asyncEnumerator in _asyncEnumerators)
                {
                    asyncEnumerator.Cancel(null);
                    asyncEnumerator.DiscardGroup(AsyncEnumeratorDiscardGroup);
                }

                _asyncEnumerators.Clear();
            }
        }

        internal void CollectQuotesAsync(string topic, CultureInfo culture, Uri uri)
        {
            AsyncEnumerator asyncEnumerator = new AsyncEnumerator();

            lock (_asyncEnumerators)
            {
                _asyncEnumerators.Add(asyncEnumerator);
            }

            asyncEnumerator.BeginExecute(ProcessCollectQuotes(asyncEnumerator, topic, culture, uri, 
                (quotes, topicTranslations) => 
                    {
                        QuotePage page = new QuotePage(topic, uri, culture, quotes, topicTranslations);

                        OnQuotesCollectingCompleted(new QuotesCollectingCompletedEventArgs(page, false));
                        _playlist.Add(page);
                    }), 
                asyncEnumerator.EndExecute);
        }

        internal void UpdateQuotesAsync(QuotePage page)
        {
            AsyncEnumerator asyncEnumerator = new AsyncEnumerator();

            lock (_asyncEnumerators)
            {
                _asyncEnumerators.Add(asyncEnumerator);
            }

            asyncEnumerator.BeginExecute(ProcessCollectQuotes(asyncEnumerator, page.Topic, page.Culture, page.Uri, 
                (quotes, topicTranslations) =>
                    {
                        page.Items = quotes;
                        page.TopicTranslations = topicTranslations;

                        OnQuotesCollectingCompleted(new QuotesCollectingCompletedEventArgs(page, true));
                    }),
                asyncEnumerator.EndExecute);
        }

        private void OnErrorCollectingQuotes(ErrorCollectingQuotesEventArgs e)
        {
            EventHandler<ErrorCollectingQuotesEventArgs> handler = ErrorCollectingQuotes;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnQuotesCollectingCompleted(QuotesCollectingCompletedEventArgs e)
        {
            EventHandler<QuotesCollectingCompletedEventArgs> handler = QuoteCollectingCompleted;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnNoQuotesCollected(NoQuotesCollectedEventArgs e)
        {
            EventHandler<NoQuotesCollectedEventArgs> handler = NoQuotesCollected;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnTopicAmbiguous(TopicAmbiguousEventArgs e)
        {
            EventHandler<TopicAmbiguousEventArgs> handler = TopicAmbiguous;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private IEnumerator<int> ProcessCollectQuotes(AsyncEnumerator asyncEnumerator, string topic, CultureInfo culture, Uri uri, AssignQuotePageDataDelegate assignQuotesDelegate)
        {
            Uri quoteSourceUri = GetQuoteSourceUri(culture, topic);

            Util.BeginFetchXmlPage(quoteSourceUri, asyncEnumerator.End(AsyncEnumeratorDiscardGroup, Util.EndFetchXmlPage), null);

            yield return 1;
            if (asyncEnumerator.IsCanceled())
            {
                yield break;
            }

            string contents;

            try
            {
                XDocument xDocument = Util.EndFetchXmlPage(asyncEnumerator.DequeueAsyncResult());

                if (xDocument == null)
                {
                    throw new NullReferenceException();
                }

                // extracting the page contents
                contents = GetXmlPageContents(xDocument);
            }
            catch (Exception ex)
            {
                OnErrorCollectingQuotes(new ErrorCollectingQuotesEventArgs(topic, culture, uri, ex));
                yield break;
            }

            IAsyncResult getQuotesResult = BeginParsePageContentsAndExtractQuotes(topic, culture, contents, null,
                asyncEnumerator.End(AsyncEnumeratorDiscardGroup, EndParsePageContentsAndExtractQuotes), null);
            IAsyncResult getTopicTranslationsResult = BeginExtractTopicTranslations(contents,
                asyncEnumerator.End(AsyncEnumeratorDiscardGroup, EndExtractTopicTranslations), null);

            yield return 2;
            if (asyncEnumerator.IsCanceled())
            {
                yield break;
            }

            try
            {
                TopicTranslation[] topicTranslations = EndExtractTopicTranslations(getTopicTranslationsResult);
                SelectableQuoteCollection quotes = EndParsePageContentsAndExtractQuotes(getQuotesResult);

                if (quotes.Count == 0)
                {
                    OnNoQuotesCollected(new NoQuotesCollectedEventArgs(topic, culture, uri));

					// Change by Ming Slogar on 26Apr2014 at 20:25
					// Reason: The following line prevents the topic from being returned
					//		when it has no listed quotes.
                    //yield break;
                }

                assignQuotesDelegate.Invoke(quotes, topicTranslations);
            }
            catch (TopicAmbiguousException ex)
            {
                OnTopicAmbiguous(new TopicAmbiguousEventArgs(topic, culture, uri, ex.TopicChoices));
                yield break;
            }
            catch (Exception ex)
            {
                OnErrorCollectingQuotes(new ErrorCollectingQuotesEventArgs(topic, culture, uri, ex));
                yield break;
            }
            finally
            {
                // cleanup
                asyncEnumerator.DequeueAsyncResult();
                asyncEnumerator.DequeueAsyncResult();

                lock (asyncEnumerator)
                {
                    _asyncEnumerators.Remove(asyncEnumerator);
                }
            }   
        }

        private IAsyncResult BeginParsePageContentsAndExtractQuotes(string topic, CultureInfo culture, string contents, SelectableQuoteCollection oldQuotes,
            AsyncCallback callback, object state)
        {
            return _parsePageContentsAndExtractQuotes.BeginInvoke(topic, culture, contents, oldQuotes, callback, state);
        }

        private SelectableQuoteCollection EndParsePageContentsAndExtractQuotes(IAsyncResult result)
        {
            return _parsePageContentsAndExtractQuotes.EndInvoke(result);
        }

        private IAsyncResult BeginExtractTopicTranslations(string contents, AsyncCallback callback, object state)
        {
            return _extractTopicTranslationsDelegate.BeginInvoke(contents, callback, state);
        }

        private TopicTranslation[] EndExtractTopicTranslations(IAsyncResult result)
        {
            return _extractTopicTranslationsDelegate.EndInvoke(result);
        }

        private static string GetXmlPageContents(XDocument xDocument)
        {
            string returnValue = (from items in xDocument.Element(MediaWikiApiSymbols.ApiName).Element(MediaWikiApiSymbols.QueryName).Element(MediaWikiApiSymbols.PagesName).
                                    Element(MediaWikiApiSymbols.PageName).Element(MediaWikiApiSymbols.RevisionsName).Descendants(MediaWikiApiSymbols.RevName)
                                  select items).First().Value;
            return returnValue;
        }

        private static IEnumerable<TopicChoice> GetTopicChoices(string contents, QuoteCollectorRules rules)
        {
            var returnValue = from Match match in rules.DisambiguationTemplateRegex.Matches(contents)
                              where match.Groups[TopicIdRegexGroup].Success
                              let topicId = HttpUtility.HtmlDecode(match.Groups[TopicIdRegexGroup].Value)
                              let topicName = match.Groups[TopicNameRegexGroup].Success ?
                                  HttpUtility.HtmlDecode(match.Groups[TopicNameRegexGroup].Value) :
                                  topicId
                              let topicDescription = match.Groups[TopicDescriptionRegexGroup].Success ?
                                  HttpUtility.HtmlDecode(ReplaceMiscSequences(match.Groups[TopicDescriptionRegexGroup].Value)) :
                                  null
                              where !topicId.StartsWith(WikipediaLinkPrefix)
                              select new TopicChoice(topicId, topicName, topicDescription);

            return returnValue;
        }

        private static string ReplaceXhtmlTags(string contents)
        {
            contents = XhtmlTagRegex.Replace(contents, (match) =>
                {
                    string returnValue = String.Empty;

                    if (match.Value.Length > 0)
                    {
                        if (match.Groups[XhtmlTagContentsRegexGroup].Success)
                        {
                            returnValue = match.Groups[XhtmlTagContentsRegexGroup].Value;
                        }
                    }

                    return returnValue;
                });

            return contents;
        }

        private static string ReplaceMiscSequences(string contents)
        {
            contents = MiscSequencesRegex.Replace(contents, (match) =>
                {
                    string returnValue = String.Empty;

                    if (match.Value.Length > 0)
                    {
                        if (match.Groups[ReplaceBySpaceRegexGroup].Success)
                        {
                            returnValue = " ";
                        }
                        else if (match.Groups[WikiLinkRegexGroup].Success)
                        {
                            if (match.Groups[WikiLinkDescriptionRegexGroup].Success)
                            {
                                returnValue = match.Groups[WikiLinkDescriptionRegexGroup].Value;
                            }
                            else
                            {
                                returnValue = match.Groups[WikiLinkRegexGroup].Value;
                            }
                        }
                        else if (match.Groups[ExternalLinkRegexGroup].Success)
                        {
                            if (match.Groups[ExternalLinkDescriptionRegexGroup].Success)
                            {
                                returnValue = match.Groups[ExternalLinkDescriptionRegexGroup].Value;
                            }
                            else
                            {
                                returnValue = match.Groups[ExternalLinkRegexGroup].Value;
                            }
                        }
                    }

                    return returnValue;
                });

            return contents;
        }

        private static SelectableQuote GetQuote(SelectableQuoteCollection oldQuotes, string quoteText, string additionalInformation)
        {
            SelectableQuote returnValue;
            SelectableQuote oldQuote;

            if (oldQuotes != null && oldQuotes.TryGetValue(quoteText, out oldQuote))
            {
                if (oldQuote.AdditionalInformation != additionalInformation)
                {
                    oldQuote.AdditionalInformation = additionalInformation;
                }

                returnValue = oldQuote;
            }
            else
            {
                returnValue = new SelectableQuote(quoteText, additionalInformation);
            }

            return returnValue;
        }

        internal static string CompileRegexPattern<T>(IList<T> dataToFillIn, string patternString)
        {
            string returnValue = null;

            if (dataToFillIn != null && dataToFillIn.Count > 0)
            {
                string regexPattern;

                if (dataToFillIn.Count > 1)
                {
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < dataToFillIn.Count - 1; i++)
                    {
                        builder.AppendFormat(patternString, dataToFillIn[i]);
                        builder.Append("|");
                    }

                    builder.AppendFormat(patternString, dataToFillIn[dataToFillIn.Count - 1]);

                    regexPattern = builder.ToString();
                }
                else
                {
                    regexPattern = String.Format(patternString, dataToFillIn[0]);
                }

                returnValue = regexPattern;
            }

            return returnValue;
        }

        internal static Regex CompileRegex<T>(IList<T> dataToFillIn, string patternString, RegexOptions regexOptions)
        {
            return new Regex(CompileRegexPattern(dataToFillIn, patternString), regexOptions); ;
        }
    }
}
