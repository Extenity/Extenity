#if UNITY_5_3_OR_NEWER

using UnityEngine;
using System.Collections.Generic;

namespace Extenity.MeshToolbox
{

	public static class MeshTools
	{
		#region Create Plane Mesh

		public static Mesh CreatePlaneXZ(float size = 1f)
		{
			float halfSize = size / 2f;
			Mesh mesh = new Mesh();

			Vector3[] vertices = new Vector3[4];
			Vector2[] UVs = new Vector2[4];
			int[] triangles = new int[6];

			vertices[0] = new Vector3(-halfSize, 0f, -halfSize);
			vertices[1] = new Vector3(-halfSize, 0f, halfSize);
			vertices[2] = new Vector3(halfSize, 0f, halfSize);
			vertices[3] = new Vector3(halfSize, 0f, -halfSize);

			UVs[0] = new Vector2(0f, 0f);
			UVs[1] = new Vector2(0f, 1f);
			UVs[2] = new Vector2(1f, 1f);
			UVs[3] = new Vector2(1f, 0f);

			triangles[0] = 0;
			triangles[1] = 1;
			triangles[2] = 2;
			triangles[3] = 0;
			triangles[4] = 2;
			triangles[5] = 3;

			mesh.vertices = vertices;
			mesh.uv = UVs;
			mesh.triangles = triangles;
			return mesh;
		}

		#endregion

		#region Create Pie Mesh

		public static void CreatePieMesh(ref Mesh mesh, float radius, float angle, float stepAngle)
		{
			if (mesh == null)
			{
				mesh = new Mesh();
			}

			Vector3[] vertices;
			int[] triangles;

			var erroneous = angle < 0.0001f || stepAngle < 0.0001f;
			if (erroneous)
			{
				vertices = mesh.ReuseOrCreateVertices(0);
				triangles = mesh.ReuseOrCreateTriangles(0);
			}
			else
			{
				var pointsOnCircleCount = Mathf.RoundToInt(angle / stepAngle) + 1;
				if (pointsOnCircleCount < 2)
				{
					pointsOnCircleCount = 2;
				}

				var pieCount = pointsOnCircleCount - 1;
				var totalVerticesCount = pointsOnCircleCount + 1; // +1 is the central point
				var totalTrianglesCount = pieCount;
				var startAngleRad = (90f - angle / 2f) * Mathf.Deg2Rad;
				var angleIncrementRad = (angle / pieCount) * Mathf.Deg2Rad;

				vertices = mesh.ReuseOrCreateVertices(totalVerticesCount);
				triangles = mesh.ReuseOrCreateTriangles(3 * totalTrianglesCount);

				vertices[0] = Vector3.zero; // Center

				int iPie;
				float currentAngleRad;
				for (iPie = 0; iPie < pieCount; iPie++)
				{
					currentAngleRad = startAngleRad + angleIncrementRad * iPie;
					var pieTriangleStartIndex = iPie * 3;
					vertices[1 + iPie] = new Vector3(Mathf.Cos(currentAngleRad) * radius, 0f, Mathf.Sin(currentAngleRad) * radius);

					triangles[pieTriangleStartIndex + 0] = 0;
					triangles[pieTriangleStartIndex + 1] = 1 + iPie + 1;
					triangles[pieTriangleStartIndex + 2] = 1 + iPie;
				}
				currentAngleRad = startAngleRad + angleIncrementRad * iPie;
				vertices[1 + iPie] = new Vector3(Mathf.Cos(currentAngleRad) * radius, 0f, Mathf.Sin(currentAngleRad) * radius);
			}

			mesh.triangles = null;
			mesh.vertices = vertices;
			mesh.triangles = triangles;
			mesh.RecalculateBounds();
		}


		#endregion

		#region Create Spline Mesh

