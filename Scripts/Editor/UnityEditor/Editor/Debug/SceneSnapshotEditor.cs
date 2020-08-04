using Extenity.UnityEditorToolbox;
using UnityEditor;

namespace Extenity.DebugToolbox
{

	public static class SceneSnapshotEditor
	{
		#region Scene Snapshot

		[MenuItem(ExtenityMenu.Analysis + "Scene/Log Scene Snapshot", priority = ExtenityMenu.AnalysisPriority + 4)]
		public static void LogSceneSnapshot()
		{
			var snapshot = new SceneSnapshot();
			snapshot.TakeSnapshotOfAllLoadedScenes();
			snapshot.LogToFile(true, true);
		}

		#endregion
	}

}
