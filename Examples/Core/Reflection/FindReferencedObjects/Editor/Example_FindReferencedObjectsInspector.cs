using System.Collections.Generic;
using Extenity.DataToolbox;
using Extenity.ReflectionToolbox;
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
			// Referenced dependencies in Me
			{
				Debug.LogFormat(Me, "================================= Searching...");

				var result = new HashSet<Object>();
				Me.CollectDependenciesReferencedInUnityObject(result);

				Debug.LogFormat(Me, "================================= Found '{0}' referenced 'dependencies' in '{1}' that is 'Me':", result.Count, Me.FullName());
				foreach (var item in result)
				{
					Debug.LogFormat(item, "   Name: '{0}' \tType: '{1}' \tObject: '{2}'", item.name, item.GetType(), item);
				}
			}

			// Referenced dependencies in Me.gameObject
			{
				Debug.LogFormat(Me, "================================= Searching...");

				var result = new HashSet<Object>();
				Me.gameObject.CollectDependenciesReferencedInUnityObject(result);

				Debug.LogFormat(Me, "================================= Found '{0}' referenced 'dependencies' in '{1}' that is 'Me.gameObject':", result.Count, Me.FullGameObjectName());
				foreach (var item in result)
				{
					Debug.LogFormat(item, "   Name: '{0}' \tType: '{1}' \tObject: '{2}'", item.name, item.GetType(), item);
				}
			}

			// Referenced GameObjects in Me
			{
				Debug.LogFormat(Me, "================================= Searching...");

				var result = new HashSet<GameObject>();
				Me.FindAllReferencedGameObjectsInUnityObject(result, null);

				Debug.LogFormat(Me, "================================= Found '{0}' referenced 'gameobjects' in '{1}' that is 'Me':", result.Count, Me.FullName());
				foreach (var item in result)
				{
					Debug.LogFormat(item, "   Name: '{0}' \tType: '{1}' \tObject: '{2}'", item.name, item.GetType(), item);
				}
			}

			// Referenced GameObjects in Me.gameObject
			{
				Debug.LogFormat(Me, "================================= Searching...");

				var result = new HashSet<GameObject>();
				Me.gameObject.FindAllReferencedGameObjectsInUnityObject(result, null);

				Debug.LogFormat(Me, "================================= Found '{0}' referenced 'gameobjects' in '{1}' that is 'Me.gameObject':", result.Count, Me.FullGameObjectName());
				foreach (var item in result)
				{
					Debug.LogFormat(item, "   Name: '{0}' \tType: '{1}' \tObject: '{2}'", item.name, item.GetType(), item);
				}
			}

			// All referenced dependencies in scene
			{
				Debug.LogFormat(Me, "================================= Searching...");

				var result = new HashSet<Object>();
				SceneManager.GetActiveScene().CollectDependenciesReferencedInScene(result);

				Debug.LogFormat("================================= Found '{0}' referenced 'dependencies' in scene:", result.Count);
				foreach (var item in result)
				{
					Debug.LogFormat(item, "   Name: '{0}' \tType: '{1}' \tObject: '{2}'", item.name, item.GetType(), item);
				}
			}

			// All referenced gameobjects in scene
			{
				Debug.LogFormat(Me, "================================= Searching...");

				var result = new HashSet<GameObject>();
				SceneManager.GetActiveScene().FindAllReferencedGameObjectsInScene(result, null);

				Debug.LogFormat("================================= Found '{0}' referenced 'gameobjects' in scene:", result.Count);
				foreach (var item in result)
				{
					Debug.LogFormat(item, "   Name: '{0}' \tType: '{1}' \tObject: '{2}'", item.name, item.GetType(), item);
				}
			}

			// All referenced materials in scene
			{
				Debug.LogFormat(Me, "================================= Searching...");

				var result = new HashSet<Material>();
				SceneManager.GetActiveScene().CollectDependenciesReferencedInScene(result);

				Debug.LogFormat("================================= Found '{0}' referenced 'materials' in scene:", result.Count);
				foreach (var item in result)
				{
					Debug.LogFormat(item, "   Name: '{0}' \tType: '{1}' \tObject: '{2}'", item.name, item.GetType(), item);
				}
			}
		}
	}

}
