#if !DisableUnityTerrain
using UnityEngine;
using UnityEditor;

namespace Extenity.TerrainToolbox.Editor
{

	public class ResizeTreeWindow : EditorWindow
	{
		[MenuItem("Tools/Terrain/Resize Trees")]
		public static void ShowWindow()
		{
			EditorWindow.GetWindow(typeof(ResizeTreeWindow));
		}

		public static float MinSize = 0.1f;
		public static float MaxSize = 0.15f;

		void OnGUI()
		{
			GUILayout.Label("Resize Trees", EditorStyles.boldLabel);

			MinSize = EditorGUILayout.FloatField("Min Size", MinSize);
			MaxSize = EditorGUILayout.FloatField("Max Size", MaxSize);

			if (GUILayout.Button("Resize Trees", GUILayout.Height(30f)))
			{
				//List<TreeInstance> newTrees = new List<TreeInstance>(Terrain.activeTerrain.terrainData.treePrototypes);
				TreeInstance[] newTrees = Terrain.activeTerrain.terrainData.treeInstances;

				for (int i = 0; i < newTrees.Length; i++)
				{
					var scale = Random.Range(MinSize, MaxSize);
					newTrees[i].heightScale = scale;
					newTrees[i].widthScale = scale;

					// Re asign it
					Terrain.activeTerrain.terrainData.treeInstances = newTrees;
					//Terrain.activeTerrain.terrainData.treeInstances[i] = ti;
				}

				Terrain.activeTerrain.terrainData.RefreshPrototypes();
				Terrain.activeTerrain.Flush();
			}
		}
	}

}
#endif
