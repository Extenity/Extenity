﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Extenity.ConsistencyToolbox;
using Extenity.DataToolbox;
using Extenity.ParallelToolbox.Editor;

namespace Extenity.DLLBuilder
{

	public static class Collector
	{

		public static void GatherDependenciesFromAll(DLLBuilderConfiguration builderConfiguration, BuildJob job, Action onSucceeded, Action<string> onFailed)
		{
			DLLBuilder.LogAndUpdateStatus("Gathering dependencies from all sources");

			if (job.CurrentlyProcessedProjectStatus.IsCollectorCompleted)
			{
				DLLBuilder.LogAndUpdateStatus("Skipping collector. Already completed.");
				//thisProjectStatus.IsCollectorCompleted = true; Already true. But we will keep it here to remind that we need to set this to true when successfully finishing this process.
				if (onSucceeded != null)
					onSucceeded();
				return;
			}

			var configurations = builderConfiguration.EnabledCollectorConfigurations;
			if (configurations.IsNullOrEmpty())
			{
				DLLBuilder.LogAndUpdateStatus("Skipping collector. Nothing to collect.");
				if (onSucceeded != null)
					onSucceeded();
				return;
			}

			InternalGatherDependencies(configurations, job, onSucceeded, onFailed).StartCoroutineInEditorUpdate();
		}

		public static IEnumerator InternalGatherDependencies(List<CollectorConfiguration> configurations, BuildJob job, Action onSucceeded, Action<string> onFailed)
		{
			for (var i = 0; i < configurations.Count; i++)
			{
				var configuration = configurations[i];
				DLLBuilder.LogAndUpdateStatus("Gathering dependencies for configuration '{0}'", configuration.ConfigurationName);

				if (!configuration.Enabled)
				{
					if (onFailed != null)
						onFailed($"Internal error. Tried to gather dependencies of a disabled configuration '{configuration.ConfigurationName}'.");
					yield break;
				}

				// Check consistency first.
				{
					var errors = new List<ConsistencyError>();
					configuration.CheckConsistency(ref errors);
					if (errors.Count > 0)
					{
						if (onFailed != null)
							onFailed(string.Format("Failed to gather dependencies because of consistency errors:\n" + errors.Serialize('\n')));
						yield break;
					}
				}

				foreach (var source in configuration.Sources)
				{
					if (!source.Enabled)
						continue;

					var sourceDirectoryPath = DLLBuilderConfiguration.InsertEnvironmentVariables(source.SourceDirectoryPath).FixDirectorySeparatorChars('/').AddDirectorySeparatorToEnd('/');
					var targetDirectoryPath = DLLBuilderConfiguration.InsertEnvironmentVariables(source.TargetDirectoryPath).FixDirectorySeparatorChars('/').AddDirectorySeparatorToEnd('/');

					// Check that the source directory exists.
					if (!Directory.Exists(sourceDirectoryPath))
					{
						if (onFailed != null)
							onFailed($"Collector source directory '{source.SourceDirectoryPath}' does not exist.");
						yield break;
					}

					// Check that the target directory exists. We want to make sure user creates the directory first. This is more safer.
					if (!Directory.Exists(targetDirectoryPath))
					{
						if (onFailed != null)
							onFailed($"Collector target directory '{source.TargetDirectoryPath}' does not exist. Please make sure the target directory is created. This is a precaution to prevent any damage caused by misconfiguration.");
						yield break;
					}

					DLLBuilder.LogAndUpdateStatus("Gathering dependencies for '{0}'", targetDirectoryPath);

					// TODO: Better just sync files, instead of deleting and copying from scratch.
					DirectoryTools.Delete(targetDirectoryPath);
					DirectoryTools.Copy(sourceDirectoryPath, SearchOption.AllDirectories, targetDirectoryPath, null, null, true, true, false, null);
				}
			}

			DLLBuilder.LogAndUpdateStatus("Marking collector completion in project status");
			job.CurrentlyProcessedProjectStatus.IsCollectorCompleted = true;

			// Recompile this project. Because we probably got new DLLs.
			DLLBuilder.ReloadAssemblies(job, onSucceeded).StartCoroutineInEditorUpdate();
		}

	}

}