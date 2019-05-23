using System;
using UnityEditor;

namespace Extenity.BuildMachine.Editor
{

	public static class BuildJobRunner
	{
		#region Running Job

		public static BuildJob RunningJob;
		public static bool IsRunning => RunningJob != null;

		#endregion

		#region Start

		internal static void Start(BuildJob job)
		{
			if (IsRunning)
			{
				throw new Exception("Tried to start a build job while there is already a running one.");
			}
			RunningJob = job;

			throw new NotImplementedException();
			//EditorCoroutineUtility.StartCoroutineOwnerless(Run());
		}

		private static void Continue(BuildJob job)
		{
			if (IsRunning)
			{
				throw new Exception("Tried to continue a build job while there is already a running one.");
			}
			RunningJob = job;

			throw new NotImplementedException();
			//EditorCoroutineUtility.StartCoroutineOwnerless(Run());
		}

		#endregion

		#region MyRegion

		//private IEnumerator Run()
		//{
		//	//yield return DoBuild();
		//	CheckBeforeStep()
		//	Log.Info($"Build '{RunningJob.Plan.Name}' started.");
		//	Log.Info($"Build '{RunningJob.Plan.Name}' continuing at phase '{RunningJob.CurrentPhase}', builder '{RunningJob.CurrentBuilder}', step '{RunningJob.CurrentStep}'.");
		//	Log.Error($"Build '{RunningJob.Plan.Name}' failed.");
		//	Log.Info($"Build '{RunningJob.Plan.Name}' succeeded.");
		//}

		private static void CheckBeforeStep()
		{
			// At this point, there should be no ongoing compilations.
			// Build system does not allow any code that triggers
			// an assembly reload in Build Step. Otherwise execution
			// gets really messy.
			if (EditorApplication.isCompiling)
			{
				throw new Exception("Compilation is not allowed in the middle of build.");
			}

			// Save the unsaved assets before making any moves.
			AssetDatabase.SaveAssets();

			// Make sure everything is imported.
			{
				AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

				// And wait for scripts to compile.
				if (EditorApplication.isCompiling)
				{
					throw new Exception("COMPILING");
				}
			}
		}

		#endregion
	}

}
