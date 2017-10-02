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
// Filename: EnglishQuoteCollectorRules.cs
// Copyright: Christian Hanser 2008
//

using System.Globalization;

namespace WikiquoteScreensaverLib.IO.WebIO.CollectorRules
{
	/// <summary>
	/// Rules for the mediawiki api quote collector that allows parsing of wikiquote projects (e.g. the
	/// English wikiquote project) using one of the following quote formats:
	/// 
	/// * (quote) ~ (additonal information)
	/// 
	/// or
	/// 
	/// * (quote)
	/// ** (additional information 1)
	/// ** (additional information 2)
	/// ...
	/// </summary>
	public sealed class EnglishQuoteCollectorRules
		: QuoteCollectorRules
	{
		// excludes list items containing URLs, all remaining items are candidates for quotes
		const string QuoteRegexPattern = @"^\*\ *((?'" + QuoteCollector.QuoteRegexGroup + @"'[^*{#]{2,}?)([\n]|(?=~)))+?" +
										 @"((^\*{2}?|~)\ *(?'" + QuoteCollector.AdditionalInformationRegexGroup + @"'.+?)\ *[\n])?";

		const string DisambiguationTemplateRegexPattern = @"[\n]\*{1,2}[^*]?\ *(?<formattingGroup>('{2,3}|'{5})?)\[\[\ *(?'" + QuoteCollector.TopicIdRegexGroup + @"'[^[|]+?)\ *" +
														  @"(\|(?'" + QuoteCollector.TopicNameRegexGroup + @"'[^[]+?)\ *)*?\]\](\k<formattingGroup>)" +
														  @"\ *[,]?\ *(?'" + QuoteCollector.TopicDescriptionRegexGroup + @"'[^*\n]+?)?\ *(?=[\n]|$)";
		const string CultureId = "en";
		readonly static string[] WikiSectionsToSkip = 
            { 
                "See also", 
                "External links", 
                "Cast", 
                "Major cast",
                "Wikipedia Articles",
                "Dialogue"
            };
		readonly static string[] DisambiguationTemplateIds = { "{{disambig}}", "{{dab}}" };

		public EnglishQuoteCollectorRules()
			: base(CultureId, QuoteRegexPattern, WikiSectionsToSkip, null, DisambiguationTemplateIds,
			DisambiguationTemplateRegexPattern)
		{
		}
	}
}
