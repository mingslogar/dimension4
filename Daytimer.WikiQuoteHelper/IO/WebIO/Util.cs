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
// Filename: Util.cs
// Copyright: Christian Hanser 2008
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using WikiquoteScreensaverLib.Common;
using System.Net;
using System.Reflection;

namespace WikiquoteScreensaverLib.IO.WebIO
{
    internal static class Util
    {
        const string XhtmlNamespace = "{http://www.w3.org/1999/xhtml}";

        readonly static FetchXmlPageDelegate _fetchXmlDelegate = FetchXmlPage;
 
        delegate XDocument FetchXmlPageDelegate(Uri pageUri);

        internal static IAsyncResult BeginFetchXmlPage(Uri pageUri, AsyncCallback callback, object state)
        {
            return _fetchXmlDelegate.BeginInvoke(pageUri, callback, state);
        }

        internal static XDocument EndFetchXmlPage(IAsyncResult result)
        {
            return _fetchXmlDelegate.EndInvoke(result);
        }

        internal static XDocument FetchXmlPage(Uri pageUri)
        {
            HttpWebResponse response = null;
            XmlReader reader = null;
            XDocument returnValue;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(pageUri);
                request.UserAgent = Assembly.GetExecutingAssembly().GetName().ToString();
                response = (HttpWebResponse)request.GetResponse();

                reader = XmlReader.Create(response.GetResponseStream());
                returnValue = XDocument.Load(reader);
            }
            catch (WebException)
            {
                returnValue = null;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }

                if (reader != null)
                {
                    reader.Close();
                }
            }

            return returnValue;
        }

        internal static string Capitalize(this string stringToCapitalize, CultureInfo culture)
        {
            string returnValue;
            string[] parts = stringToCapitalize.ToLower(culture).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 1)
            {
                StringBuilder builder = new StringBuilder(stringToCapitalize.Length);

                for (int i = 0; i < parts.Length; i++)
                {
                    builder.Append(Char.ToUpper(parts[i][0], culture));
                    builder.Append(parts[i].Substring(1));
                    if (i < parts.Length - 1)
                    {
                        builder.Append(" ");
                    }
                }

                returnValue = builder.ToString();
            }
            else
            {
                returnValue = Char.ToUpper(parts[0][0], culture).ToString() + parts[0].Substring(1);
            }

            return returnValue;
        }

        internal static SelectableQuoteCollection ToSelectableQuoteCollection(this IEnumerable<SelectableQuote> items, CultureInfo culture)
        {
            SelectableQuoteCollection returnValue = new SelectableQuoteCollection{ Culture = culture };

            foreach (SelectableQuote item in items)
            {
                returnValue.Add(item);
            }

            return returnValue;
        }
    }
}
