using System.Collections;
using System.Collections.Generic;
using Extenity.ReflectionToolbox;
using Extenity.ReflectionToolbox.Editor;
using Extenity.UnityEditorToolbox.Editor;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace ExtenityExamples.Reflection
{

	[CustomEditor(typeof(Example_FindReferencedObjects))]
	public class Example_FindReferencedObjectsInspector : ExtenityEditorBase<Example_FindReferencedObjects>
	{
		protected override void OnEnableDerived()
		{
		}

		protected override void OnDisableDerived()
		{
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
			if (GUILayout.Button("Log", BigButtonHeight))
			{
				Log();
			}
		}

		private void Log()
		{
			// Object
			{
				var result = new HashSet<Object>();
				this.FindAllReferencedObjectsInUnityObject(result);

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
				SceneManager.GetActiveScene().FindAllReferencedObjectsInScene(result);

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

			// All materials in scene
			{
				var result = new HashSet<Material>();
				SceneManager.GetActiveScene().FindAllReferencedObjectsInScene(result);

				Debug.LogFormat("================================= Found '{0}' referenced materials in scene:", result.Count);
				foreach (var item in result)
				{
					Debug.LogFormat(item, "   Name: '{0}' \tType: '{1}' \tObject: '{2}'", item.name, item.GetType(), item);
				}
			}
		}
	}

}