		public static Mesh CreateHorizontalMeshFromSpline(List<Vector3> points, float width)
		{
			Vector3[] vertices = new Vector3[points.Count * 2];
			Vector3[] normals = new Vector3[vertices.Length];
			Vector2[] UVs = new Vector2[vertices.Length];
			int[] triangles = new int[vertices.Length * 3 - 6];

			float halfWidth = width * 0.5f;

			for (int i = 0; i < points.Count; i++)
			{
				Vector3 direction = (points[(i + 1) % points.Count] - points[i]).normalized;
				Vector3 direction2 = (points[i] - points[(i - 1 + points.Count) % points.Count]).normalized;
				direction = ((direction + direction2) / 2).normalized;

				vertices[i * 2] = points[i] + Quaternion.Euler(0, 90, 0) * direction * halfWidth;
				vertices[i * 2 + 1] = points[i] + Quaternion.Euler(0, -90, 0) * direction * halfWidth;
			}

			for (int i = 0; i < triangles.Length; i += 6)
			{
				triangles[i] = (i / 3) % vertices.Length;
				triangles[i + 1] = (i / 3 + 1) % vertices.Length;
				triangles[i + 2] = (i / 3 + 2) % vertices.Length;
				triangles[i + 3] = (i / 3 + 3) % vertices.Length;
				triangles[i + 4] = (i / 3 + 2) % vertices.Length;
				triangles[i + 5] = (i / 3 + 1) % vertices.Length;
			}

			for (int i = 0; i < normals.Length; i++)
			{
				normals[i] = Vector3.up;
			}

			for (int i = 0; i < UVs.Length; i += 2)
			{
				UVs[i] = new Vector2((float)(i + 1) / UVs.Length, 0);
				UVs[i + 1] = new Vector2((float)(i + 1) / UVs.Length, 1);
			}

			Mesh mesh = new Mesh();
			mesh.vertices = vertices;
			mesh.triangles = triangles;
			mesh.normals = normals;
			mesh.uv = UVs;
			return mesh;
		}

		public static Mesh CreateHorizontalMeshFromClosedSpline(List<Vector3> points, float width)
		{
			Vector3[] vertices = new Vector3[points.Count * 2 + 2];
			Vector3[] normals = new Vector3[vertices.Length];
			Vector2[] UVs = new Vector2[vertices.Length];
			int[] triangles = new int[vertices.Length * 3];

			float halfWidth = width * 0.5f;

			for (int i = 0; i < points.Count + 1; i++)
			{
				Vector3 direction = (points[(i + 1) % points.Count] - points[i % points.Count]).normalized;
				Vector3 direction2 = (points[i % points.Count] - points[(i - 1 + points.Count) % points.Count]).normalized;
				direction = ((direction + direction2) / 2).normalized;

				vertices[i * 2] = points[i % points.Count] + Quaternion.Euler(0, 90, 0) * direction * halfWidth;
				vertices[i * 2 + 1] = points[i % points.Count] + Quaternion.Euler(0, -90, 0) * direction * halfWidth;
			}

			for (int i = 0; i < triangles.Length; i += 6)
			{
				triangles[i] = (i / 3) % vertices.Length;
				triangles[i + 1] = (i / 3 + 1) % vertices.Length;
				triangles[i + 2] = (i / 3 + 2) % vertices.Length;
				triangles[i + 3] = (i / 3 + 3) % vertices.Length;
				triangles[i + 4] = (i / 3 + 2) % vertices.Length;
				triangles[i + 5] = (i / 3 + 1) % vertices.Length;
			}

			for (int i = 0; i < normals.Length; i++)
			{
				normals[i] = Vector3.up;
			}

			for (int i = 0; i < UVs.Length; i += 2)
			{
				UVs[i] = new Vector2((float)(i + 1) / UVs.Length, 0);
				UVs[i + 1] = new Vector2((float)(i + 1) / UVs.Length, 1);
			}

			Mesh mesh = new Mesh();
			mesh.vertices = vertices;
			mesh.triangles = triangles;
			mesh.normals = normals;
			mesh.uv = UVs;
			return mesh;
		}

		#endregion

		#region Create Cylinder Mesh

