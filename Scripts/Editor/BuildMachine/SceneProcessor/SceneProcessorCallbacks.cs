using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;

namespace Extenity.BuildMachine.Editor
{

	// TODO: Detect and throw error if any changes made that needs recompilation during scene processes. Try to use CompilationPipeline.assemblyCompilationStarted. See 112739521.

	public class SceneProcessorCallbacks : IProcessSceneWithReport
	{
		public int callbackOrder => 10000;

		public void OnProcessScene(Scene scene, BuildReport report)
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
			{
				// TODO: Automatically processing the scene when pressing the Play button should be made here. But processing the scene each time the Play button gets pressed is madness. So before doing that, a mechanism for checking if a process is already done before should be implemented first.
				return;
			}

			Log.Info($"Scene processor checking in at scene process callback for scene '{scene.name}'.");

			// See 713951791.
			//DoProcessScene(scene, ...);
		}

		#region Log

		private static readonly Logger Log = new("Builder");

		#endregion
	}

}
