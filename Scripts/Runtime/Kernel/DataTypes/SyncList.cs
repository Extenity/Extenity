using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Extenity.Kernel
{

	public class SyncList<T> where T : KernelObject
	{
		/// <summary>
		/// CAUTION! Do not modify this list! Use it as readonly.
		/// </summary>
		public readonly List<T> List;

		public ID ID;

		#region Initialization

		public SyncList()
		{
			ID = ID.Invalid;
			List = new List<T>();
		}

		public SyncList([NotNull] IEnumerable<T> collection)
		{
			ID = ID.Invalid;
			List = new List<T>(collection);
		}

		public SyncList(int capacity)
		{
			ID = ID.Invalid;
			List = new List<T>(capacity);
		}

		public SyncList(ID id)
		{
			ID = id;
			List = new List<T>();
		}

		public SyncList(ID id, [NotNull] IEnumerable<T> collection)
		{
			ID = id;
			List = new List<T>(collection);
		}

		public SyncList(ID id, int capacity)
		{
			ID = id;
			List = new List<T>(capacity);
		}

		#endregion

		#region Accessors

		public T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => List[index];
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				List[index] = value;
				Versioning.Invalidate(ID);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ForEach([NotNull] Action<T> action)
		{
			List.ForEach(action);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains([CanBeNull] T item)
		{
			return List.Contains(item);
		}

		public T GetItem(Ref id)
		{
			for (var i = 0; i < List.Count; i++)
			{
				if (List[i] != null && List[i].ID == id)
				{
					return List[i];
				}
			}
			return null;
		}

		#endregion

		#region Size

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => List.Count;
		}

		public int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => List.Capacity;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				var previousCount = List.Count;
				List.Capacity = value;
				if (List.Count != previousCount) // Invalidate the data if changing capacity caused modifications in list.
				{
					Versioning.Invalidate(ID);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void TrimExcess()
		{
			List.TrimExcess();
			// Versioning.Invalidate(ID.Reference); Nope! List contents won't change.
		}

		#endregion

		#region Add / Insert

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add([CanBeNull] T item)
		{
			List.Add(item);
			Versioning.Invalidate(ID);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddRange([NotNull] IEnumerable<T> collection)
		{
			List.AddRange(collection);
			Versioning.Invalidate(ID);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Insert(int index, [CanBeNull] T item)
		{
			List.Insert(index, item);
			Versioning.Invalidate(ID);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void InsertRange(int index, [NotNull] IEnumerable<T> collection)
		{
			List.InsertRange(index, collection);
			Versioning.Invalidate(ID);
		}

		#endregion

		#region Remove / Clear

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove([CanBeNull] T item)
		{
			var result = List.Remove(item);
			Versioning.Invalidate(ID);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int RemoveAll([NotNull] Predicate<T> match)
		{
			var result = List.RemoveAll(match);
			Versioning.Invalidate(ID);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveAt(int index)
		{
			List.RemoveAt(index);
			Versioning.Invalidate(ID);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveRange(int index, int count)
		{
			List.RemoveRange(index, count);
			Versioning.Invalidate(ID);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			List.Clear();
			Versioning.Invalidate(ID);
		}

		#endregion

		#region Order

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reverse()
		{
			List.Reverse();
			Versioning.Invalidate(ID);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reverse(int index, int count)
		{
			List.Reverse(index, count);
			Versioning.Invalidate(ID);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Sort()
		{
			List.Sort();
			Versioning.Invalidate(ID);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Sort([NotNull] Comparison<T> comparison)
		{
			List.Sort(comparison);
			Versioning.Invalidate(ID);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Sort(IComparer<T> comparer)
		{
			List.Sort(comparer);
			Versioning.Invalidate(ID);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Sort(int index, int count, IComparer<T> comparer)
		{
			List.Sort(index, count, comparer);
			Versioning.Invalidate(ID);
		}

		#endregion
	}

}
