using UnityEngine;

namespace Extenity.MathToolbox
{

	public static partial class MathTools
	{
		#region Closest Point On Line

		public static Vector2 ClosestPointOnLine(Vector2 lineStart, Vector2 lineEnd, Vector2 point)
		{
			var lineDirection = (lineEnd - lineStart).normalized;
			var closestPoint = Vector2.Dot(point - lineStart, lineDirection) / Vector2.Dot(lineDirection, lineDirection);
			return lineStart + (closestPoint * lineDirection);
		}

		public static Vector3 ClosestPointOnLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
		{
			var lineDirection = (lineEnd - lineStart).normalized;
			var closestPoint = Vector3.Dot(point - lineStart, lineDirection) / Vector3.Dot(lineDirection, lineDirection);
			return lineStart + (closestPoint * lineDirection);
		}

		public static Vector3 ClosestPointOnLineSegment(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
		{
			var diff = lineEnd - lineStart;
			var lineDirection = diff.normalized;
			var closestPoint = Vector3.Dot(point - lineStart, lineDirection);

			// Clamp to line segment
			if (closestPoint < 0f)
			{
				return lineStart;
			}
			if (closestPoint > diff.magnitude)
			{
				return lineEnd;
			}

			return lineStart + (closestPoint * lineDirection);
		}

		public static Vector3 ClosestPointOnLineSegment(Vector3 lineStart, Vector3 lineEnd, Vector3 point, out float distanceFromStart)
		{
			var diff = lineEnd - lineStart;
			var lineDirection = diff.normalized;
			var closestPoint = Vector3.Dot(point - lineStart, lineDirection);

			// Clamp to line segment
			if (closestPoint < 0f)
			{
				distanceFromStart = 0f;
				return lineStart;
			}
			var diffMagnitude = diff.magnitude;
			if (closestPoint > diffMagnitude)
			{
				distanceFromStart = diffMagnitude;
				return lineEnd;
			}

			distanceFromStart = closestPoint;
			return lineStart + (closestPoint * lineDirection);
		}

		public static Vector2 ClosestPointOnLineSegment(Vector2 lineStart, Vector2 lineEnd, Vector2 point, out float distanceFromStart)
		{
			// TODO: OPTIMIZATION: This is directly copied from 3D calculations. See if there is a faster algorithm in 2D.
			var diff = lineEnd - lineStart;
			var lineDirection = diff.normalized;
			var closestPoint = Vector2.Dot(point - lineStart, lineDirection);

			// Clamp to line segment
			if (closestPoint < 0f)
			{
				distanceFromStart = 0f;
				return lineStart;
			}
			var diffMagnitude = diff.magnitude;
			if (closestPoint > diffMagnitude)
			{
				distanceFromStart = diffMagnitude;
				return lineEnd;
			}

			distanceFromStart = closestPoint;
			return lineStart + (closestPoint * lineDirection);
		}

		public static float DistanceFromStartOfClosestPointOnLineSegment(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
		{
			var diff = lineEnd - lineStart;
			var lineDirection = diff.normalized;
			var closestPoint = Vector3.Dot(point - lineStart, lineDirection);

			// Clamp to line segment
			if (closestPoint < 0f)
			{
				return 0f;
			}
			var diffMagnitude = diff.magnitude;
			if (closestPoint > diffMagnitude)
			{
				return diffMagnitude;
			}

			return closestPoint;
		}

		// TEST:
		//Vector3 lineStart;
		//Vector3 lineEnd;
		//Vector3 point;
		//Vector3 pointOnLine;

		//lineStart = new Vector3(50f, 10f, 50f);
		//lineEnd = new Vector3(100f, 10f, 50f);
		//point = new Vector3(70f, 10f, 60f);
		//pointOnLine = Tools.NearestPointOnLineSegment(lineStart, lineEnd, point);
		//Log.Write("lineStart   = " + lineStart.ToString());
		//Log.Write("lineEnd     = " + lineEnd.ToString());
		//Log.Write("point       = " + point.ToString());
		//Log.Write("pointOnLine = " + pointOnLine.ToString());

		//lineStart = new Vector3(50f, 10f, 50f);
		//lineEnd = new Vector3(100f, 10f, 50f);
		//point = new Vector3(30f, 10f, 60f);
		//pointOnLine = Tools.NearestPointOnLineSegment(lineStart, lineEnd, point);
		//Log.Write("lineStart   = " + lineStart.ToString());
		//Log.Write("lineEnd     = " + lineEnd.ToString());
		//Log.Write("point       = " + point.ToString());
		//Log.Write("pointOnLine = " + pointOnLine.ToString());

		//lineStart = new Vector3(50f, 10f, 50f);
		//lineEnd = new Vector3(100f, 10f, 50f);
		//point = new Vector3(170f, 10f, 60f);
		//pointOnLine = Tools.NearestPointOnLineSegment(lineStart, lineEnd, point);
		//Log.Write("lineStart   = " + lineStart.ToString());
		//Log.Write("lineEnd     = " + lineEnd.ToString());
		//Log.Write("point       = " + point.ToString());
		//Log.Write("pointOnLine = " + pointOnLine.ToString());

		#endregion

		#region Line Intersection

		public static bool CheckLineLineIntersection(
			Vector2 line1Point1, Vector2 line1Point2,
			Vector2 line2Point1, Vector2 line2Point2)
		{
			var b = line1Point2 - line1Point1;
			var d = line2Point2 - line2Point1;
			var bDotDPerp = b.x * d.y - b.y * d.x;

			// if b dot d == 0, it means the lines are parallel so have infinite intersection points
			if (bDotDPerp.IsZero())
				return false;

			var c = line2Point1 - line1Point1;
			var lineFactor = (c.x * d.y - c.y * d.x) / bDotDPerp;
			if (lineFactor < 0 || lineFactor > 1)
				return false;

			lineFactor = (c.x * b.y - c.y * b.x) / bDotDPerp;
			return lineFactor >= 0 && lineFactor <= 1;
		}

		#endregion

		#region Spline Operations - Bezier, CatmullRom

		public static Vector3 GetBezierPoint(Vector3 p1, Vector3 p2, Vector3 p3, float t)
		{
			var it = 1 - t;
			var it2 = it * it;
			var itt = it * t;
			var t2 = t * t;

			return new Vector3(
				(p1.x * it2 + 2 * p2.x * itt + p3.x * t2),
				(p1.y * it2 + 2 * p2.y * itt + p3.y * t2),
				(p1.z * it2 + 2 * p2.z * itt + p3.z * t2)
				);
		}

		public static Vector3 GetCatmullRomPoint(
			Vector3 previous, Vector3 start, Vector3 end, Vector3 next,
			float percentage)
		{
			// References used:
			// p.266 GemsV1
			//
			// tension is often set to 0.5 but you can use any reasonable value:
			// http://www.cs.cmu.edu/~462/projects/assn2/assn2/catmullRom.pdf
			//
			// bias and tension controls:
			// http://local.wasp.uwa.edu.au/~pbourke/miscellaneous/interpolation/

			var percentageSquare = percentage * percentage;
			var percentageCube = percentageSquare * percentage;

			return previous * (-0.5f * percentageCube +
							 percentageSquare -
							 0.5f * percentage) +
				   start * (1.5f * percentageCube +
						  -2.5f * percentageSquare + 1.0f) +
				   end * (-1.5f * percentageCube +
						2.0f * percentageSquare +
						0.5f * percentage) +
				   next * (0.5f * percentageCube -
						 0.5f * percentageSquare);
		}

		#endregion
	}

}
