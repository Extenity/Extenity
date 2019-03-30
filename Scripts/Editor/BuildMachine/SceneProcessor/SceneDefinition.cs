using System;
using System.Collections.Generic;
using Extenity.ConsistencyToolbox;

namespace Extenity.BuildMachine.Editor
{

	[Serializable]
	public class SceneDefinition : IConsistencyChecker
	{
		/// <summary>
		/// Path of the resulting scene after processing has been done. This should match the scene path in build settings.
		/// </summary>
		public string ProcessedScenePath;
		/// <summary>
		/// Base scene that will be used as the active scene. Unity uses this scene for lighting and navigation calculations.
		/// </summary>
		public string MainScenePath;
		/// <summary>
		/// Additional scenes that will be merged on top of the scene at MainScenePath.
		/// </summary>
		public string[] MergedScenePaths;

		#region Consistency

		public void CheckConsistency(ref List<ConsistencyError> errors)
		{
			if (string.IsNullOrEmpty(MainScenePath))
			{
				errors.Add(new ConsistencyError(this, "Main scene path was not specified.", true));
			}
		}

		#endregion
	}

}
