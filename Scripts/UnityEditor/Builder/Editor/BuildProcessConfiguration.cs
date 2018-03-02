using System;
using System.Collections.Generic;
using Extenity.ConsistencyToolbox;

namespace Extenity.UnityEditorToolbox
{

	[Serializable]
	public struct BuildProcessConfiguration : IConsistencyChecker
	{
		public bool SkipAtCustomProcessing;
		public bool SkipAtBuildPreprocessing;

		public bool ClearScene;
		public bool DeparentStaticObjects;
		public bool CalculateLighting;
		public LightingBuildConfiguration Lighting;
		public bool ClearStaticLightsAfterLighting;
		public bool CalculateOcclusionCulling;
		public bool CalculateNavigation;
		//public List<KeyValue<string, bool>> CustomSwitches;

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
