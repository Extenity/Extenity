using UnityEngine;
using System.Collections.Generic;

public static class MeshTools
{
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

	#region Spline

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

	#region Vertices

	public static Vector3[] GetVerticesByIndexLookup(this IList<Vector3> vertices, IList<int> indices)
	{
		var result = new Vector3[indices.Count];
		for (int i = 0; i < indices.Count; i++)
		{
			result[i] = vertices[indices[i]];
		}
		return result;
	}

	#endregion
}
