using System;

namespace Extenity.BuildMachine.Editor
{

	[Serializable]
	public struct BuildPhase
	{
		public string Name;
		public BuildStepType[] IncludedSteps;

		public BuildPhase(string name, params BuildStepType[] includedSteps)
		{
			Name = name;
			IncludedSteps = includedSteps;
		}
	}

}
