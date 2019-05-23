using System;
using Extenity.DataToolbox;

namespace Extenity.BuildMachine.Editor
{

	[Serializable]
	public class BuildPlan
	{
		#region Initialization

		public static BuildPlan Create(BuildPhaseInfo[] buildPhases, params BuilderOptions[] builderOptionsList)
		{
			return new BuildPlan(buildPhases, builderOptionsList);
		}

		private BuildPlan(BuildPhaseInfo[] buildPhases, BuilderOptions[] builderOptionsList)
		{
			if (buildPhases.IsNullOrEmpty())
				throw new ArgumentNullException(nameof(buildPhases));
			if (builderOptionsList.IsNullOrEmpty())
				throw new ArgumentNullException(nameof(builderOptionsList));

			BuildPhases = buildPhases;
			BuilderOptionsList = builderOptionsList;
		}

		#endregion

		#region Build Phases

		public readonly BuildPhaseInfo[] BuildPhases;

		#endregion

		#region Builder Options

		public readonly BuilderOptions[] BuilderOptionsList;

		#endregion
	}

}
