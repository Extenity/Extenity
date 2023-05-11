using Extenity.BuildToolbox.Editor;
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
#if !DisableExtenityBuildReport
			Log.Info("BuildMachine checking in at preprocess callback. BuildReport details: " + report.ToDetailedLogString());
#endif

			// See 713951791.
			//EditorSceneManagerTools.EnforceUserToSaveAllModifiedScenes("First you need to save the scene before building."); Disabled because it causes an internal Unity error at build time.
		}

		public void OnPostprocessBuild(BuildReport report)
		{
#if !DisableExtenityBuildReport
			Log.Info("BuildMachine checking in at postprocess callback. BuildReport details: " + report.ToDetailedLogString());
#endif
		}

		#region Log

		private static readonly Logger Log = new("Builder");

		#endregion
	}

}
