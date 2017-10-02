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
// Filename: MediaWikiApiSymbols.cs
// Copyright: Christian Hanser 2008
//

using System.Xml.Linq;

namespace WikiquoteScreensaverLib.IO.WebIO
{
    internal static class MediaWikiApiSymbols
    {
        internal readonly static XName ApiName = XName.Get("api");
        internal readonly static XName QueryName = XName.Get("query");
        internal readonly static XName PagesName = XName.Get("pages");
        internal readonly static XName PageName = XName.Get("page");
        internal readonly static XName TitleName = XName.Get("title");
        internal readonly static XName MissingName = XName.Get("missing");
        internal readonly static XName RevisionsName = XName.Get("revisions");
        internal readonly static XName RevName = XName.Get("rev");
    }
}
