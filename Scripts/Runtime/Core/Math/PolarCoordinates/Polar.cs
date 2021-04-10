using System;

namespace Extenity.MathToolbox
{

	[Serializable]
	public struct Polar
	{
		public float AngleRad;
		public float Distance;

		public Polar(float angleRad, float distance)
		{
			AngleRad = angleRad;
			Distance = distance;
		}
	}

}
