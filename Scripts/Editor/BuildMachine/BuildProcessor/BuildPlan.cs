using System;
using Extenity.DataToolbox;
using Newtonsoft.Json;

namespace Extenity.BuildMachine.Editor
{

	[JsonObject]
	public class BuildPlan
	{
		#region Initialization

		public static BuildPlan Create(string name, int addMajorVersion, int addMinorVersion, int addBuildVersion, BuildPhaseInfo[] buildPhases, params BuilderOptions[] builderOptionsList)
		{
			return new BuildPlan(name, addMajorVersion, addMinorVersion, addBuildVersion, buildPhases, builderOptionsList);
		}

		private BuildPlan()
		{
			// Nothing to do here. This empty constructor allows json deserialization.
		}

		private BuildPlan(string name, int addMajorVersion, int addMinorVersion, int addBuildVersion, BuildPhaseInfo[] buildPhases, BuilderOptions[] builderOptionsList)
		{
			if (buildPhases.IsNullOrEmpty())
				throw new ArgumentNullException(nameof(buildPhases));
			if (builderOptionsList.IsNullOrEmpty())
				throw new ArgumentNullException(nameof(builderOptionsList));

			Name = name;
			AddMajorVersion = addMajorVersion;
			AddMinorVersion = addMinorVersion;
			AddBuildVersion = addBuildVersion;
			BuildPhases = buildPhases;
			BuilderOptionsList = builderOptionsList;
		}

		#endregion

		#region Metadata

		[JsonProperty]
		public readonly string Name;

		#endregion

		#region Version Options

		public int AddMajorVersion = 0;
		public int AddMinorVersion = 0;
		public int AddBuildVersion = 0;

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
