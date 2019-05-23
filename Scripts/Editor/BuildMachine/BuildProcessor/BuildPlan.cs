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
			if (buildPhases.IsNullOrEmpty())
				throw new ArgumentNullException(nameof(buildPhases));
			if (builderOptionsList.IsNullOrEmpty())
				throw new ArgumentNullException(nameof(builderOptionsList));

			return new BuildPlan
			{
				BuildPhases = buildPhases,
				BuilderOptionsList = builderOptionsList,
			};
		}

		#endregion

		#region Build Phases

		public BuildPhaseInfo[] BuildPhases;

		#endregion

		#region Builder Options

		public BuilderOptions[] BuilderOptionsList;

		#endregion
	}

}
