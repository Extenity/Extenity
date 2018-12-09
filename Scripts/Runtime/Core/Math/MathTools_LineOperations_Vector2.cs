using UnityEngine;

namespace Extenity.MathToolbox
{

	public static partial class MathTools
	{
		#region Closest Point On Line

		public static Vector2 ClosestPointOnLine(Vector2 lineStart, Vector2 lineEnd, Vector2 point)
		{
			// TODO: OPTIMIZATION: This is directly copied from 3D calculations. See if there is a faster algorithm in 2D.
			var lineDirection = (lineEnd - lineStart).normalized;
			var closestPoint = Vector2.Dot(point - lineStart, lineDirection);
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

		#endregion
	}

}
