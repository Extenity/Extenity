using System;
using Extenity.InspectorToolbox.Editor;
using UnityEditor;

namespace Extenity.SceneManagementToolbox.Editor
{

	/// <summary>
	/// The inspector of <see cref="LevelSceneManager"/>: the level scenes it found in the build settings, kept up
	/// to date while the inspector is open.
	/// </summary>
	[CustomEditor(typeof(LevelSceneManager))]
	public sealed class LevelSceneManagerInspector : ObjectInspector<LevelSceneManager>
	{
		protected override void Configure(InspectorBuilder<LevelSceneManager> b)
		{
			b.LiveList("Level Scenes", manager => manager.LevelScenes ?? Array.Empty<string>()).ExpandedByDefault();
		}
	}

}
