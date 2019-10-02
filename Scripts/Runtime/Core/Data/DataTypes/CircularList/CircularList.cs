using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Object = System.Object;

namespace Extenity.DataToolbox
{

	/// <summary>
	/// Auto expandable circular list.
	/// </summary>
	public class CircularList<T> : ICollection//, ICollection<T>
	{
		#region Initialization

		public CircularList()
		{
			CyclicTailIndex = -1;
			CyclicHeadIndex = -1;
			Items = EmptyArray;
		}

		public CircularList(int capacity)
		{
			CyclicTailIndex = -1;
			CyclicHeadIndex = -1;

			if (capacity < 0)
				throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity is not allowed to be less than zero.");

			Items = new T[capacity];
		}

		public CircularList(IEnumerable<T> collection)
		{
			if (collection == null)
			{
				CyclicTailIndex = -1;
				CyclicHeadIndex = -1;
				throw new ArgumentNullException(nameof(collection));
			}

			var castCollection = collection as ICollection<T>;
			if (castCollection != null)
			{
				var count = castCollection.Count;
				Items = new T[count];
				castCollection.CopyTo(Items, 0);
				CyclicTailIndex = count > 0 ? 0 : -1;
				CyclicHeadIndex = count > 0 ? count - 1 : -1;
			}
			else
			{
				Items = new T[DefaultCapacity];
				CyclicTailIndex = -1;
				CyclicHeadIndex = -1;

				using (var enumerator = collection.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Add(enumerator.Current);
					}
				}
			}
		}

		#endregion

		#region Data

		private T[] Items;
		public int CyclicTailIndex { get; private set; }
		public int CyclicHeadIndex { get; private set; }

		public int Version { get; private set; }
		[NonSerialized]
		private Object SyncRootObject;

		public const int DefaultCapacity = 4;
		private static readonly T[] EmptyArray = new T[0];

		public bool IsEmpty { get { return CyclicTailIndex < 0; } }
		public bool IsEmptyOrArranged { get { return CyclicTailIndex <= 0; } }
		public bool IsArranged { get { return CyclicTailIndex == 0; } }
		public bool IsFlipped { get { return CyclicHeadIndex < CyclicTailIndex; } }

		public int Count
		{
			get
			{
				if (CyclicTailIndex < 0)
					return 0;
				if (CyclicHeadIndex < CyclicTailIndex)
				{
					return (Items.Length - CyclicTailIndex) + CyclicHeadIndex + 1;
				}
				else
				{
					return CyclicHeadIndex - CyclicTailIndex + 1;
				}
			}
		}

		//int ICollection<T>.Count
		//{
		//	get
		//	{
		//		if (CyclicTailIndex < 0)
		//			return 0;
		//		return CyclicHeadIndex - CyclicTailIndex;
		//	}
		//}

		public bool IsCapacityFilled
		{
			get
			{
				//if (Items.Length == 0) No need to check
				//	return true;
				return Count == Items.Length;
			}
		}

		public int Capacity
		{
			get { return Items.Length; }
			set
			{
				if (value < Count)
				{
					throw new ArgumentOutOfRangeException(nameof(value), value, "Capacity cannot be less than count of items.");
				}

				if (value == Items.Length)
					return; // Nothing to do.

				if (value > 0)
				{
					var newItems = new T[value];
					var count = Count;
					if (count > 0)
					{
						// Copy old items to new collection. Rearrange items while doing that.
						if (CyclicTailIndex == 0)
						{
							Array.Copy(Items, 0, newItems, 0, count);
						}
						else
						{
							Array.Copy(Items, CyclicTailIndex, newItems, 0, count - CyclicTailIndex);
							Array.Copy(Items, 0, newItems, count - CyclicTailIndex, CyclicTailIndex);
							CyclicTailIndex = 0;
							CyclicHeadIndex = count - 1;
						}
					}
					Items = newItems;
				}
				else
				{
					Items = EmptyArray;
					//CyclicTailIndex = -1;
					//CyclicHeadIndex = -1;
				}
			}
		}

