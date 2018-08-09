using Extenity.MathToolbox;
using NUnit.Framework;
using UnityEngine;

namespace ExtenityTests.MathToolbox
{

	public class Test_MathTools : AssertionHelper
	{
		#region Int Float Double

		[Test]
		public void IsZero()
		{
			Assert.True(MathTools.IsZero(0f));
			Assert.True(!MathTools.IsZero(1f));
			Assert.True(!MathTools.IsZero(-1f));
			Assert.True(!MathTools.IsZero(100000f));
			Assert.True(!MathTools.IsZero(-100000f));
			Assert.True(!MathTools.IsZero(float.MaxValue));
			Assert.True(!MathTools.IsZero(float.MinValue));
			Assert.True(!MathTools.IsZero(float.PositiveInfinity));
			Assert.True(!MathTools.IsZero(float.NegativeInfinity));
			Assert.True(!MathTools.IsZero(float.NaN));

			var tolerance = 0.001f;
			Assert.True(MathTools.IsZero(0f, tolerance));
			Assert.True(MathTools.IsZero(tolerance, tolerance) || !MathTools.IsZero(tolerance, tolerance));
			Assert.True(MathTools.IsZero(-tolerance, tolerance) || !MathTools.IsZero(-tolerance, tolerance));
			Assert.True(!MathTools.IsZero(tolerance + 0.00001f, tolerance));
			Assert.True(!MathTools.IsZero(-tolerance - 0.00001f, tolerance));
		}

		[Test]
		public void IsNotZero()
		{
			Assert.True(!MathTools.IsNotZero(0f));
			Assert.True(MathTools.IsNotZero(1f));
			Assert.True(MathTools.IsNotZero(-1f));
			Assert.True(MathTools.IsNotZero(100000f));
			Assert.True(MathTools.IsNotZero(-100000f));
			Assert.True(MathTools.IsNotZero(float.MaxValue));
			Assert.True(MathTools.IsNotZero(float.MinValue));
			Assert.True(MathTools.IsNotZero(float.PositiveInfinity));
			Assert.True(MathTools.IsNotZero(float.NegativeInfinity));
			Assert.True(!MathTools.IsNotZero(float.NaN));

			var tolerance = 0.001f;
			Assert.True(!MathTools.IsNotZero(0f, tolerance));
			Assert.True(MathTools.IsNotZero(tolerance, tolerance) || !MathTools.IsNotZero(tolerance, tolerance));
			Assert.True(MathTools.IsNotZero(-tolerance, tolerance) || !MathTools.IsNotZero(-tolerance, tolerance));
			Assert.True(MathTools.IsNotZero(tolerance + 0.00001f, tolerance));
			Assert.True(MathTools.IsNotZero(-tolerance - 0.00001f, tolerance));
		}

		[Test]
		public void IsUnit()
		{
			Assert.True(MathTools.IsUnit(1f));
			Assert.True(!MathTools.IsUnit(0f));
			Assert.True(!MathTools.IsUnit(float.MaxValue));
			Assert.True(!MathTools.IsUnit(float.MinValue));
			Assert.True(!MathTools.IsUnit(float.PositiveInfinity));
			Assert.True(!MathTools.IsUnit(float.NegativeInfinity));
			Assert.True(!MathTools.IsUnit(float.NaN));
		}

		[Test]
		public void IsAlmostEqual()
		{
			Assert.True(MathTools.IsAlmostEqual(0f, 0f));
			Assert.True(MathTools.IsAlmostEqual(1f, 1f));
			Assert.True(MathTools.IsAlmostEqual(10f, 10f));
			Assert.True(!MathTools.IsAlmostEqual(0f, 100f));
			Assert.True(!MathTools.IsAlmostEqual(1f, 100f));
			Assert.True(!MathTools.IsAlmostEqual(10f, 100f));

			Assert.True(MathTools.IsAlmostEqual(float.MaxValue, float.MaxValue));
			Assert.True(MathTools.IsAlmostEqual(float.MinValue, float.MinValue));
			Assert.True(!MathTools.IsAlmostEqual(float.MinValue, float.MaxValue));
			Assert.True(!MathTools.IsAlmostEqual(float.MaxValue, float.MinValue));

			Assert.True(!MathTools.IsAlmostEqual(float.PositiveInfinity, float.PositiveInfinity));
			Assert.True(!MathTools.IsAlmostEqual(float.NegativeInfinity, float.NegativeInfinity));
			Assert.True(!MathTools.IsAlmostEqual(float.NegativeInfinity, float.PositiveInfinity));
			Assert.True(!MathTools.IsAlmostEqual(float.PositiveInfinity, float.NegativeInfinity));

			Assert.True(!MathTools.IsAlmostEqual(float.NaN, 0f));
			Assert.True(!MathTools.IsAlmostEqual(float.NaN, 1f));
			Assert.True(!MathTools.IsAlmostEqual(float.NaN, float.PositiveInfinity));
			Assert.True(!MathTools.IsAlmostEqual(float.NaN, float.NegativeInfinity));
			Assert.True(!MathTools.IsAlmostEqual(float.NaN, float.NaN));
		}

