using System.Collections.Generic;
using Extenity.ReflectionToolbox;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ExtenityExamples.Reflection
{

	public class Example_FindReferencedObjects : MonoBehaviour
	{
		public Camera CameraLink;
		public Material MaterialLink;
		public Mesh MeshLink;
		public GameObject OtherGameObject;

		private void Awake()
		{
			if (gameObject.name != "FindReferencedObjects")
			{
				return;
			}

			// Object
			{
				var result = new HashSet<Object>();
				this.FindAllReferencedObjectsInUnityObject(result, true);

				Debug.LogFormat("================================= Found '{0}' referenced objects:", result.Count);
				foreach (var item in result)
				{
					Debug.LogFormat(item, "   Name: '{0}' \tType: '{1}' \tObject: '{2}'", item.name, item.GetType(), item);
				}
			}

			// GameObject
			{
				var result = new HashSet<GameObject>();
				this.FindAllReferencedGameObjectsInUnityObject(result, true);

				Debug.LogFormat("================================= Found '{0}' referenced gameobjects:", result.Count);
				foreach (var item in result)
				{
					Debug.LogFormat(item, "   Name: '{0}' \tType: '{1}' \tObject: '{2}'", item.name, item.GetType(), item);
				}
			}

			// All objects in scene
			{
				var result = new HashSet<Object>();
				SceneManager.GetActiveScene().FindAllReferencedObjectsInScene(result, true);

				Debug.LogFormat("================================= Found '{0}' referenced objects in scene:", result.Count);
				foreach (var item in result)
				{
					Debug.LogFormat(item, "   Name: '{0}' \tType: '{1}' \tObject: '{2}'", item.name, item.GetType(), item);
				}
			}

			// All gameobjects in scene
			{
				var result = new HashSet<GameObject>();
				SceneManager.GetActiveScene().FindAllReferencedGameObjectsInScene(result, true);

				Debug.LogFormat("================================= Found '{0}' referenced gameobjects in scene:", result.Count);
				foreach (var item in result)
				{
					Debug.LogFormat(item, "   Name: '{0}' \tType: '{1}' \tObject: '{2}'", item.name, item.GetType(), item);
				}
			}
		}
	}

}
