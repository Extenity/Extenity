using System;
using System.Collections.Generic;
using System.IO;
using Extenity.ConsistencyToolbox;
using Extenity.DataToolbox;

namespace Extenity.DLLBuilder
{

	public static class Packer
	{

		public static bool PackAll()
		{
			DLLBuilder.LogAndUpdateStatus("Packing all configurations");

			var configurations = DLLBuilderConfiguration.Instance.EnabledPackerConfigurations;
			if (configurations.IsNullOrEmpty())
			{
				DLLBuilder.LogAndUpdateStatus("Skipping packer. Nothing to pack.");
				return true;
			}

			for (var i = 0; i < configurations.Count; i++)
			{
				Pack(configurations[i]);
			}

			return true;
		}

		public static void Pack(PackerConfiguration configuration)
		{
			DLLBuilder.LogAndUpdateStatus($"Packing configuration '{configuration.ConfigurationName}'");

			if (!configuration.Enabled)
				throw new Exception($"Internal error. Tried to pack using a disabled configuration '{configuration.ConfigurationName}'.");

			// Check consistency first.
			{
				var errors = new List<ConsistencyError>();
				configuration.CheckConsistency(ref errors);
				if (errors.Count > 0)
				{
					throw new Exception("Failed to pack because of consistency errors:\n" + errors.Serialize('\n'));
				}
			}

			for (var i = 0; i < configuration.DirectoriesToCopy.Length; i++)
			{
				var directoryInfo = configuration.DirectoriesToCopy[i];
				if (!directoryInfo.Enabled)
				{
					Log.Warning($"Skipping directory '{directoryInfo.DirectoryName}'.{(string.IsNullOrEmpty(directoryInfo.BuildEngineerNotes) ? "" : $" Notes: '{directoryInfo.BuildEngineerNotes}'")}");
					continue;
				}
				else
				{
					Log.Info($"Processing directory '{directoryInfo.DirectoryName}'.{(string.IsNullOrEmpty(directoryInfo.BuildEngineerNotes) ? "" : $" Notes: '{directoryInfo.BuildEngineerNotes}'")}");
				}

				var sourceDirectoryPath = Path.Combine(DLLBuilderConfiguration.InsertEnvironmentVariables(configuration.SourceDirectoryPath), directoryInfo.DirectoryName);
				var targetDirectoryPath = Path.Combine(DLLBuilderConfiguration.InsertEnvironmentVariables(configuration.TargetDirectoryPath), directoryInfo.DirectoryName);

				// TODO: Better just sync files, instead of deleting and copying from scratch.
				DirectoryTools.Delete(targetDirectoryPath);
				var excludeFilters = configuration.ExcludeScriptFiles
					? new[] { "*.cs", "*.cs.meta" }
					: null;
				DirectoryTools.Copy(sourceDirectoryPath, SearchOption.AllDirectories, targetDirectoryPath, null, excludeFilters, true, true, false, null);
			}
		}

	}

}
