using System;
using System.Collections.Generic;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Extenity.MathToolbox
{

	public static class float3Tools
	{
		public static readonly float3 Zero = float3(0f, 0f, 0f);
		public static readonly float3 One = float3(1f, 1f, 1f);
		public static readonly float3 Forward = float3(0f, 0f, 1f);
		public static readonly float3 Back = float3(0f, 0f, -1f);
		public static readonly float3 Up = float3(0f, 1f, 0f);
		public static readonly float3 Down = float3(0f, -1f, 0f);
		public static readonly float3 Left = float3(-1f, 0f, 0f);
		public static readonly float3 Right = float3(1f, 0f, 0f);
		public static readonly float3 PositiveInfinity = float3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
		public static readonly float3 NegativeInfinity = float3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
		public static readonly float3 MaxValue = float3(float.MaxValue, float.MaxValue, float.MaxValue);
		public static readonly float3 MinValue = float3(float.MinValue, float.MinValue, float.MinValue);
		public static readonly float3 NaN = float3(float.NaN, float.NaN, float.NaN);

		#region Basic Checks - Zero

		public static bool IsAllZero(this float3 value)
		{
			return value.x.IsZero() && value.y.IsZero() && value.z.IsZero();
		}

		public static bool IsAnyZero(this float3 value)
		{
			return value.x.IsZero() || value.y.IsZero() || value.z.IsZero();
		}

		public static bool IsAllNonZero(this float3 value)
		{
			return !value.x.IsZero() && !value.y.IsZero() && !value.z.IsZero();
		}

		public static bool IsAnyNonZero(this float3 value)
		{
			return !value.x.IsZero() || !value.y.IsZero() || !value.z.IsZero();
		}

		#endregion

		#region Basic Checks - Infinity

		public static bool IsAllInfinity(this float3 value)
		{
			return float.IsInfinity(value.x) && float.IsInfinity(value.y) && float.IsInfinity(value.z);
		}

		public static bool IsAnyInfinity(this float3 value)
		{
			return float.IsInfinity(value.x) || float.IsInfinity(value.y) || float.IsInfinity(value.z);
		}

		#endregion

		#region Basic Checks - NaN

		public static bool IsAllNaN(this float3 value)
		{
			return float.IsNaN(value.x) && float.IsNaN(value.y) && float.IsNaN(value.z);
		}

		public static bool IsAnyNaN(this float3 value)
		{
			return float.IsNaN(value.x) || float.IsNaN(value.y) || float.IsNaN(value.z);
		}

		#endregion

		#region Basic Checks - Equality

		public static bool IsAllEqual(this float3 value, float val)
		{
			return value.x == val && value.y == val && value.z == val;
		}

		public static bool IsAnyEqual(this float3 value, float val)
		{
			return value.x == val || value.y == val || value.z == val;
		}

		public static bool IsAllAlmostEqual(this float3 value, float val, float precision = MathTools.ZeroTolerance)
		{
			return value.x.IsAlmostEqual(val, precision) && value.y.IsAlmostEqual(val, precision) && value.z.IsAlmostEqual(val, precision);
		}

		public static bool IsAnyAlmostEqual(this float3 value, float val, float precision = MathTools.ZeroTolerance)
		{
			return value.x.IsAlmostEqual(val, precision) || value.y.IsAlmostEqual(val, precision) || value.z.IsAlmostEqual(val, precision);
		}

		public static bool IsAlmostEqual(this float3 value1, float3 value2, float precision = MathTools.ZeroTolerance)
		{
			value1 = value1 - value2;
			return
				value1.x <= precision && value1.x >= -precision &&
				value1.y <= precision && value1.y >= -precision &&
				value1.z <= precision && value1.z >= -precision;
		}

		#endregion

		#region Basic Checks - Range

		public static bool IsAllBetween(this float3 value, float minVal, float maxVal)
		{
			return
				value.x <= maxVal && value.x >= minVal &&
				value.y <= maxVal && value.y >= minVal &&
				value.z <= maxVal && value.z >= minVal;
		}

		#endregion

		#region Basic Checks - Unit

		public static bool IsUnit(this float3 value)
		{
			return length(value).IsAlmostEqual(1f);
		}

		#endregion

		#region Mid

		public static float3 Mid(this float3 va, float3 vb)
		{
			vb.x = (va.x + vb.x) * 0.5f;
			vb.y = (va.y + vb.y) * 0.5f;
			vb.z = (va.z + vb.z) * 0.5f;
			return vb;
		}

		#endregion

		#region Clamp Components

		public static float3 ClampComponents(this float3 value, float min, float max)
		{
			return new float3(
				clamp(value.x, min, max),
				clamp(value.y, min, max),
				clamp(value.z, min, max));
		}

		#endregion

		#region Raise To Minimum

		public static float3 RaiseToMinimum(this float3 value, float min)
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

		public static float3 ClampLength01(this float3 value)
		{
			if (value.x * value.x + value.y * value.y + value.z * value.z > 1f)
				return normalize(value);
			return value;
		}

		public static float3 ClampLengthMax(this float3 value, float max)
		{
			if (length(value) > max)
				return normalize(value) * max;
			return value;
		}

		public static float3 ClampLengthMin(this float3 value, float min)
		{
			if (length(value) < min)
				return normalize(value) * min;
			return value;
		}

		public static float3 ClampSqrLengthMax(this float3 value, float sqrMax)
		{
			if (lengthsq(value) > sqrMax)
				return normalize(value) * sqrMax;
			return value;
		}

		public static float3 ClampSqrLengthMin(this float3 value, float sqrMin)
		{
			if (lengthsq(value) < sqrMin)
				return normalize(value) * sqrMin;
			return value;
		}

		#endregion

		#region Min / Max Component

		public static float MinComponent(this float3 value)
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

		public static float MaxComponent(this float3 value)
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

		public static float MaxComponentXY(this float3 value)
		{
			return value.x > value.y ? value.x : value.y;
		}

		public static float MaxComponentXZ(this float3 value)
		{
			return value.x > value.z ? value.x : value.z;
		}

		public static float MultiplyComponents(this float3 value)
		{
			return value.x * value.y * value.y;
		}

		#endregion

		#region Magnitude

		public static float MagnitudeXY(this float3 value)
		{
			return sqrt(value.x * value.x + value.y * value.y);
		}

		public static float MagnitudeXZ(this float3 value)
		{
			return sqrt(value.x * value.x + value.z * value.z);
		}

		public static float SqrMagnitudeXY(this float3 value)
		{
			return value.x * value.x + value.y * value.y;
		}

		public static float SqrMagnitudeXZ(this float3 value)
		{
			return value.x * value.x + value.z * value.z;
		}

		#endregion

		#region Dot

		public static float DotXY(this float3 lhs, float3 rhs)
		{
			return lhs.x * rhs.x + lhs.y * rhs.y;
		}

		public static float DotXZ(this float3 lhs, float3 rhs)
		{
			return lhs.x * rhs.x + lhs.z * rhs.z;
		}

		#endregion

		#region Distance and Difference

		public static float SqrDistanceTo(this float3 a, float3 b)
		{
			var dx = b.x - a.x;
			var dy = b.y - a.y;
			var dz = b.z - a.z;
			return dx * dx + dy * dy + dz * dz;
		}

		public static float SqrDistanceToXY(this float3 a, float3 b)
		{
			var dx = b.x - a.x;
			var dy = b.y - a.y;
			return dx * dx + dy * dy;
		}

		public static float SqrDistanceToXY(this float3 a, float2 b)
		{
			var dx = b.x - a.x;
			var dy = b.y - a.y;
			return dx * dx + dy * dy;
		}

		public static float SqrDistanceToXZ(this float3 a, float3 b)
		{
			var dx = b.x - a.x;
			var dz = b.z - a.z;
			return dx * dx + dz * dz;
		}

		public static float SqrDistanceToXZ(this float3 a, float2 b)
		{
			var dx = b.x - a.x;
			var dz = b.y - a.z;
			return dx * dx + dz * dz;
		}

		public static float DistanceTo(this float3 a, float3 b)
		{
			var dx = b.x - a.x;
			var dy = b.y - a.y;
			var dz = b.z - a.z;
			return sqrt(dx * dx + dy * dy + dz * dz);
		}

		public static float DistanceToXY(this float3 a, float3 b)
		{
			var dx = b.x - a.x;
			var dy = b.y - a.y;
			return sqrt(dx * dx + dy * dy);
		}

		public static float DistanceToXY(this float3 a, float2 b)
		{
			var dx = b.x - a.x;
			var dy = b.y - a.y;
			return sqrt(dx * dx + dy * dy);
		}

		public static float DistanceToXZ(this float3 a, float3 b)
		{
			var dx = b.x - a.x;
			var dz = b.z - a.z;
			return sqrt(dx * dx + dz * dz);
		}

		public static float DistanceToXZ(this float3 a, float2 b)
		{
			var dx = b.x - a.x;
			var dz = b.y - a.z;
			return sqrt(dx * dx + dz * dz);
		}

		public static float2 Difference2ToXY(this float3 a, float3 b)
		{
			return new float2(b.x - a.x, b.y - a.y);
		}

		public static float2 Difference2ToXY(this float3 a, float2 b)
		{
			return new float2(b.x - a.x, b.y - a.y);
		}

		public static float2 Difference2ToXZ(this float3 a, float3 b)
		{
			return new float2(b.x - a.x, b.z - a.z);
		}

		public static float2 Difference2ToXZ(this float3 a, float2 b)
		{
			return new float2(b.x - a.x, b.y - a.z);
		}

		public static float3 Difference3ToXY(this float3 a, float3 b)
		{
			return new float3(b.x - a.x, b.y - a.y, 0f);
		}

		public static float3 Difference3ToXZ(this float3 a, float3 b)
		{
			return new float3(b.x - a.x, 0f, b.z - a.z);
		}

		#endregion

		#region Inside Bounds

		public static bool IsInsideBounds(this float3 a, float3 b, float maxDistance)
		{
			var dx = b.x - a.x;
			var dy = b.y - a.y;
			var dz = b.z - a.z;
			return
				dx > -maxDistance && dx < maxDistance &&
				dy > -maxDistance && dy < maxDistance &&
				dz > -maxDistance && dz < maxDistance;
		}

		public static bool IsInsideBoundsXY(this float3 a, float3 b, float maxDistance)
		{
			var dx = b.x - a.x;
			var dy = b.y - a.y;
			return
				dx > -maxDistance && dx < maxDistance &&
				dy > -maxDistance && dy < maxDistance;
		}

		public static bool IsInsideBoundsXY(this float3 a, float2 b, float maxDistance)
		{
			var dx = b.x - a.x;
			var dy = b.y - a.y;
			return
				dx > -maxDistance && dx < maxDistance &&
				dy > -maxDistance && dy < maxDistance;
		}

		public static bool IsInsideBoundsXZ(this float3 a, float3 b, float maxDistance)
		{
			var dx = b.x - a.x;
			var dz = b.z - a.z;
			return
				dx > -maxDistance && dx < maxDistance &&
				dz > -maxDistance && dz < maxDistance;
		}

		public static bool IsInsideBoundsXZ(this float3 a, float2 b, float maxDistance)
		{
			var dx = b.x - a.x;
			var dz = b.y - a.z;
			return
				dx > -maxDistance && dx < maxDistance &&
				dz > -maxDistance && dz < maxDistance;
		}

		#endregion

		#region Rotation

		public static float3 RotateAroundZ(this float3 vector, float angleInRadians)
		{
			float cosa = cos(angleInRadians);
			float sina = sin(angleInRadians);
			return new float3(cosa * vector.x - sina * vector.y, sina * vector.x + cosa * vector.y, 0f);
		}

		public static float3 RotateAroundY(this float3 vector, float angleInRadians)
		{
			float cosa = cos(angleInRadians);
			float sina = sin(angleInRadians);
			return new float3(cosa * vector.x - sina * vector.z, 0f, sina * vector.x + cosa * vector.z);
		}

		public static float3 RotateAroundX(this float3 vector, float angleInRadians)
		{
			float cosa = cos(angleInRadians);
			float sina = sin(angleInRadians);
			return new float3(0f, cosa * vector.y - sina * vector.z, sina * vector.y + cosa * vector.z);
		}

		#endregion

		#region Angles

		public static float AngleBetween(this float3 vector1, float3 vector2)
		{
			return acos(dot(normalize(vector1), normalize(vector2)));
		}

		public static float AngleBetweenXY(this float3 vector1, float3 vector2)
		{
			return acos(DotXY(normalize(vector1), normalize(vector2)));
		}

		public static float AngleBetweenXZ(this float3 vector1, float3 vector2)
		{
			return acos(DotXZ(normalize(vector1), normalize(vector2)));
		}

		public static float AngleBetween_NegPIToPI(this float3 vector1, float3 vector2, float3 referencePlaneNormal)
		{
			var angle = acos(dot(normalize(vector1), normalize(vector2)));
			if (float.IsNaN(angle))
				return 0f; // Vectors are almost in the same direction.

			var crossed = cross(vector1, vector2);

			if (dot(referencePlaneNormal, crossed) > 0f)
			{
				angle = -angle;
			}
			return angle;
		}

		#endregion

		#region Direction / Projection On Ground Plane

		public static float3 GetDirectionOnGroundPlane(this float3 vector)
		{
			var result = vector;
			result.y = 0f;
			return normalize(result);
		}

		public static float3 GetProjectionOnGroundPlane(this float3 vector)
		{
			var result = vector;
			result.y = 0f;
			return result;
		}

		#endregion

		#region Swap

		public static void SwapToMakeLesserAndGreater(ref float3 shouldBeLesser, ref float3 shouldBeGreater)
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

		public static void ChangeZerosTo(ref float3 value, float changeTo)
		{
			if (value.x == 0f) value.x = changeTo;
			if (value.y == 0f) value.y = changeTo;
			if (value.z == 0f) value.z = changeTo;
		}

		public static float3 MakeZeroIfNaN(this float3 val)
		{
			if (float.IsNaN(val.x)) val.x = 0f;
			if (float.IsNaN(val.y)) val.y = 0f;
			if (float.IsNaN(val.z)) val.z = 0f;
			return val;
		}

		// @formatter:off
		public static float3 ScaleX(this float3 vector, float scale) { vector.x *= scale; return vector; }
		public static float3 ScaleY(this float3 vector, float scale) { vector.y *= scale; return vector; }
		public static float3 ScaleZ(this float3 vector, float scale) { vector.z *= scale; return vector; }

		public static float3 WithX(this float3 value, float overriddenX) { value.x = overriddenX; return value; }
		public static float3 WithY(this float3 value, float overriddenY) { value.y = overriddenY; return value; }
		public static float3 WithZ(this float3 value, float overriddenZ) { value.z = overriddenZ; return value; }

		public static float3 AddX(this float3 value, float addedX) { value.x += addedX; return value; }
		public static float3 AddY(this float3 value, float addedY) { value.y += addedY; return value; }
		public static float3 AddZ(this float3 value, float addedZ) { value.z += addedZ; return value; }
		// @formatter:on

		#endregion

		#region Flat Check

		public static bool IsFlatX(this List<float3> points)
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

		public static bool IsFlatY(this List<float3> points)
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

		public static bool IsFlatZ(this List<float3> points)
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
