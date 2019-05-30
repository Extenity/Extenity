using Newtonsoft.Json;

namespace Extenity.BuildMachine.Editor
{

	[JsonObject]
	public struct BuildPhaseInfo
	{
		[JsonProperty]
		public readonly string Name;
		[JsonProperty]
		public readonly BuildStepType[] IncludedBuildSteps;
		[JsonProperty]
		public readonly BuildStepType[] IncludedFinalizationSteps;

		public BuildPhaseInfo(string name, BuildStepType[] includedBuildSteps, BuildStepType[] includedFinalizationSteps)
		{
			Name = name;
			IncludedBuildSteps = includedBuildSteps;
			IncludedFinalizationSteps = includedFinalizationSteps;
		}
	}

}
