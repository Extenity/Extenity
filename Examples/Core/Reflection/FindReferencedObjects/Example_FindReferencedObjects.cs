using System.Collections.Generic;
using Extenity.ReflectionToolbox;
using UnityEngine;

namespace ExtenityExamples.Reflection
{

	public class Example_FindReferencedObjects : MonoBehaviour
	{
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

				Debug.LogFormat("Found '{0}' referenced objects:", result.Count);
				foreach (var item in result)
				{
					Debug.LogFormat(item, "   Name: '{0}' \tType: '{1}' \tObject: '{2}'", item.name, item.GetType(), item);
				}
			}

			// GameObject
			{
				var result = new HashSet<GameObject>();
				this.FindAllReferencedGameObjectsInUnityObject(result, true);

				Debug.LogFormat("Found '{0}' referenced gameobjects:", result.Count);
				foreach (var item in result)
				{
					Debug.LogFormat(item, "   Name: '{0}' \tType: '{1}' \tObject: '{2}'", item.name, item.GetType(), item);
				}
			}
		}
	}

}
