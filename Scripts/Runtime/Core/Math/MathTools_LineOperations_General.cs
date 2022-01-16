using Unity.Mathematics;

namespace Extenity.MathToolbox
{

	public static partial class MathTools
	{
		#region Line Intersection

		public static bool CheckLineLineIntersection(
			float2 line1Point1, float2 line1Point2,
			float2 line2Point1, float2 line2Point2)
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

		#region Spline Operations - Bezier, CatmullRom

		public static float3 GetBezierPoint(float3 p1, float3 p2, float3 p3, float t)
		{
			var it = 1 - t;
			var it2 = it * it;
			var itt = it * t;
			var t2 = t * t;

			return new float3(
				(p1.x * it2 + 2 * p2.x * itt + p3.x * t2),
				(p1.y * it2 + 2 * p2.y * itt + p3.y * t2),
				(p1.z * it2 + 2 * p2.z * itt + p3.z * t2)
				);
		}

		public static float3 GetCatmullRomPoint(
			float3 previous, float3 start, float3 end, float3 next,
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

			var percentageSquare = percentage * percentage;
			var percentageCube = percentageSquare * percentage;

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
	}

}