		/// <summary>
		/// Source: https://github.com/doukasd/Unity-Components/blob/master/ProceduralCylinder/Assets/Scripts/Procedural/ProceduralCylinder.cs
		/// </summary>
		public static Mesh CreateCylinderMesh(float radius, float length, int radialSegments, int heightSegments)
		{
			//create the mesh
			var mesh = new Mesh();
			mesh.name = "ProceduralCylinderMesh";
			mesh.hideFlags = HideFlags.DontSave;

			//calculate how many vertices we need
			var numVertexColumns = radialSegments + 1; //+1 for welding
			var numVertexRows = heightSegments + 1;

			//calculate sizes
			int numVertices = numVertexColumns * numVertexRows;
			int numUVs = numVertices; //always
			int numSideTris = radialSegments * heightSegments * 2; //for one cap
			int numCapTris = radialSegments - 2; //fact
			int trisArrayLength = (numSideTris + numCapTris * 2) * 3; //3 places in the array for each tri

			//initialize arrays
			Vector3[] Vertices = new Vector3[numVertices];
			Vector2[] UVs = new Vector2[numUVs];
			int[] Tris = new int[trisArrayLength];

			//precalculate increments to improve performance
			float heightStep = length / heightSegments;
			float angleStep = 2 * Mathf.PI / radialSegments;
			float uvStepH = 1.0f / radialSegments;
			float uvStepV = 1.0f / heightSegments;

			for (int j = 0; j < numVertexRows; j++)
			{
				for (int i = 0; i < numVertexColumns; i++)
				{
					//calculate angle for that vertex on the unit circle
					float angle = i * angleStep;

					//"fold" the sheet around as a cylinder by placing the first and last vertex of each row at the same spot
					if (i == numVertexColumns - 1)
					{
						angle = 0;
					}

					//position current vertex
					Vertices[j * numVertexColumns + i] = new Vector3(
						radius * Mathf.Cos(angle),
						radius * Mathf.Sin(angle),
						j * heightStep
					);

					//calculate UVs
					UVs[j * numVertexColumns + i] = new Vector2(i * uvStepH, j * uvStepV);

					//create the tris				
					if (j == 0 || i >= numVertexColumns - 1)
					{
						//nothing to do on the first and last "floor" on the tris, capping is done below
						//also nothing to do on the last column of vertices
						continue;
					}
					else
					{
						//create 2 tris below each vertex
						//6 seems like a magic number. For every vertex we draw 2 tris in this for-loop, therefore we need 2*3=6 indices in the Tris array
						//offset the base by the number of slots we need for the bottom cap tris. Those will be populated once we draw the cap
						int baseIndex = numCapTris * 3 + (j - 1) * radialSegments * 6 + i * 6;

						//1st tri - below and in front
						Tris[baseIndex + 0] = j * numVertexColumns + i;
						Tris[baseIndex + 1] = (j - 1) * numVertexColumns + i;
						Tris[baseIndex + 2] = j * numVertexColumns + i + 1;

						//2nd tri - the one it doesn't touch
						Tris[baseIndex + 3] = (j - 1) * numVertexColumns + i;
						Tris[baseIndex + 4] = (j - 1) * numVertexColumns + i + 1;
						Tris[baseIndex + 5] = j * numVertexColumns + i + 1;
					}
				}
			}

			//draw caps
			bool leftSided = true;
			int leftIndex = 0;
			int rightIndex = 0;
			int middleIndex = 0;
			int topCapVertexOffset = numVertices - numVertexColumns;
			for (int i = 0; i < numCapTris; i++)
			{
				int bottomCapBaseIndex = i * 3;
				int topCapBaseIndex = (numCapTris + numSideTris) * 3 + i * 3;

				if (i == 0)
				{
					middleIndex = 0;
					leftIndex = 1;
					rightIndex = numVertexColumns - 2;
					leftSided = true;
				}
				else if (leftSided)
				{
					middleIndex = rightIndex;
					rightIndex--;
				}
				else
				{
					middleIndex = leftIndex;
					leftIndex++;
				}

				leftSided = !leftSided;

				//assign bottom tris
				Tris[bottomCapBaseIndex + 0] = rightIndex;
				Tris[bottomCapBaseIndex + 1] = leftIndex;
				Tris[bottomCapBaseIndex + 2] = middleIndex;

				//assign top tris
				Tris[topCapBaseIndex + 0] = topCapVertexOffset + leftIndex;
				Tris[topCapBaseIndex + 1] = topCapVertexOffset + rightIndex;
				Tris[topCapBaseIndex + 2] = topCapVertexOffset + middleIndex;
			}

			//assign vertices, uvs and tris
			mesh.vertices = Vertices;
			mesh.uv = UVs;
			mesh.triangles = Tris;

			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			return mesh;
		}

		#endregion

		#region Primitives - Cylinder

		private static Mesh _UnitCylinderZ;
		public static Mesh UnitCylinderZ
		{
			get
			{
				if (_UnitCylinderZ)
					return _UnitCylinderZ;

				const int radialSegments = 24;
				const int heightSegments = 1;
				const float radius = 0.5f;
				const float length = 1f;

				_UnitCylinderZ = CreateCylinderMesh(radius, length, radialSegments, heightSegments);
				return _UnitCylinderZ;
			}
		}

		#endregion

		#region Scale

		public static void Scale(this Mesh mesh, Vector3 localScale)
		{
			var vertices = mesh.vertices;
			for (var i = 0; i < vertices.Length; i++)
			{
				vertices[i] = Vector3.Scale(vertices[i], localScale);
			}
			mesh.vertices = vertices;
			mesh.RecalculateBounds();
		}

		#endregion

		#region Vertices, Triangles

		public static Vector3[] GetVerticesByIndexLookup(this IList<Vector3> vertices, IList<int> indices)
		{
			var result = new Vector3[indices.Count];
			for (int i = 0; i < indices.Count; i++)
			{
				result[i] = vertices[indices[i]];
			}
			return result;
		}

