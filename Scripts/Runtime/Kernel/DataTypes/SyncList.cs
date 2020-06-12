using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Extenity.KernelToolbox
{

	public class SyncList<T, TKernel>
		where T : KernelObject, new()
		where TKernel : KernelBase<TKernel>
	{
		/// <summary>
		/// CAUTION! Do not modify this list! Use it as readonly.
		/// </summary>
		public readonly List<T> List;

		public ID ID;

		#region Initialization

		public SyncList()
		{
			List = new List<T>();
			ID = ID.Invalid;
		}

		public SyncList([NotNull] IEnumerable<T> collection)
		{
			List = new List<T>(collection);
			ID = ID.Invalid;
		}

		public SyncList(int capacity)
		{
			List = new List<T>(capacity);
			ID = ID.Invalid;
		}

		public void Initialize(ID id)
		{
			ID = id;
		}

		/* Old implementation where Kernel was a field of SyncList. Keep it for future needs.
		public SyncList(ID id, [NotNull] KernelBase kernel)
		{
			List = new List<T>();
			Initialize(id, kernel);
		}

		public SyncList(ID id, [NotNull] KernelBase kernel, [NotNull] IEnumerable<T> collection)
		{
			List = new List<T>(collection);
			Initialize(id, kernel);
		}

		public SyncList(ID id, [NotNull] KernelBase kernel, int capacity)
		{
			List = new List<T>(capacity);
			Initialize(id, kernel);
		}

		public void Initialize(ID id, KernelBase kernel)
		{
			ID = id;
			Kernel = kernel;
		}
		*/

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
				Invalidate();
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
					Invalidate();
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
			Invalidate();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddRange([NotNull] IEnumerable<T> collection)
		{
			List.AddRange(collection);
			Invalidate();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Insert(int index, [CanBeNull] T item)
		{
			List.Insert(index, item);
			Invalidate();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void InsertRange(int index, [NotNull] IEnumerable<T> collection)
		{
			List.InsertRange(index, collection);
			Invalidate();
		}

		#endregion

		#region Remove / Clear

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove([CanBeNull] T item)
		{
			var result = List.Remove(item);
			Invalidate();
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int RemoveAll([NotNull] Predicate<T> match)
		{
			var result = List.RemoveAll(match);
			Invalidate();
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveAt(int index)
		{
			List.RemoveAt(index);
			Invalidate();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveRange(int index, int count)
		{
			List.RemoveRange(index, count);
			Invalidate();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			List.Clear();
			Invalidate();
		}

		#endregion

		#region Order

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reverse()
		{
			List.Reverse();
			Invalidate();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reverse(int index, int count)
		{
			List.Reverse(index, count);
			Invalidate();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Sort()
		{
			List.Sort();
			Invalidate();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Sort([NotNull] Comparison<T> comparison)
		{
			List.Sort(comparison);
			Invalidate();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Sort(IComparer<T> comparer)
		{
			List.Sort(comparer);
			Invalidate();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Sort(int index, int count, IComparer<T> comparer)
		{
			List.Sort(index, count, comparer);
			Invalidate();
		}

		#endregion

		#region Instantiate / Destroy

		public T InstantiateAndAdd()
		{
			var instance = Kernel.Instantiate<T>();
			Add(instance);
			return instance;
		}

		public void DestroyAndRemoveAt(int index)
		{
			// TODO: Boundary check
			var instance = List[index];
			RemoveAt(index);
			Kernel.Destroy(instance);
		}

		#endregion

		#region Kernel

		[JsonIgnore]
		private KernelBase Kernel
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => KernelBase<TKernel>.Instance;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Invalidate()
		{
			Kernel.Invalidate(ID);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void InvalidateItems()
		{
			foreach (var item in List)
			{
				Kernel.Invalidate(item.ID);
			}
		}

		#endregion
	}

}
