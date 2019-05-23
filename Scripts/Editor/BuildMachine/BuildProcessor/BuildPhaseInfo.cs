using System;

namespace Extenity.BuildMachine.Editor
{

	[Serializable]
	public struct BuildPhaseInfo
	{
		public readonly string Name;
		public readonly BuildStepType[] IncludedSteps;

		public BuildPhaseInfo(string name, params BuildStepType[] includedSteps)
		{
			Name = name;
			IncludedSteps = includedSteps;
		}
	}

}
