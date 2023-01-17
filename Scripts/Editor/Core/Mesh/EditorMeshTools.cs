using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Extenity.DataToolbox;
using Extenity.FileSystemToolbox;
using Extenity.GameObjectToolbox;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Extenity.MeshToolbox
{

	public enum SubmeshSplitOverwriteRule
	{
		AlwaysRename,
		AlwaysOverwrite,
	}

	public class SubmeshSplitResult
	{
		#region Data

		public int OperationCount = 0;
		public int CreatedSubmeshAssetCount = 0;

		public List<string> OutputAssetPaths = new List<string>();

		/// <summary>
		/// Key: Source mesh asset path.
		/// Value: How many times the mesh asset is processed.
		/// </summary>
		public Dictionary<string, int> ProcessedMeshAssets = new Dictionary<string, int>();

		/// <summary>
		/// Key: Source mesh asset path.
		/// Value: How many times it is overwritten.
		/// </summary>
		public Dictionary<string, int> OverwrittenAssets = new Dictionary<string, int>();

		/// <summary>
		/// Key: Source mesh asset path.
		/// Value: Renamed output mesh asset paths.
		/// </summary>
		public Dictionary<string, List<string>> RenamedAssets = new Dictionary<string, List<string>>();

		#endregion

		#region Tools

		public void LogDetails()
		{
			var stringBuilder = new StringBuilder();

			stringBuilder.AppendLine($"Submesh split result of {OperationCount.ToStringWithEnglishPluralPostfix("time", '\'')}:");
			stringBuilder.AppendLine($"Created submesh asset count: {CreatedSubmeshAssetCount}");

			stringBuilder.AppendLine();
			stringBuilder.AppendLine($"Output asset paths ({OutputAssetPaths.Count}):");
			foreach (var outputAssetPath in OutputAssetPaths)
			{
				stringBuilder.AppendLine($"\t{outputAssetPath}");
			}

			stringBuilder.AppendLine();
			stringBuilder.AppendLine($"Processed mesh assets ({ProcessedMeshAssets.Count}):");
			foreach (var asset in ProcessedMeshAssets)
			{
				stringBuilder.AppendLine($"\t{asset.Value.ToStringWithEnglishPluralPostfix("time")}:\t{asset.Key}");
			}

			stringBuilder.AppendLine();
			stringBuilder.AppendLine($"Overwritten assets ({OverwrittenAssets.Count}):");
			foreach (var asset in OverwrittenAssets)
			{
				stringBuilder.AppendLine($"\t{asset.Value.ToStringWithEnglishPluralPostfix("time")}:\t{asset.Key}");
			}

			stringBuilder.AppendLine();
			stringBuilder.AppendLine($"Renamed assets ({RenamedAssets.Count}):");
			foreach (var asset in RenamedAssets)
			{
				stringBuilder.AppendLine($"\t'{asset.Key}' renamed to ({asset.Value.Count}):");
				foreach (var renamed in asset.Value)
				{
					stringBuilder.AppendLine($"\t\t{renamed}");
				}
			}

			Log.Info(stringBuilder.ToString());
		}

		#endregion
	}

	public class SubmeshSplitMeshDatabase
	{
		public class Entry
		{
			public readonly Mesh Mesh;

			#region Initialization

			public Entry(Mesh mesh)
			{
				if (!mesh)
					throw new ArgumentNullException(nameof(mesh));

				Mesh = mesh;
			}

			#endregion

			#region Comparison

			// Cached comparison data
			private Vector3[] vertices;
			private int[] triangles;
			private Vector3[] normals;
			private Color32[] colors32;
			private Vector2[] uv;
			private Vector2[] uv2;
			private Vector2[] uv3;
			private Vector2[] uv4;

			private void FillCachedComparisonDataIfNeeded()
			{
				if (vertices != null)
					return;

				vertices = Mesh.vertices;
				triangles = Mesh.triangles;
				normals = Mesh.normals;
				colors32 = Mesh.colors32;
				uv = Mesh.uv;
				uv2 = Mesh.uv2;
				uv3 = Mesh.uv3;
				uv4 = Mesh.uv4;
			}

			public bool IsMeshIdentical(Entry other)
			{
				FillCachedComparisonDataIfNeeded();
				other.FillCachedComparisonDataIfNeeded();

				// Quick comparison
				if (vertices.Length != other.vertices.Length ||
					triangles.Length != other.triangles.Length
				)
				{
					return false;
				}

				return
					vertices.SequenceEqual(other.vertices) &&
					triangles.SequenceEqual(other.triangles) &&
					normals.SequenceEqual(other.normals) &&
					colors32.SequenceEqual(other.colors32) &&
					uv.SequenceEqual(other.uv) &&
					uv2.SequenceEqual(other.uv2) &&
					uv3.SequenceEqual(other.uv3) &&
					uv4.SequenceEqual(other.uv4);
			}

			#endregion
		}

		#region Entries

		public List<Entry> Entries = new List<Entry>();

		public void Clear()
		{
			Entries.Clear();
		}

		public void AddMesh(Mesh mesh)
		{
			if (!mesh)
				throw new ArgumentNullException(nameof(mesh));

			Entries.Add(new Entry(mesh));
		}

		public Mesh GetIdenticalMesh(Mesh mesh)
		{
			if (!mesh)
				throw new ArgumentNullException(nameof(mesh));

			var otherMeshEntry = new Entry(mesh);

			for (var i = 0; i < Entries.Count; i++)
			{
				if (Entries[i].IsMeshIdentical(otherMeshEntry))
					return Entries[i].Mesh;
			}
			return null;
		}

		#endregion
	}

	public static class EditorMeshTools
	{
		/// <summary>
		/// Source: https://answers.unity.com/questions/1213025/separating-submeshes-into-unique-meshes.html
		/// </summary>
		/// <param name="result">Detailed information about splitting operation. The same SubmeshSplitResult can be used consecutively for multiple operations, so that all results can be appended and merged together.</param>
		/// <param name="overwriteExistingMeshAssetsRule">Whether already existing mesh assets will be overwritten. See SubmeshSplitOverwriteRule for details.</param>
		/// <param name="database">Created mesh is first checked for duplications in database if a database is specified. The mesh in database will be used instead of creating a duplicate. Also all created meshes will be added into the database.</param>
		/// <returns>Submesh count.</returns>
		public static int SplitSubmeshes(this MeshRenderer meshRenderer, ref SubmeshSplitResult result,
			string submeshNamePostfix = "_submesh-", string outputBasePath = null, string outputSubPath = null,
			SubmeshSplitOverwriteRule overwriteExistingMeshAssetsRule = SubmeshSplitOverwriteRule.AlwaysRename,
			SubmeshSplitMeshDatabase database = null)
		{
			// TODO: Make sure the resulting mesh data is not kept in scene. It should refer to the created asset file.
			// TODO: Make sure output path is selected as original Mesh asset's path if the mesh is referenced from an asset.
			// TODO: Make sure output path is selected as scene's path if the mesh is NOT referenced from an asset.
			// TODO: Make sure output path is selected as 'Assets' if the mesh is NOT referenced from an asset and the scene is not saved to a file.
			// TODO: Make sure 'overwrite' works as expected for all cases.

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

			result.OperationCount++;

			var meshPath = AssetDatabase.GetAssetPath(mesh);
			var isMeshAsset = !string.IsNullOrEmpty(meshPath); // This means original mesh data is not saved in asset, but saved in scene

			if (isMeshAsset)
			{
				result.ProcessedMeshAssets.AddOrIncrease(meshPath);
			}

			// Use mesh asset file name if the mesh data is kept in a file.
			// Otherwise use gameobject name of MeshRenderer instead.
			var baseFileName = isMeshAsset
				? Path.GetFileNameWithoutExtension(meshPath)
				: sourceGO.name;

			// Decide output base path if not specified by user.
			if (string.IsNullOrEmpty(outputBasePath))
			{
				if (!isMeshAsset)
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

			for (int i = 0; i < submeshCount; i++)
			{
				var splitSubmesh = mesh.SplitSubmesh(i);
				var splitSubmeshName = baseFileName + submeshNamePostfix + i;

				// See if database has the same mesh. Use the one in database to prevent duplications.
				var foundInDatabase = false;
				if (database != null)
				{
					var identicalMesh = database.GetIdenticalMesh(splitSubmesh);
					if (identicalMesh)
					{
						foundInDatabase = true;

						// Destroy the created mesh. We won't be using it anywhere.
						Object.DestroyImmediate(splitSubmesh);

						splitSubmesh = identicalMesh;
					}
				}

				// Save mesh to file
				if (!foundInDatabase)
				{
					// Create file path
					// TODO: Filename should be checked for invalid characters.
					var filename = splitSubmeshName + ".asset";
					var outputAssetPath = Path.Combine(outputBasePath, filename);

					// Check if a file at that path already exists
					var alreadyExists = File.Exists(outputAssetPath);
					if (alreadyExists)
					{
						switch (overwriteExistingMeshAssetsRule)
						{
							case SubmeshSplitOverwriteRule.AlwaysRename:
								{
									var newOutputAssetPath = outputAssetPath.GenerateUniqueFilePath();
									result.RenamedAssets.AddToList(outputAssetPath, newOutputAssetPath);
									outputAssetPath = newOutputAssetPath;
								}
								break;
							case SubmeshSplitOverwriteRule.AlwaysOverwrite:
								{
									// Keep output path intact. AssetDatabase.CreateAsset will overwrite.
									result.OverwrittenAssets.AddOrIncrease(outputAssetPath);
								}
								break;
							default:
								throw new ArgumentOutOfRangeException(nameof(overwriteExistingMeshAssetsRule), overwriteExistingMeshAssetsRule, null);
						}
					}

					// Save mesh
					DirectoryTools.CreateFromFilePath(outputAssetPath);
					AssetDatabase.CreateAsset(splitSubmesh, outputAssetPath);
					splitSubmesh = AssetDatabase.LoadAssetAtPath<Mesh>(outputAssetPath);
					if (!splitSubmesh)
						throw new Exception($"Failed to save and load mesh asset to file at path '{outputAssetPath}'.");
					result.CreatedSubmeshAssetCount++;
					result.OutputAssetPaths.Add(outputAssetPath);
					if (database != null)
						database.AddMesh(splitSubmesh);
				}

				// Create sub objects
				{
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
				}
			}

			return submeshCount;
		}
	}

}
