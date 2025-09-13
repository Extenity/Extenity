using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Extenity.MathToolbox
{

	public static partial class MathTools
	{
		#region Closest Point On Line

		public static float3 ClosestPointOnLine(float3 lineStart, float3 lineEnd, float3 point)
		{
			var lineDirection = normalize(lineEnd - lineStart);
			var closestPoint = dot(point - lineStart, lineDirection);
			return lineStart + (closestPoint * lineDirection);
		}

		public static float3 ClosestPointOnLineSegment(float3 lineStart, float3 lineEnd, float3 point)
		{
			var diff = lineEnd - lineStart;
			var lineDirection = normalize(diff);
			var closestPoint = dot(point - lineStart, lineDirection);

			// Clamp to line segment
			if (closestPoint < 0f)
			{
				return lineStart;
			}
			if (closestPoint > length(diff))
			{
				return lineEnd;
			}

			return lineStart + (closestPoint * lineDirection);
		}

		public static float3 ClosestPointOnLineSegment(float3 lineStart, float3 lineEnd, float3 point, out float distanceFromStart)
		{
			var diff = lineEnd - lineStart;
			var lineDirection = normalize(diff);
			var closestPoint = dot(point - lineStart, lineDirection);

			// Clamp to line segment
			if (closestPoint < 0f)
			{
				distanceFromStart = 0f;
				return lineStart;
			}
			var diffMagnitude = length(diff);
			if (closestPoint > diffMagnitude)
			{
				distanceFromStart = diffMagnitude;
				return lineEnd;
			}

			distanceFromStart = closestPoint;
			return lineStart + (closestPoint * lineDirection);
		}

		// TEST:
		//float3 lineStart;
		//float3 lineEnd;
		//float3 point;
		//float3 pointOnLine;

		//lineStart = new float3(50f, 10f, 50f);
		//lineEnd = new float3(100f, 10f, 50f);
		//point = new float3(70f, 10f, 60f);
		//pointOnLine = Tools.NearestPointOnLineSegment(lineStart, lineEnd, point);
		//Log.Write("lineStart   = " + lineStart.ToString());
		//Log.Write("lineEnd     = " + lineEnd.ToString());
		//Log.Write("point       = " + point.ToString());
		//Log.Write("pointOnLine = " + pointOnLine.ToString());

		//lineStart = new float3(50f, 10f, 50f);
		//lineEnd = new float3(100f, 10f, 50f);
		//point = new float3(30f, 10f, 60f);
		//pointOnLine = Tools.NearestPointOnLineSegment(lineStart, lineEnd, point);
		//Log.Write("lineStart   = " + lineStart.ToString());
		//Log.Write("lineEnd     = " + lineEnd.ToString());
		//Log.Write("point       = " + point.ToString());
		//Log.Write("pointOnLine = " + pointOnLine.ToString());

		//lineStart = new float3(50f, 10f, 50f);
		//lineEnd = new float3(100f, 10f, 50f);
		//point = new float3(170f, 10f, 60f);
		//pointOnLine = Tools.NearestPointOnLineSegment(lineStart, lineEnd, point);
		//Log.Write("lineStart   = " + lineStart.ToString());
		//Log.Write("lineEnd     = " + lineEnd.ToString());
		//Log.Write("point       = " + point.ToString());
		//Log.Write("pointOnLine = " + pointOnLine.ToString());

		#endregion

		#region DistanceFromStartOfClosestPointOnLineSegment

		public static float DistanceFromStartOfClosestPointOnLineSegment(float3 lineStart, float3 lineEnd, float3 point)
		{
			var diff = lineEnd - lineStart;
			var lineDirection = normalize(diff);
			var closestPoint = dot(point - lineStart, lineDirection);

			// Clamp to line segment
			if (closestPoint < 0f)
			{
				return 0f;
			}
			var diffMagnitude = length(diff);
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
		public static float DistanceToLineSegment(this float3 point, float3 lineStart, float3 lineEnd)
		{
			var v = lineEnd - lineStart;
			var w = point - lineStart;
			var c1 = dot(w, v);
			if (c1 <= 0)
				return point.DistanceTo(lineStart);
			var c2 = dot(v, v);
			if (c2 <= c1)
				return point.DistanceTo(lineEnd);

			var b = c1 / c2;
			var pb = lineStart + b * v;
			return point.DistanceTo(pb);
		}

		/// <summary>
		/// Source: http://geomalgorithms.com/a02-_lines.html
		/// </summary>
		public static float SqrDistanceToLineSegment(this float3 point, float3 lineA, float3 lineB)
		{
			var v = lineB - lineA;
			var w = point - lineA;
			var c1 = dot(w, v);
			if (c1 <= 0)
				return point.SqrDistanceTo(lineA);
			var c2 = dot(v, v);
			if (c2 <= c1)
				return point.SqrDistanceTo(lineB);

			var b = c1 / c2;
			var pb = lineA + b * v;
			return point.SqrDistanceTo(pb);
		}

		#endregion
	}

}
