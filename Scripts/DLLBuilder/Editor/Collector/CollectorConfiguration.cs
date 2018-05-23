using System;
using System.Collections.Generic;
using Extenity.ConsistencyToolbox;
using UnityEngine;

namespace Extenity.DLLBuilder
{

	[Serializable]
	public class CollectorConfiguration : IConsistencyChecker
	{
		[Serializable]
		public class CollectorSource
		{
			public bool Enabled = true;
			[Tooltip("Allows environment variables.")]
			public string SourceDirectoryPath;
			[Tooltip("Allows environment variables.")]
			public string TargetDirectoryPath;
			//public string[] ExcludedKeywords;
		}

		[Header("Meta")]
		public string ConfigurationName;
		public bool Enabled = true;

		[Header("Sources")]
		public CollectorSource[] Sources;

		public void CheckConsistency(ref List<ConsistencyError> errors)
		{
			// We won't be doing this anymore. Instead, we won't be calling consistency checks on disabled configurations.
			//if (!Enabled)
			//	return;

			if (string.IsNullOrEmpty(ConfigurationName))
			{
				errors.Add(new ConsistencyError(this, "Configuration Name must be specified."));
			}
			if (Sources == null || Sources.Length == 0)
			{
				errors.Add(new ConsistencyError(this, "There must be at least one entry in Sources."));
			}
			else
			{
				for (var i = 0; i < Sources.Length; i++)
				{
					var source = Sources[i];
					if (source.Enabled)
					{
						if (string.IsNullOrEmpty(source.SourceDirectoryPath))
						{
							errors.Add(new ConsistencyError(this, $"Source Directory Path in Sources (at index '{i}') must be specified."));
						}
						DLLBuilderConfiguration.CheckEnvironmentVariableConsistency(source.SourceDirectoryPath, ref errors);

						if (string.IsNullOrEmpty(source.TargetDirectoryPath))
						{
							errors.Add(new ConsistencyError(this, $"Target Directory Path in Sources (at index '{i}') must be specified."));
						}
						DLLBuilderConfiguration.CheckEnvironmentVariableConsistency(source.TargetDirectoryPath, ref errors);
					}
				}
			}
		}
	}

}
