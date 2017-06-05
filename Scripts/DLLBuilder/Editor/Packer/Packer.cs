using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extenity.ConsistencyToolbox;
using Extenity.DataToolbox;
using UnityEngine;

namespace Extenity.DLLBuilder
{

	public static class Packer
	{

		public static bool PackAll()
		{
			Debug.Log("--------- Packing all configurations");

			if (!DLLBuilderConfiguration.Instance.PackerConfigurations.Select(configuration => configuration.Enabled).Any())
			{
				Debug.Log("Skipping packer. Nothing to pack.");
				return true;
			}

			for (var i = 0; i < DLLBuilderConfiguration.Instance.PackerConfigurations.Length; i++)
			{
				var configuration = DLLBuilderConfiguration.Instance.PackerConfigurations[i];
				if (!configuration.Enabled)
					continue;
				if (!Pack(configuration))
					return false;
			}

			return true;
		}

		public static bool Pack(PackerConfiguration configuration)
		{
			Debug.LogFormat("Packing configuration '{0}'", configuration.ConfigurationName);

			if (!configuration.Enabled)
				throw new Exception(string.Format("Internal error. Tried to pack using a disabled configuration '{0}'.", configuration.ConfigurationName));

			// Check consistency first.
			{
				var errors = new List<ConsistencyError>();
				configuration.CheckConsistency(ref errors);
				if (errors.Count > 0)
				{
					Debug.LogError("Failed to pack because of consistency errors:\n" + errors.Serialize('\n'));
					return false;
				}
			}

			for (var i = 0; i < configuration.DirectoriesToCopy.Length; i++)
			{
				var directoryInfo = configuration.DirectoriesToCopy[i];
				if (!directoryInfo.Enabled)
				{
					Debug.LogWarningFormat("Skipping directory '{0}'.{1}", directoryInfo.DirectoryName,
						string.IsNullOrEmpty(directoryInfo.BuildEngineerNotes)
							? ""
							: string.Format(" Notes: '{0}'", directoryInfo.BuildEngineerNotes));
					continue;
				}
				else
				{
					Debug.LogFormat("Processing directory '{0}'.{1}", directoryInfo.DirectoryName,
						string.IsNullOrEmpty(directoryInfo.BuildEngineerNotes)
							? ""
							: string.Format(" Notes: '{0}'", directoryInfo.BuildEngineerNotes));
				}

				var sourceDirectoryPath = Path.Combine(configuration.SourceDirectoryPath, directoryInfo.DirectoryName);
				var targetDirectoryPath = Path.Combine(configuration.TargetDirectoryPath, directoryInfo.DirectoryName);

				// TODO: Better just sync files, instead of deleting and copying from scratch.
				DirectoryTools.Delete(targetDirectoryPath);
				DirectoryTools.Copy(sourceDirectoryPath, targetDirectoryPath, null, null, SearchOption.AllDirectories, true, true, false, null);
			}

			return true;
		}

	}

}
