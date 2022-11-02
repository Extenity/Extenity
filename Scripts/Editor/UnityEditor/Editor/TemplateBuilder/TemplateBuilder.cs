using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Extenity.ApplicationToolbox;
using Extenity.CompressionToolbox;
using Extenity.ConsistencyToolbox;
using Extenity.FileSystemToolbox;
using Extenity.UnityProjectTemplateToolbox.TarCs;
using UnityEditor;
using UnityEngine;

namespace Extenity.UnityProjectTemplateToolbox.Editor
{

	public static class TemplateBuilder
	{
		#region Unity Hub Project Template

		public static void BuildProjectTemplateForUnityHub(TemplateBuilderConfiguration config)
		{
			EditorApplication.delayCall += () => DoBuildProjectTemplateForUnityHub(config);
		}

		private static void DoBuildProjectTemplateForUnityHub(TemplateBuilderConfiguration config)
		{
			Log.Warning("Unity Hub Template Builder was abandoned. It should be almost fully functional in its current state, with the exception of generating 'dependencies' field in 'packages.json'. It's abandoned for multiple reasons. Unity expects the packed file to be a TGZ (Tar GZip) which only supports maximum of 100 file name lengths. Also Unity adds default Package Manager content even though the 'dependencies' field in 'package.json' says otherwise. The template package file should be copied every time switching to a new Unity version and there is no mechanism to get the latest version of templates, which makes matters worse. Hopefully Unity will address these issues in future.");

			// Consistency checks
			ConsistencyChecker.CheckConsistencyAndThrow(config);

			// Save all assets first
			AssetDatabase.SaveAssets();

			// Get files in project directory and apply Include and Ignore rules over the file list
			var files = GetProjectFilesAndApplyFilters(config);

			// Create temp directory to gather all files
			var gatherDirectory = FileUtil.GetUniqueTempPathInProject();
			const string GatherProjectDataPath = "package/ProjectData~";
			var gatherProjectDataDirectory = Path.Combine(gatherDirectory, GatherProjectDataPath);
			DirectoryTools.Create(gatherProjectDataDirectory);

			// Copy files into temp directory
			foreach (var file in files)
			{
				var path = Path.Combine(gatherProjectDataDirectory, file);
				FileTools.Copy(file, path);
			}

			// Create 'package.json'
			{
				const string PackageJsonPath = "package/package.json";
				var path = Path.Combine(gatherDirectory, PackageJsonPath);
				var json = JsonUtility.ToJson(config.Metadata, true);
				File.WriteAllText(path, json);
			}

			// Remove some data from ProjectSettings that should be automatically generated
			{
				RemoveGeneratedDataFromProjectSettings(gatherDirectory);
			}

			// Ask the developer for manual modifications before start packaging (Developer tool)
			{
				PromptUserForManualModifications(gatherDirectory);
			}

			// Create package
			{
				var outputFileName = $"{config.Metadata.name}-{config.Metadata.version}.tgz";
				var outputPath = Path.Combine(config.OutputDirectory, outputFileName).FixDirectorySeparatorCharsToBackward();
				CreateTarGz(outputPath, new DirectoryInfo(gatherDirectory));
				Log.Info($"Package for Unity Template '{config.Metadata.name}' created at path: {outputPath}");
				EditorUtility.RevealInFinder(outputPath);
			}

			// Increase version
			{
				var version = new ApplicationVersion(config.Metadata.version);
				version = version.IncrementedLower;
				config.Metadata.version = version.ToString();
				EditorUtility.SetDirty(config);
				AssetDatabase.SaveAssets();
			}
		}

		#endregion

		#region Project Template as Zip

		public static void BuildProjectTemplateAsZip(TemplateBuilderConfiguration config, bool dryRun, Action onCompleted = null)
		{
			EditorApplication.delayCall += () => DoBuildProjectTemplateAsZip(config, dryRun, onCompleted);
		}

