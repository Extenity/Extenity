#if UNITY

using UnityEngine;
using Extenity.MathToolbox;

namespace Extenity.DebugToolbox
{

	public static class DebugDrawTools
	{
		public static Color DefaultColor = Color.white;

		#region Line

		public static void Line(Vector3 start, Vector3 end, float duration = 0f, bool depthTest = true)
		{
			Debug.DrawLine(start, end, DefaultColor, duration, depthTest);
		}

		public static void Line(Vector3 start, Vector3 end, Color color, float duration = 0f, bool depthTest = true)
		{
			Debug.DrawLine(start, end, color, duration, depthTest);
		}

		public static void Line(Vector3 start, Vector3 end, Vector3 offset, float duration = 0f, bool depthTest = true)
		{
			Debug.DrawLine(start + offset, end + offset, DefaultColor, duration, depthTest);
		}

		public static void Line(Vector3 start, Vector3 end, Vector3 offset, Color color, float duration = 0f, bool depthTest = true)
		{
			Debug.DrawLine(start + offset, end + offset, color, duration, depthTest);
		}

		#endregion

		#region Ray

		public static void DebugDrawRay(Ray ray, float length, float duration = 0f, bool depthTest = true)
		{
			Debug.DrawLine(ray.origin, ray.origin + ray.direction * length, DefaultColor, duration, depthTest);
		}

		public static void DebugDrawRay(Ray ray, float length, Color color, float duration = 0f, bool depthTest = true)
		{
			Debug.DrawLine(ray.origin, ray.origin + ray.direction * length, color, duration, depthTest);
		}

		#endregion

		#region Plus and Cross

		public static void DebugDrawPlus(this Vector3 worldPosition, float size, float duration = 0f, bool depthTest = true)
		{
			DebugDrawPlus(worldPosition, size, DefaultColor, duration, depthTest);
		}

		public static void DebugDrawPlus(this Vector3 worldPosition, float size, Color color, float duration = 0f, bool depthTest = true)
		{
			var diffX = new Vector3(size, 0f, 0f);
			var diffY = new Vector3(0f, size, 0f);
			var diffZ = new Vector3(0f, 0f, size);
			Line(worldPosition - diffX, worldPosition + diffX, color, duration, depthTest);
			Line(worldPosition - diffY, worldPosition + diffY, color, duration, depthTest);
			Line(worldPosition - diffZ, worldPosition + diffZ, color, duration, depthTest);
		}

		public static void DebugDrawCross(this Vector3 worldPosition, float size, float duration = 0f, bool depthTest = true)
		{
			DebugDrawCross(worldPosition, size, DefaultColor, duration, depthTest);
		}

		public static void DebugDrawCross(this Vector3 worldPosition, float size, Color color, float duration = 0f, bool depthTest = true)
		{
			var diff1 = new Vector3(size, size, 0f);
			var diff2 = new Vector3(-size, size, 0f);
			var diff3 = new Vector3(0f, size, size);
			var diff4 = new Vector3(0f, size, -size);
			var diff5 = new Vector3(size, 0f, size);
			var diff6 = new Vector3(size, 0f, -size);
			Line(worldPosition - diff1, worldPosition + diff1, color, duration, depthTest);
			Line(worldPosition - diff2, worldPosition + diff2, color, duration, depthTest);
			Line(worldPosition - diff3, worldPosition + diff3, color, duration, depthTest);
			Line(worldPosition - diff4, worldPosition + diff4, color, duration, depthTest);
			Line(worldPosition - diff5, worldPosition + diff5, color, duration, depthTest);
			Line(worldPosition - diff6, worldPosition + diff6, color, duration, depthTest);
		}

		#endregion

		#region Circle

		public static void DebugDrawCircleXZ(this Circle circle, Transform transform, Color color, float angleStep = 0.4f, float duration = 0f, bool depthTest = true)
		{
			DebugDrawCircleXZ(circle.center, circle.radius, transform, color, angleStep, duration, depthTest);
		}

		public static void DebugDrawCircleXZ(Vector3 worldPosition, float radius, Color color, float angleStep = 0.4f, float duration = 0f, bool depthTest = true)
		{
			var start = worldPosition + new Vector3(radius, 0f, 0f);

			for (float angle = angleStep; angle < Mathf.PI * 2f; angle += angleStep)
			{
				var end = worldPosition + new Vector3(radius * Mathf.Cos(angle), 0f, radius * Mathf.Sin(angle));
				Line(start, end, color, duration, depthTest);
				start = end;
			}

			Line(start, worldPosition + new Vector3(radius, 0f, 0f), color, duration, depthTest);
		}

		public static void DebugDrawCircleXZ(Vector3 localPosition, float radius, Transform transform, Color color, float angleStep = 0.4f, float duration = 0f, bool depthTest = true)
		{
			var start = transform.TransformPoint(localPosition + new Vector3(radius, 0f, 0f));

			for (float angle = angleStep; angle < Mathf.PI * 2f; angle += angleStep)
			{
				var end = transform.TransformPoint(localPosition + new Vector3(radius * Mathf.Cos(angle), 0f, radius * Mathf.Sin(angle)));
				Line(start, end, color, duration, depthTest);
				start = end;
			}

			Line(start, transform.TransformPoint(localPosition + new Vector3(radius, 0f, 0f)), color, duration, depthTest);
		}

