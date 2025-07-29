#if UNITY_5_3_OR_NEWER

using UnityEngine;
using System.Collections.Generic;
using Extenity.MathToolbox;

namespace Extenity.MeshToolbox
{

	public class MarkedMeshBuilder
	{
		#region Initialization

		public void WarmupForBuild(int initialVerticesCapacity = 0, int initialIndicesCapacity = 0)
		{
			if (MarkedMesh == null)
			{
				MarkedMesh = new MarkedMesh();
			}
			if (Vertices == null)
			{
				Vertices = new List<Vector3>(initialVerticesCapacity);
				Bounds.Reset(); // Reset bounds along vertices so that bounds will be enlarged as new vertices added.
			}
			else if (Vertices.Count == 0)
			{
				// Bounds should be reset even though Vertices was initialized with zero entries.
				Bounds.Reset(); // Reset bounds along vertices so that bounds will be enlarged as new vertices added.
			}
			if (Normals == null)
			{
				Normals = new List<Vector3>(initialVerticesCapacity);
			}
			if (Indices == null)
			{
				Indices = new List<int>(initialIndicesCapacity);
			}
		}

		#endregion

		#region Deinitialization

		public Mesh Release(bool alsoReleaseMarkedMesh = true)
		{
			Mesh mesh = MarkedMesh != null
				? MarkedMesh.Mesh
				: null;

			if (MarkedMesh != null && alsoReleaseMarkedMesh)
			{
				MarkedMesh.Release();
				MarkedMesh = null;
			}

			Vertices.Clear();
			Vertices = null;
			Normals.Clear();
			Normals = null;
			Indices.Clear();
			Indices = null;

			return mesh;
		}

		public void ReleaseAndDestroyMesh()
		{
			if (MarkedMesh != null)
			{
				MarkedMesh.DestroyMesh();
			}

			Release();
		}

		#endregion

		#region Mesh

		public MarkedMesh MarkedMesh;

		public void BuildMesh()
		{
			MarkedMesh.BuildMesh(Vertices, Normals, Indices, Bounds);
		}

		#endregion

		#region Vertices And Triangles

		public List<Vector3> Vertices;
		public List<Vector3> Normals;
		public List<int> Indices;
		public Bounds Bounds;

		#endregion

		#region Create Triangle

		/// <summary>
		/// Creates a new triangle using existing vertices.
		/// </summary>
		/// <param name="triangleVertexIndex1"></param>
		/// <param name="triangleVertexIndex2"></param>
		/// <param name="triangleVertexIndex3"></param>
		/// <returns>Return the index of Indices list entry for added triangle's first vertex index.</returns>
		public int AddTriangle(int triangleVertexIndex1, int triangleVertexIndex2, int triangleVertexIndex3)
		{
			WarmupForBuild();
			Indices.Add(triangleVertexIndex1);
			Indices.Add(triangleVertexIndex2);
			Indices.Add(triangleVertexIndex3);
			return Indices.Count - 3;
		}

		/// <summary>
		/// Creates a new triangle by adding new vertices and associating triangle indices to these vertices. Normal of triangle is automatically calculated. Use other AddTriangle overrides if you want to specify a custom normal.
		/// </summary>
		/// <param name="vertex1"></param>
		/// <param name="vertex2"></param>
		/// <param name="vertex3"></param>
		/// <returns>Return the index of Indices list entry for added triangle's first vertex index.</returns>
		public int AddTriangle(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
		{
			var normal = Vector3.Cross(vertex2 - vertex1, vertex3 - vertex1).normalized;
			return AddTriangle(vertex1, vertex2, vertex3, normal);
		}

		/// <summary>
		/// Creates a new triangle by adding new vertices and associating triangle indices to these vertices.
		/// </summary>
		/// <param name="vertex1"></param>
		/// <param name="vertex2"></param>
		/// <param name="vertex3"></param>
		/// <returns>Return the index of Indices list entry for added triangle's first vertex index.</returns>
		public int AddTriangle(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3, Vector3 normal)
		{
			WarmupForBuild();

			if (!MathTools.IsValidTriangle(vertex1, vertex2, vertex3))
				return -1;

			Normals.Add(normal);
			Normals.Add(normal);
			Normals.Add(normal);
			Vertices.Add(vertex1);
			Vertices.Add(vertex2);
			Vertices.Add(vertex3);
			var triangleIndexStart = Vertices.Count - 3;
			Indices.Add(triangleIndexStart);
			Indices.Add(triangleIndexStart + 1);
			Indices.Add(triangleIndexStart + 2);
			Bounds.Encapsulate(vertex1);
			Bounds.Encapsulate(vertex2);
			Bounds.Encapsulate(vertex3);
			return Indices.Count - 3;
		}

		#endregion

		#region Create Triangle Group

		public TriangleGroup AddOrGetTriangleGroup(int id)
		{
			return MarkedMesh.AddOrGetTriangleGroup(id);
		}

		#endregion
	}

}

#endif
