using System;
using System.Collections.Generic;
using Extenity.DataToolbox;
using Extenity.DebugToolbox;
using UnityEngine;

namespace Extenity.MathToolbox
{

	public static class MathTools
	{
		#region Int Float Double

		public const float PI = Mathf.PI;
		public const float NegPI = -Mathf.PI;
		public const float PosPI = Mathf.PI;
		public const float TwoPI = 2f * Mathf.PI;
		public const float HalfPI = 0.5f * Mathf.PI;
		public const float E = 2.7182818284590452353602874f;

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

		public static int FastFloorToInt(float val)
		{
			return val >= 0.0f ? (int)val : (int)val - 1;
		}

		public static int FastRoundToInt(float val)
		{
			return val >= 0.0f ? (int)(val + 0.5f) : (int)(val - 0.5f);
		}

		public static int FastRoundToInt(double val)
		{
			return val >= 0.0 ? (int)(val + 0.5) : (int)(val - 0.5);
		}

		public static long FastRoundToLong(double val)
		{
			return val >= 0.0 ? (long)(val + 0.5) : (long)(val - 0.5);
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
			float factor = Mathf.Pow(10, digitsAfterDecimalPoint);
			return ((int)Mathf.Round(value * factor)) / factor;
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
			var a = point1.DistanceTo(point2);
			var b = point1.DistanceTo(point3);
			var c = point2.DistanceTo(point3);

			return
				a + b > c &&
				a + c > b &&
				b + c > a;
		}

		#endregion

		#region Matrix4x4

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

		#endregion

		#region Find Closest Point

		public static Vector2 FindClosest(this IList<Vector2> list, Vector2 toPoint)
		{
			var value = Vector2Tools.NaN;
			var closestDistanceSqr = float.MaxValue;
			if (list != null)
			{
				for (var i = 0; i < list.Count; i++)
				{
					var distanceSqr = list[i].SqrDistanceTo(toPoint);
					if (closestDistanceSqr > distanceSqr)
					{
						closestDistanceSqr = distanceSqr;
						value = list[i];
					}
				}
			}
			return value;
		}

		public static Vector3 FindClosest(this IList<Vector3> list, Vector3 toPoint)
		{
			var value = Vector3Tools.NaN;
			var closestDistanceSqr = float.MaxValue;
			if (list != null)
			{
				for (var i = 0; i < list.Count; i++)
				{
					var distanceSqr = list[i].SqrDistanceTo(toPoint);
					if (closestDistanceSqr > distanceSqr)
					{
						closestDistanceSqr = distanceSqr;
						value = list[i];
					}
				}
			}
			return value;
		}

		public static T FindClosest<T>(this IList<T> list, Func<T, float> calculateSqrDistance) where T : class
		{
			var value = default(T);
			var closestDistanceSqr = float.MaxValue;
			if (list != null)
			{
				for (var i = 0; i < list.Count; i++)
				{
					var distanceSqr = calculateSqrDistance(list[i]);
					if (closestDistanceSqr > distanceSqr)
					{
						closestDistanceSqr = distanceSqr;
						value = list[i];
					}
				}
			}
			return value;
		}

		#endregion

		#region Closest Point On Line

		public static Vector2 ClosestPointOnLine(Vector2 lineStart, Vector2 lineEnd, Vector2 point)
		{
			var lineDirection = (lineEnd - lineStart).normalized;
			var closestPoint = Vector2.Dot(point - lineStart, lineDirection) / Vector2.Dot(lineDirection, lineDirection);
			return lineStart + (closestPoint * lineDirection);
		}

		public static Vector3 ClosestPointOnLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
		{
			var lineDirection = (lineEnd - lineStart).normalized;
			var closestPoint = Vector3.Dot(point - lineStart, lineDirection) / Vector3.Dot(lineDirection, lineDirection);
			return lineStart + (closestPoint * lineDirection);
		}

		public static Vector3 ClosestPointOnLineSegment(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
		{
			var diff = lineEnd - lineStart;
			var lineDirection = diff.normalized;
			var closestPoint = Vector3.Dot(point - lineStart, lineDirection);

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
			var diff = lineEnd - lineStart;
			var lineDirection = diff.normalized;
			var closestPoint = Vector3.Dot(point - lineStart, lineDirection);

			// Clamp to line segment
			if (closestPoint < 0f)
			{
				distanceFromStart = 0f;
				return lineStart;
			}
			var diffMagnitude = diff.magnitude;
			if (closestPoint > diffMagnitude)
			{
				distanceFromStart = diffMagnitude;
				return lineEnd;
			}

			distanceFromStart = closestPoint;
			return lineStart + (closestPoint * lineDirection);
		}

		public static Vector2 ClosestPointOnLineSegment(Vector2 lineStart, Vector2 lineEnd, Vector2 point, out float distanceFromStart)
		{
			// TODO: OPTIMIZATION: This is directly copied from 3D calculations. See if there is a faster algorithm in 2D.
			var diff = lineEnd - lineStart;
			var lineDirection = diff.normalized;
			var closestPoint = Vector2.Dot(point - lineStart, lineDirection);

			// Clamp to line segment
			if (closestPoint < 0f)
			{
				distanceFromStart = 0f;
				return lineStart;
			}
			var diffMagnitude = diff.magnitude;
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
			var diff = lineEnd - lineStart;
			var lineDirection = diff.normalized;
			var closestPoint = Vector3.Dot(point - lineStart, lineDirection);

			// Clamp to line segment
			if (closestPoint < 0f)
			{
				return 0f;
			}
			var diffMagnitude = diff.magnitude;
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
			var b = line1Point2 - line1Point1;
			var d = line2Point2 - line2Point1;
			var bDotDPerp = b.x * d.y - b.y * d.x;

			// if b dot d == 0, it means the lines are parallel so have infinite intersection points
			if (bDotDPerp.IsZero())
				return false;

			var c = line2Point1 - line1Point1;
			var lineFactor = (c.x * d.y - c.y * d.x) / bDotDPerp;
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
				totalDistance += previousPoint.DistanceTo(currentPoint);
				previousPoint = currentPoint;
			}
			return totalDistance;
		}

