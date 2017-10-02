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
// Filename: SelectableQuote.cs
// Copyright: Christian Hanser 2008
//

using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace WikiquoteScreensaverLib.Common
{
    /// <summary>
    /// A class representing selectable quotes.
    /// </summary>
    [DataContract]
    public sealed class SelectableQuote 
        : Quote, ISelectable
    {
        [DataMember(Name = "Selected")]
        bool _selected = true; 

        /// <summary>
        /// Initializes a new SelectableQuote instance.
        /// </summary>
        /// <param name="quote">The quote's text.</param>
        /// <param name="additionalInformation">The quote's additional information.</param>
        internal SelectableQuote(string quote, string additionalInformation)
            : base(quote, additionalInformation)
        {
        } 

        /// <summary>
        /// Method used for firing a INotifyPropertyChanged.PropertyChanged event.
        /// </summary>
        /// <param name="e">The name of the modified property.</param>
        private void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        } 

        /// <summary>
        /// Returns the quote's text.
        /// </summary>
        /// <returns>The quote's text.</returns>
        public override string ToString()
        {
            return Text;
        } 

        #region ISelectable Members
        /// <summary>
        /// Gets the quote's selection state.
        /// </summary>
        public bool Selected
        {
            get
            {
                return _selected;
            }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    OnPropertyChanged("Selected");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
