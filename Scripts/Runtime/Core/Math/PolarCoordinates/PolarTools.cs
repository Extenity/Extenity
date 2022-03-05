using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Extenity.MathToolbox
{

	public static class PolarTools
	{
		public static Polar CartesianToPolar(this float2 cartesian)
		{
			return new Polar(atan2(cartesian.y, cartesian.x), length(cartesian));
		}

		public static Polar CartesianToPolar(this float2 cartesian, float overrideDistance)
		{
			return new Polar(atan2(cartesian.y, cartesian.x), overrideDistance);
		}

		public static float2 PolarToCartesian(this Polar polar)
		{
			return new float2(
				polar.Distance * cos(polar.AngleRad),
				polar.Distance * sin(polar.AngleRad));
		}

		public static Polar PolarLerp(this float2 currentPosition, float2 targetPosition, float tRotation, float tDistance)
		{
			// Convert to polar
			var currentPolar = CartesianToPolar(currentPosition);
			var targetPolar = CartesianToPolar(targetPosition);

			// Smooth in polar
			var angleDiff = targetPolar.AngleRad - currentPolar.AngleRad;
			if (angleDiff > PI) angleDiff -= MathTools.TwoPI; // Wrap around 360 degrees
			else if (angleDiff < -PI) angleDiff += MathTools.TwoPI;
			var radiusDiff = targetPolar.Distance - currentPolar.Distance;
			return new Polar(
				currentPolar.AngleRad + angleDiff * tRotation,
				currentPolar.Distance + radiusDiff * tDistance);
		}
	}

}
