using System.Linq;
using Extenity.DataToolbox;
using Extenity.UnityEditorToolbox;
using UnityEditor;

namespace Extenity.PainkillerToolbox.Editor
{

	public static class FindAssetsWithInvalidNames
	{
		#region Configuration

		/// <summary>
		/// File paths matching this filter will be excluded from invalid name checking. Feel free to modify this filter
		/// for project specific needs.
		/// </summary>
		public static StringFilter ExcludeFilter = new StringFilter(
			// new StringFilterEntry(StringFilterType.StartsWith, "Packages/"), Commented out for future needs. Don't hesitate to exclude files in Packages directory if required. Packages are expected to be tested and working alright.
			new StringFilterEntry(StringFilterType.StartsWith, "C:") // Exclude Unity installation files. // TODO: Find a cross-platform supported, not-hardcoded approach.
			);

		private const string ValidCharactersString = "0123456789" + "!/_.,-+=$%&()[]{}@#~'`^ " + "ABCDEFGHIJKLMNOPQRSTUVWXYZ" + "abcdefghijklmnopqrstuvwxyz";
		private static readonly char[] ValidCharacters = ValidCharactersString.ToCharArray();

		#endregion

		#region Menu

		[MenuItem(ExtenityMenu.Painkiller + "Asset Name Checker/Find Assets With Invalid Names", priority = ExtenityMenu.PainkillerPriority + 7)]
		public static void _FindAssetsWithInvalidNames()
		{
			Run();
		}

		#endregion

		#region Run

		public static void Run()
		{
			var assetPaths = AssetDatabase.GetAllAssetPaths()
			                              .Where(ShouldCheckFileAtPath)
			                              .ToArray();
			var found = false;

			for (int i = 0; i < assetPaths.Length; i++)
			{
				if (ContainsInvalidChar(assetPaths[i]))
				{
					Log.Warning($"{assetPaths[i]} asset has invalid name.");
					found = true;
				}
			}

			if (!found)
			{
				Log.Info($"All {assetPaths.Length} asset names are valid.");
			}
		}

		#endregion

		#region Operations

		public static bool ShouldCheckFileAtPath(string path)
		{
			return !ExcludeFilter.IsMatching(path);
		}

		public static bool ContainsInvalidChar(string path)
		{
			for (var i = 0; i < path.Length; i++)
			{
				if (!ValidCharacters.Contains(path[i]))
					return true;
			}

			return false;
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(FindAssetsWithInvalidNames));

		#endregion
	}

}
