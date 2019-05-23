using System;

namespace Extenity.BuildMachine.Editor
{

	[Serializable]
	public struct BuildPhaseInfo
	{
		public string Name;
		public BuildStepType[] IncludedSteps;

		public BuildPhaseInfo(string name, params BuildStepType[] includedSteps)
		{
			Name = name;
			IncludedSteps = includedSteps;
		}
	}

}
