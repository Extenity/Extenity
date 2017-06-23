using System;
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

			if (job.CurrentlyProcessedProjectStatus.IsRemoteBuildsCompleted)
			{
				DLLBuilder.LogAndUpdateStatus("Skipping collector. Already completed.");
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

			for (var i = 0; i < configurations.Count; i++)
			{
				InternalGatherDependencies(configurations[i], job, onSucceeded, onFailed).StartCoroutineInEditorUpdate();
			}
		}

		public static IEnumerator InternalGatherDependencies(CollectorConfiguration configuration, BuildJob job, Action onSucceeded, Action<string> onFailed)
		{
			DLLBuilder.LogAndUpdateStatus("Gathering dependencies for configuration '{0}'", configuration.ConfigurationName);

			if (!configuration.Enabled)
			{
				if (onFailed != null)
					onFailed(string.Format("Internal error. Tried to gather dependencies of a disabled configuration '{0}'.", configuration.ConfigurationName));
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

				var sourceDirectoryPath = source.SourceDirectoryPath.FixDirectorySeparatorChars('/').AddDirectorySeparatorToEnd('/');
				var targetDirectoryPath = source.TargetDirectoryPath.FixDirectorySeparatorChars('/').AddDirectorySeparatorToEnd('/');

				// Check that the source directory exists.
				if (!Directory.Exists(sourceDirectoryPath))
				{
					if (onFailed != null)
						onFailed(string.Format("Collector source directory '{0}' does not exist.", source.SourceDirectoryPath));
					yield break;
				}

				// Check that the target directory exists. We want to make sure user creates the directory first. This is more safer.
				if (!Directory.Exists(targetDirectoryPath))
				{
					if (onFailed != null)
						onFailed(string.Format("Collector target directory '{0}' does not exist. Please make sure the target directory is created. This is a precaution to prevent any damage caused by misconfiguration.", source.TargetDirectoryPath));
					yield break;
				}

				DLLBuilder.LogAndUpdateStatus("Gathering dependencies for '{0}'", targetDirectoryPath);

				// TODO: Better just sync files, instead of deleting and copying from scratch.
				DirectoryTools.Delete(targetDirectoryPath);
				DirectoryTools.Copy(sourceDirectoryPath, SearchOption.AllDirectories, targetDirectoryPath, null, null, true, true, false, null);
			}

			DLLBuilder.UpdateStatus("Marking collector completion in project status");
			job.CurrentlyProcessedProjectStatus.IsCollectorCompleted = true;

			// Recompile this project. Because we probably got new DLLs.
			DLLBuilder.ReloadAssemblies(job, onSucceeded).StartCoroutineInEditorUpdate();
		}

	}

}
