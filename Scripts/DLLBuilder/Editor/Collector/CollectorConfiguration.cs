using System;
using System.Collections.Generic;
using Extenity.ConsistencyToolbox;

namespace Extenity.DLLBuilder
{

	[Serializable]
	public class CollectorConfiguration : IConsistencyChecker
	{
		[Serializable]
		public class CollectorSource
		{
			public bool Enabled = true;
			public string SourceDirectoryPath;
			public string TargetDirectoryPath;
			//public string[] ExcludedKeywords;
		}

		public bool Enabled = true;
		public string ConfigurationName;
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
							errors.Add(new ConsistencyError(this, string.Format("Source Directory Path in Sources (at index '{0}') must be specified.", i)));
						}
						if (string.IsNullOrEmpty(source.TargetDirectoryPath))
						{
							errors.Add(new ConsistencyError(this, string.Format("Target Directory Path in Sources (at index '{0}') must be specified.", i)));
						}
					}
				}
			}
		}
	}

}
