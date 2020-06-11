using System;

namespace Extenity.KernelToolbox
{

	public static class IDGenerator
	{
		/// <summary>
		/// IDs below this value is considered to be pre-allocated.
		/// </summary>
		private const int IDStartsFrom = 1000;
		private const int IDEndsAt = Int32.MaxValue - 100;
		private const int IDAlarmsAt = Int32.MaxValue / 1000;

		private static int LastGivenID = IDStartsFrom;
		private static bool LastGivenIDOverflowWarning;

		private static int GenerateNewID()
		{
			LastGivenID++;
			if (LastGivenID > IDAlarmsAt)
			{
				if (!LastGivenIDOverflowWarning)
				{
					// That means we will need to turn versioning data into Int64.
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
			return LastGivenID;
		}

		public static ID CreateID()
		{
			return new ID(GenerateNewID());
		}
	}

}
