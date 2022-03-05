using Unity.Mathematics;

namespace Extenity.MathToolbox
{

	public struct Circle
	{
		public float3 center;
		public float radius;

		public Circle(float3 center, float radius)
		{
			this.center = center;
			this.radius = radius;
		}

	}

}
