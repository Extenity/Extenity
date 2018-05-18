using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
using UnityEditor;

namespace Extenity.MeshToolbox
{

	public static class EditorMeshTools
	{
		/// <summary>
		/// Source: https://answers.unity.com/questions/1213025/separating-submeshes-into-unique-meshes.html
		/// </summary>
		/// <param name="outputAssetPaths">Paths to created mesh assets.</param>
		/// <param name="overwriteExistingMeshAssets">Whether already existing mesh assets will be overwritten. A new numbered filename will be generated if choosen not to overwrite.</param>
		/// <returns>Submesh count.</returns>
		public static int SplitSubmeshes(this MeshRenderer meshRenderer, ref List<string> outputAssetPaths,
			string submeshNamePostfix = "_submesh-", string outputBasePath = null, string outputSubPath = null, bool overwriteExistingMeshAssets = false)
		{
			// TODO: Make sure the resulting mesh data is not kept in scene. It should refer to the created asset file.
			// TODO: Make sure output path is selected as original Mesh asset's path if the mesh is referenced from an asset.
			// TODO: Make sure output path is selected as scene's path if the mesh is NOT referenced from an asset.
			// TODO: Make sure output path is selected as 'Assets' if the mesh is NOT referenced from an asset and the scene is not saved to a file.
			// TODO: Make sure 'overwrite' works as expected when enabled.
			// TODO: Make sure 'overwrite' works as expected when disabled.

			if (!meshRenderer)
				throw new ArgumentNullException(nameof(meshRenderer));
			var sourceGO = meshRenderer.gameObject;
			if (!sourceGO.IsAnInstanceInScene())
				throw new Exception($"Non-scene objects are not supported yet. MeshRenderer '{sourceGO.FullName()}' should be an object in scene.");
			var meshFilter = meshRenderer.GetComponent<MeshFilter>();
			if (!meshFilter)
				throw new Exception($"There is no MeshFilter in object '{sourceGO.FullName()}'.");
			var mesh = meshFilter.sharedMesh;
			if (!mesh)
				throw new Exception($"There is no Mesh assigned to MeshFilter in object '{sourceGO.FullName()}'.");
			var submeshCount = mesh.subMeshCount;
			if (submeshCount <= 1)
				throw new Exception($"There should be more than one submeshes in MeshFilter of object '{sourceGO.FullName()}'.");

			// Decide output base path if not specified by user.
			if (string.IsNullOrEmpty(outputBasePath))
			{
				var meshPath = AssetDatabase.GetAssetPath(mesh);
				if (string.IsNullOrEmpty(meshPath))
				{
					// Original mesh data is not saved in asset, but saved in scene. So we use scene path.
					var scenePath = sourceGO.scene.path;
					if (!string.IsNullOrEmpty(scenePath))
						outputBasePath = Path.GetDirectoryName(scenePath);
					else
						outputBasePath = "Assets"; // Scene is not saved yet. So just use 'Assets' directory.
				}
				else
				{
					// Original mesh is an asset. So we use mesh asset path and save split meshes alongside the original mesh.
					outputBasePath = Path.GetDirectoryName(meshPath);
				}

				if (outputBasePath == null)
					outputBasePath = "";

				if (!string.IsNullOrEmpty(outputSubPath))
				{
					// Specified a sub path, while expecting the base path to be automatically selected as Scene or Mesh path.
					// So we add the sub path as prefix to the Scene or Mesh path.
					outputBasePath = Path.Combine(outputBasePath, outputSubPath);
				}
			}
			else if (!string.IsNullOrEmpty(outputSubPath))
			{
				// Specified both base and sub paths. Combine them.
				outputBasePath = Path.Combine(outputBasePath, outputSubPath);
			}

			var goName = sourceGO.name;

			for (int i = 0; i < submeshCount; i++)
			{
				var splitSubmesh = mesh.SplitSubmesh(i);
				var splitSubmeshName = goName + submeshNamePostfix + i;

				var go = new GameObject(splitSubmeshName);
				go.transform.SetParent(meshRenderer.transform);
				go.transform.ResetTransformToLocalZero();
				go.layer = sourceGO.layer;
				go.tag = sourceGO.tag;
				GameObjectUtility.SetStaticEditorFlags(go, GameObjectUtility.GetStaticEditorFlags(sourceGO));

				var newMeshFilter = go.AddComponent<MeshFilter>();
				newMeshFilter.sharedMesh = splitSubmesh;

				var newMeshRenderer = go.AddComponent<MeshRenderer>();
				EditorUtility.CopySerialized(meshRenderer, newMeshRenderer);
				var material = newMeshRenderer.sharedMaterials[i];
				newMeshRenderer.sharedMaterials = new[] { material };

				// Save mesh to file
				{
					// Create file path
					// TODO: Filename should be checked for invalid characters.
					var filename = splitSubmeshName + ".asset";
					var outputAssetPath = Path.Combine(outputBasePath, filename);

					// Check if a file at that path already exists
					if (File.Exists(outputAssetPath))
					{
						if (!overwriteExistingMeshAssets)
						{
							outputAssetPath = outputAssetPath.GenerateUniqueFilePath();
						}
					}

					// Save mesh
					DirectoryTools.CreateFromFilePath(outputAssetPath);
					AssetDatabase.CreateAsset(splitSubmesh, outputAssetPath);

					if (outputAssetPaths != null)
						outputAssetPaths.Add(outputAssetPath);
				}

			}

			return submeshCount;
		}
	}

}
