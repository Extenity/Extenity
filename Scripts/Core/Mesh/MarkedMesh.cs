using System;
using UnityEngine;
using System.Collections.Generic;

namespace Extenity.MeshToolbox
{

	[Serializable]
	public class MarkedMesh
	{
		#region Initialization

		public MarkedMesh()
		{
			TriangleGroups = new List<TriangleGroup>();
		}

		#endregion

		#region Deinitialization

		/// <summary>
		/// Returns the mesh and releases all memory for this Marked Mesh.
		/// </summary>
		public Mesh Release()
		{
			var mesh = Mesh;
			Mesh = null;

			if (TriangleGroups != null)
			{
				for (int i = 0; i < TriangleGroups.Count; i++)
				{
					TriangleGroups[i].Release();
				}

				TriangleGroups.Clear();
				TriangleGroups = null;
			}

			return mesh;
		}

		#endregion

		#region Mesh

		public Mesh Mesh;

		public void DestroyMesh()
		{
			if (Mesh != null)
			{
				GameObject.DestroyImmediate(Mesh);
				Mesh = null;
			}
		}

		#endregion

		#region Build Mesh

		public void BuildMesh(List<Vector3> vertices, List<Vector3> normals, List<int> indices, Bounds bounds)
		{
			if (Mesh == null)
			{
				Mesh = new Mesh();
			}

			Mesh.vertices = vertices.ToArray();
			Mesh.normals = normals.ToArray();
			Mesh.triangles = indices.ToArray();
			Mesh.bounds = bounds;
		}

		#endregion

		#region Triangle Groups

		public List<TriangleGroup> TriangleGroups;

		#endregion

		#region Get Related Triangle Groups

		public TriangleGroup AddOrGetTriangleGroup(int id)
		{
			var triangleGroup = GetTriangleGroup(id);
			if (triangleGroup == null)
			{
				triangleGroup = new TriangleGroup(id);

				if (TriangleGroups == null)
				{
					TriangleGroups = new List<TriangleGroup>();
				}
				TriangleGroups.Add(triangleGroup);
			}
			return triangleGroup;
		}

		public TriangleGroup GetTriangleGroup(int id)
		{
			if (TriangleGroups != null)
			{
				for (int i = 0; i < TriangleGroups.Count; i++)
				{
					var triangleGroup = TriangleGroups[i];
					if (triangleGroup.Id == id)
					{
						return triangleGroup;
					}
				}
			}
			return null;
		}

		public List<TriangleGroup> GetRelatedTriangleGroups(int triangleIndex)
		{
			EnsureTriangleIndex(triangleIndex);

			var list = new List<TriangleGroup>();
			if (TriangleGroups != null)
			{
				for (int i = 0; i < TriangleGroups.Count; i++)
				{
					var triangleGroup = TriangleGroups[i];
					if (triangleGroup.HasTriangle(triangleIndex))
					{
						list.Add(triangleGroup);
					}
				}
			}
			return list;
		}

		public TriangleGroup GetFirstRelatedTriangleGroup(int triangleIndex)
		{
			EnsureTriangleIndex(triangleIndex);

			if (TriangleGroups != null)
			{
				for (int i = 0; i < TriangleGroups.Count; i++)
				{
					var triangleGroup = TriangleGroups[i];
					if (triangleGroup.HasTriangle(triangleIndex))
					{
						return triangleGroup;
					}
				}
			}
			return null;
		}

		#endregion

		#region Checks

		private void EnsureTriangleIndex(int triangleIndex)
		{
			if (triangleIndex % 3 != 0)
			{
				throw new ArgumentException("Triangle index should be starting index of the triangle, which is always multiples of 3.", "triangleIndex");
			}
			if (Mesh == null)
			{
				throw new NullReferenceException("Mesh was not specified for MarkedMesh.");
			}
			if (Mesh.triangles == null)
			{
				throw new NullReferenceException("Mesh triangle list is null.");
			}
			if (triangleIndex < 0 || triangleIndex >= Mesh.triangles.Length)
			{
				throw new ArgumentOutOfRangeException("triangleIndex", triangleIndex, "Triangle index '" + triangleIndex + "' is out of range.");
			}
		}

		#endregion
	}

}
