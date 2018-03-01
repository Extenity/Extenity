using System;
using System.Collections.Generic;
using Extenity.ConsistencyToolbox;
using UnityEngine;

namespace Extenity.DLLBuilder
{

	[Serializable]
	public class RemoteBuilderConfiguration : IConsistencyChecker
	{
		[Tooltip("Allows environment variables.")]
		public string ProjectPath;
		public bool Enabled = true;
		public bool IgnoreIfNotFound = false;

		#region Consistency

		public void CheckConsistency(ref List<ConsistencyError> errors)
		{
			// We won't be doing this anymore. Instead, we won't be calling consistency checks on disabled configurations.
			//if (!Enabled)
			//	return;

			if (string.IsNullOrEmpty(ProjectPath))
			{
				errors.Add(new ConsistencyError(this, "Project Path must be specified."));
			}
			DLLBuilderConfiguration.CheckEnvironmentVariableConsistency(ProjectPath, ref errors);
		}

		#endregion
	}

}