		[Test]
		public void IsBetween()
		{
			Assert.True(!MathTools.IsBetween(0f, 0f, 0f));
			Assert.True(MathTools.IsBetweenOrEqual(0f, 0f, 0f));
			Assert.True(MathTools.IsBetween(0f, -1f, 1f));
			Assert.True(MathTools.IsBetween(0f, 1f, -1f)); // Inverse min and max works

			Assert.True(!MathTools.IsBetween(100f, 100f, 100f));
			Assert.True(MathTools.IsBetweenOrEqual(100f, 100f, 100f));
			Assert.True(MathTools.IsBetween(100f, 99f, 101f));
			Assert.True(MathTools.IsBetween(100f, 101f, 99f)); // Inverse min and max works

			Assert.True(!MathTools.IsBetween(float.MaxValue, float.MaxValue, float.MaxValue));
			Assert.True(!MathTools.IsBetween(float.MinValue, float.MinValue, float.MinValue));

			Assert.True(!MathTools.IsBetween(float.NaN, float.NaN, float.NaN));
			Assert.True(!MathTools.IsBetween(float.NaN, float.NegativeInfinity, float.PositiveInfinity));
		}

		[Test]
		public void IsBetweenMinMax()
		{
			Assert.True(!MathTools.IsBetweenMinMax(0f, 0f, 0f));
			Assert.True(MathTools.IsBetweenOrEqualMinMax(0f, 0f, 0f));
			Assert.True(MathTools.IsBetweenMinMax(0f, -1f, 1f));
			Assert.True(!MathTools.IsBetweenMinMax(0f, 1f, -1f)); // Inverse min and max won't work

			Assert.True(!MathTools.IsBetweenMinMax(100f, 100f, 100f));
			Assert.True(MathTools.IsBetweenOrEqualMinMax(100f, 100f, 100f));
			Assert.True(MathTools.IsBetweenMinMax(100f, 99f, 101f));
			Assert.True(!MathTools.IsBetweenMinMax(100f, 101f, 99f)); // Inverse min and max won't work

			Assert.True(!MathTools.IsBetweenMinMax(float.MaxValue, float.MaxValue, float.MaxValue));
			Assert.True(!MathTools.IsBetweenMinMax(float.MinValue, float.MinValue, float.MinValue));

			Assert.True(!MathTools.IsBetweenMinMax(float.NaN, float.NaN, float.NaN));
			Assert.True(!MathTools.IsBetweenMinMax(float.NaN, float.NegativeInfinity, float.PositiveInfinity));
		}

		[Test]
		public void MakeZeroIfNaN()
		{
			Assert.AreEqual(MathTools.MakeZeroIfNaN(0f), 0f);
			Assert.AreEqual(MathTools.MakeZeroIfNaN(1f), 1f);
			Assert.AreEqual(MathTools.MakeZeroIfNaN(-1f), -1f);
			Assert.AreEqual(MathTools.MakeZeroIfNaN(float.MinValue), float.MinValue);
			Assert.AreEqual(MathTools.MakeZeroIfNaN(float.MaxValue), float.MaxValue);
			Assert.AreEqual(MathTools.MakeZeroIfNaN(float.NegativeInfinity), float.NegativeInfinity);
			Assert.AreEqual(MathTools.MakeZeroIfNaN(float.PositiveInfinity), float.PositiveInfinity);
			Assert.AreEqual(MathTools.MakeZeroIfNaN(float.NaN), 0f);
		}

		#endregion

		#region Vector

		[Test]
		public void VectorScaleDoesNotAffectTheOriginalValue()
		{
			var originalVector2 = new Vector2(1f, 2f);

			originalVector2.ScaleX(5f);
			Assert.AreEqual(originalVector2.x, 1f);

			var newVector2 = originalVector2.ScaleX(5f);
			Assert.AreEqual(originalVector2.x, 1f);
			Assert.AreEqual(newVector2.x, 5f);
			Assert.AreNotEqual(originalVector2.x, newVector2.x);
		}

		#endregion
	}

}
