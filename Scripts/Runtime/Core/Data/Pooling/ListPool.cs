using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Extenity.DataToolbox
{

	// TODO: Continue to implement Release mechanisms. Tests needed.

	/// <remarks>
	/// Example usage for manually returning the list to the pool:
	///
	///    var theList = New.List<ListItemType>(optionalCapacity);
	///    // Do some stuff with theList
	///    Release.List(ref theList);
	///
	/// Example usage for automatically returning the list to the pool:
	///
	///    using (New.List<ListItemType>(out var theList, optionalCapacity))
	///    {
	///        // Do some stuff with theList
	///    }
	/// </remarks>
	internal static class ListPool<T>
	{
		#region Initialization

		// static ListPool()
		// {
		// 	Log.Verbose($"Creating ListPool<{typeof(T).Name}>");
		// 	ListPoolTools.RegisterForRelease(ReleaseAllListsOfType);
		// }

		#endregion

		#region Deinitialization / Release the pool itself

		// public static void ReleaseAllListsOfAllTypes()
		// {
		// 	ListPoolTools.ReleaseAllListsOfAllTypes();
		// }
		//
		// public static void ReleaseAllListsOfType()
		// {
		// 	Pool.Clear();
		// }

		#endregion

		#region Pool

		private static readonly List<List<T>> Pool = new List<List<T>>();

		#endregion

		#region Allocate / Release Lists

		internal static ListDisposer<T> Using(out List<T> list, int capacity)
		{
			lock (Pool)
			{
				if (Pool.Count > 0)
				{
					// Get the largest capacity list from the pool. See 114572342.
					var index = Pool.Count - 1;
					list = Pool[index];
					Pool.RemoveAt(index);

					// Just roughly try to detect if the list is referenced and used elsewhere.
					if (list.Count != 0)
					{
						// This is unexpected and might mean the list is referenced elsewhere and currently in use.
						// Continuing to use a released list means serious problems, so a critical error will be logged
						// to warn the developer. The developer then have to look through pooled list releases and find
						// the spots where a copy of list reference is kept after its release.
						//
						// The pool will just skip the list and create a fresh one. We may try to get a new one from
						// the pool but the overhead is not worthwhile.
						list = new List<T>(capacity);
						Log.CriticalError($"Detected a usage of released list of type '{typeof(T).Name}'.");
						return new ListDisposer<T>(list);
					}

					// Adjust the capacity if its lower than expected. Changing capacity means allocating a memory
					// block, which is not performance friendly. When adding a new item to the list, .NET increases
					// the capacity by doubling current size. That allows us to omit the second half of the capacity
					// here and not instantly do the allocation right now.
					if (list.Capacity < capacity / 2)
					{
						list.Capacity = capacity;
					}
					return new ListDisposer<T>(list);
				}
			}
			list = new List<T>(capacity);
			return new ListDisposer<T>(list);
		}

		internal static void New(out List<T> list, int capacity)
		{
			lock (Pool)
			{
				if (Pool.Count > 0)
				{
					// Get the largest capacity list from the pool. See 114572342.
					var index = Pool.Count - 1;
					list = Pool[index];
					Pool.RemoveAt(index);

					// Just roughly try to detect if the list is referenced and used elsewhere.
					if (list.Count != 0)
					{
						// This is unexpected and might mean the list is referenced elsewhere and currently in use.
						// Continuing to use a released list means serious problems, so a critical error will be logged
						// to warn the developer. The developer then have to look through pooled list releases and find
						// the spots where a copy of list reference is kept after its release.
						//
						// The pool will just skip the list and create a fresh one. We may try to get a new one from
						// the pool but the overhead is not worthwhile.
						list = new List<T>(capacity);
						Log.CriticalError($"Detected a usage of released list of type '{typeof(T).Name}'.");
						return;
					}

					// Adjust the capacity if its lower than expected. Changing capacity means allocating a memory
					// block, which is not performance friendly. When adding a new item to the list, .NET increases
					// the capacity by doubling current size. That allows us to omit the second half of the capacity
					// here and not instantly do the allocation right now.
					if (list.Capacity < capacity / 2)
					{
						list.Capacity = capacity;
					}
					return;
				}
			}
			list = new List<T>(capacity);
		}

		internal static void New(out List<T> list, [NotNull] IEnumerable<T> collection)
		{
			lock (Pool)
			{
				if (Pool.Count > 0)
				{
					// Get the largest capacity list from the pool. See 114572342.
					var index = Pool.Count - 1;
					list = Pool[index];
					Pool.RemoveAt(index);

					// Just roughly try to detect if the list is referenced and used elsewhere.
					if (list.Count != 0)
					{
						// This is unexpected and might mean the list is referenced elsewhere and currently in use.
						// Continuing to use a released list means serious problems, so a critical error will be logged
						// to warn the developer. The developer then have to look through pooled list releases and find
						// the spots where a copy of list reference is kept after its release.
						//
						// The pool will just skip the list and create a fresh one. We may try to get a new one from
						// the pool but the overhead is not worthwhile.
						list = new List<T>(collection);
						Log.CriticalError($"Detected a usage of released list of type '{typeof(T).Name}'.");
						return;
					}

					list.AddRange(collection);
					return;
				}
			}
			list = new List<T>(collection);
		}

		internal static void Release(ref List<T> listReference)
		{
			// It's okay to pass a null list. The pooling system won't judge and just continue as if nothing has happened.
			if (listReference == null)
				return;

			// Ensure the reference to the list at the caller side won't be accidentally used.
			// Do it before modifying the pool to ensure thread safety.
			var list = listReference;
			listReference = null;

			list.Clear();
			lock (Pool)
			{
				if (Pool.Count == 0)
				{
					Pool.Add(list);
				}
				else
				{
					// Insert the released list into the pool, keeping the pool sorted by list capacity. So getting
					// the largest capacity list will be lightning fast. See 114572342.
					var capacity = list.Capacity;
					for (int i = Pool.Count - 1; i >= 0; i--)
					{
						if (capacity > Pool[i].Capacity)
						{
							Pool.Insert(i + 1, list);
							return;
						}
					}
					Pool.Insert(0, list);
				}
			}
		}

		internal static void _Free(List<T> list)
		{
			list.Clear();
			lock (Pool)
			{
				if (Pool.Count == 0)
				{
					Pool.Add(list);
				}
				else
				{
					// Insert the released list into the pool, keeping the pool sorted by list capacity. So getting
					// the largest capacity list will be lightning fast. See 114572342.
					var capacity = list.Capacity;
					for (int i = Pool.Count - 1; i >= 0; i--)
					{
						if (capacity > Pool[i].Capacity)
						{
							Pool.Insert(i + 1, list);
							return;
						}
					}
					Pool.Insert(0, list);
				}
			}
		}

		#endregion
	}

	public static class ListPoolTools
	{
		#region Release

		// It was a good idea but decided not to implement this. Because we lose the ability to assign null to
		// the variable at the caller side like in 'ListPool<T>(ref List<T> listReference)'.
		// public static void Release<T>(this List<T> listReference)
		// {
		// }

		#endregion

		#region Release Pools

		// private static event Action AllReleaseCallbacks;
		// private static object ReleaseLock = new object();
		//
		// public static void ReleaseAllListsOfAllTypes()
		// {
		// 	lock (ReleaseLock)
		// 	{
		// 		if (AllReleaseCallbacks != null)
		// 		{
		// 			AllReleaseCallbacks();
		// 		}
		// 	}
		// }
		//
		// public static void RegisterForRelease(Action releaseAllListsOfTypeCallback)
		// {
		// 	lock (ReleaseLock)
		// 	{
		// 		AllReleaseCallbacks += releaseAllListsOfTypeCallback;
		// 	}
		// }

		#endregion
	}

	public static partial class New
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static List<T> List<T>(int capacity = 0)
		{
			ListPool<T>.New(out var list, capacity);
			return list;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static List<T> List<T>([NotNull] IEnumerable<T> collection)
		{
			ListPool<T>.New(out var list, collection);
			return list;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ListDisposer<T> List<T>(out List<T> list, int capacity = 0)
		{
			return ListPool<T>.Using(out list, capacity);
		}

		public static List<TSource> ToPooledList<TSource>(this IEnumerable<TSource> source)
		{
			return source != null
				? New.List<TSource>(source)
				: throw new ArgumentNullException(nameof(source));
		}
	}

	public static partial class Release
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void List<T>(ref List<T> listReference)
		{
			ListPool<T>.Release(ref listReference);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void List<T>(ref List<T> listReference1, ref List<T> listReference2)
		{
			ListPool<T>.Release(ref listReference1);
			ListPool<T>.Release(ref listReference2);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void List<T>(ref List<T> listReference1, ref List<T> listReference2, ref List<T> listReference3)
		{
			ListPool<T>.Release(ref listReference1);
			ListPool<T>.Release(ref listReference2);
			ListPool<T>.Release(ref listReference3);
		}

		/// <summary>
		/// Alternative version that does not require passing the list reference as 'ref'. It won't be possible to
		/// automatically set the reference to null which provides a safety belt to prevent continuing to accidentally
		/// use the list after it's released to the pool. So it's considered an unsafe operation. Use it with caution
		/// and DO NOT EVER try to use the list after its Release.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ListUnsafe<T>(List<T> listReference)
		{
			ListPool<T>.Release(ref listReference);
		}
	}

	public readonly struct ListDisposer<T> : IDisposable
	{
		private readonly List<T> List;

		public ListDisposer(List<T> list)
		{
			List = list;
		}

		public void Dispose()
		{
			ListPool<T>._Free(List);
		}
	}

}
