using System;
using UnityEngine;

namespace Extenity.UnityTestToolbox.Editor
{

	public static class UnityTestTools
	{
		#region Tools - Memory Checker

		private static Int64 DetectedMemoryInMemoryCheck;

		/// <summary>
		/// Note that memory check does not work consistently. So make sure you run the tests multiple times.
		/// </summary>
		public static void BeginMemoryCheck()
		{
			if (DetectedMemoryInMemoryCheck != 0)
			{
				DetectedMemoryInMemoryCheck = 0; // Reset it for further use.
				throw new Exception("Memory check was already started.");
			}

			DetectedMemoryInMemoryCheck = GC.GetTotalMemory(false);
			if (DetectedMemoryInMemoryCheck == 0)
			{
				throw new Exception("Failed to get current memory size.");
			}
		}

		/// <summary>
		/// Note that memory check does not work consistently. So make sure you run the tests multiple times.
		/// </summary>
		public static bool EndMemoryCheck()
		{
			if (DetectedMemoryInMemoryCheck == 0)
			{
				throw new Exception("Memory check was not started.");
			}

			var change = GC.GetTotalMemory(false) - DetectedMemoryInMemoryCheck;
			DetectedMemoryInMemoryCheck = 0;
			if (change != 0)
			{
				Debug.LogWarning(string.Format("Detected a memory change of '{0}' bytes.", change));
			}
			return change != 0;
		}

		#endregion
	}

}
