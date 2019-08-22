using System;
using System.Collections.Generic;
using System.Linq;
using Extenity.DataToolbox;
using Extenity.FileSystemToolbox;
using Extenity.GameObjectToolbox;
using Extenity.SceneManagementToolbox;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Extenity.UnityEditorToolbox.Editor
{

	/// <summary>
	/// Don't forget to see <see cref="InternalEditorUtility"/> for more goodness.
	/// </summary>
	public static class EditorUtilityTools
	{
		#region CollectDependenciesReferencedIn...

		public static KeyValue<Scene, TSearched[]>[] CollectDependenciesReferencedInLoadedScenes<TSearched>(bool includeActiveScene, bool includeDontDestroyOnLoadScene) where TSearched : Object
		{
			var scenes = SceneManagerTools.GetLoadedScenes(includeActiveScene, includeDontDestroyOnLoadScene);
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

		#region Load / Save Asset File

		public static void SaveUnityAssetFile(string path, Object obj)
		{
			try
			{
				DirectoryTools.CreateFromFilePath(path);
				InternalEditorUtility.SaveToSerializedFileAndForget(new[] {obj}, path, true);
			}
			catch (Exception exception)
			{
				throw new Exception($"Failed to save '{path}'. Careful inspection required to prevent losing any data.", exception);
			}
		}

		public static T LoadUnityAssetFile<T>(string path) where T : Object
		{
			try
			{
				var obj = InternalEditorUtility.LoadSerializedFileAndForget(path)[0];
				var cast = (T)obj;
				return cast;
			}
			catch (Exception exception)
			{
				throw new Exception($"Failed to load '{path}'. Careful inspection required to prevent losing any data.", exception);
			}
		}

		#endregion
	}

}
