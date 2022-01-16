using System.Collections.Generic;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Extenity.MathToolbox
{

	public struct Rectangle
	{
		public float2 centerPosition;
		public float2 size;
		public float rotation;

		#region Extract points to array

		public float2[] GetPoints()
		{
			var points = new float2[4];
			var halfSize = size * 0.5f;
			points[0] = float2(centerPosition.x, centerPosition.y) + float2(-halfSize.x, -halfSize.y).Rotate(-rotation);
			points[1] = float2(centerPosition.x, centerPosition.y) + float2(halfSize.x, -halfSize.y).Rotate(-rotation);
			points[2] = float2(centerPosition.x, centerPosition.y) + float2(halfSize.x, halfSize.y).Rotate(-rotation);
			points[3] = float2(centerPosition.x, centerPosition.y) + float2(-halfSize.x, halfSize.y).Rotate(-rotation);
			return points;
		}

		public float3[] GetPointsXY(float zValue = 0f)
		{
			var points = new float3[4];
			var halfSize = size * 0.5f;
			points[0] = float3(centerPosition.x, centerPosition.y, zValue) + float3(-halfSize.x, -halfSize.y, 0f).RotateAroundZ(-rotation);
			points[1] = float3(centerPosition.x, centerPosition.y, zValue) + float3(halfSize.x, -halfSize.y, 0f).RotateAroundZ(-rotation);
			points[2] = float3(centerPosition.x, centerPosition.y, zValue) + float3(halfSize.x, halfSize.y, 0f).RotateAroundZ(-rotation);
			points[3] = float3(centerPosition.x, centerPosition.y, zValue) + float3(-halfSize.x, halfSize.y, 0f).RotateAroundZ(-rotation);
			return points;
		}

		public float3[] GetPointsXZ(float yValue = 0f)
		{
			var points = new float3[4];
			var halfSize = size * 0.5f;
			points[0] = float3(centerPosition.x, yValue, centerPosition.y) + float3(-halfSize.x, 0f, -halfSize.y).RotateAroundY(-rotation);
			points[1] = float3(centerPosition.x, yValue, centerPosition.y) + float3(halfSize.x, 0f, -halfSize.y).RotateAroundY(-rotation);
			points[2] = float3(centerPosition.x, yValue, centerPosition.y) + float3(halfSize.x, 0f, halfSize.y).RotateAroundY(-rotation);
			points[3] = float3(centerPosition.x, yValue, centerPosition.y) + float3(-halfSize.x, 0f, halfSize.y).RotateAroundY(-rotation);
			return points;
		}

		public float3[] GetPointsYZ(float xValue = 0f)
		{
			var points = new float3[4];
			var halfSize = size * 0.5f;
			points[0] = float3(xValue, centerPosition.x, centerPosition.y) + float3(0f, -halfSize.x, -halfSize.y).RotateAroundX(-rotation);
			points[1] = float3(xValue, centerPosition.x, centerPosition.y) + float3(0f, halfSize.x, -halfSize.y).RotateAroundX(-rotation);
			points[2] = float3(xValue, centerPosition.x, centerPosition.y) + float3(0f, halfSize.x, halfSize.y).RotateAroundX(-rotation);
			points[3] = float3(xValue, centerPosition.x, centerPosition.y) + float3(0f, -halfSize.x, halfSize.y).RotateAroundX(-rotation);
			return points;
		}

		#endregion

		#region Intersection

		public static bool CheckRectRectIntersection(Rectangle rect1, Rectangle rect2)
		{
			return CheckRectRectIntersection(rect1.GetPoints(), rect2.GetPoints());
		}

		public static bool CheckRectRectIntersection(float2[] rect1, float2[] rect2)
		{
			if (DoAxisSeparationTest(rect1[0], rect1[1], rect1[2], rect2)) return false;
			if (DoAxisSeparationTest(rect1[0], rect1[3], rect1[2], rect2)) return false;
			if (DoAxisSeparationTest(rect1[3], rect1[2], rect1[0], rect2)) return false;
			if (DoAxisSeparationTest(rect1[2], rect1[1], rect1[0], rect2)) return false;
			if (DoAxisSeparationTest(rect2[0], rect2[1], rect2[2], rect1)) return false;
			if (DoAxisSeparationTest(rect2[0], rect2[3], rect2[2], rect1)) return false;
			if (DoAxisSeparationTest(rect2[3], rect2[2], rect2[0], rect1)) return false;
			if (DoAxisSeparationTest(rect2[2], rect2[1], rect2[0], rect1)) return false;
			return true;
		}

		/// <summary>
		/// Does axis separation test for a convex quadrilateral.
		/// </summary>
		/// <param name="x1">Defines together with x2 the edge of quad1 to be checked whether its a separating axis.</param>
		/// <param name="x2">Defines together with x1 the edge of quad1 to be checked whether its a separating axis.</param>
		/// <param name="x3">One of the remaining two points of quad1.</param>
		/// <param name="otherQuadPoints">The four points of the other quad.</param>
		/// <returns>Returns <c>true</c>, if the specified edge is a separating axis (and the quadrilaterals therefor don't 
		/// intersect). Returns <c>false</c>, if it's not a separating axis.</returns>
		private static bool DoAxisSeparationTest(float2 x1, float2 x2, float2 x3, IEnumerable<float2> otherQuadPoints)
		{
			float2 vec = x2 - x1;
			float2 rotated = new float2(-vec.y, vec.x);

			bool refSide = (rotated.x * (x3.x - x1.x)
						  + rotated.y * (x3.y - x1.y)) >= 0;

			foreach (var pt in otherQuadPoints)
			{
				bool side = (rotated.x * (pt.x - x1.x)
						   + rotated.y * (pt.y - x1.y)) >= 0;
				if (side == refSide)
				{
					// At least one point of the other quad is one the same side as x3. Therefor the specified edge can't be a
					// separating axis anymore.
					return false;
				}
			}

			// All points of the other quad are on the other side of the edge. Therefor the edge is a separating axis and
			// the quads don't intersect.
			return true;
		}

		#endregion
	}

}
