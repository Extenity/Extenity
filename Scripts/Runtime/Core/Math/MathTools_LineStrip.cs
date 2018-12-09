using System;
using System.Collections.Generic;
using Extenity.DataToolbox;
using Extenity.DebugToolbox;
using UnityEngine;

namespace Extenity.MathToolbox
{

	public static partial class MathTools
	{
		#region Line Strip Segment Count - Vector3

		public static int GetLineSegmentCount(this IList<Vector3> points, bool loop)
		{
			if (points == null || points.Count == 0)
				return 0;

			if (loop)
				return points.Count;
			else
				return points.Count - 1;
		}

		#endregion

		#region Line Strip Segment Count - Vector2

		public static int GetLineSegmentCount(this IList<Vector2> points, bool loop)
		{
			if (points == null || points.Count == 0)
				return 0;

			if (loop)
				return points.Count;
			else
				return points.Count - 1;
		}

		#endregion

		#region Line Strip Length - Vector3

		public static float CalculateLineStripLength(this IList<Vector3> points, bool loop)
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

		public static float CalculateLineStripLength(this IList<Vector3> points, int startIndex, int count)
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

		public static float CalculateAverageLengthOfLineStripParts(this IList<Vector3> points, bool loop)
		{
			if (points == null || points.Count < 2)
				return 0f;

			var totalLength = CalculateLineStripLength(points, loop);
			return totalLength / points.GetLineSegmentCount(loop);
		}

		#endregion

		#region Line Strip Length - Vector2

		public static float CalculateLineStripLength(this IList<Vector2> points, bool loop)
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

		public static float CalculateLineStripLength(this IList<Vector2> points, int startIndex, int count)
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

		public static float CalculateAverageLengthOfLineStripParts(this IList<Vector2> points, bool loop)
		{
			if (points == null || points.Count < 2)
				return 0f;

			var totalLength = CalculateLineStripLength(points, loop);
			return totalLength / points.GetLineSegmentCount(loop);
		}

		#endregion

		#region Line Strip Operations

