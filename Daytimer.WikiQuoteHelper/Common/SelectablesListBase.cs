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
// Filename: SelectablesListBase.cs
// Copyright: Christian Hanser 2008
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading;

namespace WikiquoteScreensaverLib.Common
{
    /// <summary>
    /// An abstract base class for lists comprising selectables.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    /// <typeparam name="TInternalCollection"></typeparam>
    [DataContract]
    public abstract class SelectablesListBase<TKey, TItem, TInternalCollection> 
        : IList<TItem>, INotifyPropertyChanged, INotifyCollectionChanged  
        where TKey : class
        where TItem : class, ISelectable
        where TInternalCollection : KeyedCollection<TKey, TItem>, new()
    {
        protected readonly static ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();
        TInternalCollection _items; 

        protected SelectablesListBase()
        {
            Items = new TInternalCollection();
        }

        protected SelectablesListBase(TInternalCollection items)
        {
            Items = items;
        } 
 
        [DataMember]
        protected internal virtual TInternalCollection Items
        {
            get
            {
                _readerWriterLock.EnterReadLock();
                TInternalCollection items = _items;
                _readerWriterLock.ExitReadLock();

                return items;
            }
            set
            {
                _readerWriterLock.EnterWriteLock();
                int oldCount = (_items != null) ? _items.Count : 0;
                int newCount = value.Count;
                _items = value;
                _readerWriterLock.ExitWriteLock();

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

                if (oldCount != newCount)
                {
                    OnPropertyChanged("Count");
                }
            }
        } 

        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler handler = CollectionChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        } 
 
        #region ICollection<TValue> Members

        public virtual void Add(TItem item)
        {
            Items.Add(item);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, IndexOf(item)));
            OnPropertyChanged("Count");
        }

        public virtual void Clear()
        {
            Items.Clear();

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            OnPropertyChanged("Count");
        }

        public bool Contains(TKey key)
        {
            return Items.Contains(key);
        }

        public bool Contains(TItem item)
        {
            return Items.Contains(item);
        }

        public void CopyTo(TItem[] array, int arrayIndex)
        {
            Items.CopyTo(array, arrayIndex);
        }

        public int Count 
        {
            get { return Items.Count; } 
        }

        public virtual bool IsReadOnly 
        { 
            get { return false; } 
        }

        public virtual bool Remove(TItem item)
        {
            bool returnValue = Items.Remove(item);

            if (returnValue)
            {
                OnPropertyChanged("Count");
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
            }

            return returnValue;
        }

        #endregion

        #region IEnumerable<TValue> Members

        public IEnumerator<TItem> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region IList<TValue> Members

        public int IndexOf(TItem item)
        {
            return Items.IndexOf(item);
        }

        public virtual void Insert(int index, TItem item)
        {
            Items.Insert(index, item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            OnPropertyChanged("Count");
        }

        public virtual void RemoveAt(int index)
        {
            TItem item = Items[index];
            Items.RemoveAt(index);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            OnPropertyChanged("Count");
        }

        public TItem this[TKey key] 
        { 
            get { return Items[key]; } 
        }

        public virtual TItem this[int index]
        {
            get
            {
                return Items[index];
            }
            set
            {
                Items[index] = value;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, Items[index], index));
            }
        }

        #endregion
    }
}
