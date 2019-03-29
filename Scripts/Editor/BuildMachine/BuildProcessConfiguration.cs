using System;
using System.Collections.Generic;
using Extenity.ConsistencyToolbox;

namespace Extenity.BuildMachine.Editor
{

	[Serializable]
	public struct BuildProcessConfiguration : IConsistencyChecker
	{
		public string Category;
		public bool DontLoadAndMergeScenes;

		public LightingBuildConfiguration Lighting;

		#region Consistency

		public void CheckConsistency(ref List<ConsistencyError> errors)
		{
			Lighting.CheckConsistency(ref errors);
		}

		#endregion
	}

}
