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
// Filename: GermanQuoteCollectorRules.cs
// Copyright: Christian Hanser 2008
//

using System.Globalization;

namespace WikiquoteScreensaverLib.IO.WebIO.CollectorRules
{
    /// <summary>
    /// Rules for the mediawiki api quote collector that allows parsing of wikiquote projects (e.g. the
    /// German wikiquote project) using the following quote format:
    /// * "(quote)" - (additional information)
    /// 
    /// It also supports parsing of templates that suggest several possible topic names.
    /// </summary>
    public sealed class GermanQuoteCollectorRules 
        : QuoteCollectorRules
    {
        const string QuoteRegexPattern = "^\\*[^*]?\"\\ *(?'" + QuoteCollector.QuoteRegexGroup + "'[^\"*{]{2,}?)\\ *\"" +
                                         "(\\s*?(-|\\*{2})\\ *)?(?'" + QuoteCollector.AdditionalInformationRegexGroup + "'.*?)\\ *$";
        const string DisambiguationTemplateRegexPattern = @"[\n]\*{1,2}[^*]?\ *\[\[\ *(?'" + QuoteCollector.TopicIdRegexGroup + @"'[^[|]+?)\ *" +
                                                          @"(\|(?'" + QuoteCollector.TopicNameRegexGroup + @"'[^[]+?)\ *)*?\]\]" +
                                                          @"\ *[,]?\ *(?'" + QuoteCollector.TopicDescriptionRegexGroup + @"'[^*\n]+?)?\ *(?=[\n]|$)";
        const string CultureId = "de";
        readonly static string[] WikiSectionsToSkip = 
            { 
                "Siehe auch", 
                "Einzelnachweise", 
                "Anmerkungen", 
                "Weblinks", 
                "Filminfo",
                "Dialoge"
            };
        readonly static string[] DisambiguationTemplateIds = 
            { 
                "{{MehrereArtikel}}", 
                "{{MehrereAutoren}}", 
                "{{Filmreihe}}" 
            };
        readonly static string[] NewLineSymbols = { "//" }; 
 
        public GermanQuoteCollectorRules()
            : base(CultureId, QuoteRegexPattern, WikiSectionsToSkip, NewLineSymbols, 
            DisambiguationTemplateIds, DisambiguationTemplateRegexPattern)
        {
        } 
    }
}
