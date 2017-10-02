using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Threading;

namespace WikiquoteScreensaver.Common.MultithreadBinding
{
	/// <summary>
	/// Implements a list that wraps another list and dispatches all collection change notifications to the
	/// <see cref="Dispatcher"/>s thread.
	/// </summary>
	/// <typeparam name="TList">
	/// The type of the underlying list.
	/// </typeparam>
	/// <typeparam name="TItem">
	/// The type of items stored in the underlying list (and consequently by this collection).
	/// </typeparam>
	public class DispatchingList<TCollection, TItem> : DispatchingCollection<TCollection, TItem>, IList<TItem>, System.Collections.IList
		where TCollection : IList<TItem>, INotifyCollectionChanged
	{
		/// <summary>
		/// Gets or sets an item in this dispatching list by index.
		/// </summary>
		/// <param name="index">
		/// The index of the item.
		/// </param>
		/// <returns>
		/// The item at the specified index.
		/// </returns>
		public TItem this[int index]
		{
			get
			{
				return UnderlyingCollection[index];
			}
			set
			{
				SetItem(index, value);
			}
		}

		/// <summary>
		/// Constructs an instance of <c>DispatchingList</c>.
		/// </summary>
		/// <param name="underlyingCollection">
		/// The collection being wrapped by this dispatching list.
		/// </param>
		/// <param name="dispatcher">
		/// The <see cref="Dispatcher"/> to which <see cref="CollectionChanged"/> notifications will be marshalled.
		/// </param>
		public DispatchingList(TCollection underlyingCollection, Dispatcher dispatcher)
			: base(underlyingCollection, dispatcher)
		{
		}

		/// <summary>
		/// Gets the index of the specified item in this dispatching list.
		/// </summary>
		/// <param name="item">
		/// The item whose index is to be determined.
		/// </param>
		/// <returns>
		/// The index of the item, or <c>-1</c> if it could not be found.
		/// </returns>
		public int IndexOf(TItem item)
		{
			return UnderlyingCollection.IndexOf(item);
		}

		private delegate void InsertHandler(int index, TItem item);

		/// <summary>
		/// Inserts a specified item into this dispatching list.
		/// </summary>
		/// <param name="index">
		/// The index at which to insert the item.
		/// </param>
		/// <param name="item">
		/// The item to insert.
		/// </param>
		public void Insert(int index, TItem item)
		{
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.Invoke(DispatcherPriority.Send, new InsertHandler(Insert), index, item);
			}
			else
			{
				UnderlyingCollection.Insert(index, item);
			}
		}

		private delegate void RemoveAtHandler(int index);

