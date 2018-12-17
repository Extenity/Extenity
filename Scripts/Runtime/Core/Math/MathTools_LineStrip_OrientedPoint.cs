using System.Collections.Generic;
using Extenity.DataToolbox;
using Extenity.DebugToolbox;
using UnityEngine;

namespace Extenity.MathToolbox
{

	public static partial class MathTools
	{
		#region Line Strip Segment Count

		public static int GetLineSegmentCount(this IList<OrientedPoint> points, bool loop)
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

		public static float CalculateLineStripLength(this IList<OrientedPoint> points, bool loop)
		{
			if (points == null || points.Count < 2)
				return 0f;

			var totalLength = 0f;
			var previousPoint = points[0].Position;
			for (int i = 1; i < points.Count; i++)
			{
				var currentPoint = points[i].Position;
				totalLength += previousPoint.DistanceTo(currentPoint);
				previousPoint = currentPoint;
			}
			if (loop)
			{
				totalLength += previousPoint.DistanceTo(points[0].Position);
			}
			return totalLength;
		}

		public static float CalculateLineStripLength(this IList<OrientedPoint> points, int startIndex, int count)
		{
			if (points == null || points.Count < 2 || count < 2)
				return 0f;

			var totalLength = 0f;
			var previousPoint = points[0].Position;
			var endIndex = startIndex + count;
			for (int i = startIndex + 1; i < endIndex; i++)
			{
				var currentPoint = points[i].Position;
				totalLength += previousPoint.DistanceTo(currentPoint);
				previousPoint = currentPoint;
			}
			return totalLength;
		}

		public static float CalculateAverageLengthOfLineStripParts(this IList<OrientedPoint> points, bool loop)
		{
			if (points == null || points.Count < 2)
				return 0f;

			var totalLength = CalculateLineStripLength(points, loop);
			return totalLength / points.GetLineSegmentCount(loop);
		}

		#endregion

		#region GetPointPositionAtDistanceFromStart

