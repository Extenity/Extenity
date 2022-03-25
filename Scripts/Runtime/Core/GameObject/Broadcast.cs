#if UNITY

using Extenity.DataToolbox;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Extenity.GameObjectToolbox
{

	public class Broadcast
	{


		public static void ToScene(string methodName)
		{
			var roots = New.List<GameObject>();

			var sceneCount = SceneManager.sceneCount;
			for (int iScene = 0; iScene < sceneCount; iScene++)
			{
				var scene = SceneManager.GetSceneAt(iScene);
				if (scene.isLoaded)
				{
					scene.GetRootGameObjects(roots);
					for (var iRoot = 0; iRoot < roots.Count; iRoot++)
					{
						roots[iRoot].BroadcastMessage(methodName, SendMessageOptions.DontRequireReceiver);
					}
					roots.Clear();
				}
			}
			Release.ListUnsafe(roots);
		}

		public static void ToScene(string methodName, object parameter)
		{
			var roots = New.List<GameObject>();

			var sceneCount = SceneManager.sceneCount;
			for (int iScene = 0; iScene < sceneCount; iScene++)
			{
				var scene = SceneManager.GetSceneAt(iScene);
				if (scene.isLoaded)
				{
					scene.GetRootGameObjects(roots);
					for (var iRoot = 0; iRoot < roots.Count; iRoot++)
					{
						roots[iRoot].BroadcastMessage(methodName, parameter, SendMessageOptions.DontRequireReceiver);
					}
					roots.Clear();
				}
			}
			Release.ListUnsafe(roots);
		}

		/* Old but gold. Keep them commented out.
		public static void ToScene(string methodName)
		{
			var objects = Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
			if (objects == null)
				return;

			for (int i = 0; i < objects.Length; i++)
			{
				var obj = objects[i];
				if (obj && !obj.transform.parent)
					obj.BroadcastMessage(methodName, SendMessageOptions.DontRequireReceiver);
			}
		}

		public static void ToScene(string methodName, object value)
		{
			var objects = Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
			if (objects == null)
				return;

			for (int i = 0; i < objects.Length; i++)
			{
				var obj = objects[i];
				if (obj && !obj.transform.parent)
					obj.BroadcastMessage(methodName, value, SendMessageOptions.DontRequireReceiver);
			}
		}
		*/
	}

}

#endif
