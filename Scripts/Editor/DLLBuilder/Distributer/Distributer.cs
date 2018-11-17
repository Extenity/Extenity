using System;
using System.Collections.Generic;
using System.IO;
using Extenity.ConsistencyToolbox;
using Extenity.DataToolbox;

namespace Extenity.DLLBuilder
{

	public static class Distributer
	{

		public static bool DistributeToAll(DLLBuilderConfiguration builderConfiguration)
		{
			DLLBuilder.LogAndUpdateStatus("Distributing to all targets");

			var configurations = builderConfiguration.EnabledDistributerConfigurations;
			if (configurations.IsNullOrEmpty())
			{
				DLLBuilder.LogAndUpdateStatus("Skipping distributer. Nothing to distribute.");
				return true;
			}

			for (var i = 0; i < configurations.Count; i++)
			{
				Distribute(configurations[i]);
			}

			return true;
		}

		public static void Distribute(DistributerConfiguration configuration)
		{
			DLLBuilder.LogAndUpdateStatus($"Distributing configuration '{configuration.ConfigurationName}'");

			if (!configuration.Enabled)
				throw new Exception($"Internal error. Tried to distribute using a disabled configuration '{configuration.ConfigurationName}'.");

			// Check consistency first.
			{
				var errors = new List<ConsistencyError>();
				configuration.CheckConsistency(ref errors);
				if (errors.Count > 0)
				{
					throw new Exception("Failed to distribute because of consistency errors:\n" + errors.Serialize('\n'));
				}
			}

			foreach (var target in configuration.Targets)
			{
				if (!target.Enabled)
					continue;

				var sourceDirectoryPath = DLLBuilderConfiguration.InsertEnvironmentVariables(target.SourceDirectoryPath).FixDirectorySeparatorChars('/').AddDirectorySeparatorToEnd('/');
				var targetDirectoryPath = DLLBuilderConfiguration.InsertEnvironmentVariables(target.TargetDirectoryPath).FixDirectorySeparatorChars('/').AddDirectorySeparatorToEnd('/');

				// Check that the target directory exists. We want to make sure user creates the directory first. This is more safer.
				if (!Directory.Exists(targetDirectoryPath))
				{
					throw new Exception($"Distribution target directory '{target.TargetDirectoryPath}' does not exist. This is a precaution to prevent any damage caused by misconfiguration. Please make sure the target directory is created.");
				}

				DLLBuilder.LogAndUpdateStatus($"Distributing to '{targetDirectoryPath}'");

				// TODO: Better just sync files, instead of deleting and copying from scratch.
				DirectoryTools.Delete(targetDirectoryPath);
				DirectoryTools.Copy(sourceDirectoryPath, SearchOption.AllDirectories, targetDirectoryPath, null, null, true, true, false, null);
			}
		}

	}

}
