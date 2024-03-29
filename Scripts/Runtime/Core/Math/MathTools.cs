using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using static Unity.Mathematics.math;
#if UNITY
using UnityEngine;
#endif

namespace Extenity.MathToolbox
{

	public enum Space
	{
		Unspecified,
		World,
		Local,
	}

	public static partial class MathTools
	{
		#region Int Float Double

		public const float PI = math.PI;
		public const float NegPI = -math.PI;
		public const float PosPI = math.PI;
		public const float TwoPI = 2f * math.PI;
		public const float HalfPI = 0.5f * math.PI;
		public const float E = 2.7182818284590452353602874f;

		public const float Deg2Rad = 0.0174532925f;
		public const float Rad2Deg = 57.295779513f;

		public const float ZeroTolerance = 1e-5f;

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

		public static bool IsBetweenMinMax(this float val, float min, float max)
		{
			return val > min && val < max;
		}
		public static bool IsBetweenOrEqualMinMax(this float val, float min, float max)
		{
			return val >= min && val <= max;
		}
		public static bool IsBetweenMinMax(this int val, int min, int max)
		{
			return val > min && val < max;
		}
		public static bool IsBetweenOrEqualMinMax(this int val, int min, int max)
		{
			return val >= min && val <= max;
		}


		public static float MakeZeroIfNaN(this float val)
		{
			if (float.IsNaN(val)) return 0f;
			return val;
		}

