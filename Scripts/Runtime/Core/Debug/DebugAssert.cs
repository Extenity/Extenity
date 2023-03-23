using System.Collections.Generic;
using System.Diagnostics;
using static Unity.Mathematics.math;

namespace Extenity.DebugToolbox
{

	public class DebugAssert
	{
		private const int AssertIgnoreMessagesFailCount = 3;
		private static int AssertFailCount = 0;

		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void Fail() { InternalCheck(false, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void Fail(string assertString) { InternalCheck(false, assertString); }

		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsTrue(bool condition) { InternalCheck(condition, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsTrue(bool condition, string assertString) { InternalCheck(condition, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsFalse(bool condition) { InternalCheck(!condition, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsFalse(bool condition, string assertString) { InternalCheck(!condition, assertString); }

		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNull(object obj) { InternalCheck(obj == null, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNull(object obj, string assertString) { InternalCheck(obj == null, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNotNull(object obj) { InternalCheck(obj != null, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNotNull(object obj, string assertString) { InternalCheck(obj != null, assertString); }

		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNullOrEmpty(string value) { InternalCheck(string.IsNullOrEmpty(value), null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNullOrEmpty<T>(ICollection<T> list) { InternalCheck(list == null || list.Count == 0, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNullOrEmpty<T>(T[] list) { InternalCheck(list == null || list.Length == 0, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNotNullOrEmpty(string value) { InternalCheck(!string.IsNullOrEmpty(value), null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNotNullOrEmpty<T>(ICollection<T> list) { InternalCheck(!(list == null || list.Count == 0), null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNotNullOrEmpty<T>(T[] list) { InternalCheck(!(list == null || list.Length == 0), null); }

		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsEqual(object value1, object value2) { InternalCheck(value1.Equals(value2), null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsEqual(object value1, object value2, string assertString) { InternalCheck(value1.Equals(value2), assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNotEqual(object value1, object value2) { InternalCheck(!value1.Equals(value2), null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNotEqual(object value1, object value2, string assertString) { InternalCheck(!value1.Equals(value2), assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsAlmostEqual(int value1, int value2, int tolerance) { InternalCheck(abs(value1 - value2) <= tolerance, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsAlmostEqual(int value1, int value2, int tolerance, string assertString) { InternalCheck(abs(value1 - value2) <= tolerance, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsAlmostEqual(long value1, long value2, long tolerance) { InternalCheck(abs(value1 - value2) <= tolerance, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsAlmostEqual(long value1, long value2, long tolerance, string assertString) { InternalCheck(abs(value1 - value2) <= tolerance, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsAlmostEqual(float value1, float value2, float tolerance) { InternalCheck(abs(value1 - value2) <= tolerance, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsAlmostEqual(float value1, float value2, float tolerance, string assertString) { InternalCheck(abs(value1 - value2) <= tolerance, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsAlmostEqual(double value1, double value2, double tolerance) { InternalCheck(abs(value1 - value2) <= tolerance, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsAlmostEqual(double value1, double value2, double tolerance, string assertString) { InternalCheck(abs(value1 - value2) <= tolerance, assertString); }

		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsZero(int value) { InternalCheck(value == 0, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsZero(int value, string assertString) { InternalCheck(value == 0, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsZero(long value) { InternalCheck(value == 0, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsZero(long value, string assertString) { InternalCheck(value == 0, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsZero(float value) { InternalCheck(value == 0, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsZero(float value, string assertString) { InternalCheck(value == 0, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsZero(double value) { InternalCheck(value == 0, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsZero(double value, string assertString) { InternalCheck(value == 0, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNotZero(int value) { InternalCheck(value != 0, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNotZero(int value, string assertString) { InternalCheck(value != 0, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNotZero(long value) { InternalCheck(value != 0, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNotZero(long value, string assertString) { InternalCheck(value != 0, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNotZero(float value) { InternalCheck(value != 0, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNotZero(float value, string assertString) { InternalCheck(value != 0, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNotZero(double value) { InternalCheck(value != 0, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNotZero(double value, string assertString) { InternalCheck(value != 0, assertString); }

		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsBetweenZeroOne(float value) { InternalCheck(value >= 0 && value <= 1, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsBetweenZeroOne(float value, string assertString) { InternalCheck(value >= 0 && value <= 1, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsBetweenZeroOne(double value) { InternalCheck(value >= 0 && value <= 1, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsBetweenZeroOne(double value, string assertString) { InternalCheck(value >= 0 && value <= 1, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsBetweenNegOnePosOne(float value) { InternalCheck(value >= -1 && value <= 1, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsBetweenNegOnePosOne(float value, string assertString) { InternalCheck(value >= -1 && value <= 1, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsBetweenNegOnePosOne(double value) { InternalCheck(value >= -1 && value <= 1, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsBetweenNegOnePosOne(double value, string assertString) { InternalCheck(value >= -1 && value <= 1, assertString); }

		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsPositive(int value) { InternalCheck(value > 0, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsPositive(int value, string assertString) { InternalCheck(value > 0, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsPositive(long value) { InternalCheck(value > 0, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsPositive(long value, string assertString) { InternalCheck(value > 0, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsPositive(float value) { InternalCheck(value > 0, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsPositive(float value, string assertString) { InternalCheck(value > 0, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsPositive(double value) { InternalCheck(value > 0, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsPositive(double value, string assertString) { InternalCheck(value > 0, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNegative(int value) { InternalCheck(value < 0, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNegative(int value, string assertString) { InternalCheck(value < 0, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNegative(long value) { InternalCheck(value < 0, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNegative(long value, string assertString) { InternalCheck(value < 0, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNegative(float value) { InternalCheck(value < 0, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNegative(float value, string assertString) { InternalCheck(value < 0, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNegative(double value) { InternalCheck(value < 0, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNegative(double value, string assertString) { InternalCheck(value < 0, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsPositiveOrZero(int value) { InternalCheck(value >= 0, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsPositiveOrZero(int value, string assertString) { InternalCheck(value >= 0, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsPositiveOrZero(long value) { InternalCheck(value >= 0, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsPositiveOrZero(long value, string assertString) { InternalCheck(value >= 0, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsPositiveOrZero(float value) { InternalCheck(value >= 0, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsPositiveOrZero(float value, string assertString) { InternalCheck(value >= 0, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsPositiveOrZero(double value) { InternalCheck(value >= 0, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsPositiveOrZero(double value, string assertString) { InternalCheck(value >= 0, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNegativeOrZero(int value) { InternalCheck(value <= 0, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNegativeOrZero(int value, string assertString) { InternalCheck(value <= 0, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNegativeOrZero(long value) { InternalCheck(value <= 0, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNegativeOrZero(long value, string assertString) { InternalCheck(value <= 0, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNegativeOrZero(float value) { InternalCheck(value <= 0, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNegativeOrZero(float value, string assertString) { InternalCheck(value <= 0, assertString); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNegativeOrZero(double value) { InternalCheck(value <= 0, null); }
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] public static void IsNegativeOrZero(double value, string assertString) { InternalCheck(value <= 0, assertString); }

		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
		private static void InternalCheck(bool condition, string assertString)
		{
			if (!condition)
			{
				AssertFailCount++;

				if (AssertFailCount <= AssertIgnoreMessagesFailCount)
				{
					var trace = new StackTrace(true);
					var frame = trace.GetFrame(2);
					var assertInformation = $"Assertion failed! {assertString}\nFilename: {frame.GetFileName()}\nMethod: {frame.GetMethod()}\nLine: {frame.GetFileLineNumber()}";

					// Logging would be included in both Debug and UNITY_EDITOR builds
					Log.With(nameof(DebugAssert)).Error(assertInformation);

#if UNITY_EDITOR
					// Pause application
					UnityEngine.Debug.Break();

					// Display message box
					//if (UnityEditor.EditorUtility.DisplayDialog("Assert!", assertString + "\n" + assertInformation, "OK"))
					//{
					//	// Goto assert line
					//	UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(myFrame.GetFileName(), myFrame.GetFileLineNumber());
					//}
#endif
				}
			}
		}
	}

}
