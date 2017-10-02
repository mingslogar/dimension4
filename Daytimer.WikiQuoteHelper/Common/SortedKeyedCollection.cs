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
// Filename: SortedKeyedCollection.cs
// Copyright: Christian Hanser 2008
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WikiquoteScreensaverLib.Common
{
    /// <summary>
    /// An abstract extended keyed collection class that inserts new items using binary insertion
    /// sort and uses binary sort to look up items.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the collection.</typeparam>
    /// <typeparam name="TValue">The type of items in the collection.</typeparam>
    public abstract class SortedKeyedCollection<TKey, TValue> 
        : KeyedCollection<TKey, TValue>
        where TKey : class
    {
        // do not create dictionary
        const int DefaultDictionaryCreationThreshold = -1;

        TKey _lastItemKey;
        readonly bool _uniqueEntries;  

        /// <summary>
        /// Initializes a new instance of the SortedKeyedCollection<TKey,TItem>
        /// class that allows multiple entries with the same key and uses the 
        /// default equality comparer.
        /// </summary>
        protected SortedKeyedCollection()
            : this(false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the SortedKeyedCollection<TKey,TItem>
        /// class that uses the default equality comparer.
        /// </summary>
        /// <param name="uniqueEntries">
        /// Determines whether the collection accepts multiple items with the 
        /// same key.
        /// </param>
        protected SortedKeyedCollection(bool uniqueEntries)
            : base(EqualityComparer<TKey>.Default, DefaultDictionaryCreationThreshold)
        {
            _uniqueEntries = uniqueEntries;
        } 

        /// <summary>
        /// Gets the default comparer for the key type <typeparamref name="TKey"/>.
        /// </summary>
        protected virtual IComparer<TKey> KeyComparer
        {
            get { return Comparer<TKey>.Default; }
        } 

        /// <summary>
        /// Inserts an element into the SortedKeyedCollection<TKey, TItem> at the specified index. 
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        protected override void InsertItem(int index, TValue item)
        {
            bool itemFound = false;

            // caller tries to insert item at the end of the collection
            // i.e. if index == 0, there are no elements available
            if (index > 0)
            {
                TKey itemKey = GetKeyForItem(item);
                IComparer<TKey> keyComparer = KeyComparer;
                int result = keyComparer.Compare(itemKey, _lastItemKey ?? GetKeyForItem(this[index - 1]));

                // deserialization speedup:
                // only search new position if last inserted item greater than current item
                // this does not occur, when the collection is being deserialized
                if (result < 0)
                {
                    _lastItemKey = null;
                    index = BinarilySearchInsertionPosition(itemKey, keyComparer, out itemFound);
                }
                else
                {
                    _lastItemKey = itemKey;
                    itemFound = (result == 0);
                }
            }

            if ((_uniqueEntries && !itemFound) || !_uniqueEntries)
            {
                // don't use base.InsertItem, cause it calls GetKeyForItem extremely often
                // using Items.Insert instead
                Items.Insert(index, item);
            }
        } 
 
        /// <summary>
        /// Performs a binary search for the specified item key and returns either a
        /// insertion position or the index of the found value.
        /// </summary>
        /// <param name="itemKey">Key to look up.</param>
        /// <param name="itemFound">
        /// When this method returns, true if the key is found; otherwise, false. 
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// Either a possible insertion position or if the key is found the index 
        /// of the according value.
        /// </returns>
        private int BinarilySearchInsertionPosition(TKey itemKey, IComparer<TKey> keyComparer, out bool itemFound)
        {
            int left = 0;
            int right = Count - 1;
            int middle = 0;
            int compareResult = 0;

            itemFound = false;

            while (left <= right)
            {
                // division by 2 and floor the fast way
                middle = (left + right) >> 1;
                compareResult = keyComparer.Compare(itemKey, GetKeyForItem(Items[middle]));

                if (compareResult < 0)
                {
                    right = middle - 1;
                }
                else if (compareResult > 0)
                {
                    left = middle + 1;
                }
                else
                {
                    itemFound = true;
                    left = middle;
                    break;
                }
            }

            return left;
        } 
 
        /// <summary>
        /// Gets the value associated with the specified key. 
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">
        /// When this method returns, the value associated with the specified key, if the key is found; 
        /// otherwise, the default value for the type of the value parameter. This parameter is passed 
        /// uninitialized.
        /// </param>
        /// <returns>
        /// true if the SortedKeyedCollection<TKey, TValue> contains an element with the specified key; 
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
                bool itemFound;
                int index = BinarilySearchInsertionPosition(key, KeyComparer, out itemFound);

                if (itemFound)
                {
                    value = Items[index];
                    returnValue = true;
                }
            }
            else
            {
                returnValue = Dictionary.TryGetValue(key, out value);
            }

            return returnValue;
        }

        public new bool Contains(TKey key)
        {
            // checking preconditions
            if (key == null)
            {
                throw new ArgumentNullException();
            }

            bool returnValue;

            if (Dictionary == null)
            {
                BinarilySearchInsertionPosition(key, KeyComparer, out returnValue);
            }
            else
            {
                returnValue = Dictionary.ContainsKey(key);
            }

            return returnValue;
        }

        public new bool Contains(TValue item)
        {
            // checking preconditions
            if (item == null)
            {
                throw new ArgumentNullException();
            }

            return Contains(GetKeyForItem(item));
        }

        public new TValue this[TKey key]
        {
            get
            {
                // checking preconditions
                if (key == null)
                {
                    throw new ArgumentNullException();
                }

                TValue returnValue;

                if (!TryGetValue(key, out returnValue))
                {
                    throw new KeyNotFoundException();
                }

                return returnValue;
            }
        } 
    }
}
