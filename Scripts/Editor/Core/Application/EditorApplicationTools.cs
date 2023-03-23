using System;
using System.Diagnostics;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Reflection;
using Extenity.FileSystemToolbox;
using Extenity.ProfilingToolbox;
using Extenity.UnityEditorToolbox;
using UnityEditor;

namespace Extenity.ApplicationToolbox.Editor
{

	public static class EditorApplicationTools
	{
		#region Paths - Unity Editor

		private static string _UnityEditorExecutableDirectory;
		public static string UnityEditorExecutableDirectory
		{
			get
			{
				if (_UnityEditorExecutableDirectory == null)
				{
					//_UnityEditorExecutableDirectory = AppDomain.CurrentDomain.BaseDirectory; This returns null for some reason.
					var file = new FileInfo(typeof(EditorApplication).Assembly.Location);
					var directory = file.Directory;
					var parentDirectory = directory.Parent;
#if UNITY_EDITOR_WIN
					if (directory.Name != "Managed" || parentDirectory.Name != "Data")
#elif UNITY_EDITOR_OSX
					if (directory.Name != "Managed" || parentDirectory.Name != "Contents")
#else
					throw new System.NotImplementedException();
#endif
					{
						throw new Exception("Unexpected Unity Editor executable location: " + file);
					}
					_UnityEditorExecutableDirectory = parentDirectory.Parent.FullName;
				}
				return _UnityEditorExecutableDirectory;
			}
		}

		private static string _UnityEditorInstallationDirectory;
		public static string UnityEditorInstallationDirectory
		{
			get
			{
				if (_UnityEditorInstallationDirectory == null)
				{
					var executableDirectory = new DirectoryInfo(UnityEditorExecutableDirectory);
#if UNITY_EDITOR_WIN
					if (executableDirectory.Name != "Editor")
#elif UNITY_EDITOR_OSX
					if (executableDirectory.Name != "Unity.app")
#else
					throw new System.NotImplementedException();
#endif
					{
						throw new Exception("Unexpected Unity Editor executable location: " + executableDirectory.FullName);
					}
					_UnityEditorInstallationDirectory = executableDirectory.Parent.FullName;
				}
				return _UnityEditorInstallationDirectory;
			}
		}

		#endregion

		#region Update Continuum

		/// <summary>
		/// Calling this method inside OnDrawGizmos method or Update method of a class which is marked with
		/// [ExecuteAlways] or [ExecuteInEditMode] will make Unity call the Update method continuously. Not only for
		/// that particular object, but for all objects in Editor.
		///
		/// Note that calling Updates frequently is not processor-friendly as all objects are processed in whatever
		/// the frame rate it can run. Think of calling this method only when required, like only when an object is
		/// selected, or only when an animation is active, etc. Otherwise Unity editor will utilize high CPU all the
		/// time.
		/// </summary>
		[Conditional("UNITY_EDITOR")]
		public static void EnsureContinuousUpdateCallsInEditor()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				// QueuePlayerLoopUpdate with RepaintAll when called inside OnDrawGizmos will make the editor call
				// Update methods frequently.
				EditorApplication.QueuePlayerLoopUpdate();
				SceneView.RepaintAll();