		/// <summary>
		/// Ensures that the capacity of this list is at least the given minimum
		/// value. If the currect capacity of the list is less than min, the 
		/// capacity is increased to twice the current capacity or to min,
		/// whichever is larger.
		/// 
		/// Items will be rearranged in case the capacity of the list increases.
		/// </summary>
		public void EnsureCapacity(int min)
		{
			if (Capacity < min)
			{
				int newCapacity = Capacity == 0 ? DefaultCapacity : Capacity * 2;
				if (newCapacity < min)
					newCapacity = min;
				Capacity = newCapacity;
			}
		}

		/// <summary>
		/// Sets the capacity of this list to the size of the list. This method can 
		/// be used to minimize a list's memory overhead once it is known that no
		/// new elements will be added to the list. 
		/// 
		/// Note that a rearrange will occur when changing the capacity of the list.
		/// 
		/// To completely clear a list and 
		/// release all memory referenced by the list, execute the following
		/// statements:
		///
		/// list.Clear(); 
		/// list.TrimExcess();
		/// </summary>
		public void TrimExcess()
		{
			Capacity = Count;
		}

		///// <summary>
		///// Shift items in list so that the cyclic start index placed at the start of the internal list. This operation is seamless to user.
		///// </summary>
		//private void Rearrange()
		//{
		//	Version++; // Update the version whether or not any changes made to the list. This will make sure behavior of the list is deterministic in user's perspective.

		//	if (IsEmptyOrArranged)
		//		return; // Nothing to do here.

		//	throw new NotImplementedException();
		//}

		#endregion

		#region Heading and Tailing

		public T HeadingItem
		{
			get
			{
				if (CyclicHeadIndex < 0)
					throw new ArgumentOutOfRangeException("HeadingItem", "Tried to get heading item while the collection is empty.");
				return Items[CyclicHeadIndex];
			}
		}

		public T TailingItem
		{
			get
			{
				if (CyclicTailIndex < 0)
					throw new ArgumentOutOfRangeException("TailingItem", "Tried to get tailing item while the collection is empty.");
				return Items[CyclicTailIndex];
			}
		}

		#endregion

		#region Add Data

		/// <summary>
		/// Adds the given object to the end of this list. The size of the list is
		/// increased by one. If required, the capacity of the list is doubled
		/// before adding the new element.
		/// 
		/// Items will be rearranged in case the capacity of the list increases.
		/// 
		/// Does not check if items are decayed.
		/// </summary>
		public void Add(T item)
		{
			Version++;

			if (IsCapacityFilled)
			{
				// Note that a rearrange will occur when changing capacity.
				EnsureCapacity(Capacity + 1);
			}

			// Initialize if this is the first item
			if (IsEmpty)
			{
				Items[0] = item;
				CyclicTailIndex = 0;
				CyclicHeadIndex = 0;
			}
			else
			{
				if (CyclicHeadIndex == Items.Length - 1)
					CyclicHeadIndex = 0;
				else
					CyclicHeadIndex++;
				Items[CyclicHeadIndex] = item;
			}
		}

		#endregion

		#region Remove Data

		//public bool Remove(T item)
		//{
		//	Version++;

		//	var index = IndexOf(item);
		//	if (index >= 0)
		//	{
		//		RemoveAt(index);
		//		return true;
		//	}
		//	return false;
		//}

		//public void RemoveAt(int index)
		//{
		//	Version++;

		//	throw new NotImplementedException();

		//	_size--;
		//	if (index < _size)
		//	{
		//		Array.Copy(_items, index + 1, _items, index, _size - index);
		//	}
		//	_items[_size] = default(T);
		//	_version++;

		//	if (IsEmpty)
		//	{
		//		CyclicTailIndex = -1;
		//		CyclicHeadIndex = -1;
		//	}
		//	else
		//	{
		//		throw new NotImplementedException();
		//	}
		//}

