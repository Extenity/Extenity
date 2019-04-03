using System.Collections.Generic;
using System.Linq;
using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
using Extenity.SceneManagementToolbox;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Extenity.UnityEditorToolbox.Editor
{

	public static class EditorUtilityTools
	{
		#region CollectDependenciesReferencedIn...

		public static KeyValue<Scene, TSearched[]>[] CollectDependenciesReferencedInLoadedScenes<TSearched>() where TSearched : Object
		{
			var scenes = SceneManagerTools.GetLoadedScenes();
			var objectsInScenes = new KeyValue<Scene, TSearched[]>[scenes.Count];
			var objects = new HashSet<TSearched>();
			for (var i = 0; i < scenes.Count; i++)
			{
				var scene = scenes[i];
				objects.Clear();
				scene.CollectDependenciesReferencedInScene(objects);

				objectsInScenes[i] = new KeyValue<Scene, TSearched[]>
				{
					Key = scene,
					Value = objects.ToArray()
				};
			}
			return objectsInScenes;
		}

		public static void CollectDependenciesReferencedInScene<TSearched>(this Scene scene, HashSet<TSearched> result) where TSearched : Object
		{
			var objects = EditorUtility.CollectDependencies(scene.ListAllGameObjectsInScene().ToArray());
			foreach (var obj in objects)
			{
				var cast = obj as TSearched;
				if (cast)
					result.Add(cast);
			}
		}

		public static void CollectDependenciesReferencedInComponents<T, TSearched>(this IEnumerable<T> components, HashSet<TSearched> result) where T : Component where TSearched : Object
		{
			var objects = EditorUtility.CollectDependencies(components.ToArray());
			foreach (var obj in objects)
			{
				var cast = obj as TSearched;
				if (cast)
					result.Add(cast);
			}
		}

		public static void CollectDependenciesReferencedInComponent<T, TSearched>(this T component, HashSet<TSearched> result) where T : Component where TSearched : Object
		{
			var objects = EditorUtility.CollectDependencies(new Object[] { component });
			foreach (var obj in objects)
			{
				var cast = obj as TSearched;
				if (cast)
					result.Add(cast);
			}
		}

		public static void CollectDependenciesReferencedInGameObject<TSearched>(this GameObject gameObject, HashSet<TSearched> result) where TSearched : Object
		{
			var objects = EditorUtility.CollectDependencies(new Object[] { gameObject });
			foreach (var obj in objects)
			{
				var cast = obj as TSearched;
				if (cast)
					result.Add(cast);
			}
		}

		public static void CollectDependenciesReferencedInUnityObject<TSearched>(this Object unityObject, HashSet<TSearched> result) where TSearched : Object
		{
			var objects = EditorUtility.CollectDependencies(new Object[] { unityObject });
			foreach (var obj in objects)
			{
				var cast = obj as TSearched;
				if (cast)
					result.Add(cast);
			}
		}

		#endregion
	}

}
