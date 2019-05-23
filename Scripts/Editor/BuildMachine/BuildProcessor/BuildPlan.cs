using System;
using Extenity.DataToolbox;
using Newtonsoft.Json;

namespace Extenity.BuildMachine.Editor
{

	[JsonObject]
	public class BuildPlan
	{
		#region Initialization

		public static BuildPlan Create(string name, BuildPhaseInfo[] buildPhases, params BuilderOptions[] builderOptionsList)
		{
			return new BuildPlan(name, buildPhases, builderOptionsList);
		}

		private BuildPlan()
		{
			// Nothing to do here. This empty constructor allows json deserialization.
		}

		private BuildPlan(string name, BuildPhaseInfo[] buildPhases, BuilderOptions[] builderOptionsList)
		{
			if (buildPhases.IsNullOrEmpty())
				throw new ArgumentNullException(nameof(buildPhases));
			if (builderOptionsList.IsNullOrEmpty())
				throw new ArgumentNullException(nameof(builderOptionsList));

			Name = name;
			BuildPhases = buildPhases;
			BuilderOptionsList = builderOptionsList;
		}

		#endregion

		#region Metadata

		[JsonProperty]
		public readonly string Name;

		#endregion

		#region Build Phases

		[JsonProperty]
		public readonly BuildPhaseInfo[] BuildPhases;

		#endregion

		#region Builder Options

		[JsonProperty]
		public readonly BuilderOptions[] BuilderOptionsList;

		#endregion
	}

}