		public void RemoveHeading()
		{
			if (IsEmpty)
				throw new Exception("Tried to remove heading item while the collection is empty.");

			Version++;
			Items[CyclicHeadIndex] = default(T);

			if (CyclicTailIndex == CyclicHeadIndex) // Check if there are any items left
			{
				// Collection is empty.
				CyclicTailIndex = -1;
				CyclicHeadIndex = -1;
			}
			else if (CyclicHeadIndex == 0) // Check if head index reached to beginning of buffer.
			{
				CyclicHeadIndex = Items.Length - 1;
			}
			else
			{
				CyclicHeadIndex--;
			}
		}

		public void RemoveTailing()
		{
			if (IsEmpty)
				throw new Exception("Tried to remove tailing item while the collection is empty.");

			Version++;
			Items[CyclicTailIndex] = default(T);

			if (CyclicTailIndex == CyclicHeadIndex) // Check if there are any items left
			{
				// Collection is empty.
				CyclicTailIndex = -1;
				CyclicHeadIndex = -1;
			}
			else if (CyclicTailIndex == Items.Length - 1) // Check if tail index reached to end of buffer.
			{
				CyclicTailIndex = 0;
			}
			else
			{
				CyclicTailIndex++;
			}
		}

		public T PopTailing()
		{
			if (CyclicTailIndex < 0)
				throw new Exception("Tried to remove tailing item while the collection is empty.");

			var item = Items[CyclicTailIndex];
			RemoveTailing();
			return item;
		}

		public void Clear()
		{
			Version++;
			if (!IsEmpty)
			{
				Array.Clear(Items, 0, Items.Length); // Clear the elements so that the gc can reclaim the references.
			}
			CyclicTailIndex = -1;
			CyclicHeadIndex = -1;
		}

		//void ICollection<T>.Clear()
		//{
		//	Clear();
		//}


		#endregion

		#region IndexOf / Contains / Find

		public int BufferIndexOf(T item)
		{
			return Array.IndexOf(Items, item, 0, Items.Length);
		}

		public bool Contains(T item)
		{
			if (item == null)
				return ContainsNull();

			var comparer = EqualityComparer<T>.Default;

			if (IsFlipped)
			{
				var capacity = Capacity;
				for (int i = CyclicTailIndex; i < capacity; i++)
				{
					if (comparer.Equals(Items[i], item))
						return true;
				}
				for (int i = 0; i <= CyclicHeadIndex; i++)
				{
					if (comparer.Equals(Items[i], item))
						return true;
				}
			}
			else
			{
				for (int i = CyclicTailIndex; i <= CyclicHeadIndex; i++)
				{
					if (comparer.Equals(Items[i], item))
						return true;
				}
			}

			return false;
		}

		public bool ContainsNull()
		{
			if (IsEmpty)
				return false;

			if (IsFlipped)
			{
				var capacity = Capacity;
				for (int i = CyclicTailIndex; i < capacity; i++)
				{
					if (Items[i] == null)
						return true;
				}
				for (int i = 0; i <= CyclicHeadIndex; i++)
				{
					if (Items[i] == null)
						return true;
				}
			}
			else
			{
				for (int i = CyclicTailIndex; i <= CyclicHeadIndex; i++)
				{
					if (Items[i] == null)
						return true;
				}
			}

			return false;
		}

		public bool Exists(Predicate<T> match)
		{
			if (match == null)
				throw new ArgumentNullException(nameof(match));
			if (IsEmpty)
				return false;

			if (IsFlipped)
			{
				var capacity = Capacity;
				for (int i = CyclicTailIndex; i < capacity; i++)
				{
					if (match(Items[i]))
						return true;
				}
				for (int i = 0; i <= CyclicHeadIndex; i++)
				{
					if (match(Items[i]))
						return true;
				}
			}
			else
			{
				for (int i = CyclicTailIndex; i <= CyclicHeadIndex; i++)
				{
					if (match(Items[i]))
						return true;
				}
			}

			return false;
		}