		private static void DoBuildProjectTemplateAsZip(TemplateBuilderConfiguration config, bool dryRun, Action onCompleted)
		{
			// Consistency checks
			ConsistencyChecker.CheckConsistencyAndThrow(config);

			// Save all assets first
			AssetDatabase.SaveAssets();

			// Get files in project directory and apply Include and Ignore rules over the file list
			var files = GetProjectFilesAndApplyFilters(config);

			// Create temp directory to gather all files
			var gatherDirectory = FileUtil.GetUniqueTempPathInProject();
			DirectoryTools.Create(gatherDirectory);

			// Copy files into temp directory
			foreach (var file in files)
			{
				var path = Path.Combine(gatherDirectory, file);
				FileTools.Copy(file, path);
			}

			// Create 'package.json'
			{
				const string PackageJsonPath = "package.json";
				var path = Path.Combine(gatherDirectory, PackageJsonPath);
				var json = JsonUtility.ToJson(config.Metadata, true);
				File.WriteAllText(path, json);
			}

			// Remove some data from ProjectSettings that should be automatically generated
			{
				RemoveGeneratedDataFromProjectSettings(gatherDirectory);
			}

			// Ask the developer for manual modifications before start packaging (Developer tool)
			{
				// PromptUserForManualModifications(gatherDirectory);
			}

			// Create package
			{
				var outputFileName = $"{config.Metadata.name}-{config.Metadata.version}.zip";
				var outputPath = Path.Combine(config.OutputDirectory, outputFileName).FixDirectorySeparatorCharsToBackward();
				CreateZip(outputPath, new DirectoryInfo(gatherDirectory));
				Log.Info($"Package for Unity Project Template '{config.Metadata.name}' created at path: {outputPath}");
				EditorUtility.RevealInFinder(outputPath);
			}

			// Increase version
			if (!dryRun)
			{
				IncreaseVersion(config);
			}

			onCompleted?.Invoke();
		}

		#endregion

		#region User Prompts

		private static void PromptUserForManualModifications(string gatherDirectory)
		{
			EditorUtility.RevealInFinder(gatherDirectory.AddDirectorySeparatorToEnd().FixDirectorySeparatorChars());
			EditorUtility.DisplayDialog(
				"Waiting for manual modifications",
				"Paused before packaging.\n\nModify the files in temp directory as required. Then press 'Continue' to proceed packaging.\n\n" + gatherDirectory,
				"Continue");
		}

		#endregion

		#region Gather Files

		private static List<string> GetProjectFilesAndApplyFilters(TemplateBuilderConfiguration config)
		{
			var projectPath = ApplicationTools.ApplicationPath;
			var allFiles = Directory.GetFiles(projectPath, "*", SearchOption.AllDirectories).ToList();

			// Convert paths relative to project directory
			allFiles = allFiles.Select(file => projectPath.MakeRelativePath(file).FixDirectorySeparatorCharsToForward()).ToList();

			// Apply 'Ignore' filter
			var notIgnoredFiles = allFiles.Where(file => !config.Ignore.IsMatching(file)).ToList();

			// Apply 'Include' Filter
			var files = notIgnoredFiles.Where(file => config.Include.IsMatching(file)).ToList();

			// Log.Info($"All {allFiles.Count} files:\n" + string.Join("\n", allFiles) + "\n");
			// Log.Info($"Not Ignored {notIgnoredFiles.Count} files:\n" + string.Join("\n", notIgnoredFiles) + "\n");
			// Log.Info($"Selected {files.Count} files:\n" + string.Join("\n", files) + "\n");

			return files;
		}

		#endregion

		#region Remove ProductGUID and ProductName from ProjectSettings

