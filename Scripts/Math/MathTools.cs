using System;
using System.Collections.Generic;
using UnityEngine;

public enum AngularDirection : byte
{
	None = 0,
	CW = 1,
	CCW = 2,
}

public static class AngularDirectionExt
{
	public static float ToFloat(this AngularDirection me)
	{
		switch (me)
		{
			case AngularDirection.None: return 0f;
			case AngularDirection.CW: return 1f;
			case AngularDirection.CCW: return -1f;
			default:
				throw new ArgumentOutOfRangeException();
		}

		//return ((float)me).Sign(); // Using sign to make sure it's normalized. Could be helpful for safety.
	}
}

public static class MathTools
{
	#region Int Float Double

	public const float PI = Mathf.PI;
	public const float NegPI = -Mathf.PI;
	public const float PosPI = Mathf.PI;
	public const float TwoPI = 2f * Mathf.PI;
	public const float HalfPI = 0.5f * Mathf.PI;
	public const float E = 2.7182818284590452353602874f;

	private const float ZeroTolerance = 1e-5f;

	public static bool IsZero(this float value)
	{
		return value < ZeroTolerance && value > -ZeroTolerance;
	}
	public static bool IsZero(this float value, float tolerance)
	{
		return value < tolerance && value > -tolerance;
	}
	public static bool IsZero(this double value)
	{
		return value < ZeroTolerance && value > -ZeroTolerance;
	}
	public static bool IsZero(this double value, double tolerance)
	{
		return value < tolerance && value > -tolerance;
	}
	public static bool IsNotZero(this float value)
	{
		return value > ZeroTolerance || value < -ZeroTolerance;
	}
	public static bool IsNotZero(this float value, float tolerance)
	{
		return value > tolerance || value < -tolerance;
	}
	public static bool IsNotZero(this double value)
	{
		return value > ZeroTolerance || value < -ZeroTolerance;
	}
	public static bool IsNotZero(this double value, double tolerance)
	{
		return value > tolerance || value < -tolerance;
	}
	public static bool IsUnit(this float value)
	{
		return value.IsAlmostEqual(1f);
	}
	public static bool IsUnit(this double value)
	{
		return value.IsAlmostEqual(1f, ZeroTolerance);
	}
	public static bool IsAlmostEqual(this double value1, double value2, double precision)
	{
		double diff = value1 - value2;
		return diff <= precision && diff >= -precision;
	}
	public static bool IsAlmostEqual(this float value1, float value2, float precision = ZeroTolerance)
	{
		float diff = value1 - value2;
		return diff <= precision && diff >= -precision;
	}
	public static bool IsBetween(this float val, float a, float b)
	{
		if (a < b)
			return val > a && val < b;
		return val > b && val < a;
	}
	public static bool IsBetweenOrEqual(this float val, float a, float b)
	{
		if (a < b)
			return val >= a && val <= b;
		return val >= b && val <= a;
	}
	public static bool IsBetween(this int val, int a, int b)
	{
		if (a < b)
			return val > a && val < b;
		return val > b && val < a;
	}
	public static bool IsBetweenOrEqual(this int val, int a, int b)
	{
		if (a < b)
			return val >= a && val <= b;
		return val >= b && val <= a;
	}

	public static float MakeZeroIfNaN(this float val)
	{
		if (float.IsNaN(val)) return 0f;
		return val;
	}
	public static Vector3 MakeZeroIfNaN(this Vector3 val)
	{
		if (float.IsNaN(val.x)) val.x = 0f;
		if (float.IsNaN(val.y)) val.y = 0f;
		if (float.IsNaN(val.z)) val.z = 0f;
		return val;
	}
	public static Vector2 MakeZeroIfNaN(this Vector2 val)
	{
		if (float.IsNaN(val.x)) val.x = 0f;
		if (float.IsNaN(val.y)) val.y = 0f;
		return val;
	}

	public static void Swap<T>(ref T variable1, ref T variable2)
	{
		var temp = variable2;
		variable2 = variable1;
		variable1 = temp;
	}

	public static T Swap<T>(this T variable1, ref T variable2)
	{
		var temp = variable2;
		variable2 = variable1;
		return temp;
	}

	public static int Sign(this sbyte value) { return value > 0 ? 1 : (value < 0 ? -1 : 0); }
	public static int Sign(this Int16 value) { return value > 0 ? 1 : (value < 0 ? -1 : 0); }
	public static int Sign(this Int32 value) { return value > 0 ? 1 : (value < 0 ? -1 : 0); }
	public static int Sign(this Int64 value) { return value > 0 ? 1 : (value < 0 ? -1 : 0); }
	public static int SignInt(this float value) { return value > 0 ? 1 : (value < 0 ? -1 : 0); }
	public static int SignInt(this double value) { return value > 0 ? 1 : (value < 0 ? -1 : 0); }
	public static float Sign(this float value) { return value > 0.0f ? 1.0f : (value < 0.0f ? -1.0f : 0.0f); }
	public static double Sign(this double value) { return value > 0.0 ? 1.0 : (value < 0.0 ? -1.0 : 0.0); }

	public static bool IsEven(this sbyte val) { return (val & 1) == 0; }
	public static bool IsEven(this byte val) { return (val & 1) == 0; }
	public static bool IsEven(this Int16 val) { return (val & 1) == 0; }
	public static bool IsEven(this Int32 val) { return (val & 1) == 0; }
	public static bool IsEven(this Int64 val) { return (val & 1) == 0; }
	public static bool IsEven(this UInt16 val) { return (val & 1) == 0; }
	public static bool IsEven(this UInt32 val) { return (val & 1) == 0; }
	public static bool IsEven(this UInt64 val) { return (val & 1) == 0; }
	public static bool IsOdd(this sbyte val) { return (val & 1) != 0; }
	public static bool IsOdd(this byte val) { return (val & 1) != 0; }
	public static bool IsOdd(this Int16 val) { return (val & 1) != 0; }
	public static bool IsOdd(this Int32 val) { return (val & 1) != 0; }
	public static bool IsOdd(this Int64 val) { return (val & 1) != 0; }
	public static bool IsOdd(this UInt16 val) { return (val & 1) != 0; }
	public static bool IsOdd(this UInt32 val) { return (val & 1) != 0; }
	public static bool IsOdd(this UInt64 val) { return (val & 1) != 0; }

	public static void Plus(this int[] array, int value)
	{
		for (int i = 0; i < array.Length; i++)
			array[i] += value;
	}
	public static void Plus(this float[] array, float value)
	{
		for (int i = 0; i < array.Length; i++)
			array[i] += value;
	}
	public static void Mul(this int[] array, int value)
	{
		for (int i = 0; i < array.Length; i++)
			array[i] *= value;
	}
	public static void Mul(this float[] array, float value)
	{
		for (int i = 0; i < array.Length; i++)
			array[i] *= value;
	}

	public static bool IsPowerOfTwo(this int val)
	{
		return (val != 0) && ((val & (val - 1)) == 0);
	}

	public static int Power(int basis, int exponent)
	{
		if (exponent < 0)
			return 0;

		switch (exponent)
		{
			case 0:
				return 1;
			case 1:
				return basis;
			case 2:
				return basis * basis;
			case 3:
				return basis * basis * basis;
			case 4:
				return basis * basis * basis * basis;
			case 5:
				return basis * basis * basis * basis * basis;
			case 6:
				return basis * basis * basis * basis * basis * basis;
			default:
				int result = 1;
				while (exponent > 0)
				{
					if ((exponent & 1) == 1)
						result *= basis;
					exponent >>= 1;
					basis *= basis;
				}
				return result;
		}
	}

