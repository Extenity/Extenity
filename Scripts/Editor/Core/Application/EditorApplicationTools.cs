using System;
using UnityEngine;
using System.IO;
using System.Reflection;
using Extenity.FileSystemToolbox;
using UnityEditor;

namespace Extenity.ApplicationToolbox.Editor
{

	public class EditorApplicationTools : MonoBehaviour
	{
		#region Paths

		public static string UnityProjectPath
		{
			get
			{
				// This does not work in threaded environment. So we use working directory instead.
				//return ApplicationTools.ApplicationPath.AddDirectorySeparatorToEnd().FixDirectorySeparatorChars();

				return Directory.GetCurrentDirectory().AddDirectorySeparatorToEnd().FixDirectorySeparatorChars();
			}
		}

		public static string EditorTempDirectoryPath
		{
			get
			{
				return Path.Combine(UnityProjectPath, "Temp").AddDirectorySeparatorToEnd().FixDirectorySeparatorChars();
			}
		}

		private static string _UnityEditorExecutableDirectory;
		public static string UnityEditorExecutableDirectory
		{
			get
			{
				if (_UnityEditorExecutableDirectory == null)
				{
					//_UnityEditorExecutableDirectory = AppDomain.CurrentDomain.BaseDirectory; This returns null for some reason.
					var file = new FileInfo(typeof(UnityEditor.EditorApplication).Assembly.Location);
					var directory = file.Directory;
					var parentDirectory = directory.Parent;
					if (directory.Name != "Managed" || parentDirectory.Name != "Data")
						throw new InternalException(9776381); // Unexpected Unity Editor executable location.
					_UnityEditorExecutableDirectory = parentDirectory.Parent.FullName;
				}
				return _UnityEditorExecutableDirectory;
			}
		}

		#endregion

		#region Update Continuum

		// TODO: See if EditorApplication.QueuePlayerLoopUpdate does the job.

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
			var type = typeof(EditorApplication).Assembly.GetType("UnityEditor.SyncVS", true, true);
			var method = type.GetMethod("SyncAndOpenSolution", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			method.Invoke(null, null);
		}

		#endregion

		#region Compilation Check

		public static void EnsureNotCompiling(bool breaking = true)
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
					var message = "There is an ongoing compilation, which was not expected.";
					if (breaking)
					{
						throw new Exception(message);
					}
					else
					{
						Log.CriticalError(message);
					}
				}
			}
		}

		#endregion

		#region Check For Android SDK Installation

		public static bool IsAndroidSDKInstalledWithUnity()
		{
#if UNITY_EDITOR_WIN
			var adbFileName = "adb.exe";
			var editorDirectory = UnityEditorExecutableDirectory;
			var paths = Directory.GetFiles(editorDirectory, adbFileName, SearchOption.AllDirectories);
			if (paths.Length == 0)
				return false;
			if (paths.Length == 1)
				return true;
			throw new Exception($"While checking if Android SDK is installed with Unity, found more than one '{adbFileName}' files under Unity Editor installation at '{editorDirectory}'.");
#else
			throw new NotImplementedException();
#endif
		}

		#endregion
	}

}
