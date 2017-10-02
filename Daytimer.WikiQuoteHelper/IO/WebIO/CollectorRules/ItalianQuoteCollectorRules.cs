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
// Filename: ItalianQuoteCollectorRules.cs
// Copyright: Christian Hanser 2008
//

using System.Globalization;

namespace WikiquoteScreensaverLib.IO.WebIO.CollectorRules
{
    public sealed class ItalianQuoteCollectorRules 
        : QuoteCollectorRules
    {
        // excludes list items containing URLs, all remaining items are candidates for quotes
        const string QuoteRegexPattern = "^\\*\\ *(?'" + QuoteCollector.QuoteRegexGroup + "'[^*{(]{2,}?)\\ *" +
                                         "(\\((?'" + QuoteCollector.AdditionalInformationRegexGroup + "'.+?)\\))?\\ *$";
        const string DisambiguationTemplateRegexPattern = @"[\n]\*{1,2}[^*]?\ *(?<formattingGroup>('{2,3}|'{5})?)\[\[\ *(?'" + QuoteCollector.TopicIdRegexGroup + @"'[^[|]+?)\ *" +
                                                          @"(\|(?'" + QuoteCollector.TopicNameRegexGroup + @"'[^[]+?)\ *)*?\]\](\k<formattingGroup>)" +
                                                          @"\ *[,]?\ *(?'" + QuoteCollector.TopicDescriptionRegexGroup + @"'[^*\n]+?)?\ *(?=[\n]|$)";
        const string CultureId = "it";
        readonly static string[] WikiSectionsToSkip = 
            { 
                "Note", 
                "Bibliografia", 
                "Altri progetti", 
                "Collegamenti esterni", 
                "Saggezza popolare", 
                "Voci correlate" 
            };
        readonly static string[] DisambiguationTemplateIds = { "{{disambigua}}" };
 
        public ItalianQuoteCollectorRules()
            : base(CultureId, QuoteRegexPattern, WikiSectionsToSkip, null, DisambiguationTemplateIds, DisambiguationTemplateRegexPattern)
        {
        }
    }
}