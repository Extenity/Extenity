using System;
using System.Collections.Generic;
using System.Linq;
using Extenity.ConsistencyToolbox;
using Extenity.DataToolbox;

namespace Extenity.DLLBuilder
{

	/*
	[Serializable]
	public class DistributerConfiguration : IConsistencyChecker
	{
		[Serializable]
		public class DistributionTarget
		{
			public bool Enabled = true;
			public bool CopyRuntimeDLL = true;
			public bool CopyRuntimeDLLDocumentation = true;
			public bool CopyRuntimeDLLDebugDatabase = true;
			public bool CopyEditorDLL = true;
			public bool CopyEditorDLLDocumentation = true;
			public bool CopyEditorDLLDebugDatabase = true;
			public string TargetRuntimePath;
			public string TargetEditorPath = "Editor";
			public bool UseRelativeEditorDLLOutputDirectoryPath = true;
		}

		public bool Enabled = true;
		public string ConfigurationName;
		public string[] AppliedDLLs;
		public DistributionTarget[] Targets;

		#region Tools

		public bool IsAppliedToDLL(string dllName)
		{
			// We won't be checking whether user entered ".dll" at the end. So trim it.
			var dllNameProcessed = dllName.TrimEnd(".dll", StringComparison.OrdinalIgnoreCase);

			return AppliedDLLs.Any(item =>
			{
				// We won't be checking whether user entered ".dll" at the end. So trim it.
				var itemProcessed = item.TrimEnd(".dll", StringComparison.OrdinalIgnoreCase);
				return itemProcessed.Equals(dllNameProcessed, StringComparison.OrdinalIgnoreCase);
			});
		}

		#endregion

		public void CheckConsistency(ref List<ConsistencyError> errors)
		{
			if (!Enabled)
				return;

			if (string.IsNullOrEmpty(ConfigurationName))
			{
				errors.Add(new ConsistencyError(this, "Configuration Name must be specified."));
			}
			if (AppliedDLLs == null || AppliedDLLs.Length == 0)
			{
				errors.Add(new ConsistencyError(this, "There must be at least one entry in Applied DLLs."));
			}
			else
			{
				for (var i = 0; i < AppliedDLLs.Length; i++)
				{
					var appliedDLL = AppliedDLLs[i];
					if (string.IsNullOrEmpty(appliedDLL))
					{
						errors.Add(new ConsistencyError(this, string.Format("Entry at index '{0}' in Applied DLLs list is empty.", i)));
					}
				}
			}
			if (Targets == null || Targets.Length == 0)
			{
				errors.Add(new ConsistencyError(this, "There must be at least one entry in Targets."));
			}
			else
			{
				for (var i = 0; i < Targets.Length; i++)
				{
					var target = Targets[i];
					if (target.Enabled)
					{
						if (string.IsNullOrEmpty(target.TargetRuntimePath))
						{
							errors.Add(new ConsistencyError(this, string.Format("Target Runtime Path in Targets (at index '{0}') must be specified.", i)));
						}
						if (!target.UseRelativeEditorDLLOutputDirectoryPath && string.IsNullOrEmpty(target.TargetEditorPath))
						{
							errors.Add(new ConsistencyError(this, string.Format("Target Editor Path in Targets (at index '{0}') must be specified when using nonrelative path which is marked via Use Relative Editor DLL Output Directory Path.", i)));
						}
						if (
							!target.CopyRuntimeDLL &&
							!target.CopyRuntimeDLLDocumentation &&
							!target.CopyRuntimeDLLDebugDatabase &&
							!target.CopyEditorDLL &&
							!target.CopyEditorDLLDocumentation &&
							!target.CopyEditorDLLDebugDatabase)
						{
							errors.Add(new ConsistencyError(this, string.Format("At least one copy operation should be marked in Targets (at index '{0}').", i)));
						}
					}
				}
			}
		}
	}
	*/

}