		public T Find(Predicate<T> match)
		{
			if (match == null)
				throw new ArgumentNullException(nameof(match));
			if (IsEmpty)
				return default(T);

			if (IsFlipped)
			{
				var capacity = Capacity;
				for (int i = CyclicTailIndex; i < capacity; i++)
				{
					if (match(Items[i]))
						return Items[i];
				}
				for (int i = 0; i <= CyclicHeadIndex; i++)
				{
					if (match(Items[i]))
						return Items[i];
				}
			}
			else
			{
				for (int i = CyclicTailIndex; i <= CyclicHeadIndex; i++)
				{
					if (match(Items[i]))
						return Items[i];
				}
			}

			return default(T);
		}

		public List<T> FindAll(Predicate<T> match)
		{
			if (match == null)
				throw new ArgumentNullException(nameof(match));
			var output = new List<T>();
			if (IsEmpty)
				return output;

			if (IsFlipped)
			{
				var capacity = Capacity;
				for (int i = CyclicTailIndex; i < capacity; i++)
				{
					if (match(Items[i]))
						output.Add(Items[i]);
				}
				for (int i = 0; i <= CyclicHeadIndex; i++)
				{
					if (match(Items[i]))
						output.Add(Items[i]);
				}
			}
			else
			{
				for (int i = CyclicTailIndex; i <= CyclicHeadIndex; i++)
				{
					if (match(Items[i]))
						output.Add(Items[i]);
				}
			}

			return output;
		}

		//public T FindLast(Predicate<T> match)
		//{
		//	if (match == null)
		//		throw new ArgumentNullException("match");
		//	if (IsEmpty)
		//		return default(T);

		//	throw new NotImplementedException();

		//	return default(T);
		//}

		#endregion

		#region Iteration

		public void ForEach(Action<T> action)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			if (CyclicTailIndex < 0)
				return;