	public static float Tanh(float x)
	{
		return (Mathf.Pow(E, x) - Mathf.Pow(E, -x)) / (Mathf.Pow(E, x) + Mathf.Pow(E, -x));
	}

	public static int Abs(this int value)
	{
		return Mathf.Abs(value);
	}

	public static float Abs(this float value)
	{
		return Mathf.Abs(value);
	}

	public static float Round(this float value)
	{
		return Mathf.Round(value);
	}

	public static int RoundToInt(this float value)
	{
		return Mathf.RoundToInt(value);
	}

	public static int Clamp(this int value, int min, int max)
	{
		if (value < min) return min;
		if (value > max) return max;
		return value;
	}
	public static float Clamp(this float value, float min, float max)
	{
		if (value < min) return min;
		if (value > max) return max;
		return value;
	}

	public static float Clamp01(this float value)
	{
		if (value < 0.0f) return 0.0f;
		if (value > 1.0f) return 1f;
		return value;
	}

	public static int ClampNeg1Pos1(this int value)
	{
		if (value < -1) return -1;
		if (value > 1) return 1;
		return value;
	}
	public static float ClampNeg1Pos1(this float value)
	{
		if (value < -1.0f) return -1.0f;
		if (value > 1.0f) return 1f;
		return value;
	}

	public static float Wrap01(this float x)
	{
		return Wrap(x, 0, 1);
	}

	public static float WrapNeg1Pos1(this float x)
	{
		return Wrap(x, -1, 1);
	}

	public static float WrapNegPIToPI(this float x)
	{
		return Wrap(x, NegPI, PosPI);
	}

	public static float WrapZeroToTwoPI(this float x)
	{
		return Wrap(x, 0, TwoPI);
	}

	public static float Wrap(this float x, float min, float max)
	{
		if (x < min)
			return max - (min - x) % (max - min);
		else
			return min + (x - min) % (max - min);
	}

	public static float Lerp(float from, float to, float t)
	{
		if (t <= 0.0f) return from;
		if (t >= 1.0f) return to;
		return from + (to - from) * t;
	}

	public static float SignedSqrt(this float value)
	{
		if (value < 0f)
			return -Mathf.Sqrt(-value);
		return Mathf.Sqrt(value);
	}

	public static float SignedSqr(this float value)
	{
		if (value < 0f)
			return -(value * value);
		return value * value;
	}

	public static float SignedPow(this float value, float power)
	{
		if (value < 0f)
			return -Mathf.Pow(-value, power);
		return Mathf.Pow(value, power);
	}

	public static float Min(float value1, float value2) { return Math.Min(value1, value2); }
	public static float Min(float value1, float value2, float value3) { return Math.Min(value1, Math.Min(value2, value3)); }
	public static float Min(float value1, float value2, float value3, float value4) { return Math.Min(value1, Math.Min(value2, Math.Min(value3, value4))); }
	public static float Min(float value1, float value2, float value3, float value4, float value5) { return Math.Min(value1, Math.Min(value2, Math.Min(value3, Math.Min(value4, value5)))); }

	public static float Max(float value1, float value2) { return Math.Max(value1, value2); }
	public static float Max(float value1, float value2, float value3) { return Math.Max(value1, Math.Max(value2, value3)); }
	public static float Max(float value1, float value2, float value3, float value4) { return Math.Max(value1, Math.Max(value2, Math.Max(value3, value4))); }
	public static float Max(float value1, float value2, float value3, float value4, float value5) { return Math.Max(value1, Math.Max(value2, Math.Max(value3, Math.Max(value4, value5)))); }

	public static float Distance(this float value1, float value2)
	{
		return Mathf.Abs(value1 - value2);
	}

	public static float ToNeg180Pos180(this float value)
	{
		if (value >= -180f && value <= 180f)
			return value;

		value -= 360f;
		if (value >= -180f && value <= 180f)
			return value;

		value = (value % 360f);
		if (value < 0f)
			value += 360f;
		if (value > 180f)
			value -= 360f;
		return value;
	}

	public static float ToNegPIPosPI(this float value)
	{
		if (value >= NegPI && value <= PosPI)
			return value;

		value -= TwoPI;
		if (value >= NegPI && value <= PosPI)
			return value;

		value = (value % TwoPI);
		if (value < 0f)
			value += TwoPI;
		if (value > PosPI)
			value -= TwoPI;
		return value;
	}

	public static float Remap(this float value, float fromMin, float fromMax, float toMin, float toMax)
	{
		if (fromMin.IsAlmostEqual(fromMax))
		{
			throw new ArgumentException("Base value min and max should not be the same.", "fromMin");
		}
		//if (fromMin > fromMax)
		//{
		//	Swap(ref fromMin, ref fromMax);
		//}
		//if (toMin > toMax)
		//{
		//	Swap(ref fromMin, ref fromMax);
		//}

		return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
	}

	public static float RemapClamped(this float value, float fromMin, float fromMax, float toMin, float toMax)
	{
		if (fromMin.IsAlmostEqual(fromMax))
		{
			throw new ArgumentException("Base value min and max should not be the same.", "fromMin");
		}
		//if (fromMin > fromMax)
		//{
		//	Swap(ref fromMin, ref fromMax);
		//}
		//if (toMin > toMax)
		//{
		//	Swap(ref fromMin, ref fromMax);
		//}

		if (value <= fromMin)
			return toMin;
		if (value >= fromMax)
			return toMax;

		return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
	}

	public static float RoundToDigits(this float value, int digitsAfterDecimalPoint)
	{
		if (digitsAfterDecimalPoint < 0) digitsAfterDecimalPoint = 0;
		float factor = Mathf.Pow(10, digitsAfterDecimalPoint);
		return ((int) Mathf.Round(value*factor))/factor;
	}

	public static double RoundToDigits(this double value, int digitsAfterDecimalPoint)
	{
		if (digitsAfterDecimalPoint < 0) digitsAfterDecimalPoint = 0;
		double factor = Math.Pow(10, digitsAfterDecimalPoint);
		return ((int)Math.Round(value * factor)) / factor;
	}

	#endregion

	#region Transform

	public static bool IsPointBehind(this Transform transform, Vector3 point)
	{
		return Vector3.Dot(transform.forward, point - transform.position) < 0.0f;
	}

	public static Vector3 GetPositionRelativeTo(this Transform me, Transform other)
	{
		return me.position - other.position;
	}

	#endregion

	#region Triangle

	public static float AngleOfTriangle(float neighbourSide1, float neighbourSide2, float oppositeSide)
	{
		return Mathf.Acos(
			(neighbourSide1 * neighbourSide1 + neighbourSide2 * neighbourSide2 - oppositeSide * oppositeSide)
			/
			(2f * neighbourSide1 * neighbourSide2)
			);
	}

	public static float AngleOfRightTriangle(float neighbourSideTo90Angle, float hypotenuse)
	{
		return Mathf.Acos(neighbourSideTo90Angle / hypotenuse);
	}

	public static bool IsValidTriangle(Vector3 point1, Vector3 point2, Vector3 point3)
	{
		var a = Vector3.Distance(point1, point2);
		var b = Vector3.Distance(point1, point3);
		var c = Vector3.Distance(point2, point3);

		return
			a + b > c &&
			a + c > b &&
			b + c > a;
	}

	#endregion

	#region Vector

