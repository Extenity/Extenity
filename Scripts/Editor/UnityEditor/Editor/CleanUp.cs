using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Extenity.AssetToolbox.Editor;
using Extenity.DataToolbox;
using Extenity.DataToolbox.Editor;
using Extenity.FileSystemToolbox;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.Editor
{

	public static class CleanUp
	{
		#region Configuration

		private const string Menu = ExtenityMenu.CleanUp;

		public static string[] IgnoredDirectories =
		{
			".hg",
			".git",
		};

		private static readonly string OrigFileFilter = "*.orig";
		private static readonly string ThumbsDBFileFilter = "thumbs.db";

		#endregion

		#region Cleanup at Editor launch

		private static BoolEditorPref EnableRunAtEditorLaunch => new("Extenity.RunCleanupAtEditorLaunch",
		                                                             PathHashPostfix.Yes,
		                                                             true,
		                                                             EditorPrefLogOptions.NoLogging);

		[InitializeOnEditorLaunchMethod]
		private static async void RunAtEditorLaunch()
		{
			if (EnableRunAtEditorLaunch.Value)
			{
				await DoClearAll();
			}
		}

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

		private static async Task DoClearFiles(string fileNameFilter, bool refreshAssetDatabase)
		{
			int progressId = Progress.Start("Clear thumbs.db files");
			await Task.Yield();

			var path = Application.dataPath;
			var items = Directory.GetFiles(path, fileNameFilter, SearchOption.AllDirectories);
			for (int i = 0; i < items.Length; i++)
			{
				AssetDatabaseTools.ManuallyDeleteMetaFileAndAsset(items[i]);
				Progress.Report(progressId, (float)(i + 1) / items.Length);
				await Task.Yield();
			}

			if (items.Length > 0)
			{
				items.LogList($"Cleared '{items.Length}' files with filter '{fileNameFilter}':");

				if (refreshAssetDatabase)
				{
					AssetDatabase.Refresh();
				}
			}

			Progress.Remove(progressId);
			await Task.Yield();
		}

		private static async Task DoClearEmptyDirectories(bool refreshAssetDatabase)
		{
			int progressId = Progress.Start("Clear empty directories");
			await Task.Yield();

			var tryAgain = true;
			var clearedItems = new List<string>();

			while (tryAgain)
			{
				var items = GetEmptyDirectories();
				for (int i = 0; i < items.Count; i++)
				{
					AssetDatabaseTools.ManuallyDeleteMetaFileAndAsset(items[i]);
					clearedItems.AddSorted(items[i]);
					Progress.Report(progressId, (float)(i + 1) / items.Count); // This is not the correct way to display the progress, but it's better than nothing.
					await Task.Yield();
				}

				tryAgain = items.Count > 0;
			}

			if (clearedItems.Count > 0)
			{
				clearedItems.LogList($"Cleared '{clearedItems.Count}' empty directories:");

				if (refreshAssetDatabase)
				{
					AssetDatabase.Refresh();
				}
			}

			Progress.Remove(progressId);
			await Task.Yield();
		}

		private static async Task DoClearAll()
		{
			await DoClearFiles(OrigFileFilter, false);
			await DoClearFiles(ThumbsDBFileFilter, false);
			await DoClearEmptyDirectories(true);
		}

		#endregion

		#region Menu

		[MenuItem(Menu + "Clear all", priority = ExtenityMenu.CleanUpPriority + 1)]
		public static async void ClearAll()
		{
			await DoClearAll();
			Log.Info("Cleanup finished.");
		}

		[MenuItem(Menu + "Clear .orig files", priority = ExtenityMenu.CleanUpPriority + 21)]
		public static async void ClearOrigFiles()
		{
			await DoClearFiles(OrigFileFilter, true);
			Log.Info("Cleanup finished.");
		}

		[MenuItem(Menu + "Clear thumbs.db files", priority = ExtenityMenu.CleanUpPriority + 22)]
		public static async void ClearThumbsDbFiles()
		{
			await DoClearFiles(ThumbsDBFileFilter, true);
			Log.Info("Cleanup finished.");
		}

		[MenuItem(Menu + "Clear empty directories", priority = ExtenityMenu.CleanUpPriority + 23)]
		public static async void ClearEmptyDirectories()
		{
			await DoClearEmptyDirectories(true);
			Log.Info("Cleanup finished.");
		}

		[MenuItem(Menu + "Auto Cleanup at Editor launch/Enable", priority = ExtenityMenu.CleanUpPriority + 41)]
		public static void EnableAutoCleanUpAtEditorLaunch()
		{
			EnableRunAtEditorLaunch.Value = true;
		}

		[MenuItem(Menu + "Auto Cleanup at Editor launch/Enable", validate = true)]
		public static bool EnableAutoCleanUpAtEditorLaunch_Validate()
		{
			return !EnableRunAtEditorLaunch.Value;
		}

		[MenuItem(Menu + "Auto Cleanup at Editor launch/Disable", priority = ExtenityMenu.CleanUpPriorityEnd)]
		public static void DisableAutoCleanUpAtEditorLaunch()
		{
			EnableRunAtEditorLaunch.Value = false;
		}

		[MenuItem(Menu + "Auto Cleanup at Editor launch/Disable", validate = true)]
		public static bool DisableAutoCleanUpAtEditorLaunch_Validate()
		{
			return EnableRunAtEditorLaunch.Value;
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(CleanUp));

		#endregion
	}

}
