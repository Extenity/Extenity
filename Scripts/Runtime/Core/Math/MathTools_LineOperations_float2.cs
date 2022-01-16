using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Extenity.MathToolbox
{

	public static partial class MathTools
	{
		#region Closest Point On Line

		public static float2 ClosestPointOnLine(float2 lineStart, float2 lineEnd, float2 point)
		{
			// TODO: OPTIMIZATION: This is directly copied from 3D calculations. See if there is a faster algorithm in 2D.
			var lineDirection = normalize(lineEnd - lineStart);
			var closestPoint = dot(point - lineStart, lineDirection);
			return lineStart + (closestPoint * lineDirection);
		}

		public static float2 ClosestPointOnLineSegment(float2 lineStart, float2 lineEnd, float2 point, out float distanceFromStart)
		{
			// TODO: OPTIMIZATION: This is directly copied from 3D calculations. See if there is a faster algorithm in 2D.
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

		#endregion
	}

}