	public static readonly Vector2 Vector2Infinity = new Vector2(Mathf.Infinity, Mathf.Infinity);
	public static readonly Vector2 Vector2NegInfinity = new Vector2(Mathf.NegativeInfinity, Mathf.NegativeInfinity);
	public static readonly Vector2 Vector2NaN = new Vector2(float.NaN, float.NaN);
	public static readonly Vector3 Vector3Infinity = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
	public static readonly Vector3 Vector3NegInfinity = new Vector3(Mathf.NegativeInfinity, Mathf.NegativeInfinity, Mathf.NegativeInfinity);
	public static readonly Vector3 Vector3NaN = new Vector3(float.NaN, float.NaN, float.NaN);

	#region Basic Checks

	public static bool IsZero(this Vector2 value)
	{
		return value.IsAllZero();
	}

	public static bool IsZero(this Vector3 value)
	{
		return value.IsAllZero();
	}

	public static bool IsUnit(this Vector2 value)
	{
		return value.magnitude.IsAlmostEqual(1f);
	}

	public static bool IsUnit(this Vector3 value)
	{
		return value.magnitude.IsAlmostEqual(1f);
	}

	public static bool IsAllEqual(this Vector2 value, float val)
	{
		return value.x == val && value.y == val;
	}

	public static bool IsAllEqual(this Vector3 value, float val)
	{
		return value.x == val && value.y == val && value.z == val;
	}

	public static bool IsAllBetween(this Vector2 value, float minVal, float maxVal)
	{
		return
			value.x <= maxVal && value.x >= minVal &&
			value.y <= maxVal && value.y >= minVal;
	}

	public static bool IsAllBetween(this Vector3 value, float minVal, float maxVal)
	{
		return
			value.x <= maxVal && value.x >= minVal &&
			value.y <= maxVal && value.y >= minVal &&
			value.z <= maxVal && value.z >= minVal;
	}

	public static bool IsAlmostEqualVector2(this Vector2 value1, Vector2 value2, float precision = ZeroTolerance)
	{
		value1 = value1 - value2;
		return
			value1.x <= precision && value1.x >= -precision &&
			value1.y <= precision && value1.y >= -precision;
	}

	public static bool IsAlmostEqualVector3(this Vector3 value1, Vector3 value2, float precision = ZeroTolerance)
	{
		value1 = value1 - value2;
		return
			value1.x <= precision && value1.x >= -precision &&
			value1.y <= precision && value1.y >= -precision &&
			value1.z <= precision && value1.z >= -precision;
	}

	public static bool IsAllZero(this Vector2 value)
	{
		return IsZero(value.x) && IsZero(value.y);
	}

	public static bool IsAllZero(this Vector3 value)
	{
		return IsZero(value.x) && IsZero(value.y) && IsZero(value.z);
	}

	public static bool IsAllInfinity(this Vector2 value)
	{
		return float.IsInfinity(value.x) && float.IsInfinity(value.y);
	}

	public static bool IsAllInfinity(this Vector3 value)
	{
		return float.IsInfinity(value.x) && float.IsInfinity(value.y) && float.IsInfinity(value.z);
	}

	public static bool IsAllNaN(this Vector2 value)
	{
		return float.IsNaN(value.x) && float.IsNaN(value.y);
	}

	public static bool IsAllNaN(this Vector3 value)
	{
		return float.IsNaN(value.x) && float.IsNaN(value.y) && float.IsNaN(value.z);
	}

	public static bool IsAnyEqual(this Vector2 value, float val)
	{
		return value.x == val || value.y == val;
	}

	public static bool IsAnyEqual(this Vector3 value, float val)
	{
		return value.x == val || value.y == val || value.z == val;
	}

	public static bool IsAnyAlmostEqual(this Vector2 value, float val)
	{
		return value.x.IsAlmostEqual(val) || value.y.IsAlmostEqual(val);
	}

	public static bool IsAnyAlmostEqual(this Vector3 value, float val)
	{
		return value.x.IsAlmostEqual(val) || value.y.IsAlmostEqual(val) || value.z.IsAlmostEqual(val);
	}

	public static bool IsAnyZero(this Vector2 value)
	{
		return IsZero(value.x) || IsZero(value.y);
	}

	public static bool IsAnyZero(this Vector3 value)
	{
		return IsZero(value.x) || IsZero(value.y) || IsZero(value.z);
	}

	public static bool IsAnyInfinity(this Vector2 value)
	{
		return float.IsInfinity(value.x) || float.IsInfinity(value.y);
	}

	public static bool IsAnyInfinity(this Vector3 value)
	{
		return float.IsInfinity(value.x) || float.IsInfinity(value.y) || float.IsInfinity(value.z);
	}

	public static bool IsAnyNaN(this Vector2 value)
	{
		return float.IsNaN(value.x) || float.IsNaN(value.y);
	}

	public static bool IsAnyNaN(this Vector3 value)
	{
		return float.IsNaN(value.x) || float.IsNaN(value.y) || float.IsNaN(value.z);
	}

	#endregion

	#region Vector2 - Vector3 Conversions

	public static Vector3 ToVector3XY(this Vector2 vector) { return new Vector3(vector.x, vector.y, 0f); }
	public static Vector3 ToVector3XZ(this Vector2 vector) { return new Vector3(vector.x, 0f, vector.y); }
	public static Vector3 ToVector3YZ(this Vector2 vector) { return new Vector3(0f, vector.x, vector.y); }
	public static Vector2 ToVector2XY(this Vector3 vector) { return new Vector2(vector.x, vector.y); }
	public static Vector2 ToVector2XZ(this Vector3 vector) { return new Vector2(vector.x, vector.z); }
	public static Vector2 ToVector2YZ(this Vector3 vector) { return new Vector2(vector.y, vector.z); }

