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
			//Debug.LogFormat("Removing SnapToGroundInEditor from scene '{0}'.", scene.name);

			var rootGameObjects = scene.GetRootGameObjects();
			if (rootGameObjects != null)
			{
				for (var iRoot = 0; iRoot < rootGameObjects.Length; iRoot++)
				{
					var rootGameObject = rootGameObjects[iRoot];
					var components = rootGameObject.GetComponentsInChildren<SnapToGroundInEditor>();
					for (var iComponent = 0; iComponent < components.Length; iComponent++)
					{
						Object.Destroy(components[iComponent]);
					}
				}
			}
		}
	}

}