		public static Vector3 GetPointAtDistanceFromStart(this IList<Vector3> points, float distanceFromStart, ref Vector3 part, int bufferSize = -1)
		{
			if (points == null || points.Count == 0)
				return Vector3Tools.NaN;
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

		public static Vector3 GetPointAtDistanceFromStart(this IList<Vector3> points, bool loop, float distanceFromStart, ref Vector3 part, int bufferSize = -1)
		{
			if (points == null || points.Count == 0)
				return Vector3Tools.NaN;
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

		public static Vector3 GetPointAtDistanceFromStart(this IList<Vector3> points, float distanceFromStart, int bufferSize = -1)
		{
			if (points == null || points.Count == 0)
				return Vector3Tools.NaN;
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

		public static Vector2 GetPointAtDistanceFromStart(this IList<Vector2> points, float distanceFromStart, int bufferSize = -1)
		{
			if (points == null || points.Count == 0)
				return Vector3Tools.NaN;
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

		public static Vector3 GetPointAtDistanceFromStart(this IList<Vector3> points, bool loop, float distanceFromStart, int bufferSize = -1)
		{
			if (points == null || points.Count == 0)
				return Vector3Tools.NaN;
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

		public static Vector3 GetLinePartAtDistanceFromStart(this IList<Vector3> points, float distanceFromStart, ref int startingPointIndexOfPart, ref Vector3 part, int bufferSize = -1)
		{
			if (points == null || points.Count == 0)
				return Vector3Tools.NaN;
			if (points.Count == 1 || distanceFromStart < 0f)
				return points[0];

			var totalDistance = 0f;
			var previousPoint = points[0];
			if (bufferSize < 0)
				bufferSize = points.Count;
			for (int i = 1; i < bufferSize; i++)
			{
				var currentPoint = points[i];
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

		public static Vector3 GetLinePartAtDistanceFromStart(this IList<Vector3> points, bool loop, float distanceFromStart, ref int startingPointIndexOfPart, ref Vector3 part, int bufferSize = -1)
		{
			if (points == null || points.Count == 0)
				return Vector3Tools.NaN;
			if (points.Count == 1 || distanceFromStart < 0f)
				return points[0];

			var totalDistance = 0f;
			var previousPoint = points[0];
			if (bufferSize < 0)
				bufferSize = points.Count;
			for (int i = 1; i < bufferSize; i++)
			{
				var currentPoint = points[i];
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
				var currentPoint = points[0];
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

		#region ClosestPointOnLineStrip

		public static Vector3 ClosestPointOnLineStrip(this IList<Vector3> points, Vector3 point, int bufferSize = -1)
		{
			if (points == null || points.Count == 0)
				return Vector3Tools.NaN;
			if (points.Count == 1)
				return points[0];

			var previousPoint = points[0];
			var closestPoint = previousPoint;
			var closestPointSqrDistance = float.PositiveInfinity;
			if (bufferSize < 0)
				bufferSize = points.Count;
			for (int i = 1; i < bufferSize; i++)
			{
				var currentPoint = points[i];

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

		public static Vector3 ClosestPointOnLineStrip(this IList<Vector3> points, Vector3 point, ref Vector3 part, int bufferSize = -1)
		{
			if (points == null || points.Count == 0)
				return Vector3Tools.NaN;
			if (points.Count == 1)
				return points[0];

			var previousPoint = points[0];
			var closestPoint = previousPoint;
			var closestPointSqrDistance = float.PositiveInfinity;
			if (bufferSize < 0)
				bufferSize = points.Count;
			for (int i = 1; i < bufferSize; i++)
			{
				var currentPoint = points[i];

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

		public static Vector3 ClosestPointOnLineStrip(this IList<Vector3> points, Vector3 point, bool loop, int bufferSize = -1)
		{
			if (points == null || points.Count == 0)
				return Vector3Tools.NaN;
			if (points.Count == 1)
				return points[0];

			var previousPoint = points[0];
			var closestPoint = previousPoint;
			var closestPointSqrDistance = float.PositiveInfinity;
			if (bufferSize < 0)
				bufferSize = points.Count;
			for (int i = 1; i < bufferSize; i++)
			{
				var currentPoint = points[i];

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
				var currentPoint = points[0];

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

		public static Vector3 ClosestPointOnLineStrip(this IList<Vector3> points, Vector3 point, bool loop, ref Vector3 part, int bufferSize = -1)
		{
			if (points == null || points.Count == 0)
				return Vector3Tools.NaN;
			if (points.Count == 1)
				return points[0];

			var previousPoint = points[0];
			var closestPoint = previousPoint;
			var closestPointSqrDistance = float.PositiveInfinity;
			if (bufferSize < 0)
				bufferSize = points.Count;
			for (int i = 1; i < bufferSize; i++)
			{
				var currentPoint = points[i];

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
				var currentPoint = points[0];

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

		public static float DistanceFromStartOfClosestPointOnLineStrip(this IList<Vector3> points, Vector3 point, int bufferSize = -1)
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

		public static float DistanceFromStartOfClosestPointOnLineStrip(this IList<Vector2> points, Vector2 point, int bufferSize = -1)
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

		public static float DistanceFromStartOfClosestPointOnLineStrip(this IList<Vector3> points, Vector3 point, bool loop, int bufferSize = -1)
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

		public static Vector3 GetPointAheadOfClosestPoint(this IList<Vector3> points, Vector3 point, float resultingPointDistanceToClosestPoint, int bufferSize = -1)
		{
			var distanceFromStartOfClosestPointOnLine = points.DistanceFromStartOfClosestPointOnLineStrip(point, bufferSize);
			var resultingPointDistanceFromStart = distanceFromStartOfClosestPointOnLine + resultingPointDistanceToClosestPoint;
			return points.GetPointAtDistanceFromStart(resultingPointDistanceFromStart, bufferSize);
		}

		public static Vector2 GetPointAheadOfClosestPoint(this IList<Vector2> points, Vector2 point, float resultingPointDistanceToClosestPoint, int bufferSize = -1)
		{
			var distanceFromStartOfClosestPointOnLine = points.DistanceFromStartOfClosestPointOnLineStrip(point, bufferSize);
			var resultingPointDistanceFromStart = distanceFromStartOfClosestPointOnLine + resultingPointDistanceToClosestPoint;
			return points.GetPointAtDistanceFromStart(resultingPointDistanceFromStart, bufferSize);
		}

		public static Vector3 GetPointAheadOfClosestPoint(this IList<Vector3> points, Vector3 point, float resultingPointDistanceToClosestPoint, out float resultingPointDistanceFromStart, int bufferSize = -1)
		{
			var distanceFromStartOfClosestPointOnLine = points.DistanceFromStartOfClosestPointOnLineStrip(point, bufferSize);
			resultingPointDistanceFromStart = distanceFromStartOfClosestPointOnLine + resultingPointDistanceToClosestPoint;
			return points.GetPointAtDistanceFromStart(resultingPointDistanceFromStart, bufferSize);
		}

		public static Vector2 GetPointAheadOfClosestPoint(this IList<Vector2> points, Vector2 point, float resultingPointDistanceToClosestPoint, out float resultingPointDistanceFromStart, int bufferSize = -1)
		{
			var distanceFromStartOfClosestPointOnLine = points.DistanceFromStartOfClosestPointOnLineStrip(point, bufferSize);
			resultingPointDistanceFromStart = distanceFromStartOfClosestPointOnLine + resultingPointDistanceToClosestPoint;
			return points.GetPointAtDistanceFromStart(resultingPointDistanceFromStart, bufferSize);
		}

		//public static Vector3 GetPointAheadOfClosestPoint(this IList<Vector3> points, Vector3 point, float resultingPointDistanceToClosestPoint, bool loop, int bufferSize = -1)
		//{
		//	var distanceFromStartOfClosestPointOnLine = points.DistanceFromStartOfClosestPointOnLineStrip(point, bufferSize);
		//	var resultingPointDistanceFromStart = distanceFromStartOfClosestPointOnLine + resultingPointDistanceToClosestPoint;
		//	if (loop)
		//	{
		//		resultingPointDistanceFromStart = resultingPointDistanceFromStart % TotalLength;
		//	}
		//	return points.GetPointAtDistanceFromStart(resultingPointDistanceFromStart, bufferSize);
		//}

		#endregion

		#region Search For Value On Line Strip Elements

		public static int FindClosestValueIndex(this IList<Vector3> values, Vector3 targetValue, int startIndex = 0)
		{
			int closestIndex = -1;
			float closestSqrDistance = float.PositiveInfinity;

			for (int i = startIndex; i < values.Count; i++)
			{
				var value = values[i];
				var sqrDistance = value.SqrDistanceTo(targetValue);
				if (closestSqrDistance > sqrDistance)
				{
					closestSqrDistance = sqrDistance;
					closestIndex = i;
				}
			}
			return closestIndex;
		}

		public static int FindClosestValueIndex(this Vector3[] values, Vector3 targetValue, int startIndex = 0)
		{
			int closestIndex = -1;
			float closestSqrDistance = float.PositiveInfinity;

			for (int i = startIndex; i < values.Length; i++)
			{
				var value = values[i];
				var sqrDistance = value.SqrDistanceTo(targetValue);
				if (closestSqrDistance > sqrDistance)
				{
					closestSqrDistance = sqrDistance;
					closestIndex = i;
				}
			}
			return closestIndex;
		}

		public static int FindClosestValueIndex(this IList<Vector2> values, Vector2 targetValue, int startIndex = 0)
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

		public static int FindClosestValueIndex(this Vector2[] values, Vector2 targetValue, int startIndex = 0)
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

		public static int FindFirstNonNaNValueIndex(this IList<Vector3> values)
		{
			for (int i = 0; i < values.Count; i++)
			{
				var value = values[i];
				if (!value.IsAnyNaN())
					return i;
			}
			return -1;
		}

		public static int FindFirstNonNaNValueIndex(this Vector3[] values)
		{
			for (int i = 0; i < values.Length; i++)
			{
				var value = values[i];
				if (!value.IsAnyNaN())
					return i;
			}
			return -1;
		}

		public static int FindFirstNonNaNValueIndex(this IList<Vector2> values)
		{
			for (int i = 0; i < values.Count; i++)
			{
				if (!values[i].IsAnyNaN())
					return i;
			}
			return -1;
		}

		public static int FindFirstNonNaNValueIndex(this Vector2[] values)
		{
			for (int i = 0; i < values.Length; i++)
			{
				if (!values[i].IsAnyNaN())
					return i;
			}
			return -1;
		}

		#endregion

		#region SortLineStripUsingClosestSequentialPointsMethod

		public static int SortLineStripUsingClosestSequentialPointsMethod(this IList<Vector3> points, Vector3 initialPointReference)
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

					var actualNextPointIndex = points.FindClosestValueIndex(currentPoint, nextPointIndex);
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

		#region Line Strip - Improved

		public static void SetPositionAndCalculateLengths(this PathPoint[] points, int pointIndex, Vector3 position)
		{
			if (points == null)
				throw new NullReferenceException("points");
			if (pointIndex < 0 || pointIndex >= points.Length)
				throw new ArgumentOutOfRangeException(nameof(pointIndex), pointIndex, "Index is out of range.");

			points[pointIndex].Position = position;

			// TODO: Optimize
			points.CalculateLengths();
			//if (pointIndex > 0)
			//{
			//	points[pointIndex - 1].SegmentLength = position.DistanceTo(points[pointIndex - 1].Position);
			//}
		}

		public static PathPoint[] InsertPositionAndCalculateLengths(this PathPoint[] points, int pointIndex, Vector3 position)
		{
			if (points == null)
				throw new NullReferenceException("points");
			if (pointIndex < 0 || pointIndex > points.Length)
				throw new ArgumentOutOfRangeException(nameof(pointIndex), pointIndex, "Index is out of range.");

			var newPoints = points.Insert(pointIndex, new PathPoint(position));

			// TODO: Optimize
			newPoints.CalculateLengths();

			return newPoints;
		}

		public static PathPoint[] AddPositionAndCalculateLengths(this PathPoint[] points, Vector3 position)
		{
			if (points == null)
				throw new NullReferenceException("points");

			var newPoints = points.Add(new PathPoint(position));

			// TODO: Optimize
			newPoints.CalculateLengths();

			return newPoints;
		}

		public static float CalculateLengths(this PathPoint[] points)
		{
			if (points == null || points.Length < 2)
			{
				if (points != null && points.Length > 0)
				{
					points[0].ResetLength();
				}
				return 0f;
			}

			var totalLength = 0f;
			for (int i = 0; i < points.Length - 1; i++)
			{
				var length = points[i].Position.DistanceTo(points[i + 1].Position);
				points[i].TotalLengthUntilThisPoint = totalLength;
				points[i].SegmentLength = length;
				totalLength += length;
			}
			points[points.Length - 1].TotalLengthUntilThisPoint = totalLength;
			points[points.Length - 1].SegmentLength = 0f;

			return totalLength;
		}

		// TODO: Measure performance difference
		//public static Vector3 GetPointAtDistanceFromStart(this IList<PathPoint> points, float distanceFromStart, ref Vector3 part, int bufferSize = -1)
		public static Vector3 GetPointAtDistanceFromStart(this PathPoint[] points, float distanceFromStart, ref Vector3 part, int bufferSize = -1)
		{
			if (points == null || points.Length == 0)
				return Vector3Tools.NaN;
			if (points.Length == 1 || distanceFromStart < 0f)
				return points[0].Position;

			if (bufferSize < 0)
				bufferSize = points.Length;
			for (int i = 1; i < bufferSize; i++)
			{
				if (points[i].TotalLengthUntilThisPoint > distanceFromStart)
				{
					var ratio = (distanceFromStart - points[i - 1].TotalLengthUntilThisPoint) / points[i - 1].SegmentLength;

					var previousPosition = points[i - 1].Position;
					var diff = points[i].Position - previousPosition;
					part = diff;
					return previousPosition + diff * ratio;
				}
			}

			return points[bufferSize - 1].Position; // Last point
		}

		public static Vector3 GetPointAtDistanceFromStart(this PathPoint[] points, float distanceFromStart, int bufferSize = -1)
		{
			if (points == null || points.Length == 0)
				return Vector3Tools.NaN;
			if (points.Length == 1 || distanceFromStart < 0f)
				return points[0].Position;

			if (bufferSize < 0)
				bufferSize = points.Length;
			for (int i = 1; i < bufferSize; i++)
			{
				if (points[i].TotalLengthUntilThisPoint > distanceFromStart)
				{
					var ratio = (distanceFromStart - points[i - 1].TotalLengthUntilThisPoint) / points[i - 1].SegmentLength;

					var previousPosition = points[i - 1].Position;
					var diff = points[i].Position - previousPosition;
					//part = diff;
					return previousPosition + diff * ratio;
				}
			}

			return points[bufferSize - 1].Position; // Last point
		}

		/// <returns>Starting point index of the line segment.</returns>
		public static int GetSegmentIndexAtDistanceFromStart(this PathPoint[] points, float distanceFromStart, int bufferSize = -1)
		{
			if (points == null || points.Length == 0)
				return -1;
			if (points.Length == 1 || distanceFromStart < 0f)
				return 0;

			if (bufferSize < 0)
				bufferSize = points.Length;
			for (int i = 1; i < bufferSize; i++)
			{
				if (points[i].TotalLengthUntilThisPoint > distanceFromStart)
				{
					return i - 1;
				}
			}

			return bufferSize - 1; // Last point
		}

		public static Vector3 GetPointAheadOfClosestPoint(this PathPoint[] points, Vector3 point, float resultingPointDistanceToClosestPoint, int bufferSize = -1)
		{
			var distanceFromStartOfClosestPointOnLine = points.DistanceFromStartOfClosestPointOnLineStrip(point, bufferSize);
			var resultingPointDistanceFromStart = distanceFromStartOfClosestPointOnLine + resultingPointDistanceToClosestPoint;
			return points.GetPointAtDistanceFromStart(resultingPointDistanceFromStart, bufferSize);
		}

		public static Vector3 GetPointAheadOfClosestPoint(this PathPoint[] points, Vector3 point, float resultingPointDistanceToClosestPoint, out float resultingPointDistanceFromStart, int bufferSize = -1)
		{
			var distanceFromStartOfClosestPointOnLine = points.DistanceFromStartOfClosestPointOnLineStrip(point, bufferSize);
			resultingPointDistanceFromStart = distanceFromStartOfClosestPointOnLine + resultingPointDistanceToClosestPoint;
			return points.GetPointAtDistanceFromStart(resultingPointDistanceFromStart, bufferSize);
		}

		public static float DistanceFromStartOfClosestPointOnLineStrip(this PathPoint[] points, Vector3 point, int bufferSize = -1)
		{
			if (points == null || points.Length == 0)
				return float.NaN;
			if (points.Length == 1)
				return 0f;

			var previousPointPosition = points[0].Position;
			var distanceFromStartOfClosestPoint = 0f;
			var closestPointSqrDistance = float.PositiveInfinity;
			if (bufferSize < 0)
				bufferSize = points.Length;
			for (int i = 1; i < bufferSize; i++)
			{
				var currentPointPosition = points[i].Position;

				var currentSegmentClosestPoint = MathTools.ClosestPointOnLineSegment(previousPointPosition, currentPointPosition, point, out var distanceFromStartOfCurrentSegmentClosestPoint);
				var sqrDistance = currentSegmentClosestPoint.SqrDistanceTo(point);

				if (closestPointSqrDistance > sqrDistance)
				{
					closestPointSqrDistance = sqrDistance;
					//closestPoint = currentSegmentClosestPoint;
					distanceFromStartOfClosestPoint = points[i - 1].TotalLengthUntilThisPoint + distanceFromStartOfCurrentSegmentClosestPoint;
				}

				previousPointPosition = currentPointPosition;
			}

			return distanceFromStartOfClosestPoint;
		}

		#endregion
	}

}
