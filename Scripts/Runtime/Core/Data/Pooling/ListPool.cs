using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Extenity.DataToolbox
{

	// TODO: Basic memory management that always gets the largest list. Tests needed.
	// TODO: Continue to implement Release mechanisms. Tests needed.
	// TODO: Multithreading support. Look at how ConcurrentBag and others are implemented. See if ThreadStatic is needed.

	/// <remarks>
	/// Example usage for manually returning the list to the pool:
	///
	///    var theList = New.List<SomeType>();
	///    // Do some stuff with theList
	///    Release.List(ref theList);
	///
	/// Example usage for automatically returning the list to the pool:
	///
	///    using (New.List<SomeType>(out var theList))
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

		[ThreadStatic]
		private static ConcurrentBag<List<T>> Pool;

		#endregion

		#region Allocate / Release Lists

		public static ListDisposer<T> Using(out List<T> list)
		{
			if (Pool != null && Pool.TryTake(out list))
			{
				list.Clear(); // Should not be needed, but let's clear the list. Just to be on the safe side.
			}
			else
			{
				list = new List<T>();
			}
			return new ListDisposer<T>(list);
		}

		public static void New(out List<T> list)
		{
			if (Pool != null && Pool.TryTake(out list))
			{
				list.Clear(); // Should not be needed, but let's clear the list. Just to be on the safe side.
			}
			else
			{
				list = new List<T>();
			}
		}

		public static void Release(ref List<T> listReference)
		{
			// It's okay to pass a null list. The pooling system won't judge and just continue as if nothing has happened.
			if (listReference == null)
				return;

			// Ensure the reference to the list at the caller side won't be accidentally used.
			// Do it before modifying the pool to ensure thread safety.
			var cached = listReference;
			listReference = null;

			cached.Clear();
			if (Pool == null)
				Pool = new ConcurrentBag<List<T>>();
			Pool.Add(cached);
		}

		internal static void _Free(List<T> list)
		{
			list.Clear();
			if (Pool == null)
				Pool = new ConcurrentBag<List<T>>();
			Pool.Add(list);
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
		public static List<T> List<T>()
		{
			ListPool<T>.New(out var list);
			return list;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ListDisposer<T> List<T>(out List<T> list)
		{
			return ListPool<T>.Using(out list);
		}
	}

	public static partial class Release
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void List<T>(ref List<T> listReference)
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
