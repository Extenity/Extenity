using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
using UnityEditor.Build;
#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Extenity.UnityEditorToolbox.Editor
{

	public class DontShowEditorHandlerSceneProcessor :
#if UNITY_2018_1_OR_NEWER
		IProcessSceneWithReport
#else
		IProcessScene
#endif
	{
		public int callbackOrder { get { return -1000; } }

#if UNITY_2018_1_OR_NEWER
		public void OnProcessScene(Scene scene, BuildReport report)
#else
		public void OnProcessScene(Scene scene)
#endif
		{
			RemoveComponentsInScene(scene, true, true);
		}

		public static void RemoveComponentsInScene(Scene scene, bool includeInactive, bool log)
		{
			var objects = scene.FindObjectsOfTypeAll<DontShowEditorHandler>(includeInactive);
			if (objects.IsNotNullAndEmpty())
			{
				if (log)
				{
					Debug.LogFormat("Removing '{0}' DontShowEditorHandler components from scene '{1}'.", objects.Count, scene.name);
				}

				for (var i = 0; i < objects.Count; i++)
				{
					Object.DestroyImmediate(objects[i]);
				}
			}
		}
	}

}