		public static float CalculateLineStripLength(this IList<Vector3> points, int startIndex, int count)
		{
			if (points == null || points.Count < 2 || count < 2)
				return 0f;

			var totalDistance = 0f;
			var previousPoint = points[0];
			var endIndex = startIndex + count;
			for (int i = startIndex + 1; i < endIndex; i++)
			{
				var currentPoint = points[i];
				totalDistance += previousPoint.DistanceTo(currentPoint);
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
			for (int i = 1; i < points.Count; i++)
			{
				var currentPoint = points[i];
				totalDistance += previousPoint.DistanceTo(currentPoint);
				previousPoint = currentPoint;
			}
			return totalDistance;
		}

		public static float CalculateLineStripLength(this IList<Vector2> points, int startIndex, int count)
		{
			if (points == null || points.Count < 2 || count < 2)
				return 0f;

			var totalDistance = 0f;
			var previousPoint = points[0];
			var endIndex = startIndex + count;
			for (int i = startIndex + 1; i < endIndex; i++)
			{
				var currentPoint = points[i];
				totalDistance += previousPoint.DistanceTo(currentPoint);
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

				float distanceFromStartOfCurrentSegmentClosestPoint;
				var currentSegmentClosestPoint = ClosestPointOnLineSegment(previousPoint, currentPoint, point, out distanceFromStartOfCurrentSegmentClosestPoint);
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

				float distanceFromStartOfCurrentSegmentClosestPoint;
				var currentSegmentClosestPoint = ClosestPointOnLineSegment(previousPoint, currentPoint, point, out distanceFromStartOfCurrentSegmentClosestPoint);
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

				float distanceFromStartOfCurrentSegmentClosestPoint;
				var currentSegmentClosestPoint = ClosestPointOnLineSegment(previousPoint, currentPoint, point, out distanceFromStartOfCurrentSegmentClosestPoint);
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

				float distanceFromStartOfCurrentSegmentClosestPoint;
				var currentSegmentClosestPoint = ClosestPointOnLineSegment(previousPoint, currentPoint, point, out distanceFromStartOfCurrentSegmentClosestPoint);
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

				float distanceFromStartOfCurrentSegmentClosestPoint;
				var currentSegmentClosestPoint = MathTools.ClosestPointOnLineSegment(previousPointPosition, currentPointPosition, point, out distanceFromStartOfCurrentSegmentClosestPoint);
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

		public static Bounds BoundsNaN = new Bounds(Vector3Tools.NaN, Vector3Tools.NaN);

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

		public static Rect Combined(this Rect rect1, Rect rect2)
		{
			var xMin = Mathf.Min(rect1.xMin, rect2.xMin);
			var yMin = Mathf.Min(rect1.yMin, rect2.yMin);
			return new Rect(
				xMin,
				yMin,
				Mathf.Max(rect1.xMax, rect2.xMax) - xMin,
				Mathf.Max(rect1.yMax, rect2.yMax) - yMin);
		}

		public static Rect Expanded(this Rect rect, float expand)
		{
			return new Rect(
					rect.xMin - expand,
					rect.yMin - expand,
					rect.width + expand * 2f,
					rect.height + expand * 2f);
		}

		public static Rect Expanded(this Rect rect, float expandX, float expandY)
		{
			return new Rect(
					rect.xMin - expandX,
					rect.yMin - expandY,
					rect.width + expandX * 2f,
					rect.height + expandY * 2f);
		}

		public static Rect Expanded(this Rect rect, float expandTop, float expandLeft, float expandBottom, float expandRight)
		{
			return new Rect(
					rect.xMin - expandLeft,
					rect.yMin - expandTop,
					rect.width + (expandLeft + expandRight),
					rect.height + (expandTop + expandBottom));
		}

		public static Rect Expanded(this Rect rect, RectOffset expand)
		{
			return new Rect(
					rect.xMin - expand.left,
					rect.yMin - expand.top,
					rect.width + expand.horizontal,
					rect.height + expand.vertical);
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

		#region Alignment

		public static int AlignCenterToContainer(this int objectDimensions, int containerDimensions)
		{
			return (containerDimensions - objectDimensions) >> 1;
		}

		public static float AlignCenterToContainer(this float objectDimensions, float containerDimensions)
		{
			return (containerDimensions - objectDimensions) * 0.5f;
		}

		public static Vector2 AlignCenterToContainer(this Vector2 objectDimensions, Vector2 containerDimensions)
		{
			return new Vector2(
				objectDimensions.x.AlignCenterToContainer(containerDimensions.x),
				objectDimensions.y.AlignCenterToContainer(containerDimensions.y));
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

		public static bool Linecast(this Plane plane, Vector3 line1, Vector3 line2)
		{
			return !plane.SameSide(line1, line2);
		}

		public static bool Linecast(this Plane plane, Vector3 line1, Vector3 line2, out Vector3 intersection)
		{
			var distanceToPoint1 = plane.GetDistanceToPoint(line1);
			var distanceToPoint2 = plane.GetDistanceToPoint(line2);
			var notIntersected = distanceToPoint1 > 0.0 && distanceToPoint2 > 0.0 || distanceToPoint1 <= 0.0 && distanceToPoint2 <= 0.0;
			if (notIntersected)
			{
				intersection = Vector3Tools.NaN;
				return false;
			}

			var totalDistance = distanceToPoint1 - distanceToPoint2;
			var ratio = distanceToPoint1 / totalDistance;
			intersection = line1 + (line2 - line1) * ratio;
			return true;
		}

		public static bool LinecastWithProximity(this Plane plane, Vector3 line1, Vector3 line2, Vector3 proximityCheckingPoint, float proximityCheckingRadius)
		{
			var distanceToPoint1 = plane.GetDistanceToPoint(line1);
			var distanceToPoint2 = plane.GetDistanceToPoint(line2);
			var notIntersected = distanceToPoint1 > 0.0 && distanceToPoint2 > 0.0 || distanceToPoint1 <= 0.0 && distanceToPoint2 <= 0.0;
			if (notIntersected)
			{
				return false;
			}

			var totalDistance = distanceToPoint1 - distanceToPoint2;
			var ratio = distanceToPoint1 / totalDistance;
			var intersection = line1 + (line2 - line1) * ratio;

			var distanceSqr = (proximityCheckingPoint - intersection).sqrMagnitude;
			return distanceSqr < proximityCheckingRadius * proximityCheckingRadius;
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
				return Vector3Tools.NaN;
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

		public static string ToIntString(this bool me)
		{
			return me ? "1" : "0";
		}

		public static bool InvertIf(this bool me, bool doInvert)
		{
			return doInvert ? !me : me;
		}

		#endregion

		#region Tancant Filter

		public static float TancantFilter(float x, float rampScale)
		{
			return (float)((Math.Tanh((x - rampScale * 0.5) * 6.0 / rampScale) + 1.0) * 0.5);
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

		public static bool IsSnapped(this Vector2 value, float snapStep, float snapOffset, float precision = 0.001f)
		{
			return
				IsSnapped(value.x, snapStep, snapOffset, precision) &&
				IsSnapped(value.y, snapStep, snapOffset, precision);
		}

		public static bool IsSnapped(this Vector3 value, float snapStep, float snapOffset, float precision = 0.001f)
		{
			return
				IsSnapped(value.x, snapStep, snapOffset, precision) &&
				IsSnapped(value.y, snapStep, snapOffset, precision) &&
				IsSnapped(value.z, snapStep, snapOffset, precision);
		}

		public static bool IsSnapped(this Vector4 value, float snapStep, float snapOffset, float precision = 0.001f)
		{
			return
				IsSnapped(value.x, snapStep, snapOffset, precision) &&
				IsSnapped(value.y, snapStep, snapOffset, precision) &&
				IsSnapped(value.z, snapStep, snapOffset, precision) &&
				IsSnapped(value.w, snapStep, snapOffset, precision);
		}

		public static float Snap(this float value, float snapStep, float snapOffset)
		{
			return Mathf.Round((value + snapOffset) / snapStep) * snapStep - snapOffset;
		}

		public static Vector2 Snap(this Vector2 value, float snapStep, float snapOffset)
		{
			return new Vector2(
				Snap(value.x, snapStep, snapOffset),
				Snap(value.y, snapStep, snapOffset));
		}

		public static Vector3 Snap(this Vector3 value, float snapStep, float snapOffset)
		{
			return new Vector3(
				Snap(value.x, snapStep, snapOffset),
				Snap(value.y, snapStep, snapOffset),
				Snap(value.z, snapStep, snapOffset));
		}

		public static Vector4 Snap(this Vector4 value, float snapStep, float snapOffset)
		{
			return new Vector4(
				Snap(value.x, snapStep, snapOffset),
				Snap(value.y, snapStep, snapOffset),
				Snap(value.z, snapStep, snapOffset),
				Snap(value.w, snapStep, snapOffset));
		}

		#endregion
	}

}