		public static Vector3[] ReuseOrCreateVertices(this Mesh mesh, int desiredCount)
		{
			if (mesh != null)
			{
				var vertices = mesh.vertices;
				if (vertices != null && vertices.Length == desiredCount)
					return vertices;
			}
			return new Vector3[desiredCount];
		}

		public static int[] ReuseOrCreateTriangles(this Mesh mesh, int desiredCount)
		{
			if (mesh != null)
			{
				var triangles = mesh.triangles;
				if (triangles != null && triangles.Length == desiredCount)
					return triangles;
			}
			return new int[desiredCount];
		}

		#endregion

		#region Split Submesh

		private class SubmeshSplitterData
		{
			private List<Vector3> vertices = null;
			private List<Vector2> uv1 = null;
			private List<Vector2> uv2 = null;
			private List<Vector2> uv3 = null;
			private List<Vector2> uv4 = null;
			private List<Vector3> normals = null;
			private List<Vector4> tangents = null;
			private List<Color32> colors32 = null;
			private List<BoneWeight> boneWeights = null;

			public SubmeshSplitterData()
			{
				vertices = new List<Vector3>();
			}

			public SubmeshSplitterData(Mesh clonedMesh)
			{
				vertices = CreateList(clonedMesh.vertices);
				uv1 = CreateList(clonedMesh.uv);
				uv2 = CreateList(clonedMesh.uv2);
				uv3 = CreateList(clonedMesh.uv3);
				uv4 = CreateList(clonedMesh.uv4);
				normals = CreateList(clonedMesh.normals);
				tangents = CreateList(clonedMesh.tangents);
				colors32 = CreateList(clonedMesh.colors32);
				boneWeights = CreateList(clonedMesh.boneWeights);
			}

			private List<T> CreateList<T>(T[] source)
			{
				if (source == null || source.Length == 0)
					return null;
				return new List<T>(source);
			}

			private void AppendToList<T>(ref List<T> destination, List<T> source, int sourceIndex)
			{
				if (source == null)
					return;
				if (destination == null)
					destination = new List<T>();
				destination.Add(source[sourceIndex]);
			}

			public int Append(SubmeshSplitterData other, int index)
			{
				var i = vertices.Count;
				AppendToList(ref vertices, other.vertices, index);
				AppendToList(ref uv1, other.uv1, index);
				AppendToList(ref uv2, other.uv2, index);
				AppendToList(ref uv3, other.uv3, index);
				AppendToList(ref uv4, other.uv4, index);
				AppendToList(ref normals, other.normals, index);
				AppendToList(ref tangents, other.tangents, index);
				AppendToList(ref colors32, other.colors32, index);
				AppendToList(ref boneWeights, other.boneWeights, index);
				return i;
			}

			public Mesh CreateNewMesh()
			{
				var mesh = new Mesh();
				mesh.SetVertices(vertices);
				if (uv1 != null) mesh.SetUVs(0, uv1);
				if (uv2 != null) mesh.SetUVs(1, uv2);
				if (uv3 != null) mesh.SetUVs(2, uv3);
				if (uv4 != null) mesh.SetUVs(3, uv4);
				if (normals != null) mesh.SetNormals(normals);
				if (tangents != null) mesh.SetTangents(tangents);
				if (colors32 != null) mesh.SetColors(colors32);
				if (boneWeights != null) mesh.boneWeights = boneWeights.ToArray();
				return mesh;
			}
		}

		/// <summary>
		/// Source: https://answers.unity.com/questions/1213025/separating-submeshes-into-unique-meshes.html
		/// </summary>
		public static Mesh SplitSubmesh(this Mesh mesh, int submeshIndex)
		{
			if (submeshIndex < 0 || submeshIndex >= mesh.subMeshCount)
				return null;
			var submeshTriangles = mesh.GetTriangles(submeshIndex);
			var sourceData = new SubmeshSplitterData(mesh);
			var resultingData = new SubmeshSplitterData();
			var indexMap = new Dictionary<int, int>();
			var newIndices = new int[submeshTriangles.Length];
			for (int i = 0; i < submeshTriangles.Length; i++)
			{
				var index = submeshTriangles[i];
				if (!indexMap.TryGetValue(index, out var newIndex))
				{
					newIndex = resultingData.Append(sourceData, index);
					indexMap.Add(index, newIndex);
				}
				newIndices[i] = newIndex;
			}
			var resultingMesh = resultingData.CreateNewMesh();
			resultingMesh.triangles = newIndices;
			return resultingMesh;
		}

		#endregion
	}

}

#endif
