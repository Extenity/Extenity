using System;
using System.Collections.Generic;
using System.Linq;
using Extenity.ConsistencyToolbox;
using Extenity.DataToolbox;

namespace Extenity.BuildMachine.Editor
{

	[Serializable]
	public struct SceneProcessorConfiguration : IConsistencyChecker
	{
		public string[] IncludedCategories;
		public string[] ExcludedCategories;
		public bool DontLoadAndMergeScenes;

		public LightingConfiguration Lighting;

		#region Consistency

		public void CheckConsistency(ref List<ConsistencyError> errors)
		{
			// Included categories must be specified and empty entries are not allowed.
			if (IncludedCategories.IsNullOrEmpty() || IncludedCategories.Any(string.IsNullOrWhiteSpace))
				errors.Add(new ConsistencyError(this, $"'{nameof(IncludedCategories)}' should not be empty and empty entries are not allowed."));
			// Excluded categories are optional. But empty entries are not allowed.
			if (ExcludedCategories.IsNotNullAndEmpty())
			{
				if (ExcludedCategories.Any(string.IsNullOrWhiteSpace))
					errors.Add(new ConsistencyError(this, $"'{nameof(ExcludedCategories)}' empty entries are not allowed."));
				// Including and excluding a category at the same time is not allowed.
				var existInBoth = IncludedCategories.FirstOrDefault(ExcludedCategories.Contains);
				if (!string.IsNullOrEmpty(existInBoth))
					errors.Add(new ConsistencyError(this, $"Category '{existInBoth}' exists in both included and excluded list."));
			}

			Lighting.CheckConsistency(ref errors);
		}

		#endregion
	}

}
