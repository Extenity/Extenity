using System;

namespace Extenity.BuildMachine.Editor
{

	public static class BuildJobRunner
	{
		#region Running Job

		public static BuildJob RunningJob;
		public static bool IsRunning => RunningJob != null;

		#endregion

		public static void Start(BuildJob job)
		{
			if (IsRunning)
			{
				throw new Exception("Tried to start a build job while there is already a running one.");
			}
			RunningJob = job;

			throw new NotImplementedException();
			//EditorCoroutineUtility.StartCoroutineOwnerless(RunProcess());
		}
	}

}
