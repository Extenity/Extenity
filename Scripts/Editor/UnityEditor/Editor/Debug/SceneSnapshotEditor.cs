using UnityEditor;

namespace Extenity.DebugToolbox
{

	public static class SceneSnapshotEditor
	{
		#region Scene Snapshot

		[MenuItem("Tools/Log Scene Snapshot", priority = 22300)]
		public static void LogSceneSnapshot()
		{
			var snapshot = new SceneSnapshot();
			snapshot.TakeSnapshotOfAllLoadedScenes();
			snapshot.LogToFile(true, true);
		}

		#endregion
	}

}
