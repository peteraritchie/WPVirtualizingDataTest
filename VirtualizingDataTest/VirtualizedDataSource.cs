using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace VirtualizingDataTest
{
	public class VirtualizedDataSource<T> : INotifyCollectionChanged, INotifyPropertyChanged, IList<T>
	{
		private readonly Dictionary<int, T> dictionary = new Dictionary<int, T>();
		private readonly int pageSize;
		private readonly Func<int, int, Task<IEnumerable<T>>> getPageOfItems;
		private readonly Func<int, T> getPlaceholder;
		private readonly List<Tuple<int, int>> rangesInRoute = new List<Tuple<int, int>>();
		
		public VirtualizedDataSource(int count, int pageSize, Func<int, int, Task<IEnumerable<T>>> getPageOfItems, Func<int, T> getPlaceholder)
		{
			Count = count;
			this.pageSize = pageSize;
			this.getPageOfItems = getPageOfItems;
			this.getPlaceholder = getPlaceholder;
		}

		public int Count { get; private set; }

		/// <summary>
		/// Returns the index of an item; used by the control when notified of a item change
		/// </summary>
		/// <param name="value">The item to find the index of</param>
		/// <returns>The index, or -1 if it doesn't exist</returns>
		public int IndexOf(T value)
		{
			KeyValuePair<int, T> item = dictionary.FirstOrDefault(e => EqualityComparer<T>.Default.Equals(e.Value, value));
			if (EqualityComparer<KeyValuePair<int, T>>.Default.Equals(default(KeyValuePair<int, T>), item))
			{
				return -1;
			}

			return item.Key;
		}

		/// <summary>
		/// Indexer for the list.  Silverlight will virtualize the UI and only ask for a subset
		/// of the items via this indexor.
		/// </summary>
		/// <param name="index">The index of the item to get</param>
		/// <returns>The item</returns>
		public T this[int index]
		{
			get
			{
				if (dictionary.ContainsKey(index)) return dictionary[index];
				if (index < 0) return default(T); // sometimes happens. #WAT
				dictionary[index] = getPlaceholder(index);

				Debug.WriteLine("don't have index " + index);
				var firstItemInPage = index - (index%pageSize);
				var lastItemInPage = firstItemInPage + pageSize;
				bool rangeInRoute;
				lock (rangesInRoute)
				{
					rangeInRoute = rangesInRoute.Any(e => index >= e.Item1 && index < e.Item2);
					if (!rangeInRoute)
					{
						rangesInRoute.Add(new Tuple<int, int>(firstItemInPage, lastItemInPage));
					}
					else Debug.WriteLine("still waiting for " + index);
				}

				if (!rangeInRoute)
				{
					Debug.WriteLine("getting items {0} to {1}", firstItemInPage, lastItemInPage - 1);

					getPageOfItems(firstItemInPage, pageSize).ContinueWith(t =>
					{
						var e = t.Result.GetEnumerator();
						for (var i = firstItemInPage; i < lastItemInPage; ++i)
						{
							e.MoveNext();
							int itemIndex = i;
							var newItem = e.Current;
							var containsKey = dictionary.ContainsKey(itemIndex);
							object oldItem = default(T);
							if (!containsKey)
							{
								dictionary[itemIndex] = newItem;
							}
							else
							{
								oldItem = dictionary[i];
							}

							dictionary[i] = newItem;
							Debug.WriteLine("notifying");
							// broken:
							//OnCollectionChanged(
							//    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
							//                                         newItem, oldItem, itemIndex));

							// works but sends list to top
							//OnCollectionChanged(
							//    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

							OnCollectionChanged(
								new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
								                                     oldItem, itemIndex));
							OnCollectionChanged(
								new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
								                                     newItem, itemIndex));
						}
					}, TaskScheduler.FromCurrentSynchronizationContext());
				}
				return dictionary[index];
			}
			set { throw new NotImplementedException(); }
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			var handler = CollectionChanged;
			if (handler != null) handler(this, e);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		#region Unimplemented IList stuff
		// None of the other IList stuff is necessary for data virtualization

		public void Add(T value)
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains(T value)
		{
			throw new NotImplementedException();
		}

		public void Insert(int index, T value)
		{
			throw new NotImplementedException();
		}

		public bool IsFixedSize
		{
			get { throw new NotImplementedException(); }
		}

		public bool IsReadOnly
		{
			get { throw new NotImplementedException(); }
		}

		public bool Remove(T value)
		{
			throw new NotImplementedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotImplementedException();
		}

		public void CopyTo(T[] array, int index)
		{
			throw new NotImplementedException();
		}

		public bool IsSynchronized
		{
			get { throw new NotImplementedException(); }
		}

		public object SyncRoot
		{
			get { throw new NotImplementedException(); }
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		public IEnumerator GetEnumerator()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
