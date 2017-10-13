using System;
using System.IO;
using System.Linq;
using Extenity.DataToolbox;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Extenity.RenderingToolbox
{

	public static class LightmapTools
	{
		#region LightmapTools

		public static void TryToAssignExistingLightingDataOfActiveScene()
		{
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

			var scenePath = SceneManager.GetActiveScene().path;
			var lightmapDirectoryPath = Path.Combine(Path.GetDirectoryName(scenePath), Path.GetFileNameWithoutExtension(scenePath)).FixDirectorySeparatorChars('/');
			var newFiles = Directory.GetFiles(lightmapDirectoryPath, "Lightmap*light.exr", SearchOption.TopDirectoryOnly).OrderBy(item => item).ToArray();
			var allLightmapFiles = Directory.GetFiles(lightmapDirectoryPath, "Lightmap*.exr", SearchOption.TopDirectoryOnly).OrderBy(item => item).ToArray();

			if (newFiles.Length != allLightmapFiles.Length)
			{
				throw new Exception("There are existing but unsupported lightmap files. See and figure out what to do with these files:\n" + (allLightmapFiles.Where(item => !newFiles.Contains(item)).ToList().Serialize('\n')));
			}

			var newLightmapData = new LightmapData[newFiles.Length];
			for (var i = 0; i < newFiles.Length; i++)
			{
				var newFile = newFiles[i];
				var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(newFile);
				if (!texture)
				{
					throw new Exception("Failed to load lightmap texture: " + newFile);
				}

				var data = new LightmapData();
				data.lightmapColor = texture;
				//data.lightmapDir =
				//data.shadowMask =
				newLightmapData[i] = data;
			}
			LightmapSettings.lightmaps = newLightmapData;

			// Save aggressively. Don't know how much of these needed but save them all anyway.
			EditorSceneManager.MarkAllScenesDirty();
			EditorSceneManager.SaveOpenScenes();
			AssetDatabase.SaveAssets();
		}

		#endregion
	}

}
