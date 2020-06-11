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
		private const int IDStartsFrom = 1000;
		private const int IDEndsAt = Int32.MaxValue - 100;
		private const int IDAlarmsAt = Int32.MaxValue / 1000;

		[SerializeField]
		private int LastGivenID = IDStartsFrom;

		private int GenerateNewID()
		{
			LastGivenID++;
			if (LastGivenID > IDAlarmsAt)
			{
				ProcessPotentialOverflow();
			}
			return LastGivenID;
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
