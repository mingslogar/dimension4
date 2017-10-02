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
// Filename: FrenchQuoteCollectorRules.cs
// Copyright: Christian Hanser 2008
//

using System.Globalization;

namespace WikiquoteScreensaverLib.IO.WebIO.CollectorRules
{
    public sealed class FrenchQuoteCollectorRules 
        : QuoteCollectorRules
    {
        // excludes list items containing URLs, all remaining items are candidates for quotes
        const string QuoteRegexPattern = @"^{{[Cc]itation\|([Cc]itation[\s]*=[\s]*)?[\s]*(?'" + QuoteCollector.QuoteRegexGroup + @"'[^|}<>]{2,})[\s]*" +
                                         @"([\s]*\|[\s]*(original|précisions)[\s]*=[\s]*(?'" + QuoteCollector.AdditionalInformationRegexGroup + @"'[^|}<>]{2,})?[\s]*[^}]*?)*}}";
        const string CultureId = "fr";
        readonly static string[] NewLineSymbols = { @"<[/]?br\ *[/]?>", @"<[/]?p\ *[/]?>" };

        public FrenchQuoteCollectorRules()
            : base(CultureId, QuoteRegexPattern, null, NewLineSymbols, null, null)
        {
        }
    }
}
