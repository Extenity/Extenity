using System;
using System.Collections.Generic;

namespace Extenity.DebugFlowTool.Generic
{

	public static class UniqueIDGenerator
	{
		#region Initialization

		private static bool IsInitialized;

		private static void InitializeIfRequired()
		{
			if (IsInitialized)
				return;
			IsInitialized = true;

			Randomizer = new Random((int)(DateTime.Now.Ticks % int.MaxValue));
			GeneratedUInt32 = new HashSet<UInt32>();
		}

		#endregion

		#region Randomizer

		private static Random Randomizer;

		#endregion

		#region Generated ID History

		private static HashSet<UInt32> GeneratedUInt32;

		#endregion

		#region Generators

		public static UInt32 GenerateUInt32()
		{
			InitializeIfRequired();

			while (true)
			{
				var value = (UInt32)Randomizer.Next(100, int.MaxValue);
				if (GeneratedUInt32.Add(value))
					return value;
			}
		}

		#endregion
	}

}
