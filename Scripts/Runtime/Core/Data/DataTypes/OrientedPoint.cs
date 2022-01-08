#if UNITY // TODO-UniversalExtenity: Convert these to Mathematics after importing it into Universal project.

using System;
using Extenity.MathToolbox;
using UnityEngine;

namespace Extenity.DataToolbox
{

	[Serializable]
	public struct OrientedPoint
	{
		public Vector3 Position;

		/// <summary>
		/// This field can be used as forward vector, up vector or euler angles in the context.
		/// </summary>
		public Vector3 Orientation;

		public OrientedPoint(Vector3 position, Vector3 orientation)
		{
			Position = position;
			Orientation = orientation;
		}

		public static readonly OrientedPoint NaN = new OrientedPoint(Vector3Tools.NaN, Vector3Tools.NaN);

		public bool IsAnyNaN => Position.IsAnyNaN() || Orientation.IsAnyNaN();

		public OrientedPoint Mid(OrientedPoint other)
		{
			var orientation = (Orientation + other.Orientation) * 0.5f;
			var length = (Orientation.magnitude + other.Orientation.magnitude) * 0.5f;
			orientation = orientation.normalized * length;
			return new OrientedPoint(
				(Position + other.Position) * 0.5f,
				orientation);
		}

		public Vector3 MidPosition(OrientedPoint other)
		{
			return (Position + other.Position) * 0.5f;
		}

		public Vector3 MidOrientation(OrientedPoint other)
		{
			var orientation = (Orientation + other.Orientation) * 0.5f;
			var length = (Orientation.magnitude + other.Orientation.magnitude) * 0.5f;
			return orientation.normalized * length;
		}

		public OrientedPoint WithPosition(Vector3 position)
		{
			return new OrientedPoint(position, Orientation);
		}

		public OrientedPoint WithOrientation(Vector3 orientation)
		{
			return new OrientedPoint(Position, orientation);
		}
	}

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

}

#endif
