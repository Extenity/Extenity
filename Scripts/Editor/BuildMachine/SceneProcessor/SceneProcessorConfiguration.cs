using System;
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

		#region Consistency

		public void CheckConsistency(ConsistencyChecker checker)
		{
			// Included categories must be specified and empty entries are not allowed.
			if (IncludedCategories.IsNullOrEmpty() || IncludedCategories.Any(string.IsNullOrWhiteSpace))
				checker.AddError($"'{nameof(IncludedCategories)}' should not be empty and empty entries are not allowed.");
			// Excluded categories are optional. But empty entries are not allowed.
			if (ExcludedCategories.IsNotNullAndEmpty())
			{
				if (ExcludedCategories.Any(string.IsNullOrWhiteSpace))
					checker.AddError($"'{nameof(ExcludedCategories)}' empty entries are not allowed.");
				// Including and excluding a category at the same time is not allowed.
				var existInBoth = IncludedCategories.FirstOrDefault(ExcludedCategories.Contains);
				if (!string.IsNullOrEmpty(existInBoth))
					checker.AddError($"Category '{existInBoth}' exists in both included and excluded list.");
			}
		}

		#endregion
	}

}
