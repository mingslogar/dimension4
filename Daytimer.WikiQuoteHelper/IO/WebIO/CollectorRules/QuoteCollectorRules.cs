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
// Filename: QuoteCollectorRules.cs
// Copyright: Christian Hanser 2008
//

using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace WikiquoteScreensaverLib.IO.WebIO.CollectorRules
{
    /// <summary>
    /// Rules class for customizing the media wiki api quote collector.
    /// It compiles and provides the regular expressions required by the quote collector.
    /// </summary>
    public abstract class QuoteCollectorRules
    {
        const string IntroductionSectionRegexPattern = @"^(.|[\n])*?(?===.+?==)|";
        const string WikiSectionsToSkipRegexPattern = @"([\n](?'headline'==|=)|(?<=[\n]==))[ ]*?{0}[ ]*?\k'headline'(.|[\s])+?(?=[^=]\k'headline'[^=]|$)";
        const string ReplaceByNewLineRegexPattern = "{0}";
        readonly static string[] HtmlNewLineSymbols = { @"</?br\ */?>", @"</?p\ */?>" };

        readonly CultureInfo _culture;
        readonly string _quoteRegexPattern;
        readonly string _disambiguationTemplateRegexPattern;
        readonly string[] _wikiSectionsToSkip;
        readonly string[] _disambiguationTemplateIdentifiers;
        readonly string[] _newlineSymbols;

        readonly object _quoteRegexLock = new object();
        readonly object _wikiSectionsToSkipRegexLock = new object();
        readonly object _newlineRegexLock = new object();
        readonly object _disambiguationTemplateRegexLock = new object();

        Regex _quoteRegex;
        Regex _wikiSectionsToSkipRegex;
        Regex _newlineRegex;
        Regex _disambiguationTemplateRegex;
 
        protected QuoteCollectorRules(string cultureId, string quoteRegexPattern, string[] wikiSectionsToSkip, string[] newlineSymbols,
            string[] disambiguationTemplateIdentifiers, string disambiguationTemplateRegexPattern)
        {
            _culture = new CultureInfo(cultureId);
            _quoteRegexPattern = quoteRegexPattern;
            _wikiSectionsToSkip = wikiSectionsToSkip;
            _newlineSymbols = newlineSymbols;
            _disambiguationTemplateIdentifiers = disambiguationTemplateIdentifiers;
            _disambiguationTemplateRegexPattern = disambiguationTemplateRegexPattern;
        } 
 
        internal Regex QuoteRegex
        {
            get 
            {
                lock (_quoteRegexLock)
                {
                    if (_quoteRegex == null)
                    {
                        _quoteRegex = new Regex(_quoteRegexPattern, RegexOptions.Multiline | RegexOptions.ExplicitCapture);
                    }

                    return _quoteRegex; 
                }
            }
        }

        internal Regex WikiSectionsToSkipRegex
        {
            get 
            {
                lock (_wikiSectionsToSkipRegexLock)
                {
                    if (_wikiSectionsToSkipRegex == null)
                    {
                        _wikiSectionsToSkipRegex = new Regex(IntroductionSectionRegexPattern + QuoteCollector.CompileRegexPattern(_wikiSectionsToSkip, WikiSectionsToSkipRegexPattern), 
                            RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
                    }

                    return _wikiSectionsToSkipRegex; 
                }
            }
        }

        internal Regex NewlineRegex
        {
            get 
            {
                lock (_newlineRegexLock)
                {
                    if (_newlineRegex == null)
                    {
                        int length = (_newlineSymbols == null) ? 0 : _newlineSymbols.Length;
                        string[] newlineSymbols = new string[HtmlNewLineSymbols.Length + length];
                        HtmlNewLineSymbols.CopyTo(newlineSymbols, 0);

                        if (length > 0)
                        {
                            _newlineSymbols.CopyTo(newlineSymbols, HtmlNewLineSymbols.Length);
                        }

                        _newlineRegex = QuoteCollector.CompileRegex(newlineSymbols, ReplaceByNewLineRegexPattern, RegexOptions.ExplicitCapture);
                    }

                    return _newlineRegex; 
                }
            }
        }

        internal string[] DisambiguationTemplateIdentifiers
        {
            get { return _disambiguationTemplateIdentifiers; }
        }

        internal Regex DisambiguationTemplateRegex
        {
            get 
            {
                lock (_disambiguationTemplateRegexLock)
                {
                    if (_disambiguationTemplateRegex == null)
                    {
                        _disambiguationTemplateRegex = (_disambiguationTemplateRegexPattern != null) ?
                            new Regex(_disambiguationTemplateRegexPattern, RegexOptions.ExplicitCapture) :
                            null;
                    }

                    return _disambiguationTemplateRegex; 
                }
            }
        }

        public CultureInfo Culture
        {
            get { return _culture; }
        }
    }
}
