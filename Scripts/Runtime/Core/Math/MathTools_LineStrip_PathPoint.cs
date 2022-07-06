#if UNITY // TODO-UniversalExtenity: Convert these to Mathematics after importing it into Universal project.

using System;
using Extenity.DataToolbox;
using UnityEngine;

namespace Extenity.MathToolbox
{

	public static partial class MathTools
	{
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

		public static void InsertPositionAndCalculateLengths(this PathPoint[] points, int pointIndex, Vector3 position, out PathPoint[] result)
		{
			if (points == null)
				throw new NullReferenceException("points");
			if (pointIndex < 0 || pointIndex > points.Length)
				throw new ArgumentOutOfRangeException(nameof(pointIndex), pointIndex, "Index is out of range.");

			points.Insert(pointIndex, new PathPoint(position), out result);

			// TODO: Optimize
			result.CalculateLengths();
		}

		public static void AddPositionAndCalculateLengths(this PathPoint[] points, Vector3 position, out PathPoint[] result)
		{
			if (points == null)
				throw new NullReferenceException("points");

			points.Add(new PathPoint(position), out result);

			// TODO: Optimize
			result.CalculateLengths();
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
	}

}

#endif
