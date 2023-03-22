using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Extenity.DataToolbox
{

	/// <remarks>
	/// Example usage for manually returning the FastHashSet to the pool:
	///
	///    var theCollection = New.FastHashSet<ItemType>(optionalCapacity, optionalEqualityComparer);
	///    // Do some stuff with theCollection
	///    Release.FastHashSet(ref theCollection);
	///
	/// Example usage for automatically returning the FastHashSet to the pool:
	///
	///    using (New.FastHashSet<ItemType>(out var theCollection, optionalCapacity, optionalEqualityComparer))
	///    {
	///        // Do some stuff with theCollection
	///    }
	/// </remarks>
	internal static class FastHashSetPool<T>
	{
		#region Initialization

		static FastHashSetPool()
		{
			Log.Verbose($"Creating {nameof(FastHashSetPool<T>)}<{typeof(T).Name}>");
			FastHashSetPoolTools.RegisterForAllPoolsRelease(ReleasePool);
		}

		#endregion

		#region Deinitialization / Release the pool itself

		public static void ReleasePool()
		{
			lock (Pool)
			{
				Pool.Clear();
			}
		}

		#endregion

		#region Pool

		internal static readonly List<FastHashSet<T>> Pool = new List<FastHashSet<T>>();

		#endregion

		#region Allocate / Release Collections

		private static FastHashSet<T> _GetNextItemInPool()
		{
			// Get the largest capacity collection from the pool. See 114572342.
			var index = Pool.Count - 1;
			var collection = Pool[index];
			Pool.RemoveAt(index);
			return collection;
		}

		private static bool _RoughlyCheckIfCollectionWasUsedElsewhere(FastHashSet<T> collection)
		{
			// Just roughly try to detect if the collection is referenced and used elsewhere.
			// Hopefully .NET will provide a way to get the Version info of collection in future.
			return collection.Count != 0;
		}

		private static void _LogErrorForUnexpectedlyUsedCollection()
		{
			// This is unexpected and might mean the collection is referenced elsewhere and currently in use.
			// Continuing to use a released collection means serious problems, so a Fatal error will be logged
			// to warn the developer. The developer then have to look through pooled collection releases and find
			// the spots where a copy of collection reference is kept after its release.
			//
			// The pool will just skip the collection and create a fresh one. We may try to get a new one from
			// the pool but the overhead is not worthwhile.
			Log.Fatal($"Detected a collection of type '{nameof(FastHashSet<T>)}<{typeof(T).Name}>' which was used even after it was released to pool.");
		}

		private static void _AdjustCapacity(FastHashSet<T> collection, int capacity)
		{
			// Adjust the capacity if its lower than expected.
			// When adding a new item to the collection, .NET increases the capacity by doubling current size.
			// Knowing that allows us to act smart here. Changing capacity means allocating a memory block,
			// which is not performance friendly. So even though the capacity is lower than the expected here,
			// we don't immediately increase the capacity and do an allocation if the capacity is already
			// greater than the half of what is expected. Because if the user would fill the collection
			// that much, .NET would already be increasing the size. It's smart not to increase it here
			// right now for the possibility that the user may not fill the collection to above its current
			// capacity. Otherwise we might end up increasing it unnecessarily.
			collection.EnsureCapacity(capacity / 2);
		}

		internal static FastHashSetDisposer<T> Using(out FastHashSet<T> collection, int capacity)
		{
			lock (Pool)
			{
				if (Pool.Count > 0)
				{
					collection = _GetNextItemInPool();

					if (_RoughlyCheckIfCollectionWasUsedElsewhere(collection))
					{
						_LogErrorForUnexpectedlyUsedCollection();
						collection = new FastHashSet<T>(capacity);
						return new FastHashSetDisposer<T>(collection);
					}

					_AdjustCapacity(collection, capacity);
					return new FastHashSetDisposer<T>(collection);
				}
			}
			collection = new FastHashSet<T>(capacity);
			return new FastHashSetDisposer<T>(collection);
		}

		internal static void New(out FastHashSet<T> collection, int capacity, IEqualityComparer<T> comparer)
		{
			lock (Pool)
			{
				if (Pool.Count > 0)
				{
					collection = _GetNextItemInPool();

					if (_RoughlyCheckIfCollectionWasUsedElsewhere(collection))
					{
						_LogErrorForUnexpectedlyUsedCollection();
						collection = new FastHashSet<T>(capacity, comparer);
						return;
					}

					_AdjustCapacity(collection, capacity);
					return;
				}
			}
			collection = new FastHashSet<T>(capacity, comparer);
		}

		internal static void New(out FastHashSet<T> collection, [NotNull] IEnumerable<T> otherCollection, IEqualityComparer<T> comparer)
		{
			lock (Pool)
			{
				if (Pool.Count > 0)
				{
					collection = _GetNextItemInPool();

					if (_RoughlyCheckIfCollectionWasUsedElsewhere(collection))
					{
						_LogErrorForUnexpectedlyUsedCollection();
						collection = new FastHashSet<T>(otherCollection, comparer);
						return;
					}

					collection.UnionWith(otherCollection);
					return;
				}
			}
			collection = new FastHashSet<T>(otherCollection, comparer);
		}

		internal static void Release(ref FastHashSet<T> collectionReference)
		{
			// It's okay to pass a null collection. The pooling system won't judge and just continue as if nothing has happened.
			if (collectionReference == null)
				return;

			// Ensure the reference to the collection at the caller side won't be accidentally used.
			// Do it before modifying the pool to ensure thread safety.
			var collection = collectionReference;
			collectionReference = null;

			_Free(collection);
		}

		internal static void _Free(FastHashSet<T> collection)
		{
			collection.Clear();
			lock (Pool)
			{
				if (Pool.Count == 0)
				{
					Pool.Add(collection);
				}
				else
				{
					// Insert the released collection into the pool, keeping the pool sorted by collection capacity.
					// So getting the largest capacity collection will be lightning fast. See 114572342.
					var capacity = collection.Capacity;
					for (int i = Pool.Count - 1; i >= 0; i--)
					{
						if (capacity > Pool[i].Capacity)
						{
							Pool.Insert(i + 1, collection);
							return;
						}
					}
					Pool.Insert(0, collection);
				}
			}
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(FastHashSetPool<T>));

		#endregion
	}

	public static class FastHashSetPoolTools
	{
		#region Release

		// It was a good idea but decided not to implement this. Because we lose the ability to assign null to
		// the variable at the caller side like in 'FastHashSetPool<T>(ref FastHashSet<T> collectionReference)'.
		// public static void Release<T>(this FastHashSet<T> collectionReference)
		// {
		// }

		#endregion

		#region Release Pools

		private static event Action AllPoolReleaseCallbacks;
		private static readonly object ReleaseLock = new object();

		public static void ReleaseAllPoolsOfAllTypes()
		{
			lock (ReleaseLock)
			{
				if (AllPoolReleaseCallbacks != null)
				{
					AllPoolReleaseCallbacks();
				}
			}
		}

		public static void RegisterForAllPoolsRelease(Action releasePoolCallback)
		{
			lock (ReleaseLock)
			{
				AllPoolReleaseCallbacks += releasePoolCallback;
			}
		}

		#endregion
	}

	public static partial class New
	{
		/// <summary>
		/// Gets the next available collection in pool or creates a new one if pool doesn't have any available.
		/// Make sure to return the collection to the pool via Release.FastHashSet<T>().
		/// </summary>
		/// <param name="capacity">
		/// The pooling system will give the collection with largest capacity first. A new container will be created
		/// with specified capacity if the pool is empty. If the pool has collections available but not one that matches
		/// the required capacity, then the system will get the next largest collection in pool and increase its
		/// capacity in a smart way. See the description in code for details about how smart it is.
		/// </param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FastHashSet<T> FastHashSet<T>(int capacity = 0, IEqualityComparer<T> comparer = null)
		{
			FastHashSetPool<T>.New(out var collection, capacity, comparer);
			return collection;
		}

		/// <summary>
		/// Gets the next available collection in pool or creates a new one if pool doesn't have any available.
		/// Make sure to return the collection to the pool via Release.FastHashSet<T>().
		/// </summary>
		/// <param name="otherCollection">
		/// Initialize the collection with given enumerable values. Note that the pooling system will give
		/// the collection with largest capacity first. So expect getting much bigger capacity even though the specified
		/// collection might be tiny in size.
		/// </param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FastHashSet<T> FastHashSet<T>([NotNull] IEnumerable<T> otherCollection, IEqualityComparer<T> comparer = null)
		{
			FastHashSetPool<T>.New(out var collection, otherCollection, comparer);
			return collection;
		}

		/// <summary>
		/// Gets the next available collection in pool or creates a new one if pool doesn't have any available.
		/// Make sure to return the collection to the pool via Release.FastHashSet<T>().
		/// </summary>
		/// <param name="capacity">
		/// The pooling system will give the collection with largest capacity first. A new container will be created
		/// with specified capacity if the pool is empty. If the pool has collections available but not one that matches
		/// the required capacity, then the system will get the next largest collection in pool and increase its
		/// capacity in a smart way. See the description in code for details about how smart it is.
		/// </param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FastHashSetDisposer<T> FastHashSet<T>(out FastHashSet<T> collection, int capacity = 0)
		{
			return FastHashSetPool<T>.Using(out collection, capacity);
		}

		public static FastHashSet<TSource> ToPooledFastHashSet<TSource>(this IEnumerable<TSource> source)
		{
			return source != null
				? New.FastHashSet<TSource>(source)
				: throw new ArgumentNullException(nameof(source));
		}
	}

	public static partial class Release
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FastHashSet<T>(ref FastHashSet<T> collectionReference)
		{
			FastHashSetPool<T>.Release(ref collectionReference);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FastHashSet<T>(ref FastHashSet<T> collectionReference1, ref FastHashSet<T> collectionReference2)
		{
			FastHashSetPool<T>.Release(ref collectionReference1);
			FastHashSetPool<T>.Release(ref collectionReference2);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FastHashSet<T>(ref FastHashSet<T> collectionReference1, ref FastHashSet<T> collectionReference2, ref FastHashSet<T> collectionReference3)
		{
			FastHashSetPool<T>.Release(ref collectionReference1);
			FastHashSetPool<T>.Release(ref collectionReference2);
			FastHashSetPool<T>.Release(ref collectionReference3);
		}

		/// <summary>
		/// Alternative version that does not require passing the collection reference as 'ref'. It won't be possible to
		/// automatically set the reference to null which provides a safety belt to prevent continuing to accidentally
		/// use the collection after it's released to the pool. So it's considered an unsafe operation. Use it with
		/// caution and DO NOT EVER try to use the collection after its Release.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FastHashSetUnsafe<T>(FastHashSet<T> collectionReference)
		{
			FastHashSetPool<T>.Release(ref collectionReference);
		}
	}

	public readonly struct FastHashSetDisposer<T> : IDisposable
	{
		private readonly FastHashSet<T> Collection;

		public FastHashSetDisposer(FastHashSet<T> collection)
		{
			Collection = collection;
		}

		public void Dispose()
		{
			FastHashSetPool<T>._Free(Collection);
		}
	}

}
