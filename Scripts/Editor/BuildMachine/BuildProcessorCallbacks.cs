using Extenity.BuildToolbox.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;

namespace Extenity.BuildMachine.Editor
{

	// TODO: Detect and throw error if any changes made that needs recompilation during scene processes. Try to use CompilationPipeline.assemblyCompilationStarted. See 112739521.

	public class BuildProcessorCallbacks :
		IPreprocessBuildWithReport,
		IPostprocessBuildWithReport,
		IProcessSceneWithReport
	{
		public int callbackOrder => 10000;

		public void OnPreprocessBuild(BuildReport report)
		{
#if Release
			var release = true;
#else
			var release = false;
#endif
			//var unityDebug = Debug.isDebugBuild; This does not say what option the build was started. Only tells if the Development option is ticked in Build Settings window.
			var unityDebug = report.summary.options.HasFlag(BuildOptions.Development);

			Log.Info($"Build processor checking in at preprocess callback (Release: {release}, UnityDev: {unityDebug})... Report details: " + report.ToDetailedLogString());

			// See 713951791.
			//EditorSceneManagerTools.EnforceUserToSaveAllModifiedScenes("First you need to save the scene before building."); Disabled because it causes an internal Unity error at build time.
		}

		public void OnPostprocessBuild(BuildReport report)
		{
			Log.Info($"Build processor checking in at postprocess callback... Report details: " + report.ToDetailedLogString());
		}

		public void OnProcessScene(Scene scene, BuildReport report)
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
			{
				// TODO: Automatically processing the scene when pressing the Play button should be made here. But processing the scene each time the Play button gets pressed is madness. So before doing that, a mechanism for checking if a process is already done before should be implemented first.
				return;
			}

			Log.Info($"Build processor checking in at scene process callback for '{scene.name}'.");

			// See 713951791.
			//DoProcessScene(scene, ...);
		}
	}

}
