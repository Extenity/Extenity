using System;
using System.Collections.Generic;
using Extenity.ConsistencyToolbox;

namespace Extenity.DLLBuilder
{

	[Serializable]
	public class PackingDirectoryInfo
	{
		public bool Enabled;
		public string DirectoryName;
		public string BuildEngineerNotes;
	}

	[Serializable]
	public class PackerConfiguration : IConsistencyChecker
	{
		public bool Enabled = true;
		public string ConfigurationName;
		public string SourceDirectoryPath;
		public string TargetDirectoryPath;
		public bool ExcludeScriptFiles = true;

		public PackingDirectoryInfo[] DirectoriesToCopy;

		public void CheckConsistency(ref List<ConsistencyError> errors)
		{
			// We won't be doing this anymore. Instead, we won't be calling consistency checks on disabled configurations.
			//if (!Enabled)
			//	return;

			if (string.IsNullOrEmpty(ConfigurationName))
			{
				errors.Add(new ConsistencyError(this, "Configuration Name must be specified."));
			}
			if (string.IsNullOrEmpty(SourceDirectoryPath))
			{
				errors.Add(new ConsistencyError(this, "Source Directory Path must be specified."));
			}
			if (string.IsNullOrEmpty(TargetDirectoryPath))
			{
				errors.Add(new ConsistencyError(this, "Target Directory Path must be specified."));
			}
			if (DirectoriesToCopy == null || DirectoriesToCopy.Length == 0)
			{
				errors.Add(new ConsistencyError(this, "There must be at least one entry in Directories To Copy."));
			}
			else
			{
				for (var i = 0; i < DirectoriesToCopy.Length; i++)
				{
					var directoryInfo = DirectoriesToCopy[i];
					if (directoryInfo.Enabled)
					{
						if (string.IsNullOrEmpty(directoryInfo.DirectoryName))
						{
							errors.Add(new ConsistencyError(this, "Directory Name in Directories To Copy must be specified."));
						}
					}
				}
			}
		}
	}

}
