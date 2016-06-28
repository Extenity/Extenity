using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Extenity.OperatingSystem;
using Object = UnityEngine.Object;
using SelectionMode = UnityEditor.SelectionMode;

namespace Extenity.Asset
{

	public static class AssetTools
	{
		#region Prefabs

		public static List<GameObject> FindAllPrefabs()
		{
			var assetFolderPaths = AssetDatabase.GetAllAssetPaths().Where(path => path.EndsWith(".prefab"));
			var assets = assetFolderPaths.Select(item => AssetDatabase.LoadAssetAtPath(item, typeof(GameObject))).Cast<GameObject>().Where(obj => obj != null);
			var list = assets.ToList();
			return list;
		}

		public static List<GameObject> FindAllPrefabsWhere(Func<GameObject, bool> predicate)
		{
			var assetFolderPaths = AssetDatabase.GetAllAssetPaths().Where(path => path.EndsWith(".prefab"));
			var assets = assetFolderPaths.Select(item => AssetDatabase.LoadAssetAtPath(item, typeof(GameObject))).Cast<GameObject>().Where(obj => obj != null).Where(predicate);
			var list = assets.ToList();
			return list;
		}

		public static List<GameObject> FindAllPrefabsContainingComponent<TComponent>() where TComponent : Component
		{
			return FindAllPrefabsWhere(gameObject => gameObject.GetComponent<TComponent>() != null);
		}

		#endregion

		[MenuItem("Assets/Copy Asset Path")]
		public static void CopySelectedAssetPaths()
		{
			var stringBuilder = new StringBuilder();

			foreach (Object obj in Selection.objects)
			{
				if (AssetDatabase.Contains(obj))
				{
					stringBuilder.AppendLine(AssetDatabase.GetAssetPath(obj));
				}
				else
				{
					Debug.LogWarning(string.Format("{0} is not a source asset.", obj));
				}
			}

			var paths = stringBuilder.ToString().Trim();
			Clipboard.SetClipboardText(paths);
		}

		public static string GetAssetPathOfActiveGameObject()
		{
			string path = AssetDatabase.GetAssetPath(Selection.activeObject);
			if (string.IsNullOrEmpty(path))
			{
				return "";
			}

			if (Path.GetExtension(path) != "")
			{
				path = path.Replace(Path.GetFileName(path), "");
			}

			return path;
		}

		public static string GenerateUniqueAssetPathAtSelectedFolder(string fileName)
		{
			try
			{
				// Private implementation of a filenaming function which puts the file at the selected path.
				Type assetDatabase = typeof(AssetDatabase);
				var method = assetDatabase.GetMethod("GetUniquePathNameAtSelectedPath", BindingFlags.NonPublic | BindingFlags.Static);
				return (string)method.Invoke(assetDatabase, new object[] { fileName });
			}
			catch
			{
				// Protection against implementation changes.
				return AssetDatabase.GenerateUniqueAssetPath("Assets/" + fileName);
			}
		}

		public static bool IsFolderAsset(Object obj)
		{
			if (obj == null)
				return false;

			var path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
			if (string.IsNullOrEmpty(path))
				return false;

			return Directory.Exists(path);
		}

		public static string GetSelectedPathOrAssetRootPath()
		{
			var path = GetSelectedPath();
			if (string.IsNullOrEmpty(path))
			{
				return "Assets";
			}
			return path;
		}

		public static string GetSelectedPath()
		{
			var filteredSelection = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets);

			foreach (UnityEngine.Object obj in filteredSelection)
			{
				var path = AssetDatabase.GetAssetPath(obj);
				if (string.IsNullOrEmpty(path))
					continue;

				if (Directory.Exists(path))
				{
					return path;
				}
				if (File.Exists(path))
				{
					return Path.GetDirectoryName(path);
				}
			}
			return "";
		}

		public static T CreateAsset<T>(string assetPath = "", bool pathRelativeToActiveObject = false) where T : ScriptableObject
		{
			// Create scriptable object instance
			T asset = ScriptableObject.CreateInstance<T>();

			// Generate asset file name
			if (string.IsNullOrEmpty(assetPath))
			{
				assetPath = "New" + typeof(T).ToString() + ".asset";
			}

			// Generate full asset path
			string fullPath;
			if (pathRelativeToActiveObject)
			{
				string path = GetAssetPathOfActiveGameObject();
				if (string.IsNullOrEmpty(path))
				{
					path = "Assets";
				}
				fullPath = path + "/" + assetPath;
			}
			else
			{
				fullPath = assetPath;
			}
			fullPath = AssetDatabase.GenerateUniqueAssetPath(fullPath);

			// Create asset
			AssetDatabase.CreateAsset(asset, fullPath);
			AssetDatabase.SaveAssets();

			// Focus on created asset
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = asset;

			return asset;
		}
	}

}
