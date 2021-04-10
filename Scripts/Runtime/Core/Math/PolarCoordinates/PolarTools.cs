using UnityEngine;

namespace Extenity.MathToolbox
{

	public static class PolarTools
	{
		public static Polar CartesianToPolar(this Vector2 cartesian)
		{
			return new Polar(Mathf.Atan2(cartesian.y, cartesian.x), cartesian.magnitude);
		}

		public static Polar CartesianToPolar(this Vector2 cartesian, float overrideDistance)
		{
			return new Polar(Mathf.Atan2(cartesian.y, cartesian.x), overrideDistance);
		}

		public static Vector2 PolarToCartesian(this Polar polar)
		{
			return new Vector2(
				polar.Distance * Mathf.Cos(polar.AngleRad),
				polar.Distance * Mathf.Sin(polar.AngleRad));
		}

		public static Polar PolarLerp(this Vector2 currentPosition, Vector2 targetPosition, float tRotation, float tDistance)
		{
			// Convert to polar
			var currentPolar = CartesianToPolar(currentPosition);
			var targetPolar = CartesianToPolar(targetPosition);

			// Smooth in polar
			var angleDiff = targetPolar.AngleRad - currentPolar.AngleRad;
			if (angleDiff > MathTools.PI) angleDiff -= MathTools.TwoPI; // Wrap around 360 degrees
			else if (angleDiff < -MathTools.PI) angleDiff += MathTools.TwoPI;
			var radiusDiff = targetPolar.Distance - currentPolar.Distance;
			return new Polar(
				currentPolar.AngleRad + angleDiff * tRotation,
				currentPolar.Distance + radiusDiff * tDistance);
		}
	}

}
