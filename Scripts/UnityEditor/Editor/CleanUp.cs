using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Extenity.DataToolbox;
using Extenity.DebugToolbox;
using UnityEditor;

namespace Extenity.UnityEditorToolbox
{

	public static class CleanUp
	{
		#region Configuration

		private const string MenuPath = "Tools/Clean Up";

		#endregion

		#region Empty directories

		private static List<string> GetEmptyDirectories()
		{
			return GetEmptyDirectories(Application.dataPath);
		}

		private static List<string> GetEmptyDirectories(string path)
		{
			var list = new List<string>();
			var subDirectories = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);

			for (int iSubDirectory = 0; iSubDirectory < subDirectories.Length; iSubDirectory++)
			{
				var subDirectory = subDirectories[iSubDirectory];
				if (DirectoryTools.IsDirectoryEmpty(subDirectory))
				{
					list.Add(subDirectory);
				}
			}

			return list;
		}

		#endregion

		#region Get files

		public static string[] GetOrigFiles()
		{
			return GetOrigFiles(Application.dataPath);
		}

		public static string[] GetOrigFiles(string path)
		{
			return Directory.GetFiles(path, "*.orig", SearchOption.AllDirectories);
		}

		public static string[] GetThumbsDbFiles()
		{
			return GetThumbsDbFiles(Application.dataPath);
		}

		public static string[] GetThumbsDbFiles(string path)
		{
			return Directory.GetFiles(path, "thumbs.db", SearchOption.AllDirectories);
		}

		#endregion

		#region Menu

		[MenuItem(MenuPath + "/Clear all")]
		public static void ClearAll()
		{
			ClearOrigFiles();
			ClearThumbsDbFiles();
			ClearEmptyDirectories();
		}

		[MenuItem(MenuPath + "/Clear .orig files")]
		public static void ClearOrigFiles()
		{
			var items = GetOrigFiles();
			for (int i = 0; i < items.Length; i++)
			{
				EditorTools.DeleteMetaFileAndItem(items[i]);
			}

			Debug.Log("Cleared '.orig' files: " + items.Length);
			if (items.Length > 0)
				items.LogList();

			AssetDatabase.Refresh();
		}

		[MenuItem(MenuPath + "/Clear thumbs.db files")]
		public static void ClearThumbsDbFiles()
		{
			var items = GetThumbsDbFiles();
			for (int i = 0; i < items.Length; i++)
			{
				EditorTools.DeleteMetaFileAndItem(items[i]);
			}

			Debug.Log("Cleared 'thumbs.db' files: " + items.Length);
			if (items.Length > 0)
				items.LogList();

			AssetDatabase.Refresh();
		}

		[MenuItem(MenuPath + "/Clear empty directories")]
		public static void ClearEmptyDirectories()
		{
			var tryAgain = true;
			var clearedItems = new List<string>();

			while (tryAgain)
			{
				var items = GetEmptyDirectories();
				for (int i = 0; i < items.Count; i++)
				{
					EditorTools.DeleteMetaFileAndItem(items[i]);
				}

				clearedItems.Combine(items);
				tryAgain = items.Count > 0;
			}

			Debug.Log("Cleared empty directories: " + clearedItems.Count);
			if (clearedItems.Count > 0)
				clearedItems.LogList();

			AssetDatabase.Refresh();
		}

		#endregion
	}

}
