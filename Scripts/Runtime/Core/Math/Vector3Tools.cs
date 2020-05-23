using System;
using System.Collections.Generic;
using UnityEngine;

namespace Extenity.MathToolbox
{

	public static class Vector3Tools
	{
		public static readonly Vector3 Zero = Vector3.zero;
		public static readonly Vector3 One = Vector3.one;
		public static readonly Vector3 Forward = Vector3.forward;
		public static readonly Vector3 Back = Vector3.back;
		public static readonly Vector3 Up = Vector3.up;
		public static readonly Vector3 Down = Vector3.down;
		public static readonly Vector3 Left = Vector3.left;
		public static readonly Vector3 Right = Vector3.right;
		public static readonly Vector3 PositiveInfinity = Vector3.positiveInfinity;
		public static readonly Vector3 NegativeInfinity = Vector3.negativeInfinity;
		public static readonly Vector3 NaN = new Vector3(float.NaN, float.NaN, float.NaN);

		#region Basic Checks - Zero

		public static bool IsAllZero(this Vector3 value)
		{
			return value.x.IsZero() && value.y.IsZero() && value.z.IsZero();
		}

		public static bool IsAnyZero(this Vector3 value)
		{
			return value.x.IsZero() || value.y.IsZero() || value.z.IsZero();
		}

		public static bool IsAllNonZero(this Vector3 value)
		{
			return !value.x.IsZero() && !value.y.IsZero() && !value.z.IsZero();
		}

		public static bool IsAnyNonZero(this Vector3 value)
		{
			return !value.x.IsZero() || !value.y.IsZero() || !value.z.IsZero();
		}

		#endregion

		#region Basic Checks - Infinity

		public static bool IsAllInfinity(this Vector3 value)
		{
			return float.IsInfinity(value.x) && float.IsInfinity(value.y) && float.IsInfinity(value.z);
		}

		public static bool IsAnyInfinity(this Vector3 value)
		{
			return float.IsInfinity(value.x) || float.IsInfinity(value.y) || float.IsInfinity(value.z);
		}

		#endregion

		#region Basic Checks - NaN

		public static bool IsAllNaN(this Vector3 value)
		{
			return float.IsNaN(value.x) && float.IsNaN(value.y) && float.IsNaN(value.z);
		}

		public static bool IsAnyNaN(this Vector3 value)
		{
			return float.IsNaN(value.x) || float.IsNaN(value.y) || float.IsNaN(value.z);
		}

		#endregion

		#region Basic Checks - Equality

		public static bool IsAllEqual(this Vector3 value, float val)
		{
			return value.x == val && value.y == val && value.z == val;
		}

		public static bool IsAnyEqual(this Vector3 value, float val)
		{
			return value.x == val || value.y == val || value.z == val;
		}

		public static bool IsAllAlmostEqual(this Vector3 value, float val, float precision = MathTools.ZeroTolerance)
		{
			return value.x.IsAlmostEqual(val, precision) && value.y.IsAlmostEqual(val, precision) && value.z.IsAlmostEqual(val, precision);
		}

		public static bool IsAnyAlmostEqual(this Vector3 value, float val, float precision = MathTools.ZeroTolerance)
		{
			return value.x.IsAlmostEqual(val, precision) || value.y.IsAlmostEqual(val, precision) || value.z.IsAlmostEqual(val, precision);
		}

		public static bool IsAlmostEqual(this Vector3 value1, Vector3 value2, float precision = MathTools.ZeroTolerance)
		{
			value1 = value1 - value2;
			return
				value1.x <= precision && value1.x >= -precision &&
				value1.y <= precision && value1.y >= -precision &&
				value1.z <= precision && value1.z >= -precision;
		}

		#endregion

		#region Basic Checks - Range

		public static bool IsAllBetween(this Vector3 value, float minVal, float maxVal)
		{
			return
				value.x <= maxVal && value.x >= minVal &&
				value.y <= maxVal && value.y >= minVal &&
				value.z <= maxVal && value.z >= minVal;
		}

		#endregion

		#region Basic Checks - Unit

		public static bool IsUnit(this Vector3 value)
		{
			return value.magnitude.IsAlmostEqual(1f);
		}

		#endregion

		#region Vector2 - Vector3 Conversions

		public static Vector2 ToVector2XY(this Vector3 vector) { return new Vector2(vector.x, vector.y); }
		public static Vector2 ToVector2XZ(this Vector3 vector) { return new Vector2(vector.x, vector.z); }
		public static Vector2 ToVector2YZ(this Vector3 vector) { return new Vector2(vector.y, vector.z); }