		public static Vector3 GetPointPositionAtDistanceFromStart(this IList<OrientedPoint> points, float distanceFromStart, ref Vector3 part, int bufferSize = -1)
		{
			if (points == null || points.Count == 0)
				return Vector3Tools.NaN;
			if (points.Count == 1 || distanceFromStart < 0f)
				return points[0].Position;

			var totalDistance = 0f;
			var previousPoint = points[0].Position;
			if (bufferSize < 0)
				bufferSize = points.Count;
			for (int i = 1; i < bufferSize; i++)
			{
				var currentPoint = points[i].Position;
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

		public static Vector3 GetPointPositionAtDistanceFromStart(this IList<OrientedPoint> points, bool loop, float distanceFromStart, ref Vector3 part, int bufferSize = -1)
		{
			if (points == null || points.Count == 0)
				return Vector3Tools.NaN;
			if (points.Count == 1 || distanceFromStart < 0f)
				return points[0].Position;

			var totalDistance = 0f;
			var previousPoint = points[0].Position;
			if (bufferSize < 0)
				bufferSize = points.Count;
			for (int i = 1; i < bufferSize; i++)
			{
				var currentPoint = points[i].Position;
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
				var currentPoint = points[0].Position;
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

		public static Vector3 GetPointPositionAtDistanceFromStart(this IList<OrientedPoint> points, float distanceFromStart, int bufferSize = -1)
		{
			if (points == null || points.Count == 0)
				return Vector3Tools.NaN;
			if (points.Count == 1 || distanceFromStart < 0f)
				return points[0].Position;

			var totalDistance = 0f;
			var previousPoint = points[0].Position;
			if (bufferSize < 0)
				bufferSize = points.Count;
			for (int i = 1; i < bufferSize; i++)
			{
				var currentPoint = points[i].Position;
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

		public static Vector3 GetPointPositionAtDistanceFromStart(this IList<OrientedPoint> points, bool loop, float distanceFromStart, int bufferSize = -1)
		{
			if (points == null || points.Count == 0)
				return Vector3Tools.NaN;
			if (points.Count == 1 || distanceFromStart < 0f)
				return points[0].Position;

			var totalDistance = 0f;
			var previousPoint = points[0].Position;
			if (bufferSize < 0)
				bufferSize = points.Count;
			for (int i = 1; i < bufferSize; i++)
			{
				var currentPoint = points[i].Position;
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
				var currentPoint = points[0].Position;
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

		#region GetLinePartAtDistanceFromStart

		public static Vector3 GetLinePartAtDistanceFromStart(this IList<OrientedPoint> points, float distanceFromStart, ref int startingPointIndexOfPart, ref Vector3 part, int bufferSize = -1)
		{
			if (points == null || points.Count == 0)
				return Vector3Tools.NaN;
			if (points.Count == 1 || distanceFromStart < 0f)
				return points[0].Position;

			var totalDistance = 0f;
			var previousPoint = points[0].Position;
			if (bufferSize < 0)
				bufferSize = points.Count;
			for (int i = 1; i < bufferSize; i++)
			{
				var currentPoint = points[i].Position;
				var distance = Vector3.Distance(previousPoint, currentPoint);

				if (distanceFromStart - totalDistance < distance)
				{
					var ratio = (distanceFromStart - totalDistance) / distance;
					DebugAssert.IsBetweenZeroOne(ratio);

					var diff = currentPoint - previousPoint;
					part = diff;
					startingPointIndexOfPart = i - 1;
					return previousPoint + diff * ratio;
				}

				totalDistance += distance;
				previousPoint = currentPoint;
			}

			return previousPoint;
		}

		public static Vector3 GetLinePartAtDistanceFromStart(this IList<OrientedPoint> points, bool loop, float distanceFromStart, ref int startingPointIndexOfPart, ref Vector3 part, int bufferSize = -1)
		{
			if (points == null || points.Count == 0)
				return Vector3Tools.NaN;
			if (points.Count == 1 || distanceFromStart < 0f)
				return points[0].Position;

			var totalDistance = 0f;
			var previousPoint = points[0].Position;
			if (bufferSize < 0)
				bufferSize = points.Count;
			for (int i = 1; i < bufferSize; i++)
			{
				var currentPoint = points[i].Position;
				var distance = Vector3.Distance(previousPoint, currentPoint);

				if (distanceFromStart - totalDistance < distance)
				{
					var ratio = (distanceFromStart - totalDistance) / distance;
					DebugAssert.IsBetweenZeroOne(ratio);

					var diff = currentPoint - previousPoint;
					part = diff;
					startingPointIndexOfPart = i - 1;
					return previousPoint + diff * ratio;
				}

				totalDistance += distance;
				previousPoint = currentPoint;
			}
			if (loop)
			{
				var currentPoint = points[0].Position;
				var distance = Vector3.Distance(previousPoint, currentPoint);

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

		#endregion

		#region ClosestPointPositionOnLineStrip

		public static Vector3 ClosestPointPositionOnLineStrip(this IList<OrientedPoint> points, Vector3 point, int bufferSize = -1)
		{
			if (points == null || points.Count == 0)
				return Vector3Tools.NaN;
			if (points.Count == 1)
				return points[0].Position;

			var previousPoint = points[0].Position;
			var closestPoint = previousPoint;
			var closestPointSqrDistance = float.PositiveInfinity;
			if (bufferSize < 0)
				bufferSize = points.Count;
			for (int i = 1; i < bufferSize; i++)
			{
				var currentPoint = points[i].Position;

				var currentSegmentClosestPoint = ClosestPointOnLineSegment(previousPoint, currentPoint, point);
				var sqrDistance = (currentSegmentClosestPoint - point).sqrMagnitude;

				if (closestPointSqrDistance > sqrDistance)
				{
					closestPointSqrDistance = sqrDistance;
					closestPoint = currentSegmentClosestPoint;
				}

				previousPoint = currentPoint;
			}

			return closestPoint;
		}

		public static Vector3 ClosestPointPositionOnLineStrip(this IList<OrientedPoint> points, Vector3 point, ref Vector3 part, int bufferSize = -1)
		{
			if (points == null || points.Count == 0)
				return Vector3Tools.NaN;
			if (points.Count == 1)
				return points[0].Position;

			var previousPoint = points[0].Position;
			var closestPoint = previousPoint;
			var closestPointSqrDistance = float.PositiveInfinity;
			if (bufferSize < 0)
				bufferSize = points.Count;
			for (int i = 1; i < bufferSize; i++)
			{
				var currentPoint = points[i].Position;

				var currentSegmentClosestPoint = ClosestPointOnLineSegment(previousPoint, currentPoint, point);
				var sqrDistance = (currentSegmentClosestPoint - point).sqrMagnitude;

				if (closestPointSqrDistance > sqrDistance)
				{
					closestPointSqrDistance = sqrDistance;
					closestPoint = currentSegmentClosestPoint;
					part = currentPoint - previousPoint;
				}

				previousPoint = currentPoint;
			}

			return closestPoint;
		}

		public static Vector3 ClosestPointPositionOnLineStrip(this IList<OrientedPoint> points, Vector3 point, bool loop, int bufferSize = -1)
		{
			if (points == null || points.Count == 0)
				return Vector3Tools.NaN;
			if (points.Count == 1)
				return points[0].Position;

			var previousPoint = points[0].Position;
			var closestPoint = previousPoint;
			var closestPointSqrDistance = float.PositiveInfinity;
			if (bufferSize < 0)
				bufferSize = points.Count;
			for (int i = 1; i < bufferSize; i++)
			{
				var currentPoint = points[i].Position;

				var currentSegmentClosestPoint = ClosestPointOnLineSegment(previousPoint, currentPoint, point);
				var sqrDistance = (currentSegmentClosestPoint - point).sqrMagnitude;

				if (closestPointSqrDistance > sqrDistance)
				{
					closestPointSqrDistance = sqrDistance;
					closestPoint = currentSegmentClosestPoint;
				}

				previousPoint = currentPoint;
			}
			if (loop)
			{
				var currentPoint = points[0].Position;

				var currentSegmentClosestPoint = ClosestPointOnLineSegment(previousPoint, currentPoint, point);
				var sqrDistance = (currentSegmentClosestPoint - point).sqrMagnitude;

				if (closestPointSqrDistance > sqrDistance)
				{
					//closestPointSqrDistance = sqrDistance;
					closestPoint = currentSegmentClosestPoint;
				}

				//previousPoint = currentPoint;
			}

			return closestPoint;
		}

		public static Vector3 ClosestPointPositionOnLineStrip(this IList<OrientedPoint> points, Vector3 point, bool loop, ref Vector3 part, int bufferSize = -1)
		{
			if (points == null || points.Count == 0)
				return Vector3Tools.NaN;
			if (points.Count == 1)
				return points[0].Position;

			var previousPoint = points[0].Position;
			var closestPoint = previousPoint;
			var closestPointSqrDistance = float.PositiveInfinity;
			if (bufferSize < 0)
				bufferSize = points.Count;
			for (int i = 1; i < bufferSize; i++)
			{
				var currentPoint = points[i].Position;

				var currentSegmentClosestPoint = ClosestPointOnLineSegment(previousPoint, currentPoint, point);
				var sqrDistance = (currentSegmentClosestPoint - point).sqrMagnitude;

				if (closestPointSqrDistance > sqrDistance)
				{
					closestPointSqrDistance = sqrDistance;
					closestPoint = currentSegmentClosestPoint;
					part = currentPoint - previousPoint;
				}

				previousPoint = currentPoint;
			}
			if (loop)
			{
				var currentPoint = points[0].Position;

				var currentSegmentClosestPoint = ClosestPointOnLineSegment(previousPoint, currentPoint, point);
				var sqrDistance = (currentSegmentClosestPoint - point).sqrMagnitude;

				if (closestPointSqrDistance > sqrDistance)
				{
					//closestPointSqrDistance = sqrDistance;
					closestPoint = currentSegmentClosestPoint;
					part = currentPoint - previousPoint;
				}

				//previousPoint = currentPoint;
			}

			return closestPoint;
		}

		#endregion

		#region DistanceFromStartOfClosestPointOnLineStrip

		public static float DistanceFromStartOfClosestPointOnLineStrip(this IList<OrientedPoint> points, Vector3 point, int bufferSize = -1)
		{
			if (points == null || points.Count == 0)
				return float.NaN;
			if (points.Count == 1)
				return 0f;

			var previousPoint = points[0].Position;
			var totalLength = 0f;
			//var closestPoint = previousPoint;
			var distanceFromStartOfClosestPoint = 0f;
			var closestPointSqrDistance = float.PositiveInfinity;
			if (bufferSize < 0)
				bufferSize = points.Count;
			for (int i = 1; i < bufferSize; i++)
			{
				var currentPoint = points[i].Position;

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

		public static float DistanceFromStartOfClosestPointOnLineStrip(this IList<OrientedPoint> points, Vector3 point, bool loop, int bufferSize = -1)
		{
			if (points == null || points.Count == 0)
				return float.NaN;
			if (points.Count == 1)
				return 0f;

			var previousPoint = points[0].Position;
			var totalLength = 0f;
			//var closestPoint = previousPoint;
			var distanceFromStartOfClosestPoint = 0f;
			var closestPointSqrDistance = float.PositiveInfinity;
			if (bufferSize < 0)
				bufferSize = points.Count;
			for (int i = 1; i < bufferSize; i++)
			{
				var currentPoint = points[i].Position;

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
				var currentPoint = points[0].Position;

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

		#region GetPointPositionAheadOfClosestPoint

		public static Vector3 GetPointPositionAheadOfClosestPoint(this IList<OrientedPoint> points, Vector3 point, float resultingPointDistanceToClosestPoint, int bufferSize = -1)
		{
			var distanceFromStartOfClosestPointOnLine = points.DistanceFromStartOfClosestPointOnLineStrip(point, bufferSize);
			var resultingPointDistanceFromStart = distanceFromStartOfClosestPointOnLine + resultingPointDistanceToClosestPoint;
			return points.GetPointPositionAtDistanceFromStart(resultingPointDistanceFromStart, bufferSize);
		}

		public static Vector3 GetPointPositionAheadOfClosestPoint(this IList<OrientedPoint> points, Vector3 point, float resultingPointDistanceToClosestPoint, out float resultingPointDistanceFromStart, int bufferSize = -1)
		{
			var distanceFromStartOfClosestPointOnLine = points.DistanceFromStartOfClosestPointOnLineStrip(point, bufferSize);
			resultingPointDistanceFromStart = distanceFromStartOfClosestPointOnLine + resultingPointDistanceToClosestPoint;
			return points.GetPointPositionAtDistanceFromStart(resultingPointDistanceFromStart, bufferSize);
		}

		public static Vector3 GetPointPositionAheadOfClosestPoint(this IList<OrientedPoint> points, Vector3 point, float resultingPointDistanceToClosestPoint, bool loop, float precalculatedTotalLength = -1f, int bufferSize = -1)
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
			return points.GetPointPositionAtDistanceFromStart(loop, distanceFromStartOfResultingPoint, bufferSize);
		}

		#endregion

		#region FindClosestValueIndex

		public static int FindClosestValueIndex(this IList<OrientedPoint> values, Vector3 targetValue, int startIndex = 0)
		{
			int closestIndex = -1;
			float closestSqrDistance = float.PositiveInfinity;

			for (int i = startIndex; i < values.Count; i++)
			{
				var sqrDistance = values[i].Position.SqrDistanceTo(targetValue);
				if (closestSqrDistance > sqrDistance)
				{
					closestSqrDistance = sqrDistance;
					closestIndex = i;
				}
			}
			return closestIndex;
		}

		public static int FindClosestValueIndex(this OrientedPoint[] values, Vector3 targetValue, int startIndex = 0)
		{
			int closestIndex = -1;
			float closestSqrDistance = float.PositiveInfinity;

			for (int i = startIndex; i < values.Length; i++)
			{
				var sqrDistance = values[i].Position.SqrDistanceTo(targetValue);
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

		public static int FindFirstNonNaNValueIndex(this IList<OrientedPoint> values)
		{
			for (int i = 0; i < values.Count; i++)
			{
				if (!values[i].IsAnyNaN)
					return i;
			}
			return -1;
		}

		public static int FindFirstNonNaNValueIndex(this OrientedPoint[] values)
		{
			for (int i = 0; i < values.Length; i++)
			{
				if (!values[i].IsAnyNaN)
					return i;
			}
			return -1;
		}

		#endregion

		#region SortLineStripUsingClosestSequentialPointsMethod

		public static int SortLineStripUsingClosestSequentialPointsMethod(this IList<OrientedPoint> points, Vector3 initialPointReference)
		{
			var swapCount = 0;

			// Find initial point and place it in the first index
			{
				var initialPointIndex = points.FindClosestValueIndex(initialPointReference);
				if (initialPointIndex != 0)
				{
					var temp = points[0];
					points[0] = points[initialPointIndex];
					points[initialPointIndex] = temp;
					swapCount++;
				}
			}

			// Sort line points
			{
				for (int iCurrent = 0; iCurrent < points.Count - 1; iCurrent++)
				{
					var currentPoint = points[iCurrent];
					var nextPointIndex = iCurrent + 1;

					var actualNextPointIndex = points.FindClosestValueIndex(currentPoint.Position, nextPointIndex);
					if (actualNextPointIndex != nextPointIndex)
					{
						var temp = points[nextPointIndex];
						points[nextPointIndex] = points[actualNextPointIndex];
						points[actualNextPointIndex] = temp;
						swapCount++;
					}
				}
			}

			return swapCount;
		}

		#endregion
	}

}
