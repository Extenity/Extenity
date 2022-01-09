using Unity.Mathematics;
using static Unity.Mathematics.math;

// ReSharper disable IdentifierTypo

namespace Extenity.MathToolbox
{

	public class Mathfx
	{
		public static float Hermite(float start, float end, float value)
		{
			return lerp(start, end, value * value * (3.0f - 2.0f * value));
		}
		public static float Hermite(float x)
		{
			if (x < 0.0f) return 0.0f;
			if (x > 1.0f) return 1.0f;
			return x * x * (3.0f - 2.0f * x);
		}
		public static float HermiteSymmetric(float x)
		{
			if (x < -1.0f) return -1.0f;
			if (x > 1.0f) return 1.0f;
			if (x > 0.0f)
				return x * x * (3.0f - 2.0f * x);
			else
			{
				x = -x;
				return -(x * x * (3.0f - 2.0f * x));
			}
		}

		public static float Sinerp(float start, float end, float value)
		{
			return lerp(start, end, sin(value * PI * 0.5f));
		}

		public static float Coserp(float start, float end, float value)
		{
			return lerp(start, end, 1.0f - cos(value * PI * 0.5f));
		}

		public static float Berp(float start, float end, float value)
		{
			value = clamp(value, 0f, 1f);
			value = (sin(value * PI * (0.2f + 2.5f * value * value * value)) * pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
			return start + (end - start) * value;
		}

		public static float SmoothStep(float x, float min, float max)
		{
			x = clamp(x, min, max);
			float v1 = (x - min) / (max - min);
			float v2 = (x - min) / (max - min);
			return -2 * v1 * v1 * v1 + 3 * v2 * v2;
		}

		public static float Lerp(float start, float end, float value)
		{
			return ((1.0f - value) * start) + (value * end);
		}

		public static float3 NearestPoint(float3 lineStart, float3 lineEnd, float3 point)
		{
			float3 lineDirection = normalize(lineEnd - lineStart);
			float closestPoint = dot((point - lineStart), lineDirection) / dot(lineDirection, lineDirection);
			return lineStart + (closestPoint * lineDirection);
		}

		public static float3 NearestPointStrict(float3 lineStart, float3 lineEnd, float3 point)
		{
			float3 fullDirection = lineEnd - lineStart;
			float3 lineDirection = normalize(fullDirection);
			float closestPoint = dot((point - lineStart), lineDirection) / dot(lineDirection, lineDirection);
			return lineStart + (clamp(closestPoint, 0.0f, length(fullDirection)) * lineDirection);
		}
		public static float Bounce(float x)
		{
			return abs(sin(6.28f * (x + 1f) * (x + 1f)) * (1f - x));
		}
		public static float BounceOnce(float x)
		{
			return sin(x * PI);
		}

		// test for value that is near specified float (due to floating point inprecision)
		// all thanks to Opless for this!
		public static bool Approx(float val, float about, float range)
		{
			return ((abs(val - about) < range));
		}

		// test if a Vector3 is close to another Vector3 (due to floating point inprecision)
		// compares the square of the distance to the square of the range as this 
		// avoids calculating a square root which is much slower than squaring the range
		public static bool Approx(float3 val, float3 about, float range)
		{
			return lengthsq(val - about) < range * range;
		}

		/*
		  * CLerp - Circular Lerp - is like lerp but handles the wraparound from 0 to 360.
		  * This is useful when interpolating eulerAngles and the object
		  * crosses the 0/360 boundary.  The standard Lerp function causes the object
		  * to rotate in the wrong direction and looks stupid. Clerp fixes that.
		  */
		public static float Clerp(float start, float end, float value)
		{
			float min = 0.0f;
			float max = 360.0f;
			float half = abs((max - min) / 2.0f);//half the distance between min and max
			float retval = 0.0f;
			float diff = 0.0f;

			if ((end - start) < -half)
			{
				diff = ((max - start) + end) * value;
				retval = start + diff;
			}
			else if ((end - start) > half)
			{
				diff = -((max - end) + start) * value;
				retval = start + diff;
			}
			else retval = start + (end - start) * value;

			//Log.Info("Start: "  + start + "   End: " + end + "  Value: " + value + "  Half: " + half + "  Diff: " + diff + "  Retval: " + retval);
			return retval;
		}

	}

}
