using Newtonsoft.Json;

namespace Extenity.BuildMachine.Editor
{

	[JsonObject]
	public struct BuildPhaseInfo
	{
		[JsonProperty]
		public readonly string Name;
		[JsonProperty]
		public readonly BuildStepType[] IncludedSteps;

		public BuildPhaseInfo(string name, params BuildStepType[] includedSteps)
		{
			Name = name;
			IncludedSteps = includedSteps;
		}
	}

}
