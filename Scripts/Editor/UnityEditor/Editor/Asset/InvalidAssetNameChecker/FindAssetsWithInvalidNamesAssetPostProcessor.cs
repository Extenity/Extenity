using UnityEditor;

namespace Extenity.AssetToolbox.Editor
{

	public class FindAssetsWithInvalidNamesAssetPostProcessor : AssetPostprocessor
	{
		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			for (var i = 0; i < importedAssets.Length; i++)
			{
				DisplayDialogIfInvalid(importedAssets[i]);
			}
			for (var i = 0; i < movedAssets.Length; i++)
			{
				DisplayDialogIfInvalid(movedAssets[i]);
			}
		}

		private static void DisplayDialogIfInvalid(string path)
		{
			if (FindAssetsWithInvalidNames.ShouldCheckFileAtPath(path) &&
			    FindAssetsWithInvalidNames.ContainsInvalidChar(path))
			{
				EditorUtility.DisplayDialog("ATTENTION!", $"File name has invalid characters:\n\n{path}\n\nPlease fix the naming before it causes Unity or Repository to implode.", "Ok! Understood!");
				Selection.activeObject = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
			}
		}
	}

}