	public static Vector2Int ToVector2IntRounded(this Vector2 vector) { return new Vector2Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y)); }
	public static Vector2Int ToVector2IntFloored(this Vector2 vector) { return new Vector2Int(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y)); }
	public static Vector2Int ToVector2IntCeiled(this Vector2 vector) { return new Vector2Int(Mathf.CeilToInt(vector.x), Mathf.CeilToInt(vector.y)); }
	public static Vector3Int ToVector3IntRounded(this Vector3 vector) { return new Vector3Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt(vector.z)); }
	public static Vector3Int ToVector3IntFloored(this Vector3 vector) { return new Vector3Int(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y), Mathf.FloorToInt(vector.z)); }
	public static Vector3Int ToVector3IntCeiled(this Vector3 vector) { return new Vector3Int(Mathf.CeilToInt(vector.x), Mathf.CeilToInt(vector.y), Mathf.CeilToInt(vector.z)); }

	#endregion

	#region Vector2 - Vector4 Conversions

	public static Vector4 ToVector4XY(this Vector2 vector) { return new Vector4(vector.x, vector.y, 0f, 0f); }
	public static Vector4 ToVector4YZ(this Vector2 vector) { return new Vector4(0f, vector.x, vector.y, 0f); }
	public static Vector4 ToVector4XZ(this Vector2 vector) { return new Vector4(vector.x, 0f, vector.y, 0f); }
	public static Vector4 ToVector4ZW(this Vector2 vector) { return new Vector4(0f, 0f, vector.x, vector.y); }
	public static Vector2 ToVector2XY(this Vector4 vector) { return new Vector2(vector.x, vector.y); }
	public static Vector2 ToVector2YZ(this Vector4 vector) { return new Vector2(vector.y, vector.z); }
	public static Vector2 ToVector2XZ(this Vector4 vector) { return new Vector2(vector.x, vector.z); }
	public static Vector2 ToVector2ZW(this Vector4 vector) { return new Vector2(vector.z, vector.w); }

	#endregion

	#region Mul / Div

	public static Vector2 Mul(this Vector2 va, Vector2 vb)
	{
		return new Vector2(va.x * vb.x, va.y * vb.y);
	}

	public static Vector3 Mul(this Vector3 va, Vector3 vb)
	{
		return new Vector3(va.x * vb.x, va.y * vb.y, va.z * vb.z);
	}

	public static Vector2 Mul(this Vector2 va, Vector2Int vb)
	{
		return new Vector2(va.x * vb.x, va.y * vb.y);
	}

	public static Vector3 Mul(this Vector3 va, Vector3Int vb)
	{
		return new Vector3(va.x * vb.x, va.y * vb.y, va.z * vb.z);
	}

	public static Vector2 Div(this Vector2 va, Vector2 vb)
	{
		return new Vector2(va.x / vb.x, va.y / vb.y);
	}

	public static Vector3 Div(this Vector3 va, Vector3 vb)
	{
		return new Vector3(va.x / vb.x, va.y / vb.y, va.z / vb.z);
	}

	public static Vector2 Div(this Vector2 va, Vector2Int vb)
	{
		return new Vector2(va.x / vb.x, va.y / vb.y);
	}

	public static Vector3 Div(this Vector3 va, Vector3Int vb)
	{
		return new Vector3(va.x / vb.x, va.y / vb.y, va.z / vb.z);
	}

	#endregion

	#region Four Basic Math Operations on Vector Arrays

	public static void Plus(this Vector2[] array, Vector2 value)
	{
		for (int i = 0; i < array.Length; i++) array[i] += value;
	}

	public static void Plus(this Vector3[] array, Vector3 value)
	{
		for (int i = 0; i < array.Length; i++) array[i] += value;
	}

	public static void Plus(this Vector2[] array, Vector2Int value)
	{
		for (int i = 0; i < array.Length; i++)
		{
			array[i].x += value.x;
			array[i].y += value.y;
		}
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

	public static void Minus(this Vector2[] array, Vector2 value)
	{
		for (int i = 0; i < array.Length; i++) array[i] -= value;
	}

	public static void Minus(this Vector3[] array, Vector3 value)
	{
		for (int i = 0; i < array.Length; i++) array[i] -= value;
	}

	public static void Minus(this Vector2[] array, Vector2Int value)
	{
		for (int i = 0; i < array.Length; i++)
		{
			array[i].x -= value.x;
			array[i].y -= value.y;
		}
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

	public static void Mul(this Vector2[] array, Vector2 value)
	{
		for (int i = 0; i < array.Length; i++) array[i] = Mul(array[i], value);
	}

	public static void Mul(this Vector3[] array, Vector3 value)
	{
		for (int i = 0; i < array.Length; i++) array[i] = Mul(array[i], value);
	}

	public static void Mul(this Vector2[] array, Vector2Int value)
	{
		for (int i = 0; i < array.Length; i++) array[i] = Mul(array[i], value);
	}

	public static void Mul(this Vector3[] array, Vector3Int value)
	{
		for (int i = 0; i < array.Length; i++) array[i] = Mul(array[i], value);
	}

	public static void Div(this Vector2[] array, Vector2 value)
	{
		for (int i = 0; i < array.Length; i++) array[i] = Div(array[i], value);
	}

	public static void Div(this Vector3[] array, Vector3 value)
	{
		for (int i = 0; i < array.Length; i++) array[i] = Div(array[i], value);
	}

	public static void Div(this Vector2[] array, Vector2Int value)
	{
		for (int i = 0; i < array.Length; i++) array[i] = Div(array[i], value);
	}

	public static void Div(this Vector3[] array, Vector3Int value)
	{
		for (int i = 0; i < array.Length; i++) array[i] = Div(array[i], value);
	}

	#endregion

	#region Mid

	public static Vector2 Mid(this Vector2 vector1, Vector2 vector2)
	{
		vector2.x = (vector1.x + vector2.x) * 0.5f;
		vector2.y = (vector1.y + vector2.y) * 0.5f;
		return vector2;
	}

	public static Vector3 Mid(this Vector3 vector1, Vector3 vector2)
	{
		vector2.x = (vector1.x + vector2.x) * 0.5f;
		vector2.y = (vector1.y + vector2.y) * 0.5f;
		vector2.z = (vector1.z + vector2.z) * 0.5f;
		return vector2;
	}

	#endregion

	#region Clamp Components

	public static Vector2 ClampComponents(this Vector2 value, float min, float max)
	{
		return new Vector2(
			Mathf.Clamp(value.x, min, max),
			Mathf.Clamp(value.y, min, max));
	}

	public static Vector3 ClampComponents(this Vector3 value, float min, float max)
	{
		return new Vector3(
			Mathf.Clamp(value.x, min, max),
			Mathf.Clamp(value.y, min, max),
			Mathf.Clamp(value.z, min, max));
	}

	#endregion

	#region Raise To Minimum

	public static Vector2 RaiseToMinimum(this Vector2 value, float min)
	{
		if (value.x > 0f && value.x < min) value.x = min;
		else if (value.x < 0f && value.x > -min) value.x = -min;
		if (value.y > 0f && value.y < min) value.y = min;
		else if (value.y < 0f && value.y > -min) value.y = -min;
		return value;
	}

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

	public static Vector2 ClampLengthMax(this Vector2 value, float max)
	{
		if (value.magnitude > max)
			return value.normalized * max;
		return value;
	}

	public static Vector3 ClampLengthMax(this Vector3 value, float max)
	{
		if (value.magnitude > max)
			return value.normalized * max;
		return value;
	}

	public static Vector2 ClampLengthMin(this Vector2 value, float min)
	{
		if (value.magnitude < min)
			return value.normalized * min;
		return value;
	}

	public static Vector3 ClampLengthMin(this Vector3 value, float min)
	{
		if (value.magnitude < min)
			return value.normalized * min;
		return value;
	}

	public static Vector2 ClampSqrLengthMax(this Vector2 value, float sqrMax)
	{
		if (value.sqrMagnitude > sqrMax)
			return value.normalized * sqrMax;
		return value;
	}

	public static Vector3 ClampSqrLengthMax(this Vector3 value, float sqrMax)
	{
		if (value.sqrMagnitude > sqrMax)
			return value.normalized * sqrMax;
		return value;
	}

	public static Vector2 ClampSqrLengthMin(this Vector2 value, float sqrMin)
	{
		if (value.sqrMagnitude < sqrMin)
			return value.normalized * sqrMin;
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

	public static Vector2 Abs(this Vector2 value)
	{
		return new Vector2(
			value.x < 0f ? -value.x : value.x,
			value.y < 0f ? -value.y : value.y);
	}

	public static Vector3 Abs(this Vector3 value)
	{
		return new Vector3(
			value.x < 0f ? -value.x : value.x,
			value.y < 0f ? -value.y : value.y,
			value.z < 0f ? -value.z : value.z);
	}

	public static Vector2 Sign(this Vector2 value)
	{
		return new Vector2(
			value.x > 0f ? 1f : (value.x < 0f ? -1f : 0f),
			value.y > 0f ? 1f : (value.y < 0f ? -1f : 0f));
	}

	public static Vector3 Sign(this Vector3 value)
	{
		return new Vector3(
			value.x > 0f ? 1f : (value.x < 0f ? -1f : 0f),
			value.y > 0f ? 1f : (value.y < 0f ? -1f : 0f),
			value.z > 0f ? 1f : (value.z < 0f ? -1f : 0f));
	}

	public static Vector2Int SignInt(this Vector2 value)
	{
		return new Vector2Int(
			value.x > 0 ? 1 : (value.x < 0 ? -1 : 0),
			value.y > 0 ? 1 : (value.y < 0 ? -1 : 0));
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

	public static float MinComponent(this Vector2 value)
	{
		return value.x < value.y ? value.x : value.y;
	}

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

	public static float MaxComponent(this Vector2 value)
	{
		return value.x > value.y ? value.x : value.y;
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

	#endregion

	#region Rotation

	public static Vector2 Rotate(this Vector2 vector, float angleInRadians)
	{
		float cosa = Mathf.Cos(angleInRadians);
		float sina = Mathf.Sin(angleInRadians);
		return new Vector2(cosa * vector.x - sina * vector.y, sina * vector.x + cosa * vector.y);
	}

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

	public static Vector3 EulerAnglesInNeg180Pos180(this Quaternion quaternion)
	{
		Vector3 angles = quaternion.eulerAngles;
		if (angles.x > 180f) angles.x -= 360f;
		if (angles.y > 180f) angles.y -= 360f;
		if (angles.z > 180f) angles.z -= 360f;
		return angles;
	}

	public static float AngleBetweenXAxis_NegPIToPI(this Vector2 vector)
	{
		return Mathf.Atan2(vector.y, vector.x);
	}

	public static float AngleBetweenXAxis_ZeroToTwoPI(this Vector2 vector)
	{
		float angle = Mathf.Atan2(vector.y, vector.x);
		if (angle < 0f)
			return angle + TwoPI;
		return angle;
	}

	public static float AngleBetween(this Vector2 vector1, Vector2 vector2)
	{
		return Mathf.Acos(Vector2.Dot(vector1.normalized, vector2.normalized));
	}

	public static float AngleBetween(this Vector3 vector1, Vector3 vector2)
	{
		return Mathf.Acos(Vector3.Dot(vector1.normalized, vector2.normalized));
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

	public static float AngleBetween_NegPIToPI(this Vector2 vector1, Vector2 vector2)
	{
		float angle = Mathf.Atan2(vector2.y, vector2.x) - Mathf.Atan2(vector1.y, vector1.x);

		if (angle < 0)
			angle += TwoPI;

		if (angle > PI)
			angle -= TwoPI;

		return angle;
	}

	#endregion

	#region Perpendicular / Reflection

	public static Vector2 Perpendicular(this Vector2 vector)
	{
		return new Vector2(-vector.y, vector.x);
	}

	public static Vector2 Reflect(this Vector2 vector, Vector2 normal)
	{
		return vector - (2 * Vector2.Dot(vector, normal) * normal);
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

	public static Vector2 Swap(this Vector2 vector)
	{
		return new Vector2(vector.y, vector.x);
	}

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

	public static void SwapToMakeLesserAndGreater(ref Vector2 shouldBeLesser, ref Vector2 shouldBeGreater)
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

	public static void ChangeZerosTo(ref Vector2 value, float changeTo)
	{
		if (value.x == 0f) value.x = changeTo;
		if (value.y == 0f) value.y = changeTo;
	}

	public static void ChangeZerosTo(ref Vector3 value, float changeTo)
	{
		if (value.x == 0f) value.x = changeTo;
		if (value.y == 0f) value.y = changeTo;
		if (value.z == 0f) value.z = changeTo;
	}

	#endregion

	#endregion

	#region Quaternion

	public static readonly Quaternion QuaternionNaN = new Quaternion(float.NaN, float.NaN, float.NaN, float.NaN);

	public static bool IsAnyNaN(this Quaternion value)
	{
		return float.IsNaN(value.x) || float.IsNaN(value.y) || float.IsNaN(value.z) || float.IsNaN(value.w);
	}

	/// <summary>
	/// Rotates a rotation from towards to. Same as Quaternion.RotateTowards except this one notifies about rotation completion via isCompleted.
	/// </summary>
	public static Quaternion RotateTowards(this Quaternion from, Quaternion to, float maxDegreesDelta, out bool isCompleted)
	{
		var totalAngles = Quaternion.Angle(from, to);
		if (totalAngles.IsZero())
		{
			isCompleted = true;
			return to;
		}
		var t = maxDegreesDelta / totalAngles;
		if (t > 1f)
		{
			isCompleted = true;
			return to;
		}
		isCompleted = false;
		return Quaternion.SlerpUnclamped(from, to, t);
	}

	#endregion

	#region Closest Point On Line

	public static Vector2 ClosestPointOnLine(Vector2 lineStart, Vector2 lineEnd, Vector2 point)
	{
		Vector2 lineDirection = (lineEnd - lineStart).normalized;
		float closestPoint = Vector2.Dot(point - lineStart, lineDirection) / Vector2.Dot(lineDirection, lineDirection);
		return lineStart + (closestPoint * lineDirection);
	}

	public static Vector3 ClosestPointOnLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
	{
		Vector3 lineDirection = Vector3.Normalize(lineEnd - lineStart);
		float closestPoint = Vector3.Dot(point - lineStart, lineDirection) / Vector3.Dot(lineDirection, lineDirection);
		return lineStart + (closestPoint * lineDirection);
	}

	public static Vector3 ClosestPointOnLineSegment(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
	{
		Vector3 diff = lineEnd - lineStart;
		Vector3 lineDirection = Vector3.Normalize(diff);
		float closestPoint = Vector3.Dot(point - lineStart, lineDirection) / Vector3.Dot(lineDirection, lineDirection);

		// Clamp to line segment
		if (closestPoint < 0f)
		{
			return lineStart;
		}
		if (closestPoint > diff.magnitude)
		{
			return lineEnd;
		}

		return lineStart + (closestPoint * lineDirection);
	}

	public static Vector3 ClosestPointOnLineSegment(Vector3 lineStart, Vector3 lineEnd, Vector3 point, out float distanceFromStart)
	{
		Vector3 diff = lineEnd - lineStart;
		Vector3 lineDirection = Vector3.Normalize(diff);
		float closestPoint = Vector3.Dot(point - lineStart, lineDirection) / Vector3.Dot(lineDirection, lineDirection);

		// Clamp to line segment
		if (closestPoint < 0f)
		{
			distanceFromStart = 0f;
			return lineStart;
		}
		float diffMagnitude = diff.magnitude;
		if (closestPoint > diffMagnitude)
		{
			distanceFromStart = diffMagnitude;
			return lineEnd;
		}

		distanceFromStart = closestPoint;
		return lineStart + (closestPoint * lineDirection);
	}

	public static float DistanceFromStartOfClosestPointOnLineSegment(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
	{
		Vector3 diff = lineEnd - lineStart;
		Vector3 lineDirection = Vector3.Normalize(diff);
		float closestPoint = Vector3.Dot(point - lineStart, lineDirection) / Vector3.Dot(lineDirection, lineDirection);

		// Clamp to line segment
		if (closestPoint < 0f)
		{
			return 0f;
		}
		float diffMagnitude = diff.magnitude;
		if (closestPoint > diffMagnitude)
		{
			return diffMagnitude;
		}

		return closestPoint;
	}

	// TEST:
	//Vector3 lineStart;
	//Vector3 lineEnd;
	//Vector3 point;
	//Vector3 pointOnLine;

	//lineStart = new Vector3(50f, 10f, 50f);
	//lineEnd = new Vector3(100f, 10f, 50f);
	//point = new Vector3(70f, 10f, 60f);
	//pointOnLine = Tools.NearestPointOnLineSegment(lineStart, lineEnd, point);
	//Log.Write("lineStart   = " + lineStart.ToString());
	//Log.Write("lineEnd     = " + lineEnd.ToString());
	//Log.Write("point       = " + point.ToString());
	//Log.Write("pointOnLine = " + pointOnLine.ToString());

	//lineStart = new Vector3(50f, 10f, 50f);
	//lineEnd = new Vector3(100f, 10f, 50f);
	//point = new Vector3(30f, 10f, 60f);
	//pointOnLine = Tools.NearestPointOnLineSegment(lineStart, lineEnd, point);
	//Log.Write("lineStart   = " + lineStart.ToString());
	//Log.Write("lineEnd     = " + lineEnd.ToString());
	//Log.Write("point       = " + point.ToString());
	//Log.Write("pointOnLine = " + pointOnLine.ToString());

	//lineStart = new Vector3(50f, 10f, 50f);
	//lineEnd = new Vector3(100f, 10f, 50f);
	//point = new Vector3(170f, 10f, 60f);
	//pointOnLine = Tools.NearestPointOnLineSegment(lineStart, lineEnd, point);
	//Log.Write("lineStart   = " + lineStart.ToString());
	//Log.Write("lineEnd     = " + lineEnd.ToString());
	//Log.Write("point       = " + point.ToString());
	//Log.Write("pointOnLine = " + pointOnLine.ToString());

	#endregion

	#region Line Intersection

	public static bool CheckLineLineIntersection(
		Vector2 line1Point1, Vector2 line1Point2,
		Vector2 line2Point1, Vector2 line2Point2)
	{
		Vector2 b = line1Point2 - line1Point1;
		Vector2 d = line2Point2 - line2Point1;
		double bDotDPerp = b.x * d.y - b.y * d.x;

		// if b dot d == 0, it means the lines are parallel so have infinite intersection points
		if (bDotDPerp.IsZero())
			return false;

		Vector2 c = line2Point1 - line1Point1;
		double lineFactor = (c.x * d.y - c.y * d.x) / bDotDPerp;
		if (lineFactor < 0 || lineFactor > 1)
			return false;

		lineFactor = (c.x * b.y - c.y * b.x) / bDotDPerp;
		return lineFactor >= 0 && lineFactor <= 1;
	}

	#endregion

	#region Line Strip Length

	public static float CalculateLineStripLength(this IList<Vector3> points)
	{
		if (points == null || points.Count < 2)
			return 0f;

		var totalDistance = 0f;
		var previousPoint = points[0];
		for (int i = 1; i < points.Count; i++)
		{
			var currentPoint = points[i];
			totalDistance += Vector3.Distance(previousPoint, currentPoint);
			previousPoint = currentPoint;
		}
		return totalDistance;
	}

	public static float CalculateAverageLengthOfLineStripParts(this IList<Vector3> points)
	{
		if (points == null || points.Count < 2)
			return 0f;

		var totalDistance = CalculateLineStripLength(points);
		return totalDistance / (points.Count - 1);
	}

	public static float CalculateLineStripLength(this IList<Vector2> points)
	{
		if (points == null || points.Count < 2)
			return 0f;

		var totalDistance = 0f;
		var previousPoint = points[0];
		for (int i = 0; i < points.Count; i++)
		{
			var currentPoint = points[i];
			totalDistance += Vector2.Distance(previousPoint, currentPoint);
			previousPoint = currentPoint;
		}
		return totalDistance;
	}

	public static float CalculateAverageLengthOfLineStripParts(this IList<Vector2> points)
	{
		if (points == null || points.Count < 2)
			return 0f;

		var totalDistance = CalculateLineStripLength(points);
		return totalDistance / (points.Count - 1);
	}

	#endregion

	#region Line Strip Operations

	public static Vector3 GetPointAtDistanceFromStart(IList<Vector3> points, float distanceFromStart, ref Vector3 part)
	{
		if (points == null || points.Count == 0)
			return Vector3NaN;
		if (points.Count == 1 || distanceFromStart < 0f)
			return points[0];

		var totalDistance = 0f;
		var previousPoint = points[0];
		for (int i = 1; i < points.Count; i++)
		{
			var currentPoint = points[i];
			var distance = Vector3.Distance(previousPoint, currentPoint);

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

	public static Vector3 GetPointAtDistanceFromStart(IList<Vector3> points, bool loop, float distanceFromStart, ref Vector3 part)
	{
		if (points == null || points.Count == 0)
			return Vector3NaN;
		if (points.Count == 1 || distanceFromStart < 0f)
			return points[0];

		var totalDistance = 0f;
		var previousPoint = points[0];
		for (int i = 1; i < points.Count; i++)
		{
			var currentPoint = points[i];
			var distance = Vector3.Distance(previousPoint, currentPoint);

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

	public static Vector3 GetPointAtDistanceFromStart(IList<Vector3> points, float distanceFromStart)
	{
		if (points == null || points.Count == 0)
			return Vector3NaN;
		if (points.Count == 1 || distanceFromStart < 0f)
			return points[0];

		var totalDistance = 0f;
		var previousPoint = points[0];
		for (int i = 1; i < points.Count; i++)
		{
			var currentPoint = points[i];
			var distance = Vector3.Distance(previousPoint, currentPoint);

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

	public static Vector3 GetPointAtDistanceFromStart(IList<Vector3> points, bool loop, float distanceFromStart)
	{
		if (points == null || points.Count == 0)
			return Vector3NaN;
		if (points.Count == 1 || distanceFromStart < 0f)
			return points[0];

		var totalDistance = 0f;
		var previousPoint = points[0];
		for (int i = 1; i < points.Count; i++)
		{
			var currentPoint = points[i];
			var distance = Vector3.Distance(previousPoint, currentPoint);

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
			var distance = Vector3.Distance(previousPoint, currentPoint);

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

	public static Vector3 ClosestPointOnLineStrip(this IList<Vector3> points, Vector3 point)
	{
		if (points == null || points.Count == 0)
			return Vector3NaN;
		if (points.Count == 1)
			return points[0];

		var previousPoint = points[0];
		var closestPoint = previousPoint;
		var closestPointSqrDistance = float.PositiveInfinity;
		for (int i = 1; i < points.Count; i++)
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

	public static Vector3 ClosestPointOnLineStrip(this IList<Vector3> points, Vector3 point, ref Vector3 part)
	{
		if (points == null || points.Count == 0)
			return Vector3NaN;
		if (points.Count == 1)
			return points[0];

		var previousPoint = points[0];
		var closestPoint = previousPoint;
		var closestPointSqrDistance = float.PositiveInfinity;
		for (int i = 1; i < points.Count; i++)
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

	public static Vector3 ClosestPointOnLineStrip(this IList<Vector3> points, Vector3 point, bool loop)
	{
		if (points == null || points.Count == 0)
			return Vector3NaN;
		if (points.Count == 1)
			return points[0];

		var previousPoint = points[0];
		var closestPoint = previousPoint;
		var closestPointSqrDistance = float.PositiveInfinity;
		for (int i = 1; i < points.Count; i++)
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

	public static Vector3 ClosestPointOnLineStrip(this IList<Vector3> points, Vector3 point, bool loop, ref Vector3 part)
	{
		if (points == null || points.Count == 0)
			return Vector3NaN;
		if (points.Count == 1)
			return points[0];

		var previousPoint = points[0];
		var closestPoint = previousPoint;
		var closestPointSqrDistance = float.PositiveInfinity;
		for (int i = 1; i < points.Count; i++)
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

	public static float DistanceFromStartOfClosestPointOnLineStrip(this IList<Vector3> points, Vector3 point)
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
		for (int i = 1; i < points.Count; i++)
		{
			var currentPoint = points[i];

			float distanceFromStartOfCurrentSegmentClosestPoint;
			var currentSegmentClosestPoint = ClosestPointOnLineSegment(previousPoint, currentPoint, point, out distanceFromStartOfCurrentSegmentClosestPoint);
			var sqrDistance = (currentSegmentClosestPoint - point).sqrMagnitude;

			if (closestPointSqrDistance > sqrDistance)
			{
				closestPointSqrDistance = sqrDistance;
				//closestPoint = currentSegmentClosestPoint;
				distanceFromStartOfClosestPoint = totalLength + distanceFromStartOfCurrentSegmentClosestPoint;
			}

			totalLength += (currentPoint - previousPoint).magnitude;
			previousPoint = currentPoint;
		}

		return distanceFromStartOfClosestPoint;
	}

	public static float DistanceFromStartOfClosestPointOnLineStrip(this IList<Vector3> points, Vector3 point, bool loop)
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
		for (int i = 1; i < points.Count; i++)
		{
			var currentPoint = points[i];

			float distanceFromStartOfCurrentSegmentClosestPoint;
			var currentSegmentClosestPoint = ClosestPointOnLineSegment(previousPoint, currentPoint, point, out distanceFromStartOfCurrentSegmentClosestPoint);
			var sqrDistance = (currentSegmentClosestPoint - point).sqrMagnitude;

			if (closestPointSqrDistance > sqrDistance)
			{
				closestPointSqrDistance = sqrDistance;
				//closestPoint = currentSegmentClosestPoint;
				distanceFromStartOfClosestPoint = totalLength + distanceFromStartOfCurrentSegmentClosestPoint;
			}

			totalLength += (currentPoint - previousPoint).magnitude;
			previousPoint = currentPoint;
		}
		if (loop)
		{
			var currentPoint = points[0];

			float distanceFromStartOfCurrentSegmentClosestPoint;
			var currentSegmentClosestPoint = ClosestPointOnLineSegment(previousPoint, currentPoint, point, out distanceFromStartOfCurrentSegmentClosestPoint);
			var sqrDistance = (currentSegmentClosestPoint - point).sqrMagnitude;

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

	public static int FindClosestValueIndex(this IList<Vector3> values, Vector3 targetValue, int startIndex = 0)
	{
		int closestIndex = -1;
		float closestSqrDistance = float.PositiveInfinity;

		for (int i = startIndex; i < values.Count; i++)
		{
			var value = values[i];
			var sqrDistance = (value - targetValue).sqrMagnitude;
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
			var sqrDistance = (value - targetValue).sqrMagnitude;
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
			var value = values[i];
			var sqrDistance = (value - targetValue).sqrMagnitude;
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
			var value = values[i];
			var sqrDistance = (value - targetValue).sqrMagnitude;
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
			var value = values[i];
			if (!value.IsAnyNaN())
				return i;
		}
		return -1;
	}

	public static int FindFirstNonNaNValueIndex(this Vector2[] values)
	{
		for (int i = 0; i < values.Length; i++)
		{
			var value = values[i];
			if (!value.IsAnyNaN())
				return i;
		}
		return -1;
	}

	public static int SortLineStripUsingClosestSequentialPointsMethod(IList<Vector3> points, Vector3 initialPointReference)
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

	#region Flat Check

	public static bool IsFlatX(this List<Vector2> points)
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

	public static bool IsFlatY(this List<Vector2> points)
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

	#region Spline Operations - Bezier, CatmullRom

	public static Vector3 GetBezierPoint(Vector3 p1, Vector3 p2, Vector3 p3, float t)
	{
		float it = 1 - t;
		float it2 = it * it;
		float itt = it * t;
		float t2 = t * t;

		return new Vector3(
			(p1.x * it2 + 2 * p2.x * itt + p3.x * t2),
			(p1.y * it2 + 2 * p2.y * itt + p3.y * t2),
			(p1.z * it2 + 2 * p2.z * itt + p3.z * t2)
			);
	}

	public static Vector3 GetCatmullRomPoint(
		Vector3 previous, Vector3 start, Vector3 end, Vector3 next,
		float percentage)
	{
		// References used:
		// p.266 GemsV1
		//
		// tension is often set to 0.5 but you can use any reasonable value:
		// http://www.cs.cmu.edu/~462/projects/assn2/assn2/catmullRom.pdf
		//
		// bias and tension controls:
		// http://local.wasp.uwa.edu.au/~pbourke/miscellaneous/interpolation/

		float percentageSquare = percentage * percentage;
		float percentageCube = percentageSquare * percentage;

		return previous * (-0.5f * percentageCube +
						 percentageSquare -
						 0.5f * percentage) +
			   start * (1.5f * percentageCube +
					  -2.5f * percentageSquare + 1.0f) +
			   end * (-1.5f * percentageCube +
					2.0f * percentageSquare +
					0.5f * percentage) +
			   next * (0.5f * percentageCube -
					 0.5f * percentageSquare);
	}

	#endregion

	#region Bounds

	public static Bounds BoundsNaN = new Bounds(Vector3NaN, Vector3NaN);

	public static void Fix(this Bounds bounds)
	{
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;

		bool switchX = min.x > max.x;
		bool switchY = min.y > max.y;
		bool switchZ = min.z > max.z;

		if (switchX)
		{
			if (switchY)
			{
				if (switchZ)
				{
					bounds.SetMinMax(
						new Vector3(max.x, max.y, max.z),
						new Vector3(min.x, min.y, min.z));
				}
				else
				{
					bounds.SetMinMax(
						new Vector3(max.x, max.y, min.z),
						new Vector3(min.x, min.y, max.z));
				}
			}
			else
			{
				if (switchZ)
				{
					bounds.SetMinMax(
						new Vector3(max.x, min.y, max.z),
						new Vector3(min.x, max.y, min.z));
				}
				else
				{
					bounds.SetMinMax(
						new Vector3(max.x, min.y, min.z),
						new Vector3(min.x, max.y, max.z));
				}
			}
		}
		else
		{
			if (switchY)
			{
				if (switchZ)
				{
					bounds.SetMinMax(
						new Vector3(min.x, max.y, max.z),
						new Vector3(max.x, min.y, min.z));
				}
				else
				{
					bounds.SetMinMax(
						new Vector3(min.x, max.y, min.z),
						new Vector3(max.x, min.y, max.z));
				}
			}
			else
			{
				if (switchZ)
				{
					bounds.SetMinMax(
						new Vector3(min.x, min.y, max.z),
						new Vector3(max.x, max.y, min.z));
				}
				else
				{
					bounds.SetMinMax(
						new Vector3(min.x, min.y, min.z),
						new Vector3(max.x, max.y, max.z));
				}
			}
		}

	}

	public static void Reset(this Bounds bounds)
	{
		bounds.SetMinMax(
			new Vector3(float.MaxValue, float.MaxValue, float.MaxValue),
			new Vector3(float.MinValue, float.MinValue, float.MinValue));
	}

	// Source: http://answers.unity3d.com/questions/361275/cant-convert-bounds-from-world-coordinates-to-loca.html
	public static Bounds TransformBounds(this Transform transform, Bounds localBounds)
	{
		var center = transform.TransformPoint(localBounds.center);

		// transform the local extents' axes
		var extents = localBounds.extents;
		var axisX = transform.TransformVector(extents.x, 0, 0);
		var axisY = transform.TransformVector(0, extents.y, 0);
		var axisZ = transform.TransformVector(0, 0, extents.z);

		// sum their absolute value to get the world extents
		extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
		extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
		extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

		return new Bounds { center = center, extents = extents };
	}

	public static Bounds TransformBounds(this Transform transform, Bounds localBounds, Transform relativeTo)
	{
		var center = relativeTo.InverseTransformPoint(transform.TransformPoint(localBounds.center));

		// transform the local extents' axes
		var extents = localBounds.extents;
		var axisX = relativeTo.InverseTransformVector(transform.TransformVector(extents.x, 0, 0));
		var axisY = relativeTo.InverseTransformVector(transform.TransformVector(0, extents.y, 0));
		var axisZ = relativeTo.InverseTransformVector(transform.TransformVector(0, 0, extents.z));

		// sum their absolute value to get the world extents
		extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
		extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
		extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

		return new Bounds { center = center, extents = extents };
	}


	#endregion

	#region Rect

	public static bool IsZero(this Rect me)
	{
		return me.x == 0 && me.y == 0 && me.width == 0 && me.height == 0;
	}

	public static Vector2 ClipPointInsideArea(this Rect area, Vector2 point)
	{
		if (point.x < area.xMin) point.x = area.xMin;
		if (point.y < area.yMin) point.y = area.yMin;
		if (point.x > area.xMax) point.x = area.xMax;
		if (point.y > area.yMax) point.y = area.yMax;
		return point;
	}

	public static Vector2 Center(this Rect rect)
	{
		return new Vector2(rect.xMin + rect.width * 0.5f, rect.yMin + rect.height * 0.5f);
	}

	public static Vector2 MinPoint(this Rect rect)
	{
		return new Vector2(rect.xMin, rect.yMin);
	}

	public static Vector2 MaxPoint(this Rect rect)
	{
		return new Vector2(rect.xMax, rect.yMax);
	}

	public static void Move(ref Rect rect, Vector2 translation)
	{
		rect.x += translation.x;
		rect.y += translation.y;
	}

	public static void Move(ref Rect rect, float translationX, float translationY)
	{
		rect.x += translationX;
		rect.y += translationY;
	}

	public static void MoveX(ref Rect rect, float translationX)
	{
		rect.x += translationX;
	}

	public static void MoveY(ref Rect rect, float translationY)
	{
		rect.y += translationY;
	}


	public static Rect MovedCopy(this Rect rect, Vector2 translation)
	{
		rect.x += translation.x;
		rect.y += translation.y;
		return rect;
	}

	public static Rect MovedCopy(this Rect rect, float translationX, float translationY)
	{
		rect.x += translationX;
		rect.y += translationY;
		return rect;
	}

	public static Rect MovedCopyX(this Rect rect, float translationX)
	{
		rect.x += translationX;
		return rect;
	}

	public static Rect MovedCopyY(this Rect rect, float translationY)
	{
		rect.y += translationY;
		return rect;
	}


	public static bool CheckLineRectIntersection(this Rect rect, Vector2 point1, Vector2 point2)
	{
		throw new NotImplementedException();
	}

	#endregion

	#region Plane

	public static bool IsAllPointsOnPlane(this IList<Vector3> points, Vector3 planeNormal, float tolerance = 0.0001f)
	{
		var plane = new Plane(planeNormal, points[0]);
		for (int i = 1; i < points.Count; i++)
		{
			var distance = plane.GetDistanceToPoint(points[i]);
			if (!distance.IsZero(tolerance))
				return false;
		}
		return true;
	}

	public static Vector3 ProjectPointOnPlane(Vector3 planeNormal, Vector3 planePoint, Vector3 point)
	{
		var distance = -Vector3.Dot(planeNormal.normalized, point - planePoint);
		return point + planeNormal * distance;
	}

	#endregion

	#region Polygon / Surface

	public static float CalculateTriangleArea(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
	{
		var cross = Vector3.Cross(vertex2 - vertex1, vertex3 - vertex1);
		return cross.magnitude / 2f;
	}

	public static float CalculatePolygonArea(this IList<Vector3> polygonPoints)
	{
		var pointCount = polygonPoints.Count;
		var crossTotal = Vector3.zero;

		for (int i = 0; i < pointCount; ++i)
		{
			var j = (i + 1) % pointCount;
			crossTotal += Vector3.Cross(polygonPoints[i], polygonPoints[j]);
		}
		return crossTotal.magnitude / 2f;
	}

	public static float CalculateTriangleArea(this IList<Vector3> allPoints, IList<int> triangleIndices)
	{
		var triangleCount = triangleIndices.Count / 3;

		if (triangleCount * 3 != triangleIndices.Count)
		{
			throw new Exception("Triangle indices list length should be multiple of 3.");
		}

		var crossTotal = Vector3.zero;

		for (int i = 0; i < triangleIndices.Count; i += 3)
		{
			var mid = allPoints[triangleIndices[i + 1]];
			var line1 = mid - allPoints[triangleIndices[i]];
			var line2 = mid - allPoints[triangleIndices[i + 2]];
			crossTotal += Vector3.Cross(line1, line2);
		}
		return crossTotal.magnitude / 2f;
	}

	/// <summary>
	/// Newell's Method
	/// Source: https://www.opengl.org/wiki/Calculating_a_Surface_Normal
	/// </summary>
	public static Vector3 CalculatePolygonNormal(this IList<Vector3> polygonPoints)
	{
		if (polygonPoints == null || polygonPoints.Count < 3)
		{
			return Vector3NaN;
		}

		var normal = Vector3.zero;
		for (int i = 0; i < polygonPoints.Count; i++)
		{
			var current = polygonPoints[i];
			var next = polygonPoints[(i + 1) % polygonPoints.Count];

			normal.x += (current.y - next.y) * (current.z + next.z);
			normal.y += (current.z - next.z) * (current.x + next.x);
			normal.z += (current.x - next.x) * (current.y + next.y);
		}
		return normal.normalized;
	}

	public static Vector3 CalculatePolygonCenter(this IList<Vector3> polygonPoints)
	{
		var center = Vector3.zero;

		for (int i = 0; i < polygonPoints.Count; i++)
		{
			center += polygonPoints[i];
		}

		return center / polygonPoints.Count;
	}

	public static bool IsPolygon(this IList<Vector2> polygonPoints, float lineCheckTolerance = 0.001f)
	{
		if (polygonPoints == null || polygonPoints.Count < 3)
			return false;

		// TODO: Optimize
		// Check if all vertices on the same line
		{
			var lineCheckToleranceSqr = lineCheckTolerance * lineCheckTolerance;
			var vertexFoundOutsideOfLine = false;
			var lineStart = polygonPoints[0];
			var lineEnd = polygonPoints[1];
			for (int i = 2; i < polygonPoints.Count; i++)
			{
				var point = polygonPoints[i];
				var closestPointOnLine = ClosestPointOnLine(lineStart, lineEnd, point);
				if ((closestPointOnLine - point).sqrMagnitude > lineCheckToleranceSqr)
				{
					vertexFoundOutsideOfLine = true;
					break;
				}
			}

			if (!vertexFoundOutsideOfLine)
			{
				return false;
			}
		}

		return true;
	}

	#endregion

	#region Bool

	public static int ToInt(this bool me)
	{
		return me ? 1 : 0;
	}

	public static string ToIntString(this bool me)
	{
		return me ? "1" : "0";
	}

	#endregion

	#region Tancant Filter

	public static float TancantFilter(float x, float rampScale)
	{
		return (float)((Math.Tanh((x - rampScale * 0.5) * 6.0 / rampScale) + 1.0) * 0.5);
	}

	#endregion
}