			for (int i = CyclicTailIndex; i < Count; i++)
				action(Items[i]);
			for (int i = 0; i < CyclicTailIndex; i++)
				action(Items[i]);
		}

		public bool TrueForAll(Predicate<T> match)
		{
			if (match == null)
				throw new ArgumentNullException(nameof(match));

			for (int i = 0; i < Count; i++)
			{
				if (!match(Items[i]))
					return false;
			}
			return true;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(this);
		}

		[Serializable]
		public struct Enumerator : IEnumerator<T>
		{
			private CircularList<T> List;
			private int Index;
			private int Remaining;
			private int Capacity;
			private int Version;
			private T _Current;

			internal Enumerator(CircularList<T> list)
			{
				List = list;
				Index = list.CyclicTailIndex;
				Remaining = list.Count;
				Capacity = list.Capacity;
				Version = list.Version;
				_Current = default(T);
			}

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				var localCollection = List;

				if (Remaining > 0 && Version == localCollection.Version)
				{
					if (Index == Capacity)
						Index = 0;
					_Current = localCollection.Items[Index];
					Index++;
					Remaining--;
					return true;
				}
				return MoveNextRare();
			}

			private bool MoveNextRare()
			{
				if (Version != List.Version)
				{
					throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
				}

				Index = -1;
				_Current = default(T);
				return false;
			}

			public T Current
			{
				get
				{
					return _Current;
				}
			}

			Object System.Collections.IEnumerator.Current
			{
				get
				{
					if (Index == List.CyclicTailIndex || Index == -1)
					{
						throw new InvalidOperationException("Enumeration has either not started or has already finished.");
					}
					return Current;
				}
			}

			void System.Collections.IEnumerator.Reset()
			{
				if (Version != List.Version)
				{
					throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
				}

				Index = List.CyclicTailIndex;
				_Current = default(T);
			}
		}

		#endregion

		#region Conversion

		public CircularList<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
		{
			if (converter == null)
				throw new ArgumentNullException(nameof(converter));

			var count = Count;
			var output = new CircularList<TOutput>(count);
			if (count > 0)
			{
				// Convert all items in order. So we start at the cycle's start index. As a bonus, we get a rearranged collection.
				var iOutput = 0;
				for (int i = CyclicTailIndex; i < count; i++)
					output.Items[iOutput++] = converter(Items[i]);
				for (int i = 0; i < CyclicTailIndex; i++)
					output.Items[iOutput++] = converter(Items[i]);
				output.CyclicTailIndex = 0;
				output.CyclicHeadIndex = count - 1;
			}
			else
			{
				output.CyclicTailIndex = -1;
				output.CyclicHeadIndex = -1;
			}
			return output;
		}

		public void CopyTo(T[] array)
		{
			CopyTo(array, 0);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			var count = Count;
			if (CyclicTailIndex == 0)
			{
				Array.Copy(Items, 0, array, arrayIndex, count);
			}
			else
			{
				Array.Copy(Items, CyclicTailIndex, array, arrayIndex, count - CyclicTailIndex);
				Array.Copy(Items, 0, array, arrayIndex + count - CyclicTailIndex, CyclicTailIndex);
			}
		}

		void ICollection.CopyTo(Array array, int arrayIndex)
		{
			if ((array != null) && (array.Rank != 1))
				throw new Exception("Multi dimensional arrays are not supported.");

			try
			{
				var count = Count;
				if (CyclicTailIndex == 0)
				{
					Array.Copy(Items, 0, array, arrayIndex, count);
				}
				else
				{
					Array.Copy(Items, CyclicTailIndex, array, arrayIndex, count - CyclicTailIndex);
					Array.Copy(Items, 0, array, arrayIndex + count - CyclicTailIndex, CyclicTailIndex);
				}
			}
			catch (ArrayTypeMismatchException)
			{
				throw new Exception("Array types does not match.");
			}
		}

		public T[] ToArray()
		{
			if (CyclicTailIndex < 0) // Means the collection is empty.
				return new T[0];

			var count = Count;
			var outputArray = new T[count];
			if (CyclicTailIndex == 0)
			{
				Array.Copy(Items, 0, outputArray, 0, count);
			}
			else
			{
				Array.Copy(Items, CyclicTailIndex, outputArray, 0, count - CyclicTailIndex);
				Array.Copy(Items, 0, outputArray, count - CyclicTailIndex, CyclicTailIndex);
			}
			return outputArray;
		}

		#endregion

		#region Parallelism

		//bool ICollection<T>.IsReadOnly { get { return false; } }
		bool ICollection.IsSynchronized { get { return false; } }

		Object ICollection.SyncRoot
		{
			get
			{
				if (SyncRootObject == null)
				{
					Interlocked.CompareExchange(ref SyncRootObject, new Object(), null);
				}
				return SyncRootObject;
			}
		}

		#endregion

		#region Decay

		//public Predicate<T> DecayCondition = null;

		//public T RemoveTailingIfDecayed()
		//{
		//	if (DecayCondition == null)
		//		throw new Exception("Tried to check for decay but decay condition was not specified.");

		//	var tailingItem = TailingItem;
		//	if (DecayCondition(tailingItem))
		//	{
		//		RemoveTailing();
		//		return tailingItem;
		//	}
		//	return null;
		//}

		//public void RemoveAllTailingIfDecayed()
		//{
		//	if (DecayCondition == null)
		//		throw new Exception("Tried to check for decay but decay condition was not specified.");

		//	if (CyclicTailIndex < 0)
		//		return;

		//	while (true)
		//	{
		//		var tailingItem = TailingItem;
		//		if (DecayCondition(tailingItem))
		//		{
		//			RemoveTailing();
		//			if (CyclicTailIndex < 0)
		//				return;
		//		}
		//		else
		//			return;
		//	}
		//}

		//public void RemoveAllTailingIfDecayed(List<T> removedItems)
		//{
		//	if (DecayCondition == null)
		//		throw new Exception("Tried to check for decay but decay condition was not specified.");
		//	if (removedItems == null)
		//		throw new ArgumentNullException("removedItems");

		//	if (CyclicTailIndex < 0)
		//		return;

		//	while (true)
		//	{
		//		var tailingItem = TailingItem;
		//		if (DecayCondition(tailingItem))
		//		{
		//			removedItems.Add(PopTailing());
		//			if (CyclicTailIndex < 0)
		//				return;
		//		}
		//		else
		//			return;
		//	}
		//}

		#endregion
	}

}