		public static void Swap<T>(ref T variable1, ref T variable2)
		{
			var temp = variable2;
			variable2 = variable1;
			variable1 = temp;
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

		/// <summary>
		/// http://stackoverflow.com/questions/12097805/how-to-check-if-number-is-divisible-in-c
		/// </summary>
		public static bool IsGoodDivision(this int value, int divisor)
		{
			if (divisor == 0)
				throw new DivideByZeroException();
			while (divisor % 2 == 0) { divisor /= 2; }
			while (divisor % 5 == 0) { divisor /= 5; }
			return value % divisor == 0;
		}

		public static bool IsDivisible(this int value, int divisor)
		{
			if (divisor == 0)
				throw new DivideByZeroException();
			return value % divisor == 0;
		}

		public static float Tanh(float x)
		{
			return (pow(E, x) - pow(E, -x)) / (pow(E, x) + pow(E, -x));
		}

		// TODO OPTIMIZATION: Floor, Ceil and Round operations should be tested and benchmarked on devices, especially when run with IL2CPP compiled code.

		public static int FloorToInt(this float val)
		{
			return val >= 0.0f ? (int)val : (int)val - 1;
		}

		public static int FloorToInt(this double val)
		{
			return val >= 0.0 ? (int)val : (int)val - 1;
		}

		public static int CeilToInt(this float val)
		{
			return (int)Math.Ceiling(val);
		}

		public static int CeilToInt(this double val)
		{
			return (int)Math.Ceiling(val);
		}

		public static int RoundToInt(this float val)
		{
			return val >= 0.0f ? (int)(val + 0.5f) : (int)(val - 0.5f);
		}

		public static int RoundToInt(this double val)
		{
			return val >= 0.0 ? (int)(val + 0.5) : (int)(val - 0.5);
		}

		/// <summary>
		/// See: https://stackoverflow.com/questions/827252/c-sharp-making-one-int64-from-two-int32s
		/// </summary>
		public static long MakeLong(int left, int right)
		{
			//implicit conversion of left to a long
			long res = left;

			//shift the bits creating an empty space on the right
			// ex: 0x0000CFFF becomes 0xCFFF0000
			res = (res << 32);

			//combine the bits on the right with the previous value
			// ex: 0xCFFF0000 | 0x0000ABCD becomes 0xCFFFABCD
			res = res | (long)(uint)right; //uint first to prevent loss of signed bit

			return res;
		}

		/// <summary>
		/// See: https://stackoverflow.com/questions/1209439/what-is-the-best-way-to-combine-two-uints-into-a-ulong-in-c-sharp
		/// </summary>
		public static ulong MakeULong(uint left, uint right)
		{
			return (ulong)left << 32 | right;
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

		public static float LerpClamped(float from, float to, float t)
		{
			if (t <= 0.0f) return from;
			if (t >= 1.0f) return to;
			return from + (to - from) * t;
		}

		public static float Lerp(float from, float to, float t)
		{
			return from + (to - from) * t;
		}

		public static float SignedSqrt(this float value)
		{
			if (value < 0f)
				return -sqrt(-value);
			return sqrt(value);
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
				return -pow(-value, power);
			return pow(value, power);
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
			return abs(value1 - value2);
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
				throw new ArgumentException("Base value min and max should not be the same.", nameof(fromMin));
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
				throw new ArgumentException("Base value min and max should not be the same.", nameof(fromMin));
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
			float factor = pow(10, digitsAfterDecimalPoint);
			return ((int)round(value * factor)) / factor;
		}

		public static double RoundToDigits(this double value, int digitsAfterDecimalPoint)
		{
			if (digitsAfterDecimalPoint < 0) digitsAfterDecimalPoint = 0;
			double factor = Math.Pow(10, digitsAfterDecimalPoint);
			return ((int)Math.Round(value * factor)) / factor;
		}

		public static int IncrementOne(ref int current, int module)
		{
			return ++current % module;
		}

		public static int DigitCount(this int value)
		{
			if (value < 0)
				value = -value;
			if (value < 10)
				return 1;
			if (value < 100)
				return 2;
			if (value < 1_000)
				return 3;
			if (value < 10_000)
				return 4;
			if (value < 100_000)
				return 5;
			if (value < 1_000_000)
				return 6;
			if (value < 10_000_000)
				return 7;
			if (value < 100_000_000)
				return 8;
			if (value < 1_000_000_000)
				return 9;
			return 10;
		}

		public static int DigitCountIncludingMinusCharacter(this int value)
		{
			if (value < 0)
				return (-value).DigitCount() + 1;
			else
				return value.DigitCount();
		}

		#endregion

		#region Transform

#if UNITY
		public static bool IsPointBehind(this Transform transform, Vector3 point)
		{
			return Vector3.Dot(transform.forward, point - transform.position) < 0.0f;
		}

		public static Vector3 GetPositionRelativeTo(this Transform me, Transform other)
		{
			return me.position - other.position;
		}
#endif

		#endregion

		#region Triangle

		public static float AngleOfTriangle(float neighbourSide1, float neighbourSide2, float oppositeSide)
		{
			return acos(
				(neighbourSide1 * neighbourSide1 + neighbourSide2 * neighbourSide2 - oppositeSide * oppositeSide)
				/
				(2f * neighbourSide1 * neighbourSide2)
				);
		}

		public static float AngleOfRightTriangle(float neighbourSideTo90Angle, float hypotenuse)
		{
			return acos(neighbourSideTo90Angle / hypotenuse);
		}

		public static bool IsValidTriangle(float3 point1, float3 point2, float3 point3)
		{
			var a = distance(point1, point2);
			var b = distance(point1, point3);
			var c = distance(point2, point3);

			return
				a + b > c &&
				a + c > b &&
				b + c > a;
		}

		#endregion

		#region Quaternion

#if UNITY
		public static bool IsAlmostEqual(this Quaternion value1, Quaternion value2, float maxAngle)
		{
			return Quaternion.Angle(value1, value2) < maxAngle;
		}
#endif

		#endregion

		#region Matrix4x4

#if UNITY
		public static Matrix4x4 UnscaledLocalToWorldMatrix(this Transform transform)
		{
			return Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
		}

		public static void SetPosition(ref Matrix4x4 matrix, Vector3 position)
		{
			matrix.m03 = position.x;
			matrix.m13 = position.y;
			matrix.m23 = position.z;
		}

		public static void SetPosition(ref Matrix4x4 matrix, float x, float y, float z)
		{
			matrix.m03 = x;
			matrix.m13 = y;
			matrix.m23 = z;
		}

		public static Vector3 TransformPointFromLocalToLocal(this Vector3 point, Transform currentTransform, Transform newTransform)
		{
			return TransformPointFromLocalToLocal(point, currentTransform.worldToLocalMatrix, newTransform.localToWorldMatrix);
		}

		public static Vector3 TransformPointFromLocalToLocal(this Vector3 point, Matrix4x4 inverseCurrentMatrix, Transform newTransform)
		{
			return TransformPointFromLocalToLocal(point, inverseCurrentMatrix, newTransform.localToWorldMatrix);
		}

		public static Vector3 TransformPointFromLocalToLocal(this Vector3 point, Matrix4x4 inverseCurrentMatrix, Matrix4x4 newMatrix)
		{
			point = inverseCurrentMatrix.MultiplyPoint(point);
			return newMatrix.MultiplyPoint(point);
		}
#endif

		#endregion

		#region Polygon / Surface

		public static float CalculateTriangleArea(float3 vertex1, float3 vertex2, float3 vertex3)
		{
			var crossed = cross(vertex2 - vertex1, vertex3 - vertex1);
			return length(crossed) / 2f;
		}

		public static float CalculatePolygonArea(this IList<float3> polygonPoints)
		{
			var pointCount = polygonPoints.Count;
			var crossTotal = float3Tools.Zero;

			for (int i = 0; i < pointCount; ++i)
			{
				var j = (i + 1) % pointCount;
				crossTotal += cross(polygonPoints[i], polygonPoints[j]);
			}
			return length(crossTotal) / 2f;
		}

		public static float CalculateTriangleArea(this IList<float3> allPoints, IList<int> triangleIndices)
		{
			var triangleCount = triangleIndices.Count / 3;

			if (triangleCount * 3 != triangleIndices.Count)
			{
				throw new Exception("Triangle indices list length should be multiple of 3.");
			}

			var crossTotal = float3Tools.Zero;

			for (int i = 0; i < triangleIndices.Count; i += 3)
			{
				var mid = allPoints[triangleIndices[i + 1]];
				var line1 = mid - allPoints[triangleIndices[i]];
				var line2 = mid - allPoints[triangleIndices[i + 2]];
				crossTotal += cross(line1, line2);
			}
			return length(crossTotal) / 2f;
		}

		/// <summary>
		/// Newell's Method
		/// Source: https://www.opengl.org/wiki/Calculating_a_Surface_Normal
		/// </summary>
		public static float3 CalculatePolygonNormal(this IList<float3> polygonPoints)
		{
			if (polygonPoints == null || polygonPoints.Count < 3)
			{
				return float3Tools.NaN;
			}

			var normal = float3Tools.Zero;
			for (int i = 0; i < polygonPoints.Count; i++)
			{
				var current = polygonPoints[i];
				var next = polygonPoints[(i + 1) % polygonPoints.Count];

				normal.x += (current.y - next.y) * (current.z + next.z);
				normal.y += (current.z - next.z) * (current.x + next.x);
				normal.z += (current.x - next.x) * (current.y + next.y);
			}
			return normalize(normal);
		}

		public static float3 CalculatePolygonCenter(this IList<float3> polygonPoints)
		{
			var center = float3Tools.Zero;

			for (int i = 0; i < polygonPoints.Count; i++)
			{
				center += polygonPoints[i];
			}

			return center / polygonPoints.Count;
		}

		public static bool IsPolygon(this IList<float2> polygonPoints, float lineCheckTolerance = 0.001f)
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
					if (closestPointOnLine.SqrDistanceTo(point) > lineCheckToleranceSqr)
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

		public static bool InvertIf(this bool me, bool doInvert)
		{
			return doInvert ? !me : me;
		}

		#endregion

		#region Tancant Filter

		public static float TancantFilter(float x, float rampScale)
		{
			return (float)((tanh((x - rampScale * 0.5) * 6.0 / rampScale) + 1.0) * 0.5);
		}

		#endregion

		#region Snapping

		public static bool IsSnapped(this float value, float snapStep, float snapOffset, float precision = 0.001f)
		{
			var diff = value - snapOffset;
			if (diff < 0)
				diff = -diff;

			var halfSnapStep = snapStep * 0.5f;
			var mod = ((diff + halfSnapStep) % snapStep) - halfSnapStep;

			return mod <= precision && mod >= -precision;
			//return mod.IsAlmostEqual(0f, precision);
		}

		public static bool IsSnapped(this float2 value, float snapStep, float snapOffset, float precision = 0.001f)
		{
			return
				IsSnapped(value.x, snapStep, snapOffset, precision) &&
				IsSnapped(value.y, snapStep, snapOffset, precision);
		}

		public static bool IsSnapped(this float3 value, float snapStep, float snapOffset, float precision = 0.001f)
		{
			return
				IsSnapped(value.x, snapStep, snapOffset, precision) &&
				IsSnapped(value.y, snapStep, snapOffset, precision) &&
				IsSnapped(value.z, snapStep, snapOffset, precision);
		}

#if UNITY
		// Old Math library support.
		public static bool IsSnapped(this Vector2 value, float snapStep, float snapOffset, float precision = 0.001f) { return IsSnapped(float2(value), snapStep, snapOffset, precision); }
		public static bool IsSnapped(this Vector3 value, float snapStep, float snapOffset, float precision = 0.001f) { return IsSnapped(float3(value), snapStep, snapOffset, precision); }
#endif

		public static float Snap(this float value, float snapStep, float snapOffset)
		{
			return round((value + snapOffset) / snapStep) * snapStep - snapOffset;
		}

		public static float2 Snap(this float2 value, float snapStep, float snapOffset)
		{
			return new float2(
				Snap(value.x, snapStep, snapOffset),
				Snap(value.y, snapStep, snapOffset));
		}

		public static float3 Snap(this float3 value, float snapStep, float snapOffset)
		{
			return new float3(
				Snap(value.x, snapStep, snapOffset),
				Snap(value.y, snapStep, snapOffset),
				Snap(value.z, snapStep, snapOffset));
		}

#if UNITY
		// Old Math library support.
		public static Vector2 Snap(this Vector2 value, float snapStep, float snapOffset) { return Snap(float2(value), snapStep, snapOffset); }
		public static Vector3 Snap(this Vector3 value, float snapStep, float snapOffset) { return Snap(float3(value), snapStep, snapOffset); }
#endif

		#endregion

		#region Math - Human Readable Rounded Value

		// Should be tested before using
		// [MethodImpl(MethodImplOptions.AggressiveInlining)]
		// public static int CalculateInsignificantDigitsForHumanReadability(this int value)
		// {
		// 	return CalculateInsignificantDigitsForHumanReadability((double)value);
		// }
		//
		// Should be tested before using
		// [MethodImpl(MethodImplOptions.AggressiveInlining)]
		// public static int CalculateInsignificantDigitsForHumanReadability(this long value)
		// {
		// 	return CalculateInsignificantDigitsForHumanReadability((double)value);
		// }
		//
		// Should be tested before using
		// [MethodImpl(MethodImplOptions.AggressiveInlining)]
		// public static int CalculateInsignificantDigitsForHumanReadability(this float value)
		// {
		// 	return CalculateInsignificantDigitsForHumanReadability((double)value);
		// }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CalculateInsignificantDigitsForHumanReadability(this double value)
		{
			if (value < 0)
				throw new NotImplementedException();

			return FloorToInt(log10(value)) - 1;
		}

		// Should be tested before using
		// [MethodImpl(MethodImplOptions.AggressiveInlining)]
		// public static int CalculateHumanReadableRoundedValue(this int value)
		// {
		// 	return (int)CalculateHumanReadableRoundedValue((double)value);
		// }
		//
		// Should be tested before using
		// [MethodImpl(MethodImplOptions.AggressiveInlining)]
		// public static long CalculateHumanReadableRoundedValue(this long value)
		// {
		// 	return (long)CalculateHumanReadableRoundedValue((double)value);
		// }
		//
		// Should be tested before using
		// [MethodImpl(MethodImplOptions.AggressiveInlining)]
		// public static float CalculateHumanReadableRoundedValue(this float value)
		// {
		// 	return (float)CalculateHumanReadableRoundedValue((double)value);
		// }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double CalculateHumanReadableRoundedValue(this double value)
		{
			var insignificantDigits = value.CalculateInsignificantDigitsForHumanReadability();
			if (insignificantDigits <= 0)
			{
				return round(value);
			}
			else
			{
				var clip = pow(10, insignificantDigits);
				return round(value / clip) * clip;
			}
		}

		#endregion
	}

}
