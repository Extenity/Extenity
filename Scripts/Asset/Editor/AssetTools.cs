using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections;

public static class AssetTools
{
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
			System.Type assetDatabase = typeof(UnityEditor.AssetDatabase);
			var method = assetDatabase.GetMethod("GetUniquePathNameAtSelectedPath",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
			return (string)method.Invoke(assetDatabase, new object[] { fileName });
		}
		catch
		{
			// Protection against implementation changes.
			return UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/" + fileName);
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
