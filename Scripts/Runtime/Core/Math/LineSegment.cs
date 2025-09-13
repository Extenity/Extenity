using Unity.Mathematics;
using static Unity.Mathematics.math;

// ReSharper disable InconsistentNaming

namespace Extenity.MathToolbox
{

	public struct LineSegment
	{
		public float3 p1;
		public float3 p2;

		public LineSegment(float3 p1, float3 p2)
		{
			this.p1 = p1;
			this.p2 = p2;
		}

		public float3 Direction
		{
			get { return normalize(p2 - p1); }
		}
	}

}
