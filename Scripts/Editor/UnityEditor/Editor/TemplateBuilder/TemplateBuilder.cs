using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Extenity.ApplicationToolbox;
using Extenity.ApplicationToolbox.Editor;
using Extenity.ConsistencyToolbox;
using Extenity.FileSystemToolbox;
using Extenity.UnityProjectTemplateToolbox.TarCs;
using UnityEditor;
using UnityEngine;

namespace Extenity.UnityProjectTemplateToolbox.Editor
{

	public static class TemplateBuilder
	{
		#region Process

		public static void BuildTemplate(TemplateBuilderConfiguration config)
		{
			EditorApplication.delayCall += () => DoBuildTemplate(config);
		}

		private static void DoBuildTemplate(TemplateBuilderConfiguration config)
		{
			Log.Warning("Template Builder was abandoned. It should be almost fully functional in its current state, with the exception of generating 'dependencies' field in 'packages.json'. It's abandoned for multiple reasons. Unity expects the packed file to be a TGZ (Tar GZip) which only supports maximum of 100 file name lengths. Also Unity adds default Package Manager content even though the 'dependencies' field in 'package.json' says otherwise. The template package file should be copied every time switching to a new Unity version and there is no mechanism to get the latest version of templates, which makes matters worse. Hopefully Unity will address these issues in future.");

			// Consistency checks
			config.CheckConsistencyAndThrow("Template Builder Configuration contains errors:");

			// Save all assets first
			AssetDatabase.SaveAssets();

			// Get all files in project directory
			var projectPath = EditorApplicationTools.UnityProjectPath;
			var allFiles = Directory.GetFiles(projectPath, "*", SearchOption.AllDirectories).ToList();
			allFiles = allFiles.Select(file => projectPath.MakeRelativePath(file).FixDirectorySeparatorCharsToForward()).ToList();

			// Apply 'Ignore' filter
			var ignoreFilter = config.Ignore;
			allFiles = allFiles.Where(file => !ignoreFilter.IsMatching(file)).ToList();

			// Apply 'Include' Filter
			var includeFilter = config.Include;
			var files = allFiles.Where(file => includeFilter.IsMatching(file)).ToList();

			// Log.Info($"All {allFiles.Count()} files:\n" + string.Join("\n", allFiles) + "\n");
			// Log.Info($"Selected {files.Count()} files:\n" + string.Join("\n", files) + "\n");

			// Create temp directory to gather all files
			var gatherDirectory = FileUtil.GetUniqueTempPathInProject();
			const string GatherProjectDataPath = "package/ProjectData~";
			var gatherProjectDataDirectory = Path.Combine(gatherDirectory, GatherProjectDataPath);
			DirectoryTools.Create(gatherProjectDataDirectory);

			// Copy files into temp directory
			foreach (var file in files)
			{
				var path = Path.Combine(gatherProjectDataDirectory, file);
				DirectoryTools.CreateFromFilePath(path);
				File.Copy(file, path);
			}

			// Create 'package.json'
			{
				const string PackageJsonPath = "package/package.json";
				var path = Path.Combine(gatherDirectory, PackageJsonPath);
				var json = JsonUtility.ToJson(config.Metadata, true);
				File.WriteAllText(path, json);
			}

			// Ask the user for manual modifications before start packaging
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
				version = version.IncrementedBuild;
				config.Metadata.version = version.ToString();
				EditorUtility.SetDirty(config);
				AssetDatabase.SaveAssets();
			}
		}

		private static void PromptUserForManualModifications(string gatherDirectory)
		{
			EditorUtility.RevealInFinder(gatherDirectory.AddDirectorySeparatorToEnd().FixDirectorySeparatorChars());
			EditorUtility.DisplayDialog(
				"Waiting for manual modifications",
				"Paused before packaging.\n\nModify the files in temp directory as required. Then press 'Continue' to proceed packaging.\n\n" + gatherDirectory,
				"Continue");
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

			// Create tgz
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

		#endregion
	}

}
