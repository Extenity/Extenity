using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Extenity.UnityEditorToolbox.Editor
{

	public class SnapToGroundInEditorSceneProcessor : IProcessScene
	{
		public int callbackOrder { get { return -1000; } }

		public void OnProcessScene(Scene scene)
		{
			RemoveSnapToGroundInEditorComponents(scene, true);
		}

		public static void RemoveSnapToGroundInEditorComponents(Scene scene, bool log)
		{
			var objects = scene.FindObjectsOfTypeAll<SnapToGroundInEditor>();
			if (objects.IsNotNullAndEmpty())
			{
				if (log)
				{
					Debug.LogFormat("Removing '{0}' SnapToGroundInEditor components from scene '{1}'.", objects.Count, scene.name);
				}

				for (var i = 0; i < objects.Count; i++)
				{
					Object.DestroyImmediate(objects[i]);
				}
			}
		}
	}

}
