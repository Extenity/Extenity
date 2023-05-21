using System;
using Extenity.DataToolbox;
using Newtonsoft.Json;

namespace Extenity.BuildMachine.Editor
{

	[JsonObject]
	public class BuildPlan
	{
		#region Initialization

		public static BuildPlan Create(string name, BuildPhaseInfo[] buildPhases, BuilderOptions builderOptions)
		{
			return new BuildPlan(name, buildPhases, builderOptions);
		}

		private BuildPlan()
		{
			// Nothing to do here. This empty constructor allows json deserialization.
		}

		private BuildPlan(string name, BuildPhaseInfo[] buildPhases, BuilderOptions builderOptions)
		{
			if (buildPhases.IsNullOrEmpty())
				throw new ArgumentNullException(nameof(buildPhases));
			if (builderOptions == null)
				throw new ArgumentNullException(nameof(builderOptions));

			Name = name;
			BuildPhases = buildPhases;
			BuilderOptions = builderOptions;
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
		public readonly BuilderOptions BuilderOptions;

		#endregion
	}

}
