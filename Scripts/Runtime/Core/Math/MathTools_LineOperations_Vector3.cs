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
	}

}
