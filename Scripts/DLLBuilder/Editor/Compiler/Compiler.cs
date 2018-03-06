//#define EnableNoStdLib

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Extenity.ConsistencyToolbox;
using Extenity.DataToolbox;
using Extenity.DebugToolbox;
using Extenity.ParallelToolbox.Editor;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Extenity.DLLBuilder
{

	public static class Compiler
	{

		public static void CompileAllDLLs(DLLBuilderConfiguration builderConfiguration, Action onSucceeded, Action<string> onFailed)
		{
			DLLBuilder.LogAndUpdateStatus("Compiling all DLLs");

			InternalCompileDLLs(builderConfiguration, onSucceeded, onFailed).StartCoroutineInEditorUpdate();
		}

		private static IEnumerator InternalCompileDLLs(DLLBuilderConfiguration builderConfiguration, Action onSucceeded, Action<string> onFailed)
		{
			var configurations = builderConfiguration.EnabledCompilerConfigurations;

			if (configurations.IsNullOrEmpty())
			{
				if (onFailed != null)
					onFailed(string.Format("DLL Builder configuration does not have any entries. Please check your configuration at path '{0}'.", AssetDatabase.GetAssetPath(builderConfiguration)));
				yield break;
			}

			// Check for consistency first
			foreach (var configuration in configurations)
			{
				var errors = configuration.CheckConsistencyAndLog("There are inconsistencies in DLL compilation configurations.");
				if (errors.Count > 0)
				{
					if (onFailed != null)
						onFailed(string.Format("Failed to initialize compilation of DLL '{0}'.", configuration.DLLNameWithoutExtension));
					yield break;
				}
			}

			foreach (var configuration in configurations)
			{
				CompilerJob job = null;

				// Initialization
				try
				{
					job = new CompilerJob(configuration);
					DLLBuilder.LogAndUpdateStatus("Compiling '{0}'", job.Configuration.DLLNameWithoutExtension);

					job.UnityVersion = Application.unityVersion;
					DLLBuilderTools.DetectUnityReferences(ref job.UnityManagedReferences);
					GenerateExportedFiles(job.Configuration.ProcessedSourcePath, false, ref job.SourceFilePathsForRuntimeDLL, job.Configuration.ExcludedKeywords);
					GenerateExportedFiles(job.Configuration.ProcessedSourcePath, true, ref job.SourceFilePathsForEditorDLL, job.Configuration.ExcludedKeywords);

					if (!job.AnySourceFiles)
					{
						throw new Exception("There are no source files to compile.");
					}
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
					if (onFailed != null)
						onFailed(string.Format("Failed initialize compilation of DLL '{0}'.", job.Configuration.DLLNameWithoutExtension));
					yield break;
				}

				// Compile runtime DLL
				try
				{
					AsyncGenerateRuntimeDLL(job);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
					if (onFailed != null)
						onFailed(string.Format("Failed to start compilation of runtime DLL '{0}'.", job.Configuration.DLLNameWithoutExtension));
					yield break;
				}

				// Wait until compilation finishes
				while (!job.RuntimeDLLFinished)
					yield return null;

				if (job.RuntimeDLLSucceeded == CompileResult.Failed)
				{
					if (onFailed != null)
						onFailed(string.Format("Failed to compile runtime DLL '{0}'.", job.Configuration.DLLNameWithoutExtension));
					yield break;
				}

				// Obfuscate (right after compilation and before using the DLL in any other place)
				if (job.Configuration.ObfuscateRuntimeDLL && job.RuntimeDLLSucceeded == CompileResult.Succeeded)
				{
					try
					{
						ObfuscationLauncher.Obfuscate(job.Configuration.DLLPath);
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						if (onFailed != null)
							onFailed(string.Format("Failed to obfuscate runtime DLL '{0}'.", job.Configuration.DLLNameWithoutExtension));
						yield break;
					}
					// Wait for a couple of frames for obfuscation result to settle down. This is not required. Just a precaution.
					for (int i = 0; i < 5; i++)
						yield return null;
				}

				// Compile editor DLL
				if (job.RuntimeDLLSucceeded == CompileResult.Succeeded || job.RuntimeDLLSucceeded == CompileResult.Skipped)
				{
					try
					{
						AsyncGenerateEditorDLL(job);
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						if (onFailed != null)
							onFailed(string.Format("Failed to start compilation of editor DLL '{0}'.", job.Configuration.DLLNameWithoutExtension));
						yield break;
					}

					// Wait until compilation finishes
					while (!job.EditorDLLFinished)
						yield return null;

					if (job.EditorDLLSucceeded == CompileResult.Failed)
					{
						if (onFailed != null)
							onFailed(string.Format("Failed to compile editor DLL '{0}'.", job.Configuration.DLLNameWithoutExtension));
						yield break;
					}

					// Obfuscate (right after compilation and before using the DLL in any other place)
					if (job.Configuration.ObfuscateEditorDLL && job.EditorDLLSucceeded == CompileResult.Succeeded)
					{
						try
						{
							ObfuscationLauncher.Obfuscate(job.Configuration.EditorDLLPath);
						}
						catch (Exception exception)
						{
							Debug.LogException(exception);
							if (onFailed != null)
								onFailed(string.Format("Failed to obfuscate editor DLL '{0}'.", job.Configuration.DLLNameWithoutExtension));
							yield break;
						}
						// Wait for a couple of frames for obfuscation result to settle down. This is not required. Just a precaution.
						for (int i = 0; i < 5; i++)
							yield return null;
					}
				}
			}

			if (onSucceeded != null)
				onSucceeded();
		}

		private static void AsyncGenerateRuntimeDLL(CompilerJob job)
		{
			var thread = new Thread(GenerateRuntimeDLL);
			thread.Name = "Generate runtime " + job.Configuration.DLLNameWithoutExtension;
			thread.Start(new object[] { job });
		}

		private static void AsyncGenerateEditorDLL(CompilerJob job)
		{
			var thread = new Thread(GenerateEditorDLL);
			thread.Name = "Generate editor " + job.Configuration.DLLNameWithoutExtension;
			thread.Start(new object[] { job });
		}

		private static void GenerateRuntimeDLL(object data)
		{
			GenerateRuntimeDLL((CompilerJob)((object[])data)[0]);
		}

		private static void GenerateEditorDLL(object data)
		{
			GenerateEditorDLL((CompilerJob)((object[])data)[0]);
		}

		private static void GenerateRuntimeDLL(CompilerJob job)
		{
			try
			{
				if (job.AnyRuntimeSourceFiles)
				{
					CopySourcesToTemporaryDirectory(job.SourceFilePathsForRuntimeDLL, job.Configuration.ProcessedSourcePath, job.Configuration.ProcessedIntermediateSourceDirectoryPath);
					job.RuntimeDLLSucceeded = CompileDLL(job.Configuration.ProcessedIntermediateSourceDirectoryPath, false, job);
					DirectoryTools.Delete(job.Configuration.ProcessedIntermediateSourceDirectoryPath);
				}
				else
				{
					// Skip
					Cleaner.ClearOutputDLLs(job.Configuration, true, true); // We still need to clear previous outputs in case there are any left from previous builds.
					DLLBuilder.LogAndUpdateStatus("Skipping runtime DLL build. No scripts to compile.");
					job.RuntimeDLLSucceeded = CompileResult.Skipped;
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}

			job.RuntimeDLLFinished = true;

			//EditorApplication.delayCall += () => { InternalOnFinished(job); }; This causes hanging sometimes. Better not to use it in thread switching.
		}

		private static void GenerateEditorDLL(CompilerJob job)
		{
			try
			{
				if (job.AnyEditorSourceFiles)
				{
					CopySourcesToTemporaryDirectory(job.SourceFilePathsForEditorDLL, job.Configuration.ProcessedSourcePath, job.Configuration.ProcessedIntermediateSourceDirectoryPath);
					job.EditorDLLSucceeded = CompileDLL(job.Configuration.ProcessedIntermediateSourceDirectoryPath, true, job);
					DirectoryTools.Delete(job.Configuration.ProcessedIntermediateSourceDirectoryPath);
				}
				else
				{
					// Skip
					Cleaner.ClearOutputDLLs(job.Configuration, false, true); // We still need to clear previous outputs in case there are any left from previous builds.
					DLLBuilder.LogAndUpdateStatus("Skipping editor DLL build. No scripts to compile.");
					job.EditorDLLSucceeded = CompileResult.Skipped;
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}

			job.EditorDLLFinished = true;

			//EditorApplication.delayCall += () => { InternalOnFinished(job); }; This causes hanging sometimes. Better not to use it in thread switching.
		}

		private static void CopySourcesToTemporaryDirectory(List<string> sourceFilePaths, string sourceBasePath, string temporaryDirectoryPath)
		{
			DLLBuilder.LogAndUpdateStatus("Copying sources into temporary directory '{0}'.", temporaryDirectoryPath);
			sourceBasePath = sourceBasePath.AddDirectorySeparatorToEnd().FixDirectorySeparatorChars('/');

			DirectoryTools.Delete(temporaryDirectoryPath);

			for (int i = 0; i < sourceFilePaths.Count; i++)
			{
				if (!sourceFilePaths[i].StartsWith(sourceBasePath))
					throw new Exception(string.Format("Source path '{0}' does not start with base path '{1}'.", sourceFilePaths[i], sourceBasePath));

				var relativeDestPath = sourceFilePaths[i].Substring(sourceBasePath.Length).FixDirectorySeparatorChars();
				if (relativeDestPath[0].IsDirectorySeparatorChar())
					relativeDestPath = relativeDestPath.Substring(1);
				var absoluteDestPath = Path.Combine(temporaryDirectoryPath, relativeDestPath);

				Directory.CreateDirectory(Path.GetDirectoryName(absoluteDestPath));

				File.Copy(sourceFilePaths[i], absoluteDestPath);
			}
		}

		private static CompileResult CompileDLL(string sourcePath, bool isEditorBuild, CompilerJob job)
		{
			try
			{
				// This will clear:
				// > both editor and runtime DLLs if we are building runtime DLL (so that editor DLL won't get outdated because of overwritten runtime DLL that was added to it's references)
				// > only editor DLL if we are building editor DLL
				Cleaner.ClearOutputDLLs(job.Configuration, !isEditorBuild, true);

				Directory.CreateDirectory(isEditorBuild ? job.Configuration.ProcessedEditorDLLOutputDirectoryPath : job.Configuration.ProcessedDLLOutputDirectoryPath);

				var defines = isEditorBuild
					? job.Configuration.EditorDefinesAsString
					: job.Configuration.RuntimeDefinesAsString;

				// Append Unity version directives.
				if (job.Configuration.IncludeUnityVersion)
				{
					var unityVersionDefines = DLLBuilderTools.GetUnityVersionDefines(job.UnityVersion);
					defines += string.IsNullOrEmpty(defines)
						? unityVersionDefines
						: ";" + unityVersionDefines;
				}

				var dllOutputPath = isEditorBuild
					? job.Configuration.EditorDLLPath
					: job.Configuration.DLLPath;

				var documentationOutputPath = isEditorBuild
					? job.Configuration.EditorDLLDocumentationPath
					: job.Configuration.DLLDocumentationPath;

				var debugDatabaseOutputPath = isEditorBuild
					? job.Configuration.EditorDLLDebugDatabasePath
					: job.Configuration.DLLDebugDatabasePath;

				// Create references list
				var allReferences = new List<string>();
				{
					//if (job.Configuration.AddAllDLLsInUnityManagedDirectory)
					//	for (int i = 0; i < job.UnityManagedReferences.Count; i++)
					//		allReferences.AddIfDoesNotContain(job.UnityManagedReferences[i].Trim()); // Directory separators already fixed.

					if (job.Configuration.RuntimeReferences != null)
						for (int i = 0; i < job.Configuration.RuntimeReferences.Length; i++)
							allReferences.AddIfDoesNotContain(DLLBuilderConfiguration.InsertEnvironmentVariables(job.Configuration.RuntimeReferences[i].Trim()).FixDirectorySeparatorChars()); // Fix directory separators because these are entered by user.

					if (isEditorBuild && job.Configuration.EditorReferences != null)
						for (int i = 0; i < job.Configuration.EditorReferences.Length; i++)
							allReferences.AddIfDoesNotContain(DLLBuilderConfiguration.InsertEnvironmentVariables(job.Configuration.EditorReferences[i].Trim()).FixDirectorySeparatorChars()); // Fix directory separators because these are entered by user.

					if (isEditorBuild && job.Configuration.AddRuntimeDLLReferenceIntoEditorDLL)
					{
						if (!File.Exists(job.Configuration.DLLPath))
							throw new Exception("Tried to add runtime DLL reference into editor DLL but runtime DLL does not exists.");
						allReferences.AddIfDoesNotContain(job.Configuration.DLLPath.Trim()); // Directory separators already fixed.
					}

					if (job.Configuration.AddUnityEngineDLLReferenceIntoRuntimeAndEditorDLLs)
						allReferences.AddIfDoesNotContain(job.UnityManagedReferences.First(item => item.EndsWith("UnityEngine.dll", StringComparison.OrdinalIgnoreCase))); // Directory separators already fixed.

					if (isEditorBuild && job.Configuration.AddUnityEditorDLLReferenceIntoEditorDLL)
						allReferences.AddIfDoesNotContain(job.UnityManagedReferences.First(item => item.EndsWith("UnityEditor.dll", StringComparison.OrdinalIgnoreCase))); // Directory separators already fixed.

					allReferences.Sort();
					allReferences.RemoveAll(string.IsNullOrEmpty);
				}

				var arguments = new List<string>(100);


				string compilerPath;

				// Initialize compiler arguments
				switch (job.Configuration.Compiler)
				{
					case CompilerType.Gmcs:
						{
							var gmcsPath = @"C:\Program Files\Unity\Editor\Data\Mono\lib\mono\2.0\gmcs.exe";
							compilerPath = @"C:\Program Files\Unity\Editor\Data\Mono\bin\mono.exe";

							arguments.Add("\"" + gmcsPath + "\"");
							arguments.Add("/target:library");

							if (job.Configuration.GenerateDocumentation)
							{
								arguments.Add("/doc:\"" + documentationOutputPath + "\"");
							}

							if (job.Configuration.GenerateDebugInfo)
							{
								arguments.Add("/debug+ /debug:full");
							}
							else
							{
								arguments.Add("/debug-");
							}

							arguments.Add("/optimize+");
							arguments.Add("/nowarn:1591,1573");
#if EnableNoStdLib
							arguments.Add("/nostdlib+");
#endif
							arguments.Add("/warnaserror");

							if (job.Configuration.Unsafe)
							{
								arguments.Add("/unsafe");
							}

							arguments.Add("/out:\"" + dllOutputPath + "\"");

							if (!string.IsNullOrEmpty(defines))
							{
								arguments.Add("/define:" + defines);
							}

							foreach (var reference in allReferences.Where(reference => !string.IsNullOrEmpty(reference)))
								arguments.Add("/reference:\"" + reference + "\"");

							arguments.Add("/recurse:\"" + Path.Combine(sourcePath, "*.cs") + "\"");
						}
						break;

					case CompilerType.Smcs:
						{
							var smcsPath = @"C:\Program Files\Unity\Editor\Data\Mono\lib\mono\unity\smcs.exe";
							compilerPath = @"C:\Program Files\Unity\Editor\Data\Mono\bin\mono.exe";

							arguments.Add("\"" + smcsPath + "\"");
							arguments.Add("/target:library");

							if (job.Configuration.GenerateDocumentation)
							{
								arguments.Add("/doc:\"" + documentationOutputPath + "\"");
							}

							if (job.Configuration.GenerateDebugInfo)
							{
								arguments.Add("/debug+ /debug:full");
							}
							else
							{
								arguments.Add("/debug-");
							}

							arguments.Add("/optimize+");
							arguments.Add("/nowarn:1591,1573");
#if EnableNoStdLib
							arguments.Add("/nostdlib+");
#endif
							arguments.Add("/warnaserror");

							if (job.Configuration.Unsafe)
							{
								arguments.Add("/unsafe");
							}

							arguments.Add("/out:\"" + dllOutputPath + "\"");

							if (!string.IsNullOrEmpty(defines))
							{
								arguments.Add("/define:" + defines);
							}

							foreach (var reference in allReferences.Where(reference => !string.IsNullOrEmpty(reference)))
								arguments.Add("/reference:\"" + reference + "\"");

							arguments.Add("/recurse:\"" + Path.Combine(sourcePath, "*.cs") + "\"");
						}
						break;

					case CompilerType.McsBleedingEdge:
						{
							var mcsPath = @"C:\Program Files\Unity\Editor\Data\MonoBleedingEdge\lib\mono\4.5\mcs.exe";
							compilerPath = @"C:\Program Files\Unity\Editor\Data\MonoBleedingEdge\bin\mono.exe";

							arguments.Add("\"" + mcsPath + "\"");
							arguments.Add("/target:library");
							arguments.Add("/sdk:4.5");

							if (job.Configuration.GenerateDocumentation)
							{
								arguments.Add("/doc:\"" + documentationOutputPath + "\"");
							}

							if (job.Configuration.GenerateDebugInfo)
							{
								arguments.Add("/debug+ /debug:full");
							}
							else
							{
								arguments.Add("/debug-");
							}

							arguments.Add("/optimize+");
							arguments.Add("/nowarn:1591,1573");
#if EnableNoStdLib
							arguments.Add("/nostdlib+");
#endif
							arguments.Add("/warnaserror");

							if (job.Configuration.Unsafe)
							{
								arguments.Add("/unsafe");
							}

							arguments.Add("/out:\"" + dllOutputPath + "\"");

							if (!string.IsNullOrEmpty(defines))
							{
								arguments.Add("/define:" + defines);
							}

							foreach (var reference in allReferences.Where(reference => !string.IsNullOrEmpty(reference)))
								arguments.Add("/reference:\"" + reference + "\"");

							arguments.Add("/recurse:\"" + Path.Combine(sourcePath, "*.cs") + "\"");
						}
						break;

					case CompilerType.MSBuild:
						{
							if (job.Configuration.GenerateDocumentation == true)
							{
								arguments.Add("/doc:\"" + documentationOutputPath + "\"");
							}

							if (job.Configuration.GenerateDebugInfo == true)
							{
								arguments.Add("/debug+ /debug:full");
							}
							else
							{
								arguments.Add("/debug-");
							}

							arguments.Add("/noconfig");
							arguments.Add("/nowarn:1591,1573");
							arguments.Add("/warnaserror");
#if EnableNoStdLib
							arguments.Add("/nostdlib+");
#endif
							arguments.Add("/nologo");
							arguments.Add("/platform:AnyCPU");
							arguments.Add("/errorreport:none");
							//arguments.Add("/warn:0");
							//arguments.Add("/filealign:512");

							if (job.Configuration.Unsafe)
							{
								arguments.Add("/unsafe");
							}

							if (!string.IsNullOrEmpty(defines))
							{
								arguments.Add("/define:" + defines);
							}

							//arguments.Add("/errorendlocation");
							//arguments.Add("/preferreduilang:en-US");
							//arguments.Add("/highentropyva-");

							foreach (var reference in allReferences.Where(reference => !string.IsNullOrEmpty(reference)))
								arguments.Add("/reference:\"" + reference + "\"");

							arguments.Add("/out:\"" + dllOutputPath + "\"");
							arguments.Add("/target:library");
							arguments.Add("/utf8output");
							arguments.Add("/recurse:\"" + Path.Combine(sourcePath, "*.cs") + "\"");

							compilerPath = DLLBuilderTools.AutoDetectCSCPath();
							//compilerPath = DLLBuilderTools.CSCPath;
						}
						break;

					default:
						throw new ArgumentOutOfRangeException();
				}


				arguments.LogList(string.Format("Launching compiler '{0}' with arguments:", job.Configuration.Compiler.ToString()));

				var process = new Process();
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardError = true;
				process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				process.StartInfo.CreateNoWindow = true;
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.FileName = compilerPath;
				process.StartInfo.Arguments = arguments.Serialize(' ');

				if (!process.Start())
				{
					DLLBuilder.LogErrorAndUpdateStatus("Compiler process stopped with code " + process.ExitCode + ".");
					return CompileResult.Failed;
				}

				var stdoutOutput = process.StandardOutput.ReadToEnd().Trim();
				var stderrOutput = process.StandardError.ReadToEnd().Trim();
				process.WaitForExit();

				if (!string.IsNullOrEmpty(stdoutOutput))
					Debug.Log("Stdout: " + stdoutOutput);
				if (!string.IsNullOrEmpty(stderrOutput))
					Debug.LogError("Stderr: " + stderrOutput);

				if (process.ExitCode == 0)
				{
					// Make sure files are generated by compiler
					{
						if (!File.Exists(dllOutputPath))
						{
							Debug.LogErrorFormat("Compiler returned no errors but output file '{0}' does not exist.", documentationOutputPath);
							return CompileResult.Failed;
						}
						if (job.Configuration.GenerateDocumentation && !File.Exists(documentationOutputPath))
						{
							Debug.LogErrorFormat("Compiler returned no errors but output file '{0}' does not exist.", documentationOutputPath);
							return CompileResult.Failed;
						}
						if (job.Configuration.GenerateDebugInfo && !File.Exists(debugDatabaseOutputPath))
						{
							Debug.LogErrorFormat("Compiler returned no errors but output file '{0}' does not exist.", debugDatabaseOutputPath);
							return CompileResult.Failed;
						}
					}

					DLLBuilder.LogAndUpdateStatus("Finished compiling '{0}'.", isEditorBuild ? job.Configuration.EditorDLLName : job.Configuration.DLLName);

					// Make sure meta files exist
					{
						if (!CheckIfMetaFileExists(dllOutputPath))
						{
							DLLBuilder.LogErrorAndUpdateStatus("Meta file does not exist for file '{0}'. You should probably be using an outside Unity project to generate these meta files for you. Meta file generation responsibility left to the user since it's a one time operation.", dllOutputPath);
							return CompileResult.Failed;
						}
						if (job.Configuration.GenerateDocumentation && !CheckIfMetaFileExists(documentationOutputPath))
						{
							DLLBuilder.LogErrorAndUpdateStatus("Meta file does not exist for file '{0}'. You should probably be using an outside Unity project to generate these meta files for you. Meta file generation responsibility left to the user since it's a one time operation.", documentationOutputPath);
							return CompileResult.Failed;
						}
						if (job.Configuration.GenerateDebugInfo && !CheckIfMetaFileExists(debugDatabaseOutputPath))
						{
							DLLBuilder.LogErrorAndUpdateStatus("Meta file does not exist for file '{0}'. You should probably be using an outside Unity project to generate these meta files for you. Meta file generation responsibility left to the user since it's a one time operation.", debugDatabaseOutputPath);
							return CompileResult.Failed;
						}
					}

					return CompileResult.Succeeded;
				}

				Debug.LogError("Compilation exit code: " + process.ExitCode);
				Debug.LogWarning("The failure might be due to a compilation error. Start Unity and manually check the errors.");
				return CompileResult.Failed;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				return CompileResult.Failed;
			}
		}

		private static bool CheckIfMetaFileExists(string filePath)
		{
			return File.Exists(filePath + ".meta");
		}

		private static void GenerateExportedFiles(string sourcePath, bool isEditor, ref List<string> exportedFiles, string[] excludedKeywords)
		{
			var paths = Directory.GetFiles(sourcePath, "*.cs", SearchOption.AllDirectories);

			exportedFiles = new List<string>(1024);

			for (int i = 0; i < paths.Length; i++)
			{
				var path = paths[i].FixDirectorySeparatorChars('/');
				if (path.Contains("/Editor/") == isEditor)
				{
					int j = 0;
					for (; j < excludedKeywords.Length; j++)
					{
						if (path.Contains(excludedKeywords[j]))
							break;
					}
					if (j < excludedKeywords.Length)
						continue;

					exportedFiles.Add(path);
				}
			}

			exportedFiles.Sort();
			exportedFiles.RemoveDuplicates();
			exportedFiles.LogList(string.Format("Source files for {0} DLL ({1}):", isEditor ? "editor" : "runtime", exportedFiles.Count));

			Debug.LogError("NOT IMPLEMENTED YET! Sources are not checked for precompiler directives.");
		}

	}

}
