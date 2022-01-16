using System;
using System.Collections.Generic;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Extenity.MathToolbox
{

	public static class float2Tools
	{
		public static readonly float2 Zero = float2(0f, 0f);
		public static readonly float2 One = float2(1f, 1f);
		public static readonly float2 Up = float2(0f, 1f);
		public static readonly float2 Down = float2(0f, -1f);
		public static readonly float2 Left = float2(-1f, 0f);
		public static readonly float2 Right = float2(1f, 0f);
		public static readonly float2 PositiveInfinity = float2(float.PositiveInfinity, float.PositiveInfinity);
		public static readonly float2 NegativeInfinity = float2(float.NegativeInfinity, float.NegativeInfinity);
		public static readonly float2 NaN = float2(float.NaN, float.NaN);

		#region Basic Checks - Zero

		public static bool IsAllZero(this float2 value)
		{
			return value.x.IsZero() && value.y.IsZero();
		}

		public static bool IsAnyZero(this float2 value)
		{
			return value.x.IsZero() || value.y.IsZero();
		}

		public static bool IsAllNonZero(this float2 value)
		{
			return !value.x.IsZero() && !value.y.IsZero();
		}

		public static bool IsAnyNonZero(this float2 value)
		{
			return !value.x.IsZero() || !value.y.IsZero();
		}

		#endregion

		#region Basic Checks - Infinity

		public static bool IsAllInfinity(this float2 value)
		{
			return float.IsInfinity(value.x) && float.IsInfinity(value.y);
		}

		public static bool IsAnyInfinity(this float2 value)
		{
			return float.IsInfinity(value.x) || float.IsInfinity(value.y);
		}

		#endregion

		#region Basic Checks - NaN

		public static bool IsAllNaN(this float2 value)
		{
			return float.IsNaN(value.x) && float.IsNaN(value.y);
		}

		public static bool IsAnyNaN(this float2 value)
		{
			return float.IsNaN(value.x) || float.IsNaN(value.y);
		}

		#endregion

		#region Basic Checks - Equality

		public static bool IsAllEqual(this float2 value, float val)
		{
			return value.x == val && value.y == val;
		}

		public static bool IsAnyEqual(this float2 value, float val)
		{
			return value.x == val || value.y == val;
		}

		public static bool IsAllAlmostEqual(this float2 value, float val, float precision = MathTools.ZeroTolerance)
		{
			return value.x.IsAlmostEqual(val, precision) && value.y.IsAlmostEqual(val, precision);
		}

		public static bool IsAnyAlmostEqual(this float2 value, float val, float precision = MathTools.ZeroTolerance)
		{
			return value.x.IsAlmostEqual(val, precision) || value.y.IsAlmostEqual(val, precision);
		}

		public static bool IsAlmostEqual(this float2 value1, float2 value2, float precision = MathTools.ZeroTolerance)
		{
			value1 = value1 - value2;
			return
				value1.x <= precision && value1.x >= -precision &&
				value1.y <= precision && value1.y >= -precision;
		}

		#endregion

		#region Basic Checks - Range

		public static bool IsAllBetween(this float2 value, float minVal, float maxVal)
		{
			return
				value.x <= maxVal && value.x >= minVal &&
				value.y <= maxVal && value.y >= minVal;
		}

		#endregion

		#region Basic Checks - Unit

		public static bool IsUnit(this float2 value)
		{
			return length(value).IsAlmostEqual(1f);
		}

		#endregion

		#region Mid

		public static float2 Mid(this float2 va, float2 vb)
		{
			vb.x = (va.x + vb.x) * 0.5f;
			vb.y = (va.y + vb.y) * 0.5f;
			return vb;
		}

		#endregion

		#region Clamp Components

		public static float2 ClampComponents(this float2 value, float min, float max)
		{
			return new float2(
				clamp(value.x, min, max),
				clamp(value.y, min, max));
		}

		#endregion

		#region Raise To Minimum

		public static float2 RaiseToMinimum(this float2 value, float min)
		{
			if (value.x > 0f && value.x < min) value.x = min;
			else if (value.x < 0f && value.x > -min) value.x = -min;
			if (value.y > 0f && value.y < min) value.y = min;
			else if (value.y < 0f && value.y > -min) value.y = -min;
			return value;
		}

		#endregion

		#region Clamp Length / SqrLength

		public static float2 ClampLength01(this float2 value)
		{
			if (value.x * value.x + value.y * value.y > 1f)
				return normalize(value);
			return value;
		}

		public static float2 ClampLengthMax(this float2 value, float max)
		{
			if (length(value) > max)
				return normalize(value) * max;
			return value;
		}

		public static float2 ClampLengthMin(this float2 value, float min)
		{
			if (length(value) < min)
				return normalize(value) * min;
			return value;
		}

		public static float2 ClampSqrLengthMax(this float2 value, float sqrMax)
		{
			if (lengthsq(value) > sqrMax)
				return normalize(value) * sqrMax;
			return value;
		}

		public static float2 ClampSqrLengthMin(this float2 value, float sqrMin)
		{
			if (lengthsq(value) < sqrMin)
				return normalize(value) * sqrMin;
			return value;
		}

		#endregion

		#region Min / Max Component

		public static float MinComponent(this float2 value)
		{
			return value.x < value.y ? value.x : value.y;
		}

		public static float MaxComponent(this float2 value)
		{
			return value.x > value.y ? value.x : value.y;
		}

		public static float MultiplyComponents(this float2 value)
		{
			return value.x * value.y;
		}

		#endregion

		#region Inside Bounds

		public static bool IsInsideBounds(this float2 a, float2 b, float maxDistance)
		{
			var dx = b.x - a.x;
			var dy = b.y - a.y;
			return
				dx > -maxDistance && dx < maxDistance &&
				dy > -maxDistance && dy < maxDistance;
		}

		#endregion

		#region Rotation

		public static float2 Rotate(this float2 vector, float angleInRadians)
		{
			float cosa = cos(angleInRadians);
			float sina = sin(angleInRadians);
			return new float2(cosa * vector.x - sina * vector.y, sina * vector.x + cosa * vector.y);
		}

		#endregion

		#region Angles

		public static float AngleBetweenXAxis_NegPIToPI(this float2 vector)
		{
			return atan2(vector.y, vector.x);
		}

		public static float AngleBetweenXAxis_ZeroToTwoPI(this float2 vector)
		{
			float angle = atan2(vector.y, vector.x);
			if (angle < 0f)
				return angle + MathTools.TwoPI;
			return angle;
		}

		public static float AngleBetweenXAxis_0To360(this float2 vector)
		{
			float angle = degrees(atan2(vector.y, vector.x));
			if (angle < 0f)
				return angle + 360f;
			return angle;
		}

		public static float AngleBetween(this float2 vector1, float2 vector2)
		{
			return acos(dot(normalize(vector1), normalize(vector2)));
		}

		public static float AngleBetween_NegPIToPI(this float2 vector1, float2 vector2)
		{
			float angle = atan2(vector2.y, vector2.x) - atan2(vector1.y, vector1.x);

			if (angle < 0)
				angle += MathTools.TwoPI;

			if (angle > MathTools.PI)
				angle -= MathTools.TwoPI;

			return angle;
		}

		public static float2 AngleRadianToFloat2(float radian)
		{
			return new float2(cos(radian), sin(radian));
		}

		public static float2 AngleDegreeToFloat2(float degree)
		{
			var radian = radians(degree);
			return new float2(cos(radian), sin(radian));
		}

		#endregion

		#region Perpendicular / Reflection

		public static float2 Perpendicular(this float2 vector)
		{
			return new float2(-vector.y, vector.x);
		}

		public static float2 Reflect(this float2 vector, float2 normal)
		{
			return vector - (2 * dot(vector, normal) * normal);
		}

		#endregion

		#region Swap

		public static float2 Swap(this float2 vector)
		{
			return new float2(vector.y, vector.x);
		}

		public static void SwapToMakeLesserAndGreater(ref float2 shouldBeLesser, ref float2 shouldBeGreater)
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
		}

		#endregion

		#region Manipulate Components

		public static void ChangeZerosTo(ref float2 value, float changeTo)
		{
			if (value.x == 0f) value.x = changeTo;
			if (value.y == 0f) value.y = changeTo;
		}

		public static float2 MakeZeroIfNaN(this float2 val)
		{
			if (float.IsNaN(val.x)) val.x = 0f;
			if (float.IsNaN(val.y)) val.y = 0f;
			return val;
		}

		// @formatter:off
		public static float2 ScaleX(this float2 vector, float scale) { vector.x *= scale; return vector; }
		public static float2 ScaleY(this float2 vector, float scale) { vector.y *= scale; return vector; }

		public static float2 WithX(this float2 value, float overriddenX) { value.x = overriddenX; return value; }
		public static float2 WithY(this float2 value, float overriddenY) { value.y = overriddenY; return value; }

		public static float2 AddX(this float2 value, float addedX) { value.x += addedX; return value; }
		public static float2 AddY(this float2 value, float addedY) { value.y += addedY; return value; }
		// @formatter:on

		#endregion

		#region Flat Check

		public static bool IsFlatX(this List<float2> points)
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

		public static bool IsFlatY(this List<float2> points)
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

		#endregion
	}

}