		/// <summary>
		/// Removes an item from a specified index.
		/// </summary>
		/// <param name="index">
		/// The index of the item to remove.
		/// </param>
		public void RemoveAt(int index)
		{
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.Invoke(DispatcherPriority.Send, new RemoveAtHandler(RemoveAt), index);
			}
			else
			{
				UnderlyingCollection.RemoveAt(index);
			}
		}

		/// <summary>
		/// Gets an enumerator for the items in this dispatching list.
		/// </summary>
		/// <returns>
		/// An enumerator.
		/// </returns>
		public new System.Collections.IEnumerator GetEnumerator()
		{
			return UnderlyingCollection.GetEnumerator();
		}

		private delegate void SetItemHandler(int index, TItem item);

		private void SetItem(int index, TItem item)
		{
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.Invoke(DispatcherPriority.Send, new SetItemHandler(SetItem), index, item);
			}
			else
			{
				UnderlyingCollection[index] = item;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this collection is of a fixed size.
		/// </summary>
		/// <remarks>
		/// </remarks>
		bool System.Collections.IList.IsFixedSize
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this collection is read only.
		/// </summary>
		bool System.Collections.IList.IsReadOnly
		{
			get
			{
				return IsReadOnly;
			}
		}

		/// <summary>
		/// Gets or sets an item in this filtered collection.
		/// </summary>
		/// <param name="index">
		/// The index of the item.
		/// </param>
		/// <returns>
		/// The item at the specified index.
		/// </returns>
		object System.Collections.IList.this[int index]
		{
			get
			{
				return this[index];
			}
			set
			{
				ArgumentHelper.AssertNotNull(value, "value");
				ExceptionHelper.ThrowIf(!(value is TItem), "WrongType", value.GetType().FullName, typeof(TItem).FullName);
				this[index] = (TItem) value;
			}
		}

		/// <summary>
		/// Adds an item to this collection.
		/// </summary>
		/// <param name="value">
		/// The item to be added.
		/// </param>
		/// <returns>
		/// The index of the added item.
		/// </returns>
		int System.Collections.IList.Add(object value)
		{
			ArgumentHelper.AssertNotNull(value, "value");
			ExceptionHelper.ThrowIf(!(value is TItem), "WrongType", value.GetType().FullName, typeof(TItem).FullName);
			Add((TItem) value);
			return Count - 1;
		}

		/// <summary>
		/// Clears the items in this collection.
		/// </summary>
		void System.Collections.IList.Clear()
		{
			Clear();
		}

		/// <summary>
		/// Determines whether this collection contains the specified value.
		/// </summary>
		/// <param name="value">
		/// The value to check for.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if <paramref name="value"/> is contained in this collection, otherwise
		/// <see langword="false"/>.
		/// </returns>
		bool System.Collections.IList.Contains(object value)
		{
			ArgumentHelper.AssertNotNull(value, "value");
			ExceptionHelper.ThrowIf(!(value is TItem), "WrongType", value.GetType().FullName, typeof(TItem).FullName);
			return Contains((TItem) value);
		}

		/// <summary>
		/// Copies all items in this collection to the specified array.
		/// </summary>
		/// <param name="array">
		/// The array to copy to.
		/// </param>
		/// <param name="arrayIndex">
		/// The starting index for the copy operation.
		/// </param>
		void System.Collections.ICollection.CopyTo(Array array, int index)
		{
			ArgumentHelper.AssertNotNull(array, "array");
			ExceptionHelper.ThrowIf(index < 0, "CopyTo.ArrayIndexNegative");

			for (int i = 0; i < this.Count; ++i)
			{
				array.SetValue(this[i], i + index);
			}
		}

		/// <summary>
		/// Determines the index of <paramref name="value"/> within this collection.
		/// </summary>
		/// <param name="value">
		/// The item to search for.
		/// </param>
		/// <returns>
		/// The index of the item in this filtered collection, or <c>-1</c> if it could not be found.
		/// </returns>
		int System.Collections.IList.IndexOf(object value)
		{
			if (value != null)
			{
				ExceptionHelper.ThrowIf(!(value is TItem), "WrongType", value.GetType().FullName, typeof(TItem).FullName);
			}

			return IndexOf((TItem) value);
		}

		/// <summary>
		/// Inserts <paramref name="value"/> into this filtered collection at the specified index.
		/// </summary>
		/// <param name="index">
		/// The index at which the item should be inserted.
		/// </param>
		/// <param name="value">
		/// The item to be inserted.
		/// </param>
		void System.Collections.IList.Insert(int index, object value)
		{
			ArgumentHelper.AssertNotNull(value, "value");
			ExceptionHelper.ThrowIf(!(value is TItem), "WrongType", value.GetType().FullName, typeof(TItem).FullName);
			Insert(index, (TItem) value);
		}

		/// <summary>
		/// Removes the specified item from this filtered collection.
		/// </summary>
		/// <param name="value">
		/// The item to remove.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if the item was removed, otherwise <see langword="false"/>.
		/// </returns>
		void System.Collections.IList.Remove(object value)
		{
			ArgumentHelper.AssertNotNull(value, "value");
			ExceptionHelper.ThrowIf(!(value is TItem), "WrongType", value.GetType().FullName, typeof(TItem).FullName);
			Remove((TItem) value);
		}

		/// <summary>
		/// Removes the item at the specified index.
		/// </summary>
		/// <param name="index">
		/// The index of the item to remove.
		/// </param>
		void System.Collections.IList.RemoveAt(int index)
		{
			RemoveAt(index);
		}

		/// <summary>
		/// Gets a value indicating whether this collection is synchronized.
		/// </summary>
		bool System.Collections.ICollection.IsSynchronized
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Gets a value that can be used to synchronize access to this collection.
		/// </summary>
		object System.Collections.ICollection.SyncRoot
		{
			get
			{
				return this;
			}
		}
	}
}
