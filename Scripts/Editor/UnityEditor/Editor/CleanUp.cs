using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extenity.AssetToolbox.Editor;
using Extenity.DataToolbox;
using Extenity.FileSystemToolbox;
using Extenity.ParallelToolbox.Editor;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.Editor
{

	public static class CleanUp
	{
		#region Configuration

		private const string Menu = ExtenityMenu.CleanUp;

		private static readonly string[] IgnoredDirectories =
		{
			".hg",
			".git",
		};

		private static readonly string OrigFileFilter = "*.orig";
		private static readonly string ThumbsDBFileFilter = "thumbs.db";

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
				var subDirectory = subDirectories[iSubDirectory].FixDirectorySeparatorChars();
				if (DirectoryTools.IsDirectoryEmpty(subDirectory))
				{
					var directoryNames = subDirectory.Split(PathTools.DirectorySeparatorChar);
					if (!directoryNames.Any(item => IgnoredDirectories.Contains(item)))
					{
						list.Add(subDirectory);
					}
				}
			}

			return list;
		}

		#endregion

		#region Clear Files and Directories

		private static IEnumerator DoClearFiles(string fileNameFilter, bool refreshAssetDatabase)
		{
			int progressId = Progress.Start("Clear thumbs.db files");
			yield return null;

			var path = Application.dataPath;
			var items = Directory.GetFiles(path, fileNameFilter, SearchOption.AllDirectories);
			for (int i = 0; i < items.Length; i++)
			{
				AssetTools.ManuallyDeleteMetaFileAndAsset(items[i]);
				Progress.Report(progressId, (float)(i + 1) / items.Length);
				yield return null;
			}

			Log.Info($"Cleared '{fileNameFilter}' files: " + items.Length);
			if (items.Length > 0)
				items.LogList();

			if (refreshAssetDatabase)
			{
				AssetDatabase.Refresh();
			}

			Progress.Remove(progressId);
		}

		private static IEnumerator DoClearEmptyDirectories(bool refreshAssetDatabase)
		{
			int progressId = Progress.Start("Clear empty directories");
			yield return null;

			var tryAgain = true;
			var clearedItems = new List<string>();

			while (tryAgain)
			{
				var items = GetEmptyDirectories();
				for (int i = 0; i < items.Count; i++)
				{
					AssetTools.ManuallyDeleteMetaFileAndAsset(items[i]);
					Progress.Report(progressId, (float)(i + 1) / items.Count); // This is not the correct way to display the progress, but it's better than nothing.
					yield return null;
				}

				clearedItems.Combine(items);
				tryAgain = items.Count > 0;
			}

			Log.Info("Cleared empty directories: " + clearedItems.Count);
			if (clearedItems.Count > 0)
				clearedItems.LogList();

			if (refreshAssetDatabase)
			{
				AssetDatabase.Refresh();
			}

			Progress.Remove(progressId);
		}

		private static IEnumerator DoClearAll()
		{
			yield return EditorCoroutineUtility.StartCoroutineOwnerless(DoClearFiles(OrigFileFilter, false));
			yield return EditorCoroutineUtility.StartCoroutineOwnerless(DoClearFiles(ThumbsDBFileFilter, false));
			yield return EditorCoroutineUtility.StartCoroutineOwnerless(DoClearEmptyDirectories(true));
		}

		#endregion

		#region Menu

		[MenuItem(Menu + "/Clear all", priority = ExtenityMenu.CleanUpPriority + 1)]
		public static void ClearAll()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(DoClearAll());
		}

		[MenuItem(Menu + "/Clear .orig files", priority = ExtenityMenu.CleanUpPriority + 21)]
		public static void ClearOrigFiles()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(DoClearFiles(OrigFileFilter, true));
		}

		[MenuItem(Menu + "/Clear thumbs.db files", priority = ExtenityMenu.CleanUpPriority + 22)]
		public static void ClearThumbsDbFiles()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(DoClearFiles(ThumbsDBFileFilter, true));
		}

		[MenuItem(Menu + "/Clear empty directories", priority = ExtenityMenu.CleanUpPriorityEnd)]
		public static void ClearEmptyDirectories()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(DoClearEmptyDirectories(true));
		}

		#endregion
	}

}