		#endregion

		#region Rectangle

		public static void DebugDrawRectangleXZ(this Rectangle rect, float height, Color color, float duration = 0f, bool depthTest = true)
		{
			var points = rect.GetPointsXZ(height);

			Line(points[0], points[1], color, duration, depthTest);
			Line(points[1], points[2], color, duration, depthTest);
			Line(points[2], points[3], color, duration, depthTest);
			Line(points[3], points[0], color, duration, depthTest);
		}

		#endregion

		#region Plane

		public static void DebugDrawPlane(Vector3 center, Vector3 normal, float size, float duration = 0f, bool depthTest = true)
		{
			DebugDrawPlane(center, normal, size, DefaultColor, duration, depthTest);
		}

		public static void DebugDrawPlane(Vector3 center, Vector3 normal, float size, Color color, float duration = 0f, bool depthTest = true)
		{
			var p0 = new Vector3(-size, 0, -size); // left forward
			var p1 = new Vector3(size, 0, -size); // right forward
			var p2 = new Vector3(size, 0, size); // right back
			var p3 = new Vector3(-size, 0, size); // left back

			var rotation = Quaternion.LookRotation(normal, Vector3.up) * Quaternion.Euler(90f, 0.0f, 0.0f);

			p0 = rotation * p0 + center;
			p1 = rotation * p1 + center;
			p2 = rotation * p2 + center;
			p3 = rotation * p3 + center;

			var difP3P0 = (p3 - p0);
			var difP2P1 = (p2 - p1);
			var difP1P0 = (p1 - p0);
			var difP2P3 = (p2 - p3);

			var step = 0.1f;

			for (float xR = step; xR <= 1; xR += step)
			{
				Line(p0 + difP3P0 * xR, p1 + difP2P1 * xR, color * 0.5f, duration, depthTest);
			}

			for (float yR = step; yR <= 1; yR += step)
			{
				Line(p0 + difP1P0 * yR, p3 + difP2P3 * yR, color * 0.5f, duration, depthTest);
			}

			Line(p0, p1, color, duration, depthTest);
			Line(p1, p2, color, duration, depthTest);
			Line(p2, p3, color, duration, depthTest);
			Line(p3, p0, color, duration, depthTest);

			Line(center, center + normal, new Color(1.0f - color.r, 1.0f - color.g, 1.0f - color.b, 1.0f), duration, depthTest);
		}

		#endregion

		#region AAB

		public static void DebugDrawAAB(this Bounds bounds, float duration = 0f, bool depthTest = true)
		{
			DebugDrawAAB(bounds.min, bounds.max, DefaultColor, duration, depthTest);
		}

		public static void DebugDrawAAB(this Bounds bounds, Color color, float duration = 0f, bool depthTest = true)
		{
			DebugDrawAAB(bounds.min, bounds.max, color, duration, depthTest);
		}

		public static void DebugDrawAAB(this Bounds bounds, Transform transform, float duration = 0f, bool depthTest = true)
		{
			DebugDrawAAB(bounds.min, bounds.max, transform, DefaultColor, duration, depthTest);
		}

		public static void DebugDrawAAB(this Bounds bounds, Transform transform, Color color, float duration = 0f, bool depthTest = true)
		{
			DebugDrawAAB(bounds.min, bounds.max, transform, color, duration, depthTest);
		}

		public static void DebugDrawAAB(Vector3 min, Vector3 max, float duration = 0f, bool depthTest = true)
		{
			DebugDrawAAB(min, max, DefaultColor, duration, depthTest);
		}

