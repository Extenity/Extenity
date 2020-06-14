using System;
using UnityEngine;

namespace Extenity.KernelToolbox
{

	[Serializable]
	public class IDGenerator
	{
		/// <summary>
		/// IDs below this value is considered to be pre-allocated.
		/// </summary>
		private const uint IDStartsFrom = 1000;
		private const uint IDEndsAt = UInt32.MaxValue - 100;
		private const uint IDAlarmsAt = UInt32.MaxValue / 1000;

		[SerializeField]
		private uint LastGivenID = IDStartsFrom;

		private uint GenerateNewID()
		{
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

		public ID CreateID()
		{
			return new ID(GenerateNewID());
		}

		#region Overflow Warning

		private bool LastGivenIDOverflowWarning;

		private void ProcessPotentialOverflow()
		{
			if (!LastGivenIDOverflowWarning)
			{
				// That means we will need to turn IDs into Int64.
				Log.CriticalError("ID generator will overflow soon.");
				LastGivenIDOverflowWarning = true;
			}
			if (LastGivenID > IDEndsAt)
			{
				Log.CriticalError("ID generator overflow.");
				LastGivenID = IDStartsFrom;
				LastGivenIDOverflowWarning = false;
			}
		}

		#endregion
	}

}
