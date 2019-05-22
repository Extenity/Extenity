using System;

namespace Extenity.BuildMachine.Editor
{

	[Serializable]
	public struct BuildJobPhaseDefinition
	{
		public string Name;
		public BuildStepType[] IncludedSteps;

		public BuildJobPhaseDefinition(string name, params BuildStepType[] includedSteps)
		{
			Name = name;
			IncludedSteps = includedSteps;
		}
	}

}
