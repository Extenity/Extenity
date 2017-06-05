using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Extenity.AssetToolbox.Editor;
using Extenity.ConsistencyToolbox;
using Extenity.DataToolbox;
using Extenity.DebugToolbox;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Extenity.DLLBuilder
{

	public static class Compiler
	{

		public static void CompileAllDLLs(Action onSucceeded, Action<string> onFailed)
		{
			Debug.Log("--------- Compiling all DLLs");

			InternalCompileDLLs(onSucceeded, onFailed).StartCoroutineInEditorUpdate();
		}

		private static IEnumerator InternalCompileDLLs(Action onSucceeded, Action<string> onFailed)
		{
			if (DLLBuilderConfiguration.Instance.CompilerConfigurations.IsNullOrEmpty())
			{
				if (onFailed != null)
					onFailed(string.Format("DLL Builder configuration does not have any entries. Please check your configuration at path '{0}'.", AssetDatabase.GetAssetPath(DLLBuilderConfiguration.Instance)));
				yield break;
			}

			// Check for consistency first
			foreach (var compilerConfiguration in DLLBuilderConfiguration.Instance.CompilerConfigurations)
			{
				var errors = compilerConfiguration.CheckConsistencyAndLog("There are inconsistencies in DLL compilation configurations.");
				if (errors.Count > 0)
				{
					if (onFailed != null)
						onFailed(string.Format("Failed to initialize compilation of DLL '{0}'.", compilerConfiguration.DLLNameWithoutExtension));
					yield break;
				}
			}

			foreach (var compilerConfiguration in DLLBuilderConfiguration.Instance.CompilerConfigurations)
			{
				if (!compilerConfiguration.Enabled)
					continue;

				CompilerJob job = null;

				try
				{
					job = new CompilerJob(compilerConfiguration);
					AsyncGenerateDLL(job);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
					if (onFailed != null)
						onFailed(string.Format("Failed to compile {0} DLL '{1}'.", job.RuntimeDLLSucceeded == CompileResult.Failed ? "runtime" : "editor", job.Configuration.DLLNameWithoutExtension));
				}

				// Wait until compilation finishes
				while (!job.Finished)
					yield return null;

				if (job.RuntimeDLLSucceeded == CompileResult.Failed || job.EditorDLLSucceeded == CompileResult.Failed)
				{
					if (onFailed != null)
						onFailed(string.Format("Failed to compile {0} DLL '{1}'.", job.RuntimeDLLSucceeded == CompileResult.Failed ? "runtime" : "editor", job.Configuration.DLLNameWithoutExtension));
					yield break;
				}
			}

			if (onSucceeded != null)
				onSucceeded();
		}

		private static void InternalOnFinished(CompilerJob job)
		{
			Debug.Log("####### DLL build finished. RuntimeDLLSucceeded: " + job.RuntimeDLLSucceeded + "     EditorDLLSucceeded: " + job.EditorDLLSucceeded);
			job.Finished = true;
			if (job.OnFinished != null)
			{
				job.OnFinished(job);
			}
		}

		private static void AsyncGenerateDLL(CompilerJob job)
		{
			if (string.IsNullOrEmpty(job.Configuration.SourcePath))
				throw new ArgumentNullException("job.Configuration.SourcePath");

			Debug.LogFormat("Compiling '{0}'", job.Configuration.DLLNameWithoutExtension);

			DLLBuilderTools.DetectUnityReferences(ref job.UnityManagedReferences);
			GenerateExportedFiles(job.Configuration.SourcePath, false, ref job.SourceFilePathsForRuntimeDLL, job.Configuration.ExcludedKeywords);
			GenerateExportedFiles(job.Configuration.SourcePath, true, ref job.SourceFilePathsForEditorDLL, job.Configuration.ExcludedKeywords);

			var thread = new Thread(GenerateDLL);
			thread.Name = "Generate " + job.Configuration.DLLNameWithoutExtension;
			thread.Start(new object[] { job });
		}

		private static void GenerateDLL(object data)
		{
			GenerateDLL((CompilerJob)((object[])data)[0]);
		}

		private static void GenerateDLL(CompilerJob job)
		{
			try
			{
				var anyRuntimeSourceFiles = job.SourceFilePathsForRuntimeDLL.IsNotNullAndEmpty();
				var anyEditorSourceFiles = job.SourceFilePathsForEditorDLL.IsNotNullAndEmpty();
				if (!anyRuntimeSourceFiles && !anyEditorSourceFiles)
				{
					throw new Exception("There are no source files to compile.");
				}

				if (anyRuntimeSourceFiles)
				{
					CopySourcesToTemporaryDirectory(job.SourceFilePathsForRuntimeDLL, job.Configuration.SourcePath, job.Configuration.IntermediateSourceDirectoryPath);
					job.RuntimeDLLSucceeded = CompileDLL(job.Configuration.IntermediateSourceDirectoryPath, false, job);
					DirectoryTools.Delete(job.Configuration.IntermediateSourceDirectoryPath);
				}
				else
				{
					// Skip
					Cleaner.ClearOutputDLLs(job.Configuration, true, true); // We still need to clear previous outputs in case there are any left from previous builds.
					Debug.Log("Skipping runtime DLL build. No scripts to compile for runtime DLL.");
					job.RuntimeDLLSucceeded = CompileResult.Skipped;
				}

				if (job.RuntimeDLLSucceeded == CompileResult.Succeeded || job.RuntimeDLLSucceeded == CompileResult.Skipped)
				{
					if (anyEditorSourceFiles)
					{
						CopySourcesToTemporaryDirectory(job.SourceFilePathsForEditorDLL, job.Configuration.SourcePath, job.Configuration.IntermediateSourceDirectoryPath);
						job.EditorDLLSucceeded = CompileDLL(job.Configuration.IntermediateSourceDirectoryPath, true, job);
						DirectoryTools.Delete(job.Configuration.IntermediateSourceDirectoryPath);
					}
					else
					{
						// Skip
						Cleaner.ClearOutputDLLs(job.Configuration, false, true); // We still need to clear previous outputs in case there are any left from previous builds.
						Debug.Log("Skipping editor DLL build. No scripts to compile for editor DLL.");
						job.EditorDLLSucceeded = CompileResult.Skipped;
					}
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}

			EditorApplication.delayCall += () => { InternalOnFinished(job); };
		}

		private static void CopySourcesToTemporaryDirectory(List<string> sourceFilePaths, string sourceBasePath, string temporaryDirectoryPath)
		{
			Debug.LogFormat("Copying sources into temporary directory '{0}'.", temporaryDirectoryPath);

			DirectoryTools.Delete(temporaryDirectoryPath);

			for (int i = 0; i < sourceFilePaths.Count; i++)
			{
				if (!sourceFilePaths[i].StartsWith(sourceBasePath))
					throw new Exception("Source file path does not start with base path.");

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

				var outputPath = isEditorBuild
					? job.Configuration.EditorDLLPath
					: job.Configuration.DLLPath;

				// Create references list
				var allReferences = new List<string>();
				{
					//if (job.Configuration.AddAllDLLsInUnityManagedDirectory)
					//	for (int i = 0; i < job.UnityManagedReferences.Count; i++)
					//		allReferences.AddIfDoesNotContain(job.UnityManagedReferences[i].Trim()); // Directory separators already fixed.

					if (job.Configuration.RuntimeReferences != null)
						for (int i = 0; i < job.Configuration.RuntimeReferences.Length; i++)
							allReferences.AddIfDoesNotContain(job.Configuration.RuntimeReferences[i].Trim().FixDirectorySeparatorChars()); // Fix directory separators because these are entered by user.

					if (isEditorBuild && job.Configuration.EditorReferences != null)
						for (int i = 0; i < job.Configuration.EditorReferences.Length; i++)
							allReferences.AddIfDoesNotContain(job.Configuration.EditorReferences[i].Trim().FixDirectorySeparatorChars()); // Fix directory separators because these are entered by user.

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
					allReferences.RemoveAll(item => string.IsNullOrEmpty(item));
				}

				var arguments = new List<string>(100);









				var gmcsPath = @"C:\Program Files\Unity\Editor\Data\Mono\lib\mono\2.0\gmcs.exe";
				var compilerPath = @"C:\Program Files\Unity\Editor\Data\Mono\bin\mono.exe";

				arguments.Add("\"" + gmcsPath + "\"");
				arguments.Add("/target:library");

				Debug.LogError("NOT IMPLEMENTED YET! Generate Documentation and Generate Debug Info!");
				//if (job.Configuration.GenerateDocumentation)
				//{
				//	arguments.Add("/doc:\"" + documentationOutputPath + "\"");
				//}

				//if (job.Configuration.GenerateDebugInfo)
				//{
				//	arguments.Add("/debug+ /debug:full");
				//}
				//else
				{
					arguments.Add("/debug-");
				}

				arguments.Add("/optimize+");


				arguments.Add("/out:\"" + outputPath + "\"");

				//defines = defines.Trim();
				////// Append Unity version directives.
				////{
				////	var unityVersionDefines = GetUnityVersionDefines(unityVersion);
				////	defines += string.IsNullOrEmpty(defines)
				////		? unityVersionDefines
				////		: ";" + unityVersionDefines;
				////}

				if (!string.IsNullOrEmpty(defines))
				{
					arguments.Add("/define:" + defines);
				}

				for (int i = 0; i < allReferences.Count; i++)
					arguments.Add("/reference:\"" + allReferences[i] + "\"");

				arguments.Add("/recurse:\"" + Path.Combine(sourcePath, "*.cs") + "\"");











				//if (job.Configuration.GenerateDocumentation == true)
				//{
				//	arguments.Add("/doc:\"" + documentationOutputPath + "\"");
				//}

				//if (job.Configuration.GenerateDebugInfo == true)
				//{
				//	arguments.Add("/debug+ /debug:full");
				//}
				//else
				//{
				//	arguments.Add("/debug-");
				//}

				//arguments.Add("/noconfig /nowarn:1701,1702,2008 /nostdlib+ /nologo /platform:AnyCPU /errorreport:none /warn:0 /filealign:512");

				//defines = defines.Trim();
				////// Append Unity version directives.
				////{
				////	var unityVersionDefines = GetUnityVersionDefines(unityVersion);
				////	defines += string.IsNullOrEmpty(defines)
				////		? unityVersionDefines
				////		: ";" + unityVersionDefines;
				////}

				//if (!string.IsNullOrEmpty(defines))
				//{
				//	arguments.Add("/define:" + defines);
				//}

				//arguments.Add("/errorendlocation /preferreduilang:en-US /highentropyva-");

				//for (int i = 0; i < allReferences.Count; i++)
				//	arguments.Add("/reference:\"" + allReferences[i] + "\"");

				//arguments.Add("/out:\"" + dllOutputPath + "\"");
				//arguments.Add("/target:library");
				//arguments.Add("/utf8output");
				//arguments.Add("/recurse:\"" + Path.Combine(sourcePath, "*.cs") + "\"");

				//var compilerPath = AutoDetectCSCPath();









				arguments.LogList("Launching compiler with arguments:");

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
					Debug.LogError("Compiler process stopped with code " + process.ExitCode + ".");
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
					Debug.LogFormat("Finished compiling '{0}'.", isEditorBuild ? job.Configuration.EditorDLLName : job.Configuration.DLLName);

					if (!CheckIfMetaExists(outputPath))
					{
						Debug.LogErrorFormat("Meta file does not exist for file '{0}'. You should probably be using an outside Unity project to generate these meta files for you. Meta file generation responsibility left to the user since it's a one time operation.", outputPath);
						return CompileResult.Failed;
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

		private static bool CheckIfMetaExists(string filePath)
		{
			return File.Exists(filePath + ".meta");
		}

		private static void GenerateExportedFiles(string sourcePath, bool isEditor, ref List<string> exportedFiles, string[] excludedKeywords)
		{
			var paths = AssetDatabase.GetAllAssetPaths();

			exportedFiles = new List<string>(1024);

			for (int i = 0; i < paths.Length; i++)
			{
				if (DLLBuilderTools.IsAssetInPath(paths[i], sourcePath) &&
					paths[i].Contains("/Editor/") == isEditor &&
					paths[i].EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
				{
					int j = 0;
					for (; j < excludedKeywords.Length; j++)
					{
						if (paths[i].Contains(excludedKeywords[j]) == true)
							break;
					}
					if (j < excludedKeywords.Length)
						continue;

					exportedFiles.Add(paths[i]);
				}
			}

			exportedFiles.Sort();
			exportedFiles.RemoveDuplicates();
			exportedFiles.LogList(string.Format("Source files for {0} DLL ({1}):", isEditor ? "editor" : "runtime", exportedFiles.Count));

			Debug.LogError("NOT IMPLEMENTED YET! Sources are not checked for precompiler directives.");
		}








		// TODO: Move into Extenity
		public static void StartCoroutineInEditorUpdate(this IEnumerator update, Action onFinished = null)
		{
			EditorApplication.CallbackFunction onUpdate = null;

			onUpdate = () =>
			{
				try
				{
					if (update.MoveNext() == false)
					{
						if (onFinished != null)
							onFinished();
						EditorApplication.update -= onUpdate;
					}
				}
				catch (Exception ex)
				{
					if (onFinished != null)
						onFinished();
					Debug.LogException(ex);
					EditorApplication.update -= onUpdate;
				}
			};

			EditorApplication.update += onUpdate;
		}




	}

}
