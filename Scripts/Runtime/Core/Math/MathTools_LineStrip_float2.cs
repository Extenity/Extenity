using System.Collections.Generic;
using Extenity.DebugToolbox;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Extenity.MathToolbox
{

	public static partial class MathTools
	{
		#region Line Strip Segment Count

		public static int GetLineSegmentCount(this IList<float2> points, bool loop)
		{
			if (points == null || points.Count == 0)
				return 0;

			if (loop)
				return points.Count;
			else
				return points.Count - 1;
		}

		#endregion

		#region Line Strip Length

		public static float CalculateLineStripLength(this IList<float2> points, bool loop)
		{
			if (points == null || points.Count < 2)
				return 0f;

			var totalLength = 0f;
			var previousPoint = points[0];
			for (int i = 1; i < points.Count; i++)
			{
				var currentPoint = points[i];
				totalLength += previousPoint.DistanceTo(currentPoint);
				previousPoint = currentPoint;
			}
			if (loop)
			{
				totalLength += previousPoint.DistanceTo(points[0]);
			}
			return totalLength;
		}

		public static float CalculateLineStripLength(this IList<float2> points, int startIndex, int count)
		{
			if (points == null || points.Count < 2 || count < 2)
				return 0f;

			var totalLength = 0f;
			var previousPoint = points[0];
			var endIndex = startIndex + count;
			for (int i = startIndex + 1; i < endIndex; i++)
			{
				var currentPoint = points[i];
				totalLength += previousPoint.DistanceTo(currentPoint);
				previousPoint = currentPoint;
			}
			return totalLength;
		}

		public static float CalculateAverageLengthOfLineStripParts(this IList<float2> points, bool loop)
		{
			if (points == null || points.Count < 2)
				return 0f;

			var totalLength = CalculateLineStripLength(points, loop);
			return totalLength / points.GetLineSegmentCount(loop);
		}

		#endregion

		#region GetPointAtDistanceFromStart

		public static float2 GetPointAtDistanceFromStart(this IList<float2> points, float distanceFromStart, ref float2 part, int bufferSize = -1)
		{
			if (points == null || points.Count == 0)
				return float2Tools.NaN;
			if (points.Count == 1 || distanceFromStart < 0f)
				return points[0];

			var totalDistance = 0f;
			var previousPoint = points[0];
			if (bufferSize < 0)
				bufferSize = points.Count;
			for (int i = 1; i < bufferSize; i++)
			{
				var currentPoint = points[i];
				var distance = previousPoint.DistanceTo(currentPoint);

				if (distanceFromStart - totalDistance < distance)
				{
					var ratio = (distanceFromStart - totalDistance) / distance;
					DebugAssert.IsBetweenZeroOne(ratio);

					var diff = currentPoint - previousPoint;
					part = diff;
					return previousPoint + diff * ratio;
				}

				totalDistance += distance;
				previousPoint = currentPoint;
			}

			return previousPoint;
		}

		public static float2 GetPointAtDistanceFromStart(this IList<float2> points, bool loop, float distanceFromStart, ref float2 part, int bufferSize = -1)
		{
			if (points == null || points.Count == 0)
				return float2Tools.NaN;
			if (points.Count == 1 || distanceFromStart < 0f)
				return points[0];

			var totalDistance = 0f;
			var previousPoint = points[0];
			if (bufferSize < 0)
				bufferSize = points.Count;
			for (int i = 1; i < bufferSize; i++)
			{
				var currentPoint = points[i];
				var distance = previousPoint.DistanceTo(currentPoint);

				if (distanceFromStart - totalDistance < distance)
				{
					var ratio = (distanceFromStart - totalDistance) / distance;
					DebugAssert.IsBetweenZeroOne(ratio);

					var diff = currentPoint - previousPoint;
					part = diff;
					return previousPoint + diff * ratio;
				}

				totalDistance += distance;
				previousPoint = currentPoint;
			}
			if (loop)
			{
				var currentPoint = points[0];
				var distance = previousPoint.DistanceTo(currentPoint);

				if (distanceFromStart - totalDistance < distance)
				{
					var ratio = (distanceFromStart - totalDistance) / distance;
					DebugAssert.IsBetweenZeroOne(ratio);

					var diff = currentPoint - previousPoint;
					part = diff;
					return previousPoint + diff * ratio;
				}

				//totalDistance += distance;
				previousPoint = currentPoint;
			}

			return previousPoint;
		}

		public static float2 GetPointAtDistanceFromStart(this IList<float2> points, float distanceFromStart, int bufferSize = -1)
		{
			if (points == null || points.Count == 0)
				return float2Tools.NaN;
			if (points.Count == 1 || distanceFromStart < 0f)
				return points[0];

			var totalDistance = 0f;
			var previousPoint = points[0];
			if (bufferSize < 0)
				bufferSize = points.Count;
			for (int i = 1; i < bufferSize; i++)
			{
				var currentPoint = points[i];
				var distance = previousPoint.DistanceTo(currentPoint);

				if (distanceFromStart - totalDistance < distance)
				{
					var ratio = (distanceFromStart - totalDistance) / distance;
					DebugAssert.IsBetweenZeroOne(ratio);

					var diff = currentPoint - previousPoint;
					return previousPoint + diff * ratio;
				}

				totalDistance += distance;
				previousPoint = currentPoint;
			}

			return previousPoint;
		}
		
		public static float2 GetPointAtDistanceFromStart(this IList<float2> points, bool loop, float distanceFromStart, int bufferSize = -1)
		{
			if (points == null || points.Count == 0)
				return float2Tools.NaN;
			if (points.Count == 1 || distanceFromStart < 0f)
				return points[0];

			var totalDistance = 0f;
			var previousPoint = points[0];
			if (bufferSize < 0)
				bufferSize = points.Count;
			for (int i = 1; i < bufferSize; i++)
			{
				var currentPoint = points[i];
				var distance = previousPoint.DistanceTo(currentPoint);

				if (distanceFromStart - totalDistance < distance)
				{
					var ratio = (distanceFromStart - totalDistance) / distance;
					DebugAssert.IsBetweenZeroOne(ratio);

					var diff = currentPoint - previousPoint;
					return previousPoint + diff * ratio;
				}

				totalDistance += distance;
				previousPoint = currentPoint;
			}
			if (loop)
			{
				var currentPoint = points[0];
				var distance = previousPoint.DistanceTo(currentPoint);

				if (distanceFromStart - totalDistance < distance)
				{
					var ratio = (distanceFromStart - totalDistance) / distance;
					DebugAssert.IsBetweenZeroOne(ratio);

					var diff = currentPoint - previousPoint;
					return previousPoint + diff * ratio;
				}

				//totalDistance += distance;
				previousPoint = currentPoint;
			}

			return previousPoint;
		}

		#endregion

		#region DistanceFromStartOfClosestPointOnLineStrip

		public static float DistanceFromStartOfClosestPointOnLineStrip(this IList<float2> points, float2 point, int bufferSize = -1)
		{
			if (points == null || points.Count == 0)
				return float.NaN;
			if (points.Count == 1)
				return 0f;

			var previousPoint = points[0];
			var totalLength = 0f;
			//var closestPoint = previousPoint;
			var distanceFromStartOfClosestPoint = 0f;
			var closestPointSqrDistance = float.PositiveInfinity;
			if (bufferSize < 0)
				bufferSize = points.Count;
			for (int i = 1; i < bufferSize; i++)
			{
				var currentPoint = points[i];

				var currentSegmentClosestPoint = ClosestPointOnLineSegment(previousPoint, currentPoint, point, out var distanceFromStartOfCurrentSegmentClosestPoint);
				var sqrDistance = currentSegmentClosestPoint.SqrDistanceTo(point);

				if (closestPointSqrDistance > sqrDistance)
				{
					closestPointSqrDistance = sqrDistance;
					//closestPoint = currentSegmentClosestPoint;
					distanceFromStartOfClosestPoint = totalLength + distanceFromStartOfCurrentSegmentClosestPoint;
				}

				totalLength += currentPoint.DistanceTo(previousPoint);
				previousPoint = currentPoint;
			}

			return distanceFromStartOfClosestPoint;
		}

		public static float DistanceFromStartOfClosestPointOnLineStrip(this IList<float2> points, float2 point, bool loop, int bufferSize = -1)
		{
			if (points == null || points.Count == 0)
				return float.NaN;
			if (points.Count == 1)
				return 0f;

			var previousPoint = points[0];
			var totalLength = 0f;
			//var closestPoint = previousPoint;
			var distanceFromStartOfClosestPoint = 0f;
			var closestPointSqrDistance = float.PositiveInfinity;
			if (bufferSize < 0)
				bufferSize = points.Count;
			for (int i = 1; i < bufferSize; i++)
			{
				var currentPoint = points[i];

				var currentSegmentClosestPoint = ClosestPointOnLineSegment(previousPoint, currentPoint, point, out var distanceFromStartOfCurrentSegmentClosestPoint);
				var sqrDistance = currentSegmentClosestPoint.SqrDistanceTo(point);

				if (closestPointSqrDistance > sqrDistance)
				{
					closestPointSqrDistance = sqrDistance;
					//closestPoint = currentSegmentClosestPoint;
					distanceFromStartOfClosestPoint = totalLength + distanceFromStartOfCurrentSegmentClosestPoint;
				}

				totalLength += currentPoint.DistanceTo(previousPoint);
				previousPoint = currentPoint;
			}
			if (loop)
			{
				var currentPoint = points[0];

				var currentSegmentClosestPoint = ClosestPointOnLineSegment(previousPoint, currentPoint, point, out var distanceFromStartOfCurrentSegmentClosestPoint);
				var sqrDistance = currentSegmentClosestPoint.SqrDistanceTo(point);

				if (closestPointSqrDistance > sqrDistance)
				{
					//closestPointSqrDistance = sqrDistance;
					////closestPoint = currentSegmentClosestPoint;
					distanceFromStartOfClosestPoint = totalLength + distanceFromStartOfCurrentSegmentClosestPoint;
				}

				//totalLength += (currentPoint - previousPoint).magnitude;
				//previousPoint = currentPoint;
			}

			return distanceFromStartOfClosestPoint;
		}

		#endregion

		#region GetPointAheadOfClosestPoint

		public static float2 GetPointAheadOfClosestPoint(this IList<float2> points, float2 point, float resultingPointDistanceToClosestPoint, int bufferSize = -1)
		{
			var distanceFromStartOfClosestPointOnLine = points.DistanceFromStartOfClosestPointOnLineStrip(point, bufferSize);
			var resultingPointDistanceFromStart = distanceFromStartOfClosestPointOnLine + resultingPointDistanceToClosestPoint;
			return points.GetPointAtDistanceFromStart(resultingPointDistanceFromStart, bufferSize);
		}

		public static float2 GetPointAheadOfClosestPoint(this IList<float2> points, float2 point, float resultingPointDistanceToClosestPoint, out float resultingPointDistanceFromStart, int bufferSize = -1)
		{
			var distanceFromStartOfClosestPointOnLine = points.DistanceFromStartOfClosestPointOnLineStrip(point, bufferSize);
			resultingPointDistanceFromStart = distanceFromStartOfClosestPointOnLine + resultingPointDistanceToClosestPoint;
			return points.GetPointAtDistanceFromStart(resultingPointDistanceFromStart, bufferSize);
		}

		public static float2 GetPointAheadOfClosestPoint(this IList<float2> points, float2 point, float resultingPointDistanceToClosestPoint, bool loop, float precalculatedTotalLength = -1f, int bufferSize = -1)
		{
			var distanceFromStartOfClosestPointOnLine = points.DistanceFromStartOfClosestPointOnLineStrip(point, loop, bufferSize);
			var distanceFromStartOfResultingPoint = distanceFromStartOfClosestPointOnLine + resultingPointDistanceToClosestPoint;
			if (loop)
			{
				if (precalculatedTotalLength < 0f)
				{
					precalculatedTotalLength = points.CalculateLineStripLength(loop);
				}
				distanceFromStartOfResultingPoint = distanceFromStartOfResultingPoint % precalculatedTotalLength;
			}
			return points.GetPointAtDistanceFromStart(loop, distanceFromStartOfResultingPoint, bufferSize);
		}

		#endregion

		#region FindClosestValueIndex

		public static int FindClosestValueIndex(this IList<float2> values, float2 targetValue, int startIndex = 0)
		{
			int closestIndex = -1;
			float closestSqrDistance = float.PositiveInfinity;

			for (int i = startIndex; i < values.Count; i++)
			{
				var sqrDistance = values[i].SqrDistanceTo(targetValue);
				if (closestSqrDistance > sqrDistance)
				{
					closestSqrDistance = sqrDistance;
					closestIndex = i;
				}
			}
			return closestIndex;
		}

		public static int FindClosestValueIndex(this float2[] values, float2 targetValue, int startIndex = 0)
		{
			int closestIndex = -1;
			float closestSqrDistance = float.PositiveInfinity;

			for (int i = startIndex; i < values.Length; i++)
			{
				var sqrDistance = values[i].SqrDistanceTo(targetValue);
				if (closestSqrDistance > sqrDistance)
				{
					closestSqrDistance = sqrDistance;
					closestIndex = i;
				}
			}
			return closestIndex;
		}

		#endregion

		#region FindFirstNonNaNValueIndex

		public static int FindFirstNonNaNValueIndex(this IList<float2> values)
		{
			for (int i = 0; i < values.Count; i++)
			{
				if (!values[i].IsAnyNaN())
					return i;
			}
			return -1;
		}

		public static int FindFirstNonNaNValueIndex(this float2[] values)
		{
			for (int i = 0; i < values.Length; i++)
			{
				if (!values[i].IsAnyNaN())
					return i;
			}
			return -1;
		}

		#endregion

		#region Point Inside Polygon

		/// <summary>
		/// See also <seealso cref="Polygon2"/> for bounds checking.
		///
		/// Source: https://stackoverflow.com/questions/217578/how-can-i-determine-whether-a-2d-point-is-within-a-polygon
		/// </summary>
		public static bool IsPointInsidePolygon(this IList<float2> polygon, float2 point)
		{
			// https://wrf.ecse.rpi.edu/Research/Short_Notes/pnpoly.html
			bool inside = false;
			for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
			{
				if ((polygon[i].y > point.y) != (polygon[j].y > point.y) &&
				    point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x)
				{
					inside = !inside;
				}
			}
			return inside;
		}

		#endregion
	}

}
