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
// Filename: Quote.cs
// Copyright: Christian Hanser 2008
//

using System;
using System.Runtime.Serialization;

namespace WikiquoteScreensaverLib.Common
{
    /// <summary>
    /// Abstract class that comprises a quote's text and if available the additional information
    /// that goes along with it.
    /// </summary>
    [Serializable]
    [DataContract]
    public abstract class Quote
    {
        [DataMember(Name = "Text")]
        readonly string _quote;
        string _additionalInformation; 
 
        /// <summary>
        /// Initializes a new Quote instance.
        /// </summary>
        /// <param name="quote">The quote's text.</param>
        /// <param name="additionalInformation">The quote's additional information.</param>
        internal Quote(string quote, string additionalInformation)
        {
            _quote = quote;
            _additionalInformation = additionalInformation;
        } 
 
        /// <summary>
        /// Gets or sets the quote's additional information.
        /// </summary>
        /// <remarks>
        /// Note that this property is not auto implemented by intention.
        /// Otherwise the BinaryFormatter wouldn't recognize it.
        /// </remarks>
        [DataMember(Name = "AdditionalInformation")]
        public string AdditionalInformation
        {
            get { return _additionalInformation; }
            set { _additionalInformation = value; }
        }

        /// <summary>
        /// Gets the quote's text.
        /// </summary>
        public string Text
        {
            get { return _quote; }
        } 
    }
}