		public static Vector3Int ToVector3IntRounded(this Vector3 vector) { return new Vector3Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt(vector.z)); }
		public static Vector3Int ToVector3IntFloored(this Vector3 vector) { return new Vector3Int(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y), Mathf.FloorToInt(vector.z)); }
		public static Vector3Int ToVector3IntCeiled(this Vector3 vector) { return new Vector3Int(Mathf.CeilToInt(vector.x), Mathf.CeilToInt(vector.y), Mathf.CeilToInt(vector.z)); }

		#endregion

		#region Mul / Div

		public static Vector3 Mul(this Vector3 va, Vector3 vb)
		{
			return new Vector3(va.x * vb.x, va.y * vb.y, va.z * vb.z);
		}

		public static Vector3 Mul(this Vector3 va, Vector3Int vb)
		{
			return new Vector3(va.x * vb.x, va.y * vb.y, va.z * vb.z);
		}

		public static Vector3 Div(this Vector3 va, Vector3 vb)
		{
			return new Vector3(va.x / vb.x, va.y / vb.y, va.z / vb.z);
		}

		public static Vector3 Div(this Vector3 va, Vector3Int vb)
		{
			return new Vector3(va.x / vb.x, va.y / vb.y, va.z / vb.z);
		}

		#endregion

		#region Four Basic Math Operations on Vector Arrays

		public static void Plus(this Vector3[] array, Vector3 value)
		{
			for (int i = 0; i < array.Length; i++) array[i] += value;
		}

		public static void Plus(this Vector3[] array, Vector3Int value)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i].x += value.x;
				array[i].y += value.y;
				array[i].z += value.z;
			}
		}

		public static void Minus(this Vector3[] array, Vector3 value)
		{
			for (int i = 0; i < array.Length; i++) array[i] -= value;
		}

		public static void Minus(this Vector3[] array, Vector3Int value)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i].x -= value.x;
				array[i].y -= value.y;
				array[i].z -= value.z;
			}
		}

		public static void Mul(this Vector3[] array, Vector3 value)
		{
			for (int i = 0; i < array.Length; i++) array[i] = Mul(array[i], value);
		}

		public static void Mul(this Vector3[] array, Vector3Int value)
		{
			for (int i = 0; i < array.Length; i++) array[i] = Mul(array[i], value);
		}

		public static void Div(this Vector3[] array, Vector3 value)
		{
			for (int i = 0; i < array.Length; i++) array[i] = Div(array[i], value);
		}

		public static void Div(this Vector3[] array, Vector3Int value)
		{
			for (int i = 0; i < array.Length; i++) array[i] = Div(array[i], value);
		}

		#endregion

		#region Mid

		public static Vector3 Mid(this Vector3 va, Vector3 vb)
		{
			vb.x = (va.x + vb.x) * 0.5f;
			vb.y = (va.y + vb.y) * 0.5f;
			vb.z = (va.z + vb.z) * 0.5f;
			return vb;
		}

		#endregion

		#region Clamp Components

		public static Vector3 ClampComponents(this Vector3 value, float min, float max)
		{
			return new Vector3(
				Mathf.Clamp(value.x, min, max),
				Mathf.Clamp(value.y, min, max),
				Mathf.Clamp(value.z, min, max));
		}

		#endregion

		#region Raise To Minimum

		public static Vector3 RaiseToMinimum(this Vector3 value, float min)
		{
			if (value.x > 0f && value.x < min) value.x = min;
			else if (value.x < 0f && value.x > -min) value.x = -min;
			if (value.y > 0f && value.y < min) value.y = min;
			else if (value.y < 0f && value.y > -min) value.y = -min;
			if (value.z > 0f && value.z < min) value.z = min;
			else if (value.z < 0f && value.z > -min) value.z = -min;
			return value;
		}

		#endregion

		#region Clamp Length / SqrLength

		public static Vector3 ClampLength01(this Vector3 value)
		{
			if (value.x * value.x + value.y * value.y + value.z * value.z > 1f)
				return value.normalized;
			return value;
		}

		public static Vector3 ClampLengthMax(this Vector3 value, float max)
		{
			if (value.magnitude > max)
				return value.normalized * max;
			return value;
		}

		public static Vector3 ClampLengthMin(this Vector3 value, float min)
		{
			if (value.magnitude < min)
				return value.normalized * min;
			return value;
		}

		public static Vector3 ClampSqrLengthMax(this Vector3 value, float sqrMax)
		{
			if (value.sqrMagnitude > sqrMax)
				return value.normalized * sqrMax;
			return value;
		}

		public static Vector3 ClampSqrLengthMin(this Vector3 value, float sqrMin)
		{
			if (value.sqrMagnitude < sqrMin)
				return value.normalized * sqrMin;
			return value;
		}

		#endregion

		#region Abs / Sign

		public static Vector3 Abs(this Vector3 value)
		{
			return new Vector3(
				value.x < 0f ? -value.x : value.x,
				value.y < 0f ? -value.y : value.y,
				value.z < 0f ? -value.z : value.z);
		}

		public static Vector3 Sign(this Vector3 value)
		{
			return new Vector3(
				value.x > 0f ? 1f : (value.x < 0f ? -1f : 0f),
				value.y > 0f ? 1f : (value.y < 0f ? -1f : 0f),
				value.z > 0f ? 1f : (value.z < 0f ? -1f : 0f));
		}

		public static Vector3Int SignInt(this Vector3 value)
		{
			return new Vector3Int(
				value.x > 0 ? 1 : (value.x < 0 ? -1 : 0),
				value.y > 0 ? 1 : (value.y < 0 ? -1 : 0),
				value.z > 0 ? 1 : (value.z < 0 ? -1 : 0));
		}

		#endregion

		#region Min / Max Component

		public static float MinComponent(this Vector3 value)
		{
			if (value.x < value.y)
			{
				if (value.x < value.z)
					return value.x;
				//else
				return value.z;
			}
			//else
			//{
			if (value.y < value.z)
				return value.y;
			//else
			return value.z;
			//}
		}

		public static float MaxComponent(this Vector3 value)
		{
			if (value.x > value.y)
			{
				if (value.x > value.z)
					return value.x;
				//else
				return value.z;
			}
			//else
			//{
			if (value.y > value.z)
				return value.y;
			//else
			return value.z;
			//}
		}

		public static float MaxComponentXY(this Vector3 value)
		{
			return value.x > value.y ? value.x : value.y;
		}

		public static float MaxComponentXZ(this Vector3 value)
		{
			return value.x > value.z ? value.x : value.z;
		}

		public static float MultiplyComponents(this Vector3 value)
		{
			return value.x * value.y * value.y;
		}

		#endregion

		#region Magnitude

		public static float MagnitudeXY(this Vector3 value)
		{
			return Mathf.Sqrt(value.x * value.x + value.y * value.y);
		}

		public static float MagnitudeXZ(this Vector3 value)
		{
			return Mathf.Sqrt(value.x * value.x + value.z * value.z);
		}

		public static float SqrMagnitudeXY(this Vector3 value)
		{
			return value.x * value.x + value.y * value.y;
		}

		public static float SqrMagnitudeXZ(this Vector3 value)
		{
			return value.x * value.x + value.z * value.z;
		}

		#endregion

		#region Dot

		public static float DotXY(this Vector3 lhs, Vector3 rhs)
		{
			return lhs.x * rhs.x + lhs.y * rhs.y;
		}

		public static float DotXZ(this Vector3 lhs, Vector3 rhs)
		{
			return lhs.x * rhs.x + lhs.z * rhs.z;
		}

		#endregion

		#region Distance and Difference

		public static float SqrDistanceTo(this Vector3 a, Vector3 b)
		{
			var dx = b.x - a.x;
			var dy = b.y - a.y;
			var dz = b.z - a.z;
			return dx * dx + dy * dy + dz * dz;
		}

		public static float SqrDistanceToXY(this Vector3 a, Vector3 b)
		{
			var dx = b.x - a.x;
			var dy = b.y - a.y;
			return dx * dx + dy * dy;
		}

		public static float SqrDistanceToXY(this Vector3 a, Vector2 b)
		{
			var dx = b.x - a.x;
			var dy = b.y - a.y;
			return dx * dx + dy * dy;
		}

		public static float SqrDistanceToXZ(this Vector3 a, Vector3 b)
		{
			var dx = b.x - a.x;
			var dz = b.z - a.z;
			return dx * dx + dz * dz;
		}

		public static float SqrDistanceToXZ(this Vector3 a, Vector2 b)
		{
			var dx = b.x - a.x;
			var dz = b.y - a.z;
			return dx * dx + dz * dz;
		}

		public static float DistanceTo(this Vector3 a, Vector3 b)
		{
			var dx = b.x - a.x;
			var dy = b.y - a.y;
			var dz = b.z - a.z;
			return Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
		}

		public static float DistanceToXY(this Vector3 a, Vector3 b)
		{
			var dx = b.x - a.x;
			var dy = b.y - a.y;
			return Mathf.Sqrt(dx * dx + dy * dy);
		}

		public static float DistanceToXY(this Vector3 a, Vector2 b)
		{
			var dx = b.x - a.x;
			var dy = b.y - a.y;
			return Mathf.Sqrt(dx * dx + dy * dy);
		}

		public static float DistanceToXZ(this Vector3 a, Vector3 b)
		{
			var dx = b.x - a.x;
			var dz = b.z - a.z;
			return Mathf.Sqrt(dx * dx + dz * dz);
		}

		public static float DistanceToXZ(this Vector3 a, Vector2 b)
		{
			var dx = b.x - a.x;
			var dz = b.y - a.z;
			return Mathf.Sqrt(dx * dx + dz * dz);
		}

		public static Vector2 Difference2ToXY(this Vector3 a, Vector3 b)
		{
			return new Vector2(b.x - a.x, b.y - a.y);
		}

		public static Vector2 Difference2ToXY(this Vector3 a, Vector2 b)
		{
			return new Vector2(b.x - a.x, b.y - a.y);
		}

		public static Vector2 Difference2ToXZ(this Vector3 a, Vector3 b)
		{
			return new Vector2(b.x - a.x, b.z - a.z);
		}

		public static Vector2 Difference2ToXZ(this Vector3 a, Vector2 b)
		{
			return new Vector2(b.x - a.x, b.y - a.z);
		}

		public static Vector3 Difference3ToXY(this Vector3 a, Vector3 b)
		{
			return new Vector3(b.x - a.x, b.y - a.y, 0f);
		}

		public static Vector3 Difference3ToXZ(this Vector3 a, Vector3 b)
		{
			return new Vector3(b.x - a.x, 0f, b.z - a.z);
		}

		#endregion

		#region Inside Bounds

		public static bool IsInsideBounds(this Vector3 a, Vector3 b, float maxDistance)
		{
			var dx = b.x - a.x;
			var dy = b.y - a.y;
			var dz = b.z - a.z;
			return
				dx > -maxDistance && dx < maxDistance &&
				dy > -maxDistance && dy < maxDistance &&
				dz > -maxDistance && dz < maxDistance;
		}

		public static bool IsInsideBoundsXY(this Vector3 a, Vector3 b, float maxDistance)
		{
			var dx = b.x - a.x;
			var dy = b.y - a.y;
			return
				dx > -maxDistance && dx < maxDistance &&
				dy > -maxDistance && dy < maxDistance;
		}

		public static bool IsInsideBoundsXY(this Vector3 a, Vector2 b, float maxDistance)
		{
			var dx = b.x - a.x;
			var dy = b.y - a.y;
			return
				dx > -maxDistance && dx < maxDistance &&
				dy > -maxDistance && dy < maxDistance;
		}

		public static bool IsInsideBoundsXZ(this Vector3 a, Vector3 b, float maxDistance)
		{
			var dx = b.x - a.x;
			var dz = b.z - a.z;
			return
				dx > -maxDistance && dx < maxDistance &&
				dz > -maxDistance && dz < maxDistance;
		}

		public static bool IsInsideBoundsXZ(this Vector3 a, Vector2 b, float maxDistance)
		{
			var dx = b.x - a.x;
			var dz = b.y - a.z;
			return
				dx > -maxDistance && dx < maxDistance &&
				dz > -maxDistance && dz < maxDistance;
		}

		#endregion

		#region Rotation

		public static Vector3 RotateAroundZ(this Vector3 vector, float angleInRadians)
		{
			float cosa = Mathf.Cos(angleInRadians);
			float sina = Mathf.Sin(angleInRadians);
			return new Vector3(cosa * vector.x - sina * vector.y, sina * vector.x + cosa * vector.y, 0f);
		}

		public static Vector3 RotateAroundY(this Vector3 vector, float angleInRadians)
		{
			float cosa = Mathf.Cos(angleInRadians);
			float sina = Mathf.Sin(angleInRadians);
			return new Vector3(cosa * vector.x - sina * vector.z, 0f, sina * vector.x + cosa * vector.z);
		}

		public static Vector3 RotateAroundX(this Vector3 vector, float angleInRadians)
		{
			float cosa = Mathf.Cos(angleInRadians);
			float sina = Mathf.Sin(angleInRadians);
			return new Vector3(0f, cosa * vector.y - sina * vector.z, sina * vector.y + cosa * vector.z);
		}

		#endregion

		#region Angles

		public static float AngleBetween(this Vector3 vector1, Vector3 vector2)
		{
			return Mathf.Acos(Vector3.Dot(vector1.normalized, vector2.normalized));
		}

		public static float AngleBetweenXY(this Vector3 vector1, Vector3 vector2)
		{
			return Mathf.Acos(DotXY(vector1.normalized, vector2.normalized));
		}

		public static float AngleBetweenXZ(this Vector3 vector1, Vector3 vector2)
		{
			return Mathf.Acos(DotXZ(vector1.normalized, vector2.normalized));
		}

		public static float AngleBetween_NegPIToPI(this Vector3 vector1, Vector3 vector2, Vector3 referencePlaneNormal)
		{
			var angle = Mathf.Acos(Vector3.Dot(vector1.normalized, vector2.normalized));
			if (float.IsNaN(angle))
				return 0f; // Vectors are almost in the same direction.

			var cross = Vector3.Cross(vector1, vector2);

			if (Vector3.Dot(referencePlaneNormal, cross) > 0f)
			{
				angle = -angle;
			}
			return angle;
		}

		#endregion

		#region Direction / Projection On Ground Plane

		public static Vector3 GetDirectionOnGroundPlane(this Vector3 vector)
		{
			var result = vector;
			result.y = 0f;
			return result.normalized;
		}

		public static Vector3 GetProjectionOnGroundPlane(this Vector3 vector)
		{
			var result = vector;
			result.y = 0f;
			return result;
		}

		#endregion

		#region Swap

		public static void SwapToMakeLesserAndGreater(ref Vector3 shouldBeLesser, ref Vector3 shouldBeGreater)
		{
			float temp;

			if (shouldBeLesser.x > shouldBeGreater.x)
			{
				temp = shouldBeLesser.x;
				shouldBeLesser.x = shouldBeGreater.x;
				shouldBeGreater.x = temp;
			}

			if (shouldBeLesser.y > shouldBeGreater.y)
			{
				temp = shouldBeLesser.y;
				shouldBeLesser.y = shouldBeGreater.y;
				shouldBeGreater.y = temp;
			}

			if (shouldBeLesser.z > shouldBeGreater.z)
			{
				temp = shouldBeLesser.z;
				shouldBeLesser.z = shouldBeGreater.z;
				shouldBeGreater.z = temp;
			}
		}

		#endregion

		#region Manipulate Components

		public static void ChangeZerosTo(ref Vector3 value, float changeTo)
		{
			if (value.x == 0f) value.x = changeTo;
			if (value.y == 0f) value.y = changeTo;
			if (value.z == 0f) value.z = changeTo;
		}

		public static Vector3 MakeZeroIfNaN(this Vector3 val)
		{
			if (float.IsNaN(val.x)) val.x = 0f;
			if (float.IsNaN(val.y)) val.y = 0f;
			if (float.IsNaN(val.z)) val.z = 0f;
			return val;
		}

		public static Vector3 ScaleX(this Vector3 vector, float scale)
		{
			vector.x *= scale;
			return vector;
		}
		public static Vector3 ScaleY(this Vector3 vector, float scale)
		{
			vector.y *= scale;
			return vector;
		}
		public static Vector3 ScaleZ(this Vector3 vector, float scale)
		{
			vector.z *= scale;
			return vector;
		}

		public static Vector3 WithX(this Vector3 value, float overriddenX)
		{
			value.x = overriddenX;
			return value;
		}

		public static Vector3 WithY(this Vector3 value, float overriddenY)
		{
			value.y = overriddenY;
			return value;
		}

		public static Vector3 WithZ(this Vector3 value, float overriddenZ)
		{
			value.z = overriddenZ;
			return value;
		}

		#endregion

		#region Flat Check

		public static bool IsFlatX(this List<Vector3> points)
		{
			if (points == null || points.Count == 0)
				throw new Exception("List contains no points.");

			var value = points[0].x;
			for (int i = 1; i < points.Count; i++)
			{
				if (!value.IsAlmostEqual(points[i].x))
					return false;
			}

			return true;
		}

		public static bool IsFlatY(this List<Vector3> points)
		{
			if (points == null || points.Count == 0)
				throw new Exception("List contains no points.");

			var value = points[0].y;
			for (int i = 1; i < points.Count; i++)
			{
				if (!value.IsAlmostEqual(points[i].y))
					return false;
			}

			return true;
		}

		public static bool IsFlatZ(this List<Vector3> points)
		{
			if (points == null || points.Count == 0)
				throw new Exception("List contains no points.");

			var value = points[0].z;
			for (int i = 1; i < points.Count; i++)
			{
				if (!value.IsAlmostEqual(points[i].z))
					return false;
			}

			return true;
		}

		#endregion
	}

}
