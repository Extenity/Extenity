#if ExtenityKernel

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Extenity.KernelToolbox
{

	public class SyncList<TKernelObject, TKernel> : KernelObject<TKernel>
		where TKernelObject : KernelObject<TKernel>, new()
		where TKernel : KernelBase<TKernel>
	{
		/// <summary>
		/// CAUTION! Do not modify this list! Use it as readonly.
		/// </summary>
		public readonly List<TKernelObject> List;

		#region Initialization

		[JsonConstructor]
		public SyncList()
		{
			List = new List<TKernelObject>();
		}

		#endregion

		#region Accessors

		public TKernelObject this[int index]
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
		public void ForEach([NotNull] Action<TKernelObject> action)
		{
			List.ForEach(action);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains([CanBeNull] TKernelObject item)
		{
			return List.Contains(item);
		}

		public bool Contains(Ref<TKernelObject, TKernel> id)
		{
			for (var i = 0; i < List.Count; i++)
			{
				if (List[i] != null && List[i].ID == id)
				{
					return true;
				}
			}
			return false;
		}

		public TKernelObject GetItem(Ref<TKernelObject, TKernel> id)
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
		public void Add([CanBeNull] TKernelObject item)
		{
			List.Add(item);
			Invalidate();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddRange([NotNull] IEnumerable<TKernelObject> collection)
		{
			List.AddRange(collection);
			Invalidate();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Insert(int index, [CanBeNull] TKernelObject item)
		{
			List.Insert(index, item);
			Invalidate();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void InsertRange(int index, [NotNull] IEnumerable<TKernelObject> collection)
		{
			List.InsertRange(index, collection);
			Invalidate();
		}

		#endregion

		#region Remove / Clear

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove([CanBeNull] TKernelObject item)
		{
			var result = List.Remove(item);
			Invalidate();
			return result;
		}

		public bool Remove(Ref<TKernelObject, TKernel> id)
		{
			for (var i = 0; i < List.Count; i++)
			{
				if (List[i] != null && List[i].ID == id)
				{
					List.RemoveAt(i);
					Invalidate();
					return true;
				}
			}
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int RemoveAll([NotNull] Predicate<TKernelObject> match)
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
		public void Sort([NotNull] Comparison<TKernelObject> comparison)
		{
			List.Sort(comparison);
			Invalidate();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Sort(IComparer<TKernelObject> comparer)
		{
			List.Sort(comparer);
			Invalidate();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Sort(int index, int count, IComparer<TKernelObject> comparer)
		{
			List.Sort(index, count, comparer);
			Invalidate();
		}

		#endregion

		#region Instantiate / Destroy

		public TKernelObject InstantiateAndAdd()
		{
			var instance = Kernel.Instantiate<TKernelObject>();
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

#endif
