#if ExtenityKernel

using System;
using UnityEngine;

namespace Extenity.KernelToolbox
{

	[Serializable]
	public class IDGenerator
	{
		/// <summary>
		/// IDs below that are considered to be pre-allocated. Also IDs should not be treated as indexes of containers.
		/// Starting from other than 0 prevents possible misuse.
		/// </summary>
		private const UInt32 IDStartsFrom = 1000;
		private const UInt32 IDEndsAt = UInt32.MaxValue - 100;
		private const UInt32 IDAlarmsAt = UInt32.MaxValue / 1000;

		[SerializeField]
		private UInt32 LastGivenID = IDStartsFrom;

		private UInt32 GenerateNewID()
		{
			// Interlocked Increment and CompareExchange can be used instead of lock if performance becomes an issue.
			// Writing tests might be a good idea in that case.
			lock (this)
			{
				LastGivenID++;
				if (LastGivenID > IDAlarmsAt)
				{
					ProcessPotentialOverflow();
				}
				return LastGivenID;
			}
		}

		public UInt32 CreateID()
		{
			return GenerateNewID();
		}

		#region Overflow Warning

		private bool LastGivenIDOverflowWarning;

		private void ProcessPotentialOverflow()
		{
			if (!LastGivenIDOverflowWarning)
			{
				// That means we might need to turn IDs into UInt64.
				Log.Fatal("ID generator will overflow soon.");
				LastGivenIDOverflowWarning = true;
			}
			if (LastGivenID > IDEndsAt)
			{
				Log.Fatal("ID generator overflow.");
				LastGivenID = IDStartsFrom;
				LastGivenIDOverflowWarning = false;
			}
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(IDGenerator));

		#endregion
	}

}

#endif