				// Adding a delayed call for QueuePlayerLoopUpdate inside Update method will make the editor call
				// Update methods infrequently.
				EditorApplication.delayCall += EditorApplication.QueuePlayerLoopUpdate;
			}
#endif
		}

		/// <summary>
		/// Creates and destroys gameobjects to keep EditorApplication.update calls coming.
		/// That's the worst idea ever but it's the only way I could find.
		/// 
		/// Note that this is costly so try to use it only when needed.
		/// </summary>
		public static void GuaranteeNextUpdateCall()
		{
			var go = new GameObject("_EditorApplicationUpdateHelper");
			GameObject.DestroyImmediate(go);
		}

		/// <summary>
		/// A little trick to hopefully keep EditorApplication.update calls coming. It uses
		/// EditorApplication.delayCall to trigger a call to EditorApplication.update.
		/// This won't help if Editor window does not have focus.
		/// 
		/// Note that it may greatly increase calls to EditorApplication.update beyond needs.
		/// Scripts that do costly operations in their updates would slow down the editor.
		/// </summary>
		public static void IncreaseChancesOfNextUpdateCall()
		{
			EditorApplication.delayCall += () =>
			{
				EditorApplication.update.Invoke();
			};
		}

		#endregion

		#region Sync And Open C# Project

		public static void SyncAndOpenSolution()
		{
			var type = typeof(EditorApplication).Assembly.GetType("UnityEditor.CodeEditorProjectSync", true, true);
			var method = type.GetMethod("SyncAndOpenSolution", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			method.Invoke(null, null);
		}

		[MenuItem(ExtenityMenu.AssetsBaseContext + "Open C# Project (Force Rebuild)", priority = 999)] // Priority is just above Unity's 'Open C# Project'
		public static void OpenCSProjectForceRebuild()
		{
			// Delete SLN and CSPROJ files. That will force Unity to rebuild them from ground up.
			var projectPath = ApplicationTools.ApplicationPath;
			var files = new[] { "*.sln", "*.csproj" }
			            .SelectMany(filter => Directory.GetFiles(projectPath, filter, SearchOption.TopDirectoryOnly))
			            .ToArray();

			foreach (var file in files)
			{
				try
				{
					FileTools.Delete(file);
				}
				catch
				{
					Log.Warning("Skipping file: " + file);
				}
			}

			// Call Unity's regular 'Open C# Project' method.
			SyncAndOpenSolution();
		}

		#endregion

		#region Compilation Check

		public static void EnsureNotCompiling(bool breaking, string message = "There is an ongoing compilation, which was not expected.")
		{
			if (EditorApplication.isCompiling)
			{
				// These lines are kept here commented out for future needs. The idea was to hopefully
				// and desperately find a way to continue execution after Unity recompiles the code.
				//
				// It seems that waiting for compilation to finish by sleeping the thread is useless.
				// Not tried in a thread, but it's useless when sleeping in main thread. When isCompiling
				// called after an AssetDatabase.Refresh, Unity sets isCompiling to true, but won't start
				// the compilation until our method execution completes and Unity Editor's main loop takes
				// control.
				//
				// Also desperately tried to check for isCompiling inside editor coroutines. That way,
				// compilation starts after AssetDatabase.Refresh. Then Unity reloads the domain after
				// compilation. Which makes us lose the domain where coroutines live and new domain won't
				// try to continue coroutine executions from where they are left. Just like that.
				//
				//if (tryToSleepItOff)
				//{
				//	var tryCount = 60;
				//	while (EditorApplication.isCompiling && --tryCount > 0)
				//		Thread.Sleep(1000);
				//}

				// Just check once more. Maybe Unity has not yet realized the compilation was finished.
				if (EditorApplication.isCompiling)
				{
					if (breaking)
					{
						throw new Exception(message);
					}
					else
					{
						Log.Fatal(message);
					}
				}
			}
		}

		#endregion

		#region Check For Android SDK Installation

		[MenuItem(ExtenityMenu.System + "Tell If Android SDK Is Installed With Unity", priority = ExtenityMenu.SystemPriority + 1)]
		private static void TellIfAndroidSDKInstalledWithUnity()
		{
			bool isInstalled;
			using (new QuickProfilerStopwatch(Log, "Android SDK installation detection"))
			{
				isInstalled = IsAndroidSDKInstalledWithUnity();
			}

			EditorUtility.DisplayDialog("Info", $"Android SDK is {(isInstalled ? "" : "NOT ")}installed with Unity.", "Okay");
		}		
		
		public static bool IsAndroidSDKInstalledWithUnity()
		{
#if UNITY_EDITOR_WIN
			var adbFileName = "adb.exe";
#elif UNITY_EDITOR_OSX
			var adbFileName = "adb";
#else
			var adbFileName = "";
			throw new System.NotImplementedException();
#endif
			var editorDirectory = UnityEditorInstallationDirectory;
			var paths = Directory.GetFiles(editorDirectory, adbFileName, SearchOption.AllDirectories);
			if (paths.Length == 0)
				return false;
			if (paths.Length == 1)
				return true;
			throw new Exception($"While checking if Android SDK is installed with Unity, found more than one '{adbFileName}' files under Unity Editor installation at '{editorDirectory}'.");
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(EditorApplicationTools));

		#endregion
	}

}
