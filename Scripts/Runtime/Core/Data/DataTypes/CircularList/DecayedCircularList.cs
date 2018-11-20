using System;
using System.Collections.Generic;

namespace Extenity.DataToolbox
{

	/// <summary>
	/// Auto expandable circular list that allows user to define a decay condition 
	/// that makes auto removal of items as new items arrive.
	/// </summary>
	public class DecayedCircularList<T> : CircularList<T>
	{
		#region Initialization

		public DecayedCircularList() : base()
		{
		}

		public DecayedCircularList(int capacity) : base(capacity)
		{
		}

		public DecayedCircularList(IEnumerable<T> collection) : base(collection)
		{
		}

		#endregion

		#region Decay

		public Predicate<T> DecayCondition = null;

		public void RemoveTailingIfDecayed()
		{
			if (DecayCondition == null)
				throw new Exception("Tried to check for decay but decay condition was not specified.");

			if (IsEmpty)
				return;

			if (DecayCondition(TailingItem))
			{
				RemoveTailing();
			}
		}

		public T PopTailingIfDecayed()
		{
			if (DecayCondition == null)
				throw new Exception("Tried to check for decay but decay condition was not specified.");

			if (IsEmpty)
				return default(T);

			var tailingItem = TailingItem;
			if (DecayCondition(tailingItem))
			{
				RemoveTailing();
				return tailingItem;
			}
			return default(T);
		}

		public void RemoveAllTailingIfDecayed()
		{
			if (DecayCondition == null)
				throw new Exception("Tried to check for decay but decay condition was not specified.");

			if (IsEmpty)
				return;

			while (true)
			{
				var tailingItem = TailingItem;
				if (DecayCondition(tailingItem))
				{
					RemoveTailing();
					if (CyclicTailIndex < 0)
						return;
				}
				else
					return;
			}
		}

		public int PopAllTailingIfDecayed(List<T> removedItems)
		{
			if (DecayCondition == null)
				throw new Exception("Tried to check for decay but decay condition was not specified.");
			if (removedItems == null)
				throw new ArgumentNullException(nameof(removedItems));

			if (IsEmpty)
				return 0;

			var removedCount = 0;
			while (true)
			{
				var tailingItem = TailingItem;
				if (DecayCondition(tailingItem))
				{
					removedItems.Add(PopTailing());
					removedCount++;
					if (CyclicTailIndex < 0)
						return removedCount;
				}
				else
					return removedCount;
			}
		}

		#endregion
	}

}
