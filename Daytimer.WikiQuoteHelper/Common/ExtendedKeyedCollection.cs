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
// Filename: ExtendedKeyedCollection.cs
// Copyright: Christian Hanser 2008
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WikiquoteScreensaverLib.Common
{
    /// <summary>
    /// Abstract KeyedCollection that additionally provides a TryGetValue() method.
    /// <typeparam name="TKey">The type of keys in the collection.</typeparam>
    /// <typeparam name="TValue">The type of items in the collection.</typeparam>
    /// </summary>
    public abstract class ExtendedKeyedCollection<TKey, TValue> 
        : KeyedCollection<TKey, TValue>
    {
        /// <summary>
        /// Initializes a new instance of the ExtendedKeyedCollection<TKey,TItem>
        /// class that uses the default equality comparer.
        /// </summary>
        /// <param name="dictionaryCreationThreshold">
        /// The number of elements the collection can hold without creating a lookup
        /// dictionary (0 creates the lookup dictionary when the first item is added),
        /// or –1 to specify that a lookup dictionary is never created.
        /// </param>
        protected ExtendedKeyedCollection(int dictionaryCreationThreshold)
            : base(EqualityComparer<TKey>.Default, dictionaryCreationThreshold)
        {
        } 

        /// <summary>
        /// Gets the value associated with the specified key. 
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">
        /// When this method returns, the value associated with the specified key, if the key is found; otherwise, the default 
        /// value for the type of the value parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// true if the ExtendedKeyedCollection<TKey, TValue> contains an element with the specified key; 
        /// otherwise, false.
        /// </returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            // checking preconditions
            if (key == null)
            {
                throw new ArgumentNullException();
            }

            bool returnValue = false;
            value = default(TValue);

            if (Dictionary == null)
            {
                TValue item;

                for (int i = 0; i < Items.Count; i++)
                {
                    item = Items[i];

                    if (Comparer.Equals(GetKeyForItem(item), key))
                    {
                        value = item;
                        returnValue = true;
                        break;
                    }
                }
            }
            else
            {
                returnValue = Dictionary.TryGetValue(key, out value);
            }

            return returnValue;
        } 
    }
}
