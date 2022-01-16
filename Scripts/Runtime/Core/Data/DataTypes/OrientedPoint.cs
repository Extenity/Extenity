using System;
using Extenity.MathToolbox;
using Unity.Mathematics;
using static Unity.Mathematics.math;
#if UNITY
using UnityEngine;
#endif

namespace Extenity.DataToolbox
{

	[Serializable]
	public struct OrientedPoint
	{
		public float3 Position;

		/// <summary>
		/// This field can be used as forward vector, up vector or euler angles in the context.
		/// </summary>
		public float3 Orientation;

		public OrientedPoint(float3 position, float3 orientation)
		{
			Position = position;
			Orientation = orientation;
		}

		public static readonly OrientedPoint NaN = new OrientedPoint(float3Tools.NaN, float3Tools.NaN);

		public bool IsAnyNaN => Position.IsAnyNaN() || Orientation.IsAnyNaN();

		public OrientedPoint Mid(OrientedPoint other)
		{
			var orientation = (Orientation + other.Orientation) * 0.5f;
			var lengths = (length(Orientation) + length(other.Orientation)) * 0.5f;
			orientation = normalize(orientation) * lengths;
			return new OrientedPoint(
				(Position + other.Position) * 0.5f,
				orientation);
		}

		public float3 MidPosition(OrientedPoint other)
		{
			return (Position + other.Position) * 0.5f;
		}

		public float3 MidOrientation(OrientedPoint other)
		{
			var orientation = (Orientation + other.Orientation) * 0.5f;
			var lengths = (length(Orientation) + length(other.Orientation)) * 0.5f;
			return normalize(orientation) * lengths;
		}

		public OrientedPoint WithPosition(float3 position)
		{
			return new OrientedPoint(position, Orientation);
		}

		public OrientedPoint WithOrientation(float3 orientation)
		{
			return new OrientedPoint(Position, orientation);
		}
	}

#if UNITY
	public static class OrientedPointTools
	{
		public static OrientedPoint TransformOrientedPoint(this Transform transform, OrientedPoint point)
		{
			point.Position = transform.TransformPoint(point.Position);
			point.Orientation = transform.TransformVector(point.Orientation);
			return point;
		}

		public static OrientedPoint InverseTransformOrientedPoint(this Transform transform, OrientedPoint point)
		{
			point.Position = transform.InverseTransformPoint(point.Position);
			point.Orientation = transform.InverseTransformVector(point.Orientation);
			return point;
		}

		public static OrientedPoint MultiplyPoint(this Matrix4x4 matrix, OrientedPoint point)
		{
			point.Position = matrix.MultiplyPoint(point.Position);
			point.Orientation = matrix.MultiplyVector(point.Orientation);
			return point;
		}

		public static OrientedPoint TransformPointFromLocalToLocal(this OrientedPoint point, Transform currentTransform, Transform newTransform)
		{
			return TransformPointFromLocalToLocal(point, currentTransform.worldToLocalMatrix, newTransform.localToWorldMatrix);
		}

		public static OrientedPoint TransformPointFromLocalToLocal(this OrientedPoint point, Matrix4x4 inverseCurrentMatrix, Transform newTransform)
		{
			return TransformPointFromLocalToLocal(point, inverseCurrentMatrix, newTransform.localToWorldMatrix);
		}

		public static OrientedPoint TransformPointFromLocalToLocal(this OrientedPoint point, Matrix4x4 inverseCurrentMatrix, Matrix4x4 newMatrix)
		{
			point.Position = inverseCurrentMatrix.MultiplyPoint(point.Position);
			point.Position = newMatrix.MultiplyPoint(point.Position);

			point.Orientation = inverseCurrentMatrix.MultiplyVector(point.Orientation);
			point.Orientation = newMatrix.MultiplyVector(point.Orientation);

			return point;
		}
	}
#endif

}
