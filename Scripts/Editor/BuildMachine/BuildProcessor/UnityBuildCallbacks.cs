using Extenity.BuildToolbox.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Extenity.BuildMachine.Editor
{

	public class UnityBuildCallbacks :
		IPreprocessBuildWithReport,
		IPostprocessBuildWithReport
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

			Log.Info($"BuildMachine checking in at preprocess callback (Release: {release}, UnityDev: {unityDebug})... Report details: " + report.ToDetailedLogString());

			// See 713951791.
			//EditorSceneManagerTools.EnforceUserToSaveAllModifiedScenes("First you need to save the scene before building."); Disabled because it causes an internal Unity error at build time.
		}

		public void OnPostprocessBuild(BuildReport report)
		{
			Log.Info($"BuildMachine checking in at postprocess callback... Report details: " + report.ToDetailedLogString());
		}
	}

}
