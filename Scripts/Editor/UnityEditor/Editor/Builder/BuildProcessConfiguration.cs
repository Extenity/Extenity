using System;
using System.Collections.Generic;
using Extenity.ConsistencyToolbox;

namespace Extenity.UnityEditorToolbox
{

	[Serializable]
	public struct BuildProcessConfiguration : IConsistencyChecker
	{
		public string Category;
		public bool DontLoadAndMergeScenes;

		public bool SkipAtCustomProcessing;
		public bool SkipAtBuildPreprocessing;

		public LightingBuildConfiguration Lighting;

		public bool NeedsProcessing(bool isLaunchedByUser)
		{
			return isLaunchedByUser ? !SkipAtCustomProcessing : !SkipAtBuildPreprocessing;
		}

		#region Consistency

		public void CheckConsistency(ref List<ConsistencyError> errors)
		{
			Lighting.CheckConsistency(ref errors);
		}

		#endregion
	}

}
