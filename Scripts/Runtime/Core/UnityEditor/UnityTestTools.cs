using System;
using Extenity.SceneManagementToolbox;
using UnityEngine;

namespace Extenity.UnityTestToolbox
{

	public static class UnityTestTools
	{
		#region Cleanup

		public static void Cleanup()
		{
			SceneManagerTools.GetLoadedScenes(true, false).ForEach(scene =>
			{
				foreach (var rootObject in scene.GetRootGameObjects())
				{
					if (!rootObject.GetComponent("UnityEngine.TestTools.TestRunner.PlaymodeTestsController"))
					{
						GameObject.DestroyImmediate(rootObject);
					}
				}
			});
		}

		#endregion

		#region Memory Checker

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

			//GC.Collect(); Calling garbage collection just before GetTotalMemory will make it work more accurately. But it takes so long, so we ignore it.
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

			//GC.Collect(); Calling garbage collection just before GetTotalMemory will make it work more accurately. But it takes so long, so we ignore it.
			var change = GC.GetTotalMemory(false) - DetectedMemoryInMemoryCheck;
			DetectedMemoryInMemoryCheck = 0;
			if (change != 0)
			{
				Log.Warning($"Detected a memory change of '{change:N0}' bytes.");
			}
			return change != 0;
		}

		#endregion
	}

}