		public static void DebugDrawAAB(Vector3 min, Vector3 max, Color color, float duration = 0f, bool depthTest = true)
		{
			var a = min;
			var b = min;

			b.x = max.x;
			Line(a, b, color, duration, depthTest);
			b.x = min.x;
			b.y = max.y;
			Line(a, b, color, duration, depthTest);
			a.y = max.y;
			b.x = max.x;
			Line(a, b, color, duration, depthTest);
			a.x = max.x;
			a.y = min.y;
			Line(a, b, color, duration, depthTest);

			a.x = min.x;
			a.z = max.z;
			b.y = min.y;
			b.z = max.z;
			Line(a, b, color, duration, depthTest);
			b.x = min.x;
			b.y = max.y;
			Line(a, b, color, duration, depthTest);
			a.y = max.y;
			b.x = max.x;
			Line(a, b, color, duration, depthTest);
			a.x = max.x;
			a.y = min.y;
			Line(a, b, color, duration, depthTest);

			a.x = min.x;
			a.z = min.z;
			b.x = min.x;
			b.y = min.y;
			Line(a, b, color, duration, depthTest);
			a.y = max.y;
			b.y = max.y;
			Line(a, b, color, duration, depthTest);
			a.x = max.x;
			a.y = min.y;
			b.x = max.x;
			b.y = min.y;
			Line(a, b, color, duration, depthTest);
			a.y = max.y;
			b.y = max.y;
			Line(a, b, color, duration, depthTest);

			//Line(new Vector3(min.x, min.y, min.z), new Vector3(max.x, min.y, min.z), color, duration, depthTest);
			//Line(new Vector3(min.x, min.y, min.z), new Vector3(min.x, max.y, min.z), color, duration, depthTest);
			//Line(new Vector3(min.x, max.y, min.z), new Vector3(max.x, max.y, min.z), color, duration, depthTest);
			//Line(new Vector3(max.x, min.y, min.z), new Vector3(max.x, max.y, min.z), color, duration, depthTest);

			//Line(new Vector3(min.x, min.y, max.z), new Vector3(max.x, min.y, max.z), color, duration, depthTest);
			//Line(new Vector3(min.x, min.y, max.z), new Vector3(min.x, max.y, max.z), color, duration, depthTest);
			//Line(new Vector3(min.x, max.y, max.z), new Vector3(max.x, max.y, max.z), color, duration, depthTest);
			//Line(new Vector3(max.x, min.y, max.z), new Vector3(max.x, max.y, max.z), color, duration, depthTest);

			//Line(new Vector3(min.x, min.y, min.z), new Vector3(min.x, min.y, max.z), color, duration, depthTest);
			//Line(new Vector3(min.x, max.y, min.z), new Vector3(min.x, max.y, max.z), color, duration, depthTest);
			//Line(new Vector3(max.x, min.y, min.z), new Vector3(max.x, min.y, max.z), color, duration, depthTest);
			//Line(new Vector3(max.x, max.y, min.z), new Vector3(max.x, max.y, max.z), color, duration, depthTest);
		}

		public static void DebugDrawAAB(Vector3 min, Vector3 max, Transform transform, Color color, float duration = 0f, bool depthTest = true)
		{
			if (!transform)
			{
				DebugDrawAAB(min, max, color);
				return;
			}

			Vector3 a = min;
			Vector3 b = min;

			b.x = max.x;
			Line(transform.TransformPoint(a), transform.TransformPoint(b), color, duration, depthTest);
			b.x = min.x;
			b.y = max.y;
			Line(transform.TransformPoint(a), transform.TransformPoint(b), color, duration, depthTest);
			a.y = max.y;
			b.x = max.x;
			Line(transform.TransformPoint(a), transform.TransformPoint(b), color, duration, depthTest);
			a.x = max.x;
			a.y = min.y;
			Line(transform.TransformPoint(a), transform.TransformPoint(b), color, duration, depthTest);

			a.x = min.x;
			a.z = max.z;
			b.y = min.y;
			b.z = max.z;
			Line(transform.TransformPoint(a), transform.TransformPoint(b), color, duration, depthTest);
			b.x = min.x;
			b.y = max.y;
			Line(transform.TransformPoint(a), transform.TransformPoint(b), color, duration, depthTest);
			a.y = max.y;
			b.x = max.x;
			Line(transform.TransformPoint(a), transform.TransformPoint(b), color, duration, depthTest);
			a.x = max.x;
			a.y = min.y;
			Line(transform.TransformPoint(a), transform.TransformPoint(b), color, duration, depthTest);

			a.x = min.x;
			a.z = min.z;
			b.x = min.x;
			b.y = min.y;
			Line(transform.TransformPoint(a), transform.TransformPoint(b), color, duration, depthTest);
			a.y = max.y;
			b.y = max.y;
			Line(transform.TransformPoint(a), transform.TransformPoint(b), color, duration, depthTest);
			a.x = max.x;
			a.y = min.y;
			b.x = max.x;
			b.y = min.y;
			Line(transform.TransformPoint(a), transform.TransformPoint(b), color, duration, depthTest);
			a.y = max.y;
			b.y = max.y;
			Line(transform.TransformPoint(a), transform.TransformPoint(b), color, duration, depthTest);
		}

		#endregion

		#region Mesh Helpers

		public static void DebugDrawMeshNormals(this Mesh mesh, float lineLenght, Color color, float duration = 0f, bool depthTest = true)
		{
			if (mesh.normals == null)
				return;

			var vertices = mesh.vertices;
			var normals = mesh.normals;

			DebugAssert.IsEqual(normals.Length, vertices.Length);

			for (int i = 0; i < normals.Length; i++)
			{
				Line(vertices[i], vertices[i] + (normals[i] * lineLenght), color, duration, depthTest);
			}
		}

		#endregion

		#region Frustum

		public static void DebugDrawFrustum(this Transform transform, float fovY, float maxRange, float minRange, float aspect, Color color, Vector3 center)
		{
			var previousMatrix = Gizmos.matrix;
			var previousColor = Gizmos.color;

			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = color;
			Gizmos.DrawFrustum(center, fovY, maxRange, minRange, aspect);

			Gizmos.matrix = previousMatrix;
			Gizmos.color = previousColor;
		}

		#endregion
	}

}

#endif
