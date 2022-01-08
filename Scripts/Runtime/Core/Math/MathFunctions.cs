using Extenity.DebugToolbox;
using static Unity.Mathematics.math;

namespace Extenity.MathToolbox
{

	public class BiasLerpContext
	{
		public float lastBias = -1.0f;
		public float lastExponent = 0.0f;
	}

	public static class MathFunctions
	{
		#region FastTanH

		/// <summary>
		/// Really close approximation of TanH.
		/// Source: https://math.stackexchange.com/questions/107292/rapid-approximation-of-tanhx
		/// </summary>
		public static float FastTanH(float x)
		{
			if (x > 4.971787f)
				return 1f;
			if (x < 4.971787f)
				return -1f;
			var x2 = x * x;
			var a = x * (135135.0f + x2 * (17325.0f + x2 * (378.0f + x2)));
			var b = 135135.0f + x2 * (62370.0f + x2 * (3150.0f + x2 * 28.0f));
			return a / b;
		}

		#endregion

		#region TanHLike

		public static float TanHLike(float x, float centerX, float endX)
		{
			DebugAssert.IsTrue(centerX < endX);
			return TanHLike((x - centerX) / (endX - centerX));
		}

		public static double TanHLike(double x, double centerX, double endX)
		{
			DebugAssert.IsTrue(centerX < endX);
			return TanHLike((x - centerX) / (endX - centerX));
		}

		public static float TanHLike(float x, float endX)
		{
			DebugAssert.IsPositive(endX);
			return TanHLike(x / endX);
		}

		public static double TanHLike(double x, double endX)
		{
			DebugAssert.IsPositive(endX);
			return TanHLike(x / endX);
		}

		public static float TanHLike(float x)
		{
			if (x >= 1) return 1;
			if (x <= -1) return -1;
			if (x < 0)
			{
				x = x + 1;
				return x * x - 1;
			}
			x = 1 - x;
			return 1 - x * x;
		}

		public static double TanHLike(double x)
		{
			if (x >= 1) return 1;
			if (x <= -1) return -1;
			if (x < 0)
			{
				x = x + 1;
				return x * x - 1;
			}
			x = 1 - x;
			return 1 - x * x;
		}

		#endregion

		#region TanHLikeInverted

		/// <summary>
		/// 1  |---------------------------------------------
		///    |                                          .:`
		///    |                                         .:  
		///    |                                        :-   
		///    |                                      `:`    
		///    |                                     --`     
		///    |                                   .:.       
		///    |                                 .:.         
		///    |                              `.:.           
		///    |                           `.--`             
		/// 0.5|                 ``....----.`                
		///    |             `.--..`                         
		///    |           ---`                              
		///    |        `--.                                 
		///    |      `-:`                                   
		///    |     .:`                                     
		///    |   `--                                       
		///    |  .:`                                        
		///    | --                                          
		///    |--                                           
		///    |.                                            
		///    |---------------------|----------------------|
		///   0                     0.5                     1
		/// </summary>
		public static float TanHLikeInverted(float x)
		{
			if (x >= 1f) return 1f;
			if (x <= 0f) return 0f;
			x = x - 0.5f;
			return (x * x * (x > 0f ? 2f : -2f)) + 0.5f;
		}

		public static float TanHLikeInverted(float x, float bias)
		{
			if (x >= 1f) return 1f;
			if (x <= 0f) return 0f;
			var a = x - 0.5f;
			a = (a * a * (a > 0f ? 2f : -2f)) + 0.5f;
			return x + (a - x) * bias; // This is basically a Lerp(x, a, t)
		}

		public static float TanHLikeInvertedUnsafe(float x, float bias)
		{
			var a = x - 0.5f;
			a = (a * a * (a > 0f ? 2f : -2f)) + 0.5f;
			return x + (a - x) * bias; // This is basically a Lerp(x, a, t)
		}

		#endregion

		// TODO: Rename
		public static float Decreasing(float x)
		{
			// Function: 1 - ((1 - x) * (1 - x))
			if (x <= 0f)
				return 0f;
			if (x >= 1f)
				return 1f;
			var a = 1f - x;
			return 1 - a * a;
		}

		// TODO: Rename
		public static float Decreasing(float x, float endX)
		{
			// Function: 1 - ((1 - x * 5) * (1 - x * 5))
			if (x <= 0f)
				return 0f;
			if (x >= endX)
				return 1f;
			var a = 1f - (x / endX);
			return 1 - a * a;
		}

		// TODO: Rename
		public static float Decreasing(float x, float startX, float endX)
		{
			// Function: 1 - ((1 - (x - startX) / (endX - startX)) * (1 - (x - startX) / (endX - startX)))
			if (x <= startX)
				return 0f;
			if (x >= endX)
				return 1f;
			var a = 1f - ((x - startX) / (endX - startX));
			return 1 - a * a;
		}

		#region Biased Lerp

		// Generic biased lerp with optional context optimization:
		//
		// 	BiasedLerp(x, bias)				generic unoptimized
		//	BiasedLerp(x, bias, context)	optimized for bias which changes unfrequently

		private static float BiasWithContext(float x, float bias, BiasLerpContext context)
		{
			if (x <= 0.0f) return 0.0f;
			if (x >= 1.0f) return 1.0f;

			if (bias != context.lastBias)
			{
				if (bias <= 0.0f) return x >= 1.0f ? 1.0f : 0.0f;
				else if (bias >= 1.0f) return x > 0.0f ? 1.0f : 0.0f;
				else if (bias == 0.5f) return x;

				context.lastExponent = log(bias) * -1.4427f;
				context.lastBias = bias;
			}

			return pow(x, context.lastExponent);
		}

		private static float BiasRaw(float x, float bias)
		{
			if (x <= 0.0f) return 0.0f;
			if (x >= 1.0f) return 1.0f;

			if (bias <= 0.0f) return x >= 1.0f ? 1.0f : 0.0f;
			else if (bias >= 1.0f) return x > 0.0f ? 1.0f : 0.0f;
			else if (bias == 0.5f) return x;

			var exponent = log(bias) * -1.4427f;
			return pow(x, exponent);
		}

		public static float BiasedLerp(float x, float bias)
		{
			var result = bias <= 0.5f
				? BiasRaw(abs(x), bias)
				: 1.0f - BiasRaw(1.0f - abs(x), 1.0f - bias);
			return x < 0.0f ? -result : result;
		}

		public static float BiasedLerp(float x, float bias, BiasLerpContext context)
		{
			var result = bias <= 0.5f
				? BiasWithContext(abs(x), bias, context)
				: 1.0f - BiasWithContext(1.0f - abs(x), 1.0f - bias, context);
			return x < 0.0f ? -result : result;
		}

		#endregion
	}

}
