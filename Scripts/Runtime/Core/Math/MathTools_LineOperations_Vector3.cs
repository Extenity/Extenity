#if UNITY // TODO-UniversalExtenity: Convert these to Mathematics after importing it into Universal project.

using UnityEngine;

namespace Extenity.MathToolbox
{

	public static partial class MathTools
	{
		#region Closest Point On Line

		public static Vector3 ClosestPointOnLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
		{
			var lineDirection = (lineEnd - lineStart).normalized;
			var closestPoint = Vector3.Dot(point - lineStart, lineDirection);
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

		#region DistanceFromStartOfClosestPointOnLineSegment

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

		#endregion

		#region Distance To Line Segment

		/// <summary>
		/// Source: http://geomalgorithms.com/a02-_lines.html
		/// </summary>
		public static float DistanceToLineSegment(this Vector3 point, Vector3 lineStart, Vector3 lineEnd)
		{
			var v = lineEnd - lineStart;
			var w = point - lineStart;
			var c1 = Vector3.Dot(w, v);
			if (c1 <= 0)
				return Vector3.Distance(point, lineStart);
			var c2 = Vector3.Dot(v, v);
			if (c2 <= c1)
				return Vector3.Distance(point, lineEnd);

			var b = c1 / c2;
			var Pb = lineStart + b * v;
			return Vector3.Distance(point, Pb);
		}

		/// <summary>
		/// Source: http://geomalgorithms.com/a02-_lines.html
		/// </summary>
		public static float SqrDistanceToLineSegment(this Vector3 point, Vector3 lineA, Vector3 lineB)
		{
			var v = lineB - lineA;
			var w = point - lineA;
			var c1 = Vector3.Dot(w, v);
			if (c1 <= 0)
				return point.SqrDistanceTo(lineA);
			var c2 = Vector3.Dot(v, v);
			if (c2 <= c1)
				return point.SqrDistanceTo(lineB);

			var b = c1 / c2;
			var Pb = lineA + b * v;
			return point.SqrDistanceTo(Pb);
		}

		#endregion
	}

}

#endif