		private static void RemoveGeneratedDataFromProjectSettings(string projectDirectory)
		{
			var projectSettingsAssetPath = Path.Combine(projectDirectory, ApplicationTools.UnityProjectPaths.ProjectSettingsDirectory, "ProjectSettings.asset");
			var lines = File.ReadAllLines(projectSettingsAssetPath).ToList();

			// A fresh productGUID will be generated by Unity at first launch. So it won't collide with any other
			// existing projects.
			RemoveKey(lines, "productGUID:");

			// Product name will be generated by Unity at first launch. It will be set the same as the directory name
			// of the Unity project.
			RemoveKey(lines, "productName:");

			// Remove applicationIdentifier to prevent duplicate IDs between multiple projects. It should be entered
			// from scratch in new Unity project.
			RemoveKey(lines, "applicationIdentifier:");

			File.WriteAllLines(projectSettingsAssetPath, lines);
		}

		private static void RemoveKey(List<string> lines, string key)
		{
			var index = lines.FindIndex(line => line.Contains(key));
			if (index < 0)
				throw new Exception($"Failed to find '{key}'.");
			var whitespace = GetStartingWhitespaceOfLine(lines[index]);
			lines.RemoveAt(index);
			while (lines[index][whitespace.Length] == ' ' || lines[index][whitespace.Length] == '\t')
			{
				lines.RemoveAt(index);
			}
		}

		private static string GetStartingWhitespaceOfLine(string line)
		{
			var result = "";
			for (int i = 0; i < line.Length; i++)
			{
				if (line[i] != ' ' && line[i] != '\t')
					break;

				result += line[i];
			}
			return result;
		}

		#endregion

		#region Compression

		private static void CreateTarGz(string outputPath, DirectoryInfo inputDirectory)
		{
			if (!outputPath.EndsWith(".tgz", StringComparison.OrdinalIgnoreCase))
			{
				throw new Exception("Output path should be an address of '.tgz' file.");
			}

			// Get all files in input directory
			var basePath = inputDirectory.FullName.AddDirectorySeparatorToEnd().FixDirectorySeparatorCharsToForward();
			var inputFiles = inputDirectory.GetFiles("*", SearchOption.AllDirectories);

			// Create the archive
			DirectoryTools.CreateFromFilePath(outputPath);
			using (var gzipFileStream = File.Create(outputPath))
			using (var gzipStream = new GZipStream(gzipFileStream, CompressionMode.Compress))
			using (var writer = new TarWriter(gzipStream))
			{
				foreach (var inputFile in inputFiles)
				{
					var path = inputFile.FullName;
					var localPath = basePath.MakeRelativePath(path).FixDirectorySeparatorCharsToForward();

					using (var fileStream = File.OpenRead(path))
					{
						writer.Write(fileStream, fileStream.Length, localPath, 61, 61, 511, File.GetLastWriteTime(fileStream.Name));
					}
				}
			}
		}

		private static void CreateZip(string outputPath, DirectoryInfo inputDirectory)
		{
			if (!outputPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
			{
				throw new Exception("Output path should be an address of '.zip' file.");
			}

			// Get all files in input directory
			var basePath = inputDirectory.FullName.AddDirectorySeparatorToEnd().FixDirectorySeparatorCharsToForward();
			var inputFiles = inputDirectory.GetFiles("*", SearchOption.AllDirectories)
			                               .Select(inputFile => basePath.MakeRelativePath(inputFile.FullName).FixDirectorySeparatorCharsToForward())
			                               .ToList();

			// Create the archive
			DirectoryTools.CreateFromFilePath(outputPath);
			SharpZipLibTools.CompressFiles(outputPath, 9, basePath, inputFiles, "");
		}

		#endregion

		#region Builder Version

		private static void IncreaseVersion(TemplateBuilderConfiguration config)
		{
			var version = new ApplicationVersion(config.Metadata.version);
			version = version.IncrementedLower;
			config.Metadata.version = version.ToString();
			EditorUtility.SetDirty(config);
			AssetDatabase.SaveAssets();
		}

		#endregion
	}

}